# PickMeUp — Recommender documentation

This document describes the recommender that powers **FR-X3** and **feature 4.11 (Recommended
companies)**: which signals it stores, how they are turned into a score, and where every part of it
lives in the source tree.

The rule this implementation is held to is **NFR-18** — the scoring signals must be *persisted and
actually used*, not merely collected. Section 4 walks a single recommendation from the stored rows
to the number the API returns, so that claim can be checked rather than taken on trust.

---

## 1. What is stored

Every signal is a row in the **`InteractionHistory`** table. One row is one thing a passenger did
with one taxi company.

| Column | Meaning |
| --- | --- |
| `UserId` | the passenger the signal belongs to (FK → `Users`) |
| `TaxiCompanyId` | the company the signal is about (FK → `TaxiCompanies`) |
| `SignalTypeId` | `1 = Ordered`, `2 = Completed`, `3 = Reviewed` |
| `Value` | `1` for Ordered and Completed; the **star rating (1–5)** for Reviewed |
| `OrderId` | the ride the signal came from (FK → `Orders`, nullable) |
| `CreatedAt` | when the signal was recorded (UTC) |

Three signal types are captured. A *viewed* signal was deliberately **not** added: the API has no
company-view write path, and writing to the database on an HTTP `GET` is an anti-pattern.

**Source**

- Entity — `PickMeUp/PickMeUp.Services/Database/InteractionHistory.cs`
- Signal type enum — `PickMeUp/PickMeUp.Model/Enums/InteractionSignalType.cs`
- Table configuration, FKs, indexes, soft-delete filter — `PickMeUp/PickMeUp.Services/Database/PickMeUpConfiguration.cs` (`CreateConfiguration`, the `Entity<InteractionHistory>()` block)
- Migration — `PickMeUp/PickMeUp.Services/Migrations/20260805175432_AddInteractionHistory.cs`
- `DbSet` — `PickMeUp/PickMeUp.Services/Database/PickMeUpDbContext.cs` (`InteractionHistories`)

## 2. When signals are written

Signals are written by the application **as passengers use it** — there is no batch job and nothing
is reconstructed after the fact. Each write happens *after* the surrounding database transaction has
committed, and a failure to write a signal is logged but never rolls the action back: a ride must not
fail because a recommender signal could not be stored.

| Signal | Written at | Source |
| --- | --- | --- |
| `Ordered` | a passenger places an order for a known company | `PickMeUp/PickMeUp.Services/OrderService.cs` → `CreateAsync` (line 51, hook at line 97) |
| `Ordered` | a *general* order (no company chosen) is accepted, which is the moment a company is assigned to it | `PickMeUp/PickMeUp.Services/OrderService.cs` → `AcceptAsync` (line 106, hook at line 141) |
| `Completed` | the driver completes the ride | `PickMeUp/PickMeUp.Services/OrderService.cs` → `CompleteAsync` (line 209, hook at line 239) |
| `Reviewed` | the passenger reviews the company, `Value` = the rating given | `PickMeUp/PickMeUp.Services/ReviewService.cs` → `InsertAsync` (line 36, hook at line 93) |

A general order records its `Ordered` signal at acceptance rather than at creation, because until a
driver accepts it there is no company for the signal to point at.

All four hooks go through one small writer so the behaviour is defined in a single place:

- `PickMeUp/PickMeUp.Services/Recommendations/IInteractionRecorder.cs`
- `PickMeUp/PickMeUp.Services/Recommendations/InteractionRecorder.cs`
- registered in `PickMeUp/PickMeUp.WebAPI/Program.cs`

## 3. How the score is computed

The recommender is a **hybrid**: a content-based part built from what *this* passenger has done, and
a popularity part built from what *all* passengers have done, plus a small bonus for the city the
passenger actually rides in.

Both parts are aggregated **in the database** with `GROUP BY` over `InteractionHistory` — the whole
ranking costs **two SQL statements** regardless of how many companies or signals exist.

**Source** — `PickMeUp/PickMeUp.Services/Recommendations/RecommendationService.cs`
(weights in `PickMeUp/PickMeUp.Model/Constants/RecommenderWeights.cs`).

### 3.1 Content-based part

For the signed-in passenger P and a company C, from P's own rows:

| Component | Formula | Weight |
| --- | --- | --- |
| Usage | `min(completed rides with C / 5, 1)` | 0.50 |
| Ordering | `min(orders placed with C / 5, 1)` | 0.20 |
| Own rating | `(P's average rating of C − 1) / 4`, or `0.5` when P has never rated C | 0.30 |

```
content = 0.50 · usage + 0.20 · ordering + 0.30 · ownRating        → 0 … 1
```

Five rides is the saturation point: past that, more rides no longer raise the usage term, so a long
history cannot drown out a better-rated alternative.

### 3.2 Popularity part

For company C, across **all** passengers:

| Component | Formula | Weight |
| --- | --- | --- |
| Ride share | `C's completed rides / the highest completed-ride count among all companies` | 0.50 |
| Overall rating | `(C's average rating − 1) / 4`, or `0.5` when C has no reviews | 0.50 |

```
popularity = 0.50 · rideShare + 0.50 · overallRating              → 0 … 1
```

### 3.3 City affinity

