# PickMeUp — The recommender

## 1. Overview

PickMeUp suggests taxi companies to a passenger through a **Recommended for you** list: the active
companies on the platform, ranked best first, with the reason for each placement shown next to it.

Our recommender is a hybrid of two complementary approaches. A **content-based** part looks only at
the passenger's own record with each company: how often they have ordered from it, how many of those
rides they completed, and how they rated it afterwards. A **popularity** part looks at how every
passenger treats that company, through its completed rides and its average rating. A small **city
affinity** bonus then favours companies in the city where the passenger actually rides. Blending the
two lets us give a useful answer both to a regular with years of habits and to somebody who signed up
a minute ago, without switching to a different algorithm for each.

Two design commitments shape everything below. Every score is built from signals we recorded while
people used the application, never from values supplied by a client. And every recommendation is
**explainable**: each result carries a sentence, written from the same numbers that produced its
score, saying why that company is on the list.

## 2. The signals we collect

We record passenger behaviour in a single table, `InteractionHistory`. Each row ties one passenger to
one company, with a signal type, a numeric value, the order it came from, and the moment it was
written:

| Column | Meaning |
| --- | --- |
| `UserId` | The passenger the signal belongs to |
| `TaxiCompanyId` | The company it is about |
| `SignalTypeId` | `1` Ordered, `2` Completed, `3` Reviewed |
| `Value` | `1` for Ordered and Completed; the star rating for Reviewed |
| `OrderId` | The ride that produced the signal |
| `CreatedAt` | When it was written |

These rows accumulate as a side effect of ordinary use. There are exactly four places in our code
that write them, and each one fires at the moment the underlying action succeeds:

1. **A passenger places an order with a company.** `OrderService.CreateAsync` writes an `Ordered`
   signal once the order is committed.
2. **A driver accepts a ride that was not addressed to a company.** When a passenger orders without
   naming a company, we do not know the company until a driver takes the job, so
   `OrderService.AcceptAsync` writes the `Ordered` signal at that point instead. This way a
   dispatched ride counts towards the passenger's history exactly like a directly ordered one.
3. **A ride is completed.** `OrderService.CompleteAsync` writes a `Completed` signal.
4. **A passenger reviews a ride.** `ReviewService` writes a `Reviewed` signal whose `Value` is the
   rating given, so the star count travels with the signal.

Recording is deliberately non-fatal. `InteractionRecorder` wraps the write in a try/catch, and if it
fails it logs the error and lets the original action stand. Losing a recommender signal must never
cost a passenger their completed ride or their review.

A fresh database also arrives with a seeded history: past rides, reviews and the matching signal rows
for passengers with deliberately different habits, including one with no history at all. Those rows
have the same shape live use produces, so the recommender can be demonstrated on a clean install. No
other mechanism writes to the table, and nothing is back-filled or synthesised at query time.

**Source** — `PickMeUp.Services/Recommendations/InteractionRecorder.cs`,
`PickMeUp.Services/Database/InteractionHistory.cs`

## 3. How we score a company

We score every active company for the signed-in passenger. Ratings enter the formulas on a normalised
scale, where a rating `r` out of 5 becomes `(r − 1) / 4`, so 1 star maps to 0 and 5 stars to 1. A
company with no ratings is treated as neutral at `0.5` rather than as bad. Counts enter through a
saturating function, `min(count / 5, 1)`, so the fifth ride with a company matters and the fiftieth
does not distort the ranking.

### 3.1 The content-based part

This part sees only the passenger's own signals for the company:

```
usage      = min(completedRidesByThisPassenger / 5, 1)
loyalty    = min(ordersByThisPassenger / 5, 1)
ownRating  = (thisPassengersAverageRating − 1) / 4      (0.5 when they have not rated it)

content    = 0.50·usage + 0.20·loyalty + 0.30·ownRating
```

Completed rides carry more weight than orders because a finished ride is stronger evidence of a good
experience than a placed one, and the passenger's own rating carries more than raw ordering volume.

### 3.2 The popularity part

