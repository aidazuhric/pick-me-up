# PickMeUp — The recommender

## 1. Overview

PickMeUp suggests taxi companies to a passenger through a **Recommended for you** list: the active
companies on the platform, ranked best first, with the reason for each placement shown next to it.

Our recommender is a hybrid of two complementary approaches. A **content-based** part looks only at
the passenger's own record with each company: how often they have ordered from it, how many of those
rides they completed, and how they rated it afterwards. A **popularity** part looks at how every
passenger treats that company, through its completed rides and its average rating. Blending the two
lets us give a useful answer both to a regular with years of habits and to somebody who signed up a
minute ago, without switching to a different algorithm for each.

The list is drawn from one city. A passenger chooses their city when they register, and only active
companies operating in that city are candidates, because a company cannot serve a ride that starts
somewhere it does not work. Everything below therefore ranks companies inside the passenger's own
city; we never put a company from another city on the list.

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
   rating given, so the history records what was said and when.

Ratings are the one signal we do **not** score from this table. A passenger may edit or delete a
review afterwards, and a signal row written at the time cannot follow them, so scoring from it would
rank a company on a rating its passenger has since withdrawn. Both the passenger's own rating and the
company's overall rating are therefore read from the live `Review` rows, and the `Reviewed` signals
stay as the audit trail of what happened. Order and completion counts still come from the signals,
because those facts never change after the event.

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
ownRating  = (thisPassengersCurrentAverage − 1) / 4     (0.5 when they have not rated it)

content    = 0.50·usage + 0.20·loyalty + 0.30·ownRating
```

Completed rides carry more weight than orders because a finished ride is stronger evidence of a good
experience than a placed one, and the passenger's own rating carries more than raw ordering volume.
`thisPassengersCurrentAverage` is the average of the reviews this passenger has live right now, so an
edited or deleted review changes this part of the score immediately.

### 3.2 The popularity part

This part is identical for every passenger and reflects the company's standing across the platform:

```
ridesShare = completedRidesOverall / completedRidesOfTheBusiestCompany
rating     = (currentAverageOverall − 1) / 4            (0.5 when it has no reviews)

popularity = 0.50·ridesShare + 0.50·rating
```

Completed rides are measured against the busiest company rather than in absolute terms, which keeps
the component meaningful whether the platform has ten rides or ten thousand. The rides share is
counted over the whole platform, not only the passenger's city, so the denominator does not move when
different passengers ask. `currentAverageOverall` is likewise the average of the reviews the company
holds right now.

### 3.3 City affinity

The term exists in the formula and is worth explaining honestly, because in the shipped
implementation it cannot change an ordering.

We derive a home city rather than asking for one: among the candidate companies the passenger has
signals for, we group by city, total each city's orders and completed rides and take the strongest,
with ties broken on the lower city id. A candidate sitting in that city earns a flat bonus.

Because the candidates were already narrowed to the passenger's registered city in section 1, every
candidate shares one city. The derivation therefore has only two outcomes: it names that same city,
and the bonus is added to every company equally, or the passenger has no signals with any candidate
and no company receives it. Either way the bonus shifts all scores together and the ranking is
unchanged. It is a constant, not a tie-breaker, and we do not claim otherwise.

The term is kept rather than deleted because `isInHomeCity` is part of the figures each result
returns and because it is what would carry the weight if candidates were ever drawn from more than
one city. Any such change is a change to the application's scope, not to this document.

### 3.4 The final score

```
score = 0.65·content + 0.35·popularity + (0.10 if the company is in the home city)
```

The 0.65 to 0.35 split favours the passenger's own history while leaving popularity enough weight to
matter, which is also what carries a brand new passenger: with no personal signals, `usage` and
`loyalty` are 0 and `ownRating` sits at the neutral 0.5, so the content part contributes a constant
0.15 to every company and the ranking is decided entirely by popularity. The 0.10 city term, as
section 3.3 explains, lands on every candidate or on none, so it raises or lowers the whole column
without reordering it. The list is ordered by score descending, then by completed rides overall, then
by company name, so equal scores still produce a stable order.

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
| Passenger has no history at all | *"Popular with other passengers: 5 rides completed and rated 4.7 out of 5."* |
| Untried, has a derived home city, company rated | *"Rated 4.5 out of 5 by other passengers in Mostar, where you ride most."* |
| Untried, has a derived home city, no ratings yet | *"A company in Mostar, where you ride most, that you have not tried yet."* |
| Untried, no derived home city, company rated | *"New to you, but rated 4.7 out of 5 across 5 rides."* |
| Untried and never reviewed | *"New to you and still waiting for its first reviews."* |

The last two rows read as the "outside the home city" cases but are not: a candidate is always in the
passenger's city, so what separates them is whether a home city could be derived at all — that is,
whether the passenger has any signals with any candidate. A sentence placing a company outside the
passenger's city is not reachable and none is listed.

Alongside the sentence, each result returns the figures behind it — `yourOrders`,
`yourCompletedRides`, `yourAverageRating`, `completedRidesOverall`, `averageRatingOverall`,
`reviewCountOverall`, `contentScore`, `popularityScore` and `isInHomeCity` — so the ranking can be
audited without reading the code. Ratings inside the sentences are rounded to one decimal for
readability, while scoring uses the unrounded average.

## 5. A worked example

The following trace comes from a running instance on a freshly seeded database. `passenger3` is
registered in Mostar, so the list holds the two active Mostar companies and nothing else; the two
Sarajevo companies are not candidates for this passenger and never appear.

**Before.** The passenger has ordered once from each company and completed neither, and has rated
nothing.

```
1. Mostar Taxi      0.5589  You have ordered from them once before.
2. Neretva Prevoz   0.3373  You have ordered from them once before.
```

The passenger then takes one ride with `Neretva Prevoz` through the normal flow — order, card
payment, driver accepts, pick-up, completion — and rates it 5 stars. Nothing else on the platform
changes.

**After the ride and the 5 star review**

```
1. Neretva Prevoz   0.6264  You completed 1 ride with them and rated them 5 out of 5.
2. Mostar Taxi      0.5589  You have ordered from them once before.
```

Recomputing the new leader by hand. The passenger now has two orders and one completed ride with
them, and their 5 joins the single existing 2 star review, so the company average becomes 3.5:

```
usage      = min(1/5, 1)                            = 0.2
loyalty    = min(2/5, 1)                            = 0.4
ownRating  = (5 − 1) / 4                            = 1.0
content    = 0.50·0.2 + 0.20·0.4 + 0.30·1.0         = 0.4800