The passenger's home city is **derived**, not stored: group the passenger's own signals by the city
of the company each signal points at, and take the city with the most signals. Companies in that
city get a flat bonus of **0.10**. A passenger with no signals has no derived city and no bonus.

This is the honest version of the feature's "best in your city" wording. PickMeUp stores no home
address or coordinates for a user, so proximity is not part of the score — the city a passenger
actually rides in is inferred from their rides instead.

### 3.4 The final score

```
score = 0.65 · content + 0.35 · popularity + (0.10 if the company is in the derived home city)
```

Results are sorted by score descending, then by overall completed rides, then by name. The response
is paginated (5 per page by default, capped at 100).

### 3.5 Cold start

A passenger with no signals gets a content score of `0.30 · 0.5 = 0.15` for *every* company — the
same constant everywhere, so it cannot change the order. The ranking therefore collapses to pure
popularity on its own, with no special-case branch, and the explanation says so: *"Popular with other
passengers: 5 rides completed and rated 4.7 out of 5."*

### 3.6 Explanations

Every recommendation carries a human-readable reason drawn from the same numbers that produced the
score, strongest reason first:

| Situation | Explanation |
| --- | --- |
| P completed rides with C and rated it | *"You completed 5 rides with them and rated them 4.7 out of 5."* |
| P completed rides but never rated | *"You completed 2 rides with them."* |
| P ordered but never completed | *"You have ordered from them 3 times before."* |
| Untried company in the derived home city | *"Rated 4.5 out of 5 by other passengers in Sarajevo, where you ride most."* |
| Passenger has no history at all | *"Popular with other passengers: 5 rides completed and rated 4.7 out of 5."* |
| Untried company outside the home city | *"New to you, but rated 4.7 out of 5 across 5 rides."* |

The response also returns the raw counts behind the score — `yourOrders`, `yourCompletedRides`,
`yourAverageRating`, `completedRidesOverall`, `averageRatingOverall`, `reviewCountOverall`,
`contentScore`, `popularityScore`, `isInHomeCity` — so a client (or an examiner) can see exactly what
drove the ranking.

**Source** — `PickMeUp/PickMeUp.Model/Responses/RecommendationResponse.cs`

## 4. Proof that the signals drive the score

Worked example, taken from a running instance. `passenger3` starts with no history and is offered the
companies in pure popularity order; `Bascarsija Taxi` is **last** at `0.2200`. The passenger then
takes three rides with `Bascarsija Taxi` through the normal ordering flow and rates each one 5 stars.
Nothing else changes.

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

The three rides wrote nine rows (3 × `Ordered`, `Completed`, `Reviewed`), and those rows alone
account for the change. Recomputing the top score by hand from them:

```
usage      = min(3/5, 1)            = 0.6
ordering   = min(3/5, 1)            = 0.6
ownRating  = (5 − 1) / 4            = 1.0
content    = 0.50·0.6 + 0.20·0.6 + 0.30·1.0        = 0.7200

rideShare  = 4 completed / 5 (the highest, Mostar Taxi)  = 0.8
rating     = (4.5 − 1) / 4                              = 0.875
popularity = 0.50·0.8 + 0.50·0.875                = 0.8375

score      = 0.65·0.7200 + 0.35·0.8375 + 0.10     = 0.8611
```

which is exactly what `GET /Recommendations` returns. Note that `Sarajevo Express` also moved up:
the new signals gave the passenger a derived home city of Sarajevo, so it now earns the 0.10 bonus.

## 5. The endpoint

```
GET /Recommendations?Page=1&PageSize=5&IncludeTotalCount=true
```

Returns the ranked companies for the **signed-in passenger**, newest scoring first, as the standard
`PageResult<T>`. The endpoint is restricted to the `Passenger` role and takes the passenger from the
JWT — there is no `userId` parameter, so one passenger cannot ask for another's recommendations.

**Source** — `PickMeUp/PickMeUp.WebAPI/Controllers/RecommendationsController.cs`

## 6. Where the data comes from — an honest note

Two things fill `InteractionHistory`, and both are real:

1. **The running application.** Ordering, completing and reviewing a ride each write a signal at the
   moment they happen, through the hooks listed in section 2. This is the mechanism that would
   accumulate signals in production.
2. **The seed.** A clean `docker compose up` pre-populates historical rides, reviews and their
   matching signal rows for three passengers with deliberately different habits — a Mostar regular, a
   Sarajevo regular, and a passenger with no history at all — so the recommender can be demonstrated
   on a fresh database without anyone first clicking through the API to manufacture a history. The
   seeded rows are exactly the rows live use would have produced: an order in the seed has its
   matching `Ordered` and `Completed` signals, and a seeded review has its `Reviewed` signal carrying
   the same rating.

There is no third mechanism. Nothing is recomputed, back-filled or synthesised at query time, and
every signal in the table — seeded or live — is read by the scoring query described in section 3.

**Source** — `PickMeUp/PickMeUp.Services/Database/PickMeUpSeed.cs`
(`SeedOrders`, `SeedReviews`, `SeedInteractionHistories`)

## 7. Screenshots from the running app

> **Pending.** The passenger-facing "Recommended for you" screen is built in the mobile phase of the
> project. Screenshots of the recommender as the passenger sees it will be added to this section once
> that screen exists. Nothing is shown here in the meantime rather than illustrating the feature with
> anything that is not a real screenshot of this application.