This part is identical for every passenger and reflects the company's standing across the platform:

```
ridesShare = completedRidesOverall / completedRidesOfTheBusiestCompany
rating     = (averageRatingOverall − 1) / 4             (0.5 when it has no reviews)

popularity = 0.50·ridesShare + 0.50·rating
```

Completed rides are measured against the busiest company rather than in absolute terms, which keeps
the component meaningful whether the platform has ten rides or ten thousand.

### 3.3 City affinity

We derive a home city rather than asking for one. Among the companies the passenger has signals for,
we group by city, total each city's orders and completed rides, and take the strongest; ties break on
the lower city id. A candidate company sitting in that city earns a flat bonus. A passenger with no
signals has no derived home city, and no company receives the bonus.

### 3.4 The final score

```
score = 0.65·content + 0.35·popularity + (0.10 if the company is in the home city)
```

The 0.65 to 0.35 split favours the passenger's own history while leaving popularity enough weight to
matter, which is also what carries a brand new passenger: with no personal signals, `usage` and
`loyalty` are 0 and `ownRating` sits at the neutral 0.5, so the content part contributes a constant
0.15 to every company and the ranking is decided entirely by popularity. The list is ordered by score
descending, then by completed rides overall, then by company name, so equal scores still produce a
stable order.

**Source** — `PickMeUp.Services/Recommendations/RecommendationService.cs`,
`PickMeUp.Model/Constants/RecommenderWeights.cs`

## 4. Explaining a recommendation

Every result carries an `Explanation` string drawn from the same numbers that produced its score. We
pick the strongest true statement available, in this order:

| Situation | Explanation |
| --- | --- |
| Completed rides with the company and rated it | *"You completed 5 rides with them and rated them 4.7 out of 5."* |
| Completed rides but never rated it | *"You completed 2 rides with them."* |
| Ordered but never completed a ride | *"You have ordered from them 3 times before."* |
| Untried, in the home city, has ratings | *"Rated 4.5 out of 5 by other passengers in Sarajevo, where you ride most."* |
| Untried, in the home city, no ratings yet | *"A company in Sarajevo, where you ride most, that you have not tried yet."* |
| Passenger has no history at all | *"Popular with other passengers: 5 rides completed and rated 4.7 out of 5."* |
| Untried, outside the home city | *"New to you, but rated 4.7 out of 5 across 5 rides."* |
| Untried and never reviewed | *"New to you and still waiting for its first reviews."* |

Alongside the sentence, each result returns the figures behind it — `yourOrders`,
`yourCompletedRides`, `yourAverageRating`, `completedRidesOverall`, `averageRatingOverall`,
`reviewCountOverall`, `contentScore`, `popularityScore` and `isInHomeCity` — so the ranking can be
audited without reading the code. Ratings inside the sentences are rounded to one decimal for
readability, while scoring uses the unrounded average.

## 5. A worked example

The following trace comes from a running instance. `passenger3` begins with no history, so the list
is ranked on popularity alone and `Bascarsija Taxi` sits **last** at `0.2200`. The passenger then
takes three rides with `Bascarsija Taxi` through the normal ordering flow and rates each one 5 stars.
Nothing else on the platform changes.

**Before**

```
1. Mostar Taxi        0.4329  Popular with other passengers: 5 rides completed and rated 4.7 out of 5.
2. Sarajevo Express   0.3906  Popular with other passengers: 4 rides completed and rated 4.5 out of 5.
3. Bascarsija Taxi    0.2200  Popular with other passengers: 1 ride completed and rated 3 out of 5.
4. Neretva Prevoz     0.2112  Popular with other passengers: 2 rides completed and rated 2 out of 5.
```

**After**

```
1. Bascarsija Taxi    0.8611  You completed 3 rides with them and rated them 5 out of 5.
2. Sarajevo Express   0.4906  Rated 4.5 out of 5 by other passengers in Sarajevo, where you ride most.
3. Mostar Taxi        0.4329  New to you, but rated 4.7 out of 5 across 5 rides.
4. Neretva Prevoz     0.2112  New to you, but rated 2 out of 5 across 2 rides.
```