ridesShare = 3 completed / 5 (Mostar Taxi, busiest) = 0.6
rating     = (3.5 − 1) / 4                          = 0.625
popularity = 0.50·0.6 + 0.50·0.625                  = 0.6125

score      = 0.65·0.4800 + 0.35·0.6125 + 0.10       = 0.6264
```

which is what the endpoint returns.

**After editing the same review from 5 stars to 1**

```
1. Mostar Taxi      0.5589  You have ordered from them once before.
2. Neretva Prevoz   0.3439  You completed 1 ride with them and rated them 1 out of 5.
```

The company falls back below `Mostar Taxi` and the sentence it carries changes with it. Both halves
of its score move, because the edit lowers the passenger's own rating to 1 and the company average
from 3.5 to 1.5:

```
content    = 0.50·0.2 + 0.20·0.4 + 0.30·0.0         = 0.1800
popularity = 0.50·0.6 + 0.50·(1.5 − 1)/4            = 0.3625
score      = 0.65·0.1800 + 0.35·0.3625 + 0.10       = 0.3439
```

This is the case that makes scoring from live reviews necessary. The `Reviewed` signal written when
the review was created still reads `5.00` in `InteractionHistory` after the edit, and scoring from
that row would have left the company ranked first on a rating its passenger had already withdrawn.

**After deleting the review**

```
1. Mostar Taxi      0.5589  You have ordered from them once before.
2. Neretva Prevoz   0.4633  You completed 1 ride with them.
```

The passenger's own rating disappears and the neutral 0.5 takes its place, the company average
returns to the 2 stars of the one remaining review, and the explanation drops its rating clause
rather than quoting a review that no longer exists.

Three details are worth drawing out. `Mostar Taxi` scores `0.5589` in all four states: it gained
nothing throughout, and the 0.10 city term it receives is also received by `Neretva Prevoz`, so the
term never separates them — it is the constant described in section 3.3. The ranking swings entirely
on the one review being added, changed and removed. And the rides share is measured against
`Mostar Taxi` at 5 completed rides, which is the busiest company on the whole platform rather than
the busiest of the two candidates.

## 6. Where it lives and how it runs

```
GET /Recommendations?Page=1&PageSize=5&IncludeTotalCount=true
```

The endpoint is restricted to the passenger role and takes the passenger from the JWT. There is no
`userId` parameter, so nobody can request another passenger's recommendations. Results come back as
our standard `PageResult<T>`, five per page by default and capped at 100.

Inside `RecommendationService` the work happens in four aggregations. Two run over
`InteractionHistory` with a `GROUP BY`: one for the platform-wide order and completion counts, one
for the signed-in passenger's own. Two more run over `Review`: one for each company's current average
across all passengers, one for the ratings this passenger holds right now. The company-wide pair is
left joined onto the active companies of the passenger's city, so a company with no signals and no
reviews still appears with zeroes instead of dropping out of the list.

All four aggregations run in PostgreSQL rather than in application memory, and none of them loads
individual signal or review rows; only the per-company totals come back. We then apply the weights
from section 3 to those totals, sort, and page the result.

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