Those three rides wrote nine rows — three each of `Ordered`, `Completed` and `Reviewed` — and they
alone account for the movement. Recomputing the winning score by hand:

```
usage      = min(3/5, 1)                            = 0.6
loyalty    = min(3/5, 1)                            = 0.6
ownRating  = (5 − 1) / 4                            = 1.0
content    = 0.50·0.6 + 0.20·0.6 + 0.30·1.0         = 0.7200

ridesShare = 4 completed / 5 (Mostar Taxi, busiest) = 0.8
rating     = (4.5 − 1) / 4                          = 0.875
popularity = 0.50·0.8 + 0.50·0.875                  = 0.8375

score      = 0.65·0.7200 + 0.35·0.8375 + 0.10       = 0.8611
```

which is what the endpoint returns. Three details in the trace are worth drawing out. The company's
own average rose from 3 to 4.5 because the passenger's three 5 star reviews joined the single
existing 3 star one, so the passenger's ratings move both halves of the score. `Sarajevo Express`
climbed without gaining a single signal of its own: the new history gave the passenger a derived home
city of Sarajevo, and the 0.10 bonus is the whole of its `0.3906 → 0.4906` gain. And `Mostar Taxi`
did not move at all, since it gained no signals and is not in the home city.

## 6. Where it lives and how it runs

```
GET /Recommendations?Page=1&PageSize=5&IncludeTotalCount=true
```

The endpoint is restricted to the passenger role and takes the passenger from the JWT. There is no
`userId` parameter, so nobody can request another passenger's recommendations. Results come back as
our standard `PageResult<T>`, five per page by default and capped at 100.

Inside `RecommendationService` the work happens in two queries. The first aggregates the popularity
figures with a `GROUP BY` over `InteractionHistory` and left joins them onto the active companies, so
a company with no signals yet still appears with zeroes instead of dropping out of the list. The
second aggregates the same table for the signed-in passenger alone. Both aggregations run in
PostgreSQL rather than in application memory, and neither loads individual signal rows; only the
per-company totals come back. We then apply the weights from section 3 to those totals, sort, and
page the result.

**Source** — `PickMeUp.WebAPI/Controllers/RecommendationsController.cs`,
`PickMeUp.Services/Recommendations/RecommendationService.cs`

## 7. The screen

Passengers reach the recommender from the mobile drawer, under **Recommended for you**.

The screen opens with the heading *"Picked for you"* and a line of orientation: *"Ranked from the
rides you have taken and how other passengers rate each company. Every card says why it is here."*
Below it the ranked companies appear as cards, strongest first, and the list can be pulled down to
refresh. When there is nothing to show, the screen says *"No companies to recommend yet."* rather
than presenting an empty page.

Each card carries a numbered rank badge, the company logo, the company name with its city beneath,
and a row of stars showing the average rating and the number of reviews behind it. The rank badge is
the visible form of the score: passengers see the position, never the number that produced it.

Beneath that sits the explanation panel, which is the part that makes a recommendation answerable.
The client distinguishes the two kinds of reasoning by checking whether the passenger has any orders
or completed rides with that company, and styles the panel accordingly:

* **Why you see this** — cyan, with a history icon, when the recommendation rests on the passenger's
  own record: *"You completed 5 rides with them and rated them 4.7 out of 5."*
* **Why this is here** — neutral, with a group icon, when the passenger has no record with that
  company and the reason falls back to other passengers or to the home city.

A passenger with no history sees the same screen with every card headed **Why this is here**, because
the content-based part has nothing to work with and popularity carries the ranking on its own. The
screen therefore never looks broken for a new account; it degrades into a plain popularity list that
still explains itself.

> Captured screenshots belong at `screenshots/recommender-passenger1.png` for a passenger with ride
> history, showing both panel styles, and `screenshots/recommender-coldstart.png` for a passenger
> with none, showing the popularity-only ranking.

**Source** — `PickMeUp/UI/pickmeup_mobile/lib/screens/passenger/recommended_screen.dart`
