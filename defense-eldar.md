# PickMeUp — Defense Dossier: Eldar Šoše

**Responsibility split.** Per `CLAUDE.md`, Eldar defends the **Admin desktop** app (taxi-company CRUD, user management, support tickets from the admin side, reference data, PDF reports, admin settings/2FA), the **Passenger mobile** app (login with two-factor, ordering a taxi with map pick + quote + pay-first, the Stripe payment sheet, browsing companies and recommendations, live ride status, ride and payment history, reviews, online support, notifications and profile), and the **shared backend he personally defends**: the payment pipeline (`PaymentService` + Stripe webhook), reviews (`ReviewService`), the recommender (`RecommendationService`), and his half of the order/fare/quote and auth flow. Work owned by Aida — Company desktop, Driver mobile, the driver heads-up banner, the dispatch/shift rules on the driver side — is flagged as hers where it touches these flows rather than claimed here.

All excerpts below are verbatim from the repository at the stated paths and line numbers.

## Table of contents

**Backend Eldar defends**
1. [Fare is computed by the server, never sent by the client](#1-fare-is-computed-by-the-server-never-sent-by-the-client)
2. [Fare quote before ordering](#2-fare-quote-before-ordering)
3. [Order creation fixes the price](#3-order-creation-fixes-the-price)
4. [Pay-before-dispatch](#4-pay-before-dispatch)
5. [Stripe webhook — signature, idempotency, dispatch release](#5-stripe-webhook--signature-idempotency-dispatch-release)
6. [Cancel a paid, untaken ride → automatic refund](#6-cancel-a-paid-untaken-ride--automatic-refund)
7. [Reviews — only for your own completed ride, once](#7-reviews--only-for-your-own-completed-ride-once)
8. [Recommender — persisted signals, explainable score](#8-recommender--persisted-signals-explainable-score)
9. [Two-factor authentication — three modes](#9-two-factor-authentication--three-modes)

**Admin desktop**
10. [Creating a taxi company provisions its login](#10-creating-a-taxi-company-provisions-its-login)
11. [Deleting a company revokes every login it owns](#11-deleting-a-company-revokes-every-login-it-owns)
12. [User management — uniqueness rules](#12-user-management--uniqueness-rules)
13. [PDF reports (admin-only)](#13-pdf-reports-admin-only)
14. [Support tickets — admin resolution](#14-support-tickets--admin-resolution)
15. [Reference data with caching](#15-reference-data-with-caching)

**Passenger mobile**
16. [Sign-up asks for a home city](#16-sign-up-asks-for-a-home-city)
17. [Companies and recommendations are scoped to the home city](#17-companies-and-recommendations-are-scoped-to-the-home-city)
18. [Order screen — live quote then pay](#18-order-screen--live-quote-then-pay)
19. [Stripe payment sheet flow](#19-stripe-payment-sheet-flow)
20. [Live ride status over SignalR](#20-live-ride-status-over-signalr)

---

# Backend Eldar defends

## 1. Fare is computed by the server, never sent by the client

**What it does.** The price of a ride is worked out entirely on the server from the pickup→destination distance and a price list. Every ride is billed for at least one kilometre, so a very short hop still pays the starting price plus one kilometre.

**Why we built it this way.** NFR-9 forbids accepting an amount from a client. The minimum kilometre exists because a straight-line distance of a few metres would otherwise let someone book a real car for the starting price alone.

**How it works.** `Calculate` rounds the distance to 2 decimals, raises anything under 1 km to exactly 1 km, applies `start + km × perKm`, then guards that the result is never below the starting price.

**Code**

`PickMeUp.Services/Orders/FareCalculator.cs:1-26`
```csharp
namespace PickMeUp.Services.Orders
{
    public class FareCalculator : IFareCalculator
    {
        public const int DistanceDecimals = 2;

        public const decimal MinimumBilledKm = 1.0m;

        public decimal Calculate(decimal startingPrice, decimal pricePerKm, decimal? distanceKm)
        {
            var distance = distanceKm.HasValue ? RoundDistance(distanceKm.Value) : 0m;

            if (distance < MinimumBilledKm)
            {
                distance = MinimumBilledKm;
            }

            var fare = startingPrice + (distance * pricePerKm);

            return Math.Round(Math.Max(fare, startingPrice), 2, MidpointRounding.AwayFromZero);
        }

        public decimal RoundDistance(decimal distanceKm) =>
            Math.Round(distanceKm, DistanceDecimals, MidpointRounding.AwayFromZero);
    }
}
```

Which price list applies — a chosen company's own, or the **average of the active companies in the pickup city** for a general order:

`PickMeUp.Services/OrderService.cs:506-553`
```csharp
        private async Task<decimal> ComputeFareAsync(Order order)
        {
            var (startingPrice, pricePerKm) = order.TaxiCompanyId.HasValue
                ? await ReadCompanyRateAsync(order.TaxiCompanyId.Value)
                : await ReadCityAverageRateAsync(order.CityId);

            return _fareCalculator.Calculate(startingPrice, pricePerKm, order.DistanceKm);
        }

        private async Task<(decimal StartingPrice, decimal PricePerKm)> ReadCompanyRateAsync(int companyId)
        {
            var pricing = await _dbContext.TaxiCompanies
                .Where(t => t.Id == companyId)
                .Select(t => new { t.StartingPrice, t.PricePerKm })
                .FirstOrDefaultAsync()
                ?? throw new BusinessException("The company price list could not be found.");

            return (pricing.StartingPrice, pricing.PricePerKm);
        }

        private async Task<(decimal StartingPrice, decimal PricePerKm)> ReadCityAverageRateAsync(int? cityId)
        {
            if (cityId == null)
            {
                throw new BusinessException(
                    "We could not work out which city this pick-up is in, so we cannot price the ride.");
            }

            var rate = await _dbContext.TaxiCompanies
                .Where(t => t.CityId == cityId.Value && t.IsActive)
                .GroupBy(t => 1)
                .Select(g => new
                {
                    StartingPrice = g.Average(t => t.StartingPrice),
                    PricePerKm = g.Average(t => t.PricePerKm)
                })
                .FirstOrDefaultAsync();

            if (rate == null)
            {
                throw new BusinessException(
                    "No taxi company operates in this city yet, so the ride cannot be ordered.");
            }

            return (
                Math.Round(rate.StartingPrice, 2, MidpointRounding.AwayFromZero),
                Math.Round(rate.PricePerKm, 2, MidpointRounding.AwayFromZero));
        }
```

The request DTO a passenger posts — note there is **no amount or fare field**:

`PickMeUp.Model/Requests/OrderInsertRequest.cs:1-13`
```csharp
namespace PickMeUp.Model.Requests
{
    public class OrderInsertRequest
    {
        public int? TaxiCompanyId { get; set; }
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public double? DestinationLat { get; set; }
        public double? DestinationLng { get; set; }
        public string? DestinationAddress { get; set; }
    }
}
```

**Defense notes**
- The client physically cannot influence the price: `OrderInsertRequest` has no amount field, and the same DTO is used for both the quote and the create call.
- General-order pricing is the **unweighted arithmetic mean** of `StartingPrice` and of `PricePerKm` across active companies in the pickup city, each averaged independently and rounded to 2 dp — not weighted by fleet size or rating.
- If no active company operates in that city, the order is refused rather than priced at zero.

## 2. Fare quote before ordering

**What it does.** Before committing, the passenger's app asks the server what this exact trip will cost. The number returned is the number that will be charged.

**Why we built it this way.** Since the price is now taken up front, showing a client-side estimate risks disagreeing with the payment sheet. The quote runs the identical `ComputeFareAsync` path as order creation, so the two cannot drift.

**How it works.** `QuoteAsync` validates the same request, resolves the pickup city, builds a non-persisted `Order` probe, derives the distance and prices it.

**Code**

`PickMeUp.Services/OrderService.cs:119-153`
```csharp
        public async Task<OrderQuoteResponse> QuoteAsync(OrderInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            RequireUserId();

            var cityId = await ResolveNearestCityIdAsync(request.PickupLat, request.PickupLng);

            var probe = new Order
            {
                CityId = cityId,
                TaxiCompanyId = request.TaxiCompanyId,
                PickupLat = request.PickupLat,
                PickupLng = request.PickupLng,
                DestinationLat = request.DestinationLat,
                DestinationLng = request.DestinationLng
            };

            probe.DistanceKm = DeriveDistanceKm(probe);

            var companyName = request.TaxiCompanyId == null
                ? null
                : await _dbContext.TaxiCompanies
                    .Where(t => t.Id == request.TaxiCompanyId.Value)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync();

            return new OrderQuoteResponse
            {
                DistanceKm = probe.DistanceKm,
                Fare = await ComputeFareAsync(probe),
                IsGeneralOrder = request.TaxiCompanyId == null,
                CompanyName = companyName
            };
        }
```

Exposed at `PickMeUp.WebAPI/Controllers/OrdersController.cs:38-39` as `POST /Orders/Quote`, restricted to the Passenger role.

**Defense notes**
- The probe `Order` is never added to the `DbContext` — quoting has no side effects and writes nothing.
- Quote and create share one code path (`ComputeFareAsync`), which is why the quoted figure always equals the charged figure.

## 3. Order creation fixes the price

**What it does.** When the order is placed, the fare is computed once and stored on the row. It is never recomputed afterwards — completing the ride does not change the price.

**Why we built it this way.** The product decision is that a passenger knows and pays the price up front. Pricing at completion would mean nobody knew the cost until the ride was over.

**How it works.** `CreateAsync` derives the distance, computes the fare, saves the order as `Pending` inside a transaction, writes the first audit row, and records an `Ordered` recommender signal when a specific company was chosen.

**Code**

`PickMeUp.Services/OrderService.cs:90-117`
```csharp
                DestinationAddress = request.DestinationAddress,
                OrderStatusId = (int)OrderStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };

            order.DistanceKm = DeriveDistanceKm(order);
            order.Fare = await ComputeFareAsync(order);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            await RecordHistoryAsync(order.Id, null, OrderStatusEnum.Pending, userId, "Order placed");
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            if (order.TaxiCompanyId.HasValue)
            {
                await _interactions.RecordAsync(
                    userId, order.TaxiCompanyId.Value, InteractionSignalType.Ordered, 1m, order.Id);
            }

            await _dispatcher.OrderCreatedAsync(order.Id);

            return await LoadResponseAsync(order.Id);
        }
```

**Defense notes**
- `CompleteAsync` no longer contains any fare computation — grep for `ComputeFareAsync` and it appears only in `CreateAsync` and `QuoteAsync`.
- The order row and its first `OrderStatusHistory` audit row are written in one transaction, satisfying NFR-16/NFR-17 (every transition audited).

## 4. Pay-before-dispatch

**What it does.** A newly created order is `Pending` but invisible to drivers. It only becomes dispatchable once the payment is confirmed.

**Why we built it this way.** It removes the "driver drives out, passenger doesn't pay" failure, and it is what makes the refund-on-cancel rule meaningful.

**How it works.** `QueryAvailableFor` is the single query every driver-facing list goes through. It requires `IsPrepaid`, an unassigned order, a company and city match, no prior rejection by this driver, driver dispatch-eligibility, and that the driver is not already on a ride.

**Code**

`PickMeUp.Services/OrderService.cs:459-473`
```csharp
        private IQueryable<Order> QueryAvailableFor(IQueryable<Order> query, int driverId, int companyId, int? cityId)
        {
            var eligibleDriverIds = _shiftService.QueryDispatchEligibleDriverIds();

            return query.Where(o => o.OrderStatusId == (int)OrderStatusEnum.Pending
                && o.IsPrepaid
                && o.DriverId == null
                && (o.TaxiCompanyId == null || o.TaxiCompanyId == companyId)
                && (o.CityId == null || o.CityId == cityId)
                && !o.Rejections.Any(r => r.DriverId == driverId)
                && eligibleDriverIds.Contains(driverId)
                && !_dbContext.Orders.Any(active => active.DriverId == driverId
                    && (active.OrderStatusId == (int)OrderStatusEnum.Confirmed
                        || active.OrderStatusId == (int)OrderStatusEnum.InProgress)));
        }
```

Payment can only be started for an order that is still waiting for a driver:

`PickMeUp.Services/PaymentService.cs:72-78`
```csharp
            if (order.OrderStatusId != (int)OrderStatusEnum.Pending)
            {
                throw new BusinessException("Only a ride that is still waiting for a driver can be paid for.");
            }

            var amount = order.Fare
                ?? throw new BusinessException("This ride has no fare yet, so it cannot be paid for.");
```

**Defense notes**
- `amount` is read from `order.Fare` — the stored, server-computed value. `PaymentIntentRequest` carries only `OrderId` (`PickMeUp.Model/Requests/PaymentIntentRequest.cs:1-7`), so no amount can be injected.
- The driver-side eligibility rules (`QueryDispatchEligibleDriverIds`, shift/break state) are **Aida's** area; this query consumes them.
- An unpaid order is not merely hidden in the UI — it fails the server query, so no client can request it.

## 5. Stripe webhook — signature, idempotency, dispatch release

**What it does.** A payment is marked Paid only when Stripe's webhook says so, with a verified signature. The same webhook flips the order to prepaid and releases it to drivers.

**Why we built it this way.** NFR-5/NFR-6/NFR-10: paid only after webhook confirmation, confirm must be idempotent, and an existing open/completed payment must be checked before starting a new one. Trusting the client's "payment succeeded" would be forgeable.

**How it works.** The gateway verifies the Stripe signature first; the service ignores non-success events, ignores replays of an already-paid payment, then marks payment and order in one transaction and pushes availability.

**Code** — signature verification happens before anything is trusted:

`PickMeUp.Services/Payments/StripeGateway.cs:95-124`
```csharp
        public StripeWebhookEvent ReadWebhook(string payload, string signatureHeader)
        {
            if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
            {
                throw new BusinessException("Stripe webhook secret is not configured.");
            }

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signatureHeader,
                    _options.WebhookSecret,
                    throwOnApiVersionMismatch: false);
            }
            catch (StripeException exception)
            {
                throw new BusinessException($"The webhook signature could not be verified: {exception.Message}");
            }
            catch (Exception exception)
            {
                throw new BusinessException($"The webhook payload could not be read: {exception.Message}");
            }

            var intentId = stripeEvent.Data.Object is PaymentIntent intent ? intent.Id : null;

            return new StripeWebhookEvent(stripeEvent.Type, intentId);
        }
```

`PickMeUp.Services/PaymentService.cs:151-205`
```csharp
        public async Task<bool> HandleWebhookAsync(string payload, string signatureHeader)
        {
            var stripeEvent = _stripeGateway.ReadWebhook(payload, signatureHeader);

            if (stripeEvent.Type != PaymentSucceededEvent || string.IsNullOrWhiteSpace(stripeEvent.PaymentIntentId))
            {
                _logger.LogInformation("Ignoring Stripe event {Type}", stripeEvent.Type);
                return false;
            }

            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripeEvent.PaymentIntentId);

            if (payment == null)
            {
                _logger.LogWarning("No payment matches Stripe intent {IntentId}", stripeEvent.PaymentIntentId);
                return false;
            }

            if (payment.IsPaid)
            {
                _logger.LogInformation("Payment {PaymentId} is already paid; webhook replay had no effect.", payment.Id);
                return false;
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var paidAt = DateTime.UtcNow;

            payment.IsPaid = true;
            payment.PaymentStatusId = (int)PaymentStatusEnum.Paid;
            payment.PaidAt = paidAt;
            payment.UpdatedAt = paidAt;

            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order != null && !order.IsPrepaid)
            {
                order.IsPrepaid = true;
                order.PrepaidAt = paidAt;
                order.UpdatedAt = paidAt;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _dispatcher.PaymentConfirmedAsync(payment.Id);

            if (order != null && order.OrderStatusId == (int)OrderStatusEnum.Pending)
            {
                await _orderDispatcher.OrderBecameAvailableAsync(order.Id, excludeDriverId: null);
            }

            return true;
        }
```

**Defense notes**
- Idempotency is the `if (payment.IsPaid) return false` guard — a replayed webhook changes nothing and logs that it had no effect.
- `IsPaid = true` appears exactly once in runtime code (this method). Everywhere else it is a seed value or a read.
- Payment and order are flipped inside one transaction, so an order can never be prepaid without its payment being paid.

## 6. Cancel a paid, untaken ride → automatic refund

**What it does.** A passenger can cancel a ride they have paid for but which no driver has taken. The money is refunded through Stripe and the refund is recorded.

**Why we built it this way.** Pay-before-dispatch is only fair if the passenger can get the money back when nothing happens. It replaces a background expiry job with an explicit user action.

**How it works.** `CancelAsync` runs the centralized state machine, writes the audit row, commits, and then — only if the order was prepaid and not already completed — asks `PaymentService` to refund the remaining amount.

**Code**

`PickMeUp.Services/OrderService.cs:316-334`
```csharp
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            order.CancelledAt = DateTime.UtcNow;
            order.CancellationReason = request.Reason;
            ApplyStatus(order, target);

            await RecordHistoryAsync(order.Id, from, target, userId, request.Reason);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            if (order.IsPrepaid && from != OrderStatusEnum.Completed)
            {
                await _payments.RefundCancelledOrderAsync(order.Id, $"Ride cancelled: {request.Reason}");
            }

            await _dispatcher.OrderCancelledAsync(order.Id, request.Reason, userId);

            return await LoadResponseAsync(order.Id);
        }
```

`PickMeUp.Services/PaymentService.cs:269-299`
```csharp
        public async Task<bool> RefundCancelledOrderAsync(int orderId, string reason)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.IsPaid);

            if (payment == null)
            {
                return false;
            }

            var remaining = payment.AmountCharged - payment.Refunds.Sum(r => r.RefundedAmount);

            if (remaining <= 0m)
            {
                return false;
            }

            var stripeRefund = await _stripeGateway.RefundAsync(
                payment.StripePaymentIntentId ?? string.Empty, remaining, reason);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.PaymentRefunds.Add(new PaymentRefund
            {
                PaymentId = payment.Id,
                RefundedAmount = stripeRefund.Amount,
                StripeRefundId = stripeRefund.RefundId,
                Reason = reason,
                RefundedAt = DateTime.UtcNow
            });
```

**Defense notes**
- The refund amount is `AmountCharged` minus refunds already issued, so a double cancel cannot refund twice — the second call returns `false` at `remaining <= 0m`.
- `EnsureCancellationReason` is called first (`PickMeUp.Services/OrderService.cs:306`), satisfying the FR-X1 rule that cancellation requires a reason.
- The refund is deliberately outside the order transaction: a Stripe call must not hold a database transaction open.

## 7. Reviews — only for your own completed ride, once

**What it does.** A passenger can rate the company and/or the driver of a ride they took, once the ride is completed, and only once per target.

**Why we built it this way.** Ratings feed the recommender and the Top-5 report, so they must be anchored to a real completed ride by the person who took it.

**How it works.** Ownership, completion and target existence are checked, then a unique index catches a concurrent duplicate; a successful company review writes a `Reviewed` recommender signal carrying the rating value.

**Code**

`PickMeUp.Services/ReviewService.cs:36-98`
```csharp
        public override async Task<ReviewResponse> InsertAsync(ReviewInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var userId = RequireUserId();

            var order = await _dbContext.Orders
                .Include(o => o.Driver)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId)
                ?? throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");

            if (order.PassengerId != userId)
            {
                throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");
            }

            if (order.OrderStatusId != (int)OrderStatusEnum.Completed)
            {
                throw new BusinessException("You can review a ride once it has been completed.");
            }

            if (request.ReviewCompany && order.TaxiCompanyId == null)
            {
                throw new BusinessException("This ride has no company to review.");
            }

            if (request.ReviewDriver && order.DriverId == null)
            {
                throw new BusinessException("This ride has no driver to review.");
            }

            await EnsureNotAlreadyReviewedAsync(order.Id, request);

            var review = new Review
            {
                OrderId = order.Id,
                PassengerId = userId,
                TargetCompanyId = request.ReviewCompany ? order.TaxiCompanyId : null,
                TargetDriverId = request.ReviewDriver ? order.DriverId : null,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Reviews.Add(review);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation())
            {
                throw new BusinessException("You have already reviewed this ride.");
            }

            if (review.TargetCompanyId.HasValue)
            {
                await _interactions.RecordAsync(
                    userId, review.TargetCompanyId.Value, InteractionSignalType.Reviewed, review.Rating, order.Id);
            }

            return await LoadResponseAsync(review.Id);
        }
```

**Defense notes**
- A ride belonging to someone else throws `KeyNotFoundException`, not a 403 — it does not leak that the order exists.
- The application check and the database unique index are both present: the check gives a readable message, the index closes the race between two concurrent submits.

## 8. Recommender — persisted signals, explainable score

**What it does.** Recommends taxi companies to the signed-in passenger, blending what this passenger has done before with how popular the company is overall, plus a bonus for their home city. Each result carries a human-readable explanation.

**Why we built it this way.** NFR-18 and feature 4.11 require that the signals are persisted, demonstrably drive the score, and that each recommendation explains itself. Signals are written by the real flows — `Ordered` at order creation, `Completed` at ride completion, `Reviewed` at review — into `InteractionHistories`, so nothing is synthesized at read time.

**How it works.** One query aggregates global popularity, a second reads this passenger's own signals; candidates are restricted to the passenger's registered city; each candidate is scored and sorted in memory.

**Code** — the two aggregate queries (the whole call is **2 SQL round-trips**):

`PickMeUp.Services/Recommendations/RecommendationService.cs:23-54`
```csharp
        public async Task<PageResult<RecommendationResponse>> GetForCurrentPassengerAsync(
            RecommendationSearch? search = null)
        {
            var userId = _userAccessor.GetUserId()
                ?? throw new BusinessException("The caller could not be identified.");

            var ordered = (int)InteractionSignalType.Ordered;
            var completed = (int)InteractionSignalType.Completed;
            var reviewed = (int)InteractionSignalType.Reviewed;

            var popularity = _dbContext.InteractionHistories
                .AsNoTracking()
                .GroupBy(i => i.TaxiCompanyId)
                .Select(g => new
                {
                    TaxiCompanyId = g.Key,
                    CompletedRides = g.Sum(i => i.SignalTypeId == completed ? 1 : 0),
                    ReviewCount = g.Sum(i => i.SignalTypeId == reviewed ? 1 : 0),
                    RatingTotal = g.Sum(i => i.SignalTypeId == reviewed ? i.Value : 0m)
                });

            var registeredCityId = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.CityId)
                .FirstOrDefaultAsync();

            var cityId = registeredCityId ?? search?.CityId;

            var candidates = await (from company in _dbContext.TaxiCompanies.AsNoTracking()
                                    where company.IsActive
                                       && (cityId == null || company.CityId == cityId)
```

The scoring formula:

`PickMeUp.Services/Recommendations/RecommendationService.cs:136-168`
```csharp
        private static RecommendationResponse Score(
            CompanyPopularity candidate,
            PersonalSignals? mine,
            int maxCompletedRides,
            int? homeCityId,
            bool hasHistory)
        {
            var ownRating = Average(mine?.RatingTotal, mine?.ReviewCount ?? 0);
            var overallRating = Average(candidate.RatingTotal, candidate.ReviewCount);

            var usage = Saturated(mine?.CompletedRides ?? 0);
            var loyalty = Saturated(mine?.Orders ?? 0);
            var personalRating = Normalized(ownRating);

            var contentScore =
                RecommenderWeights.ContentCompletedRides * usage
                + RecommenderWeights.ContentOrders * loyalty
                + RecommenderWeights.ContentOwnRating * personalRating;

            var ridesShare = maxCompletedRides == 0
                ? 0d
                : (double)candidate.CompletedRides / maxCompletedRides;

            var popularityScore =
                RecommenderWeights.PopularityCompletedRides * ridesShare
                + RecommenderWeights.PopularityRating * Normalized(overallRating);

            var isInHomeCity = homeCityId != null && candidate.CityId == homeCityId.Value;

            var score =
                RecommenderWeights.Content * contentScore
                + RecommenderWeights.Popularity * popularityScore
                + (isInHomeCity ? RecommenderWeights.HomeCityBonus : 0d);
```

The weights, in one place:

`PickMeUp.Model/Constants/RecommenderWeights.cs:1-20`
```csharp
namespace PickMeUp.Model.Constants
{
    public static class RecommenderWeights
    {
        public const double Content = 0.65;
        public const double Popularity = 0.35;

        public const double ContentCompletedRides = 0.50;
        public const double ContentOrders = 0.20;
        public const double ContentOwnRating = 0.30;

        public const double PopularityCompletedRides = 0.50;
        public const double PopularityRating = 0.50;

        public const double HomeCityBonus = 0.10;

        public const int UsageSaturation = 5;
        public const double NeutralRating = 0.5;
    }
}
```

**Defense notes**
- Exactly **two SQL queries** per `/Recommendations` call — the aggregates are composed in `IQueryable` and executed once each; there is no N+1.
- A passenger with no history is not a special case: their content score is zero and the ranking falls back to popularity.
- The recommendation is always for the signed-in passenger; there is no parameter to request someone else's (`GetForCurrentPassengerAsync` reads the id from the token).

## 9. Two-factor authentication — three modes

**What it does.** An account can require an emailed code on every login, only until its email is confirmed once, or never.

**Why we built it this way.** NFR-8 requires email two-factor. A single on/off flag could not express "prove the address once, then stop asking", which is what we wanted for the demo accounts. Folding the flag into one enum removes the possibility of contradictory state.

**How it works.** `LoginAsync` verifies the password, then asks `ChallengeIsRequired`. If a challenge is needed it returns a token-less response carrying only a masked email and an expiry. `VerifyAsync` consumes the code and, for `ConfirmOnce`, stamps the confirmation.

**Code**

`PickMeUp.WebAPI/Services/AccessManager/AccessManager.cs:37-75`
```csharp
        public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request)
        {
            var user = await _userService.GetByUsernameAsync(request.Username);

            if (user == null
                || !_cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password)
                || !CanSignIn(user))
            {
                throw new BusinessException("Invalid username or password, or the account is not active.");
            }

            if (ChallengeIsRequired(user))
            {
                var challenge = await _twoFactorService.IssueChallengeAsync(user.Id);

                return new UserLoginResponse
                {
                    RequiresTwoFactor = true,
                    MaskedEmail = challenge.MaskedEmail,
                    CodeExpiresInSeconds = challenge.ExpiresInSeconds
                };
            }

            return await IssueSessionAsync(user);
        }

        public async Task<UserLoginResponse> VerifyTwoFactorAsync(TwoFactorVerifyRequest request)
        {
            var user = await _userService.GetByUsernameAsync(request.Username);

            if (user == null || !ChallengeIsRequired(user) || !CanSignIn(user))
            {
                throw new BusinessException("That code has expired. Sign in again to get a new one.");
            }

            await _twoFactorService.VerifyAsync(user.Id, request.Code);

            return await IssueSessionAsync(user);
        }
```

`PickMeUp.WebAPI/Services/AccessManager/AccessManager.cs:125-130`
```csharp
        private static bool ChallengeIsRequired(UserResponse user) => user.TwoFactorMode switch
        {
            TwoFactorModeEnum.EveryLogin => true,
            TwoFactorModeEnum.ConfirmOnce => user.TwoFactorConfirmedAt == null,
            _ => false
        };
```

The code is stored hashed and salted, never in plain text:

`PickMeUp.Services/TwoFactorService.cs:56-79`
```csharp
            await InvalidateLiveCodesAsync(userId, now, cancellationToken);

            var code = GenerateCode();
            var salt = _cryptoService.GenerateSlat();
            var expiresAt = now.AddMinutes(_options.ExpiryMinutes);

            _dbContext.TwoFactorCodes.Add(new TwoFactorCode
            {
                UserId = userId,
                CodeHash = _cryptoService.GenerateHash(code, salt),
                CodeSalt = salt,
                CreatedAt = now,
                ExpiresAt = expiresAt
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            await PublishCodeAsync(user, code, cancellationToken);

            return new TwoFactorChallenge(
                MaskEmail(user.Email),
                SecondsUntil(expiresAt, now),
                CodeSent: true);
        }
```

Attempt limiting and the one-time confirmation stamp:

`PickMeUp.Services/TwoFactorService.cs:100-130`
```csharp
            challenge.AttemptCount++;

            if (challenge.AttemptCount > _options.MaxAttempts)
            {
                challenge.ConsumedAt = now;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Two-factor challenge for user {UserId} was blocked after too many attempts.", userId);

                throw new BusinessException("Too many wrong codes. Sign in again to get a new one.");
            }

            if (!_cryptoService.Verify(challenge.CodeHash, challenge.CodeSalt, code.Trim()))
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new BusinessException("That code is not correct.");
            }

            challenge.ConsumedAt = now;

            var user = await LoadUserAsync(userId);

            if (user.TwoFactorMode == TwoFactorModeEnum.ConfirmOnce && user.TwoFactorConfirmedAt == null)
            {
                user.TwoFactorConfirmedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return userId;
        }
```

**Defense notes**
- No JWT is minted until the code is verified — the login response in the challenge branch has `Accesstoken` null.
- The code exists in plain text only in the email; the database stores `CodeHash` + `CodeSalt` via the same PBKDF2 helper used for passwords. That is why the demo reads codes from Mailpit, not from the database.
- The attempt counter is saved even on a wrong code, so attempts cannot be reset by retrying.
- Two-factor is evaluated **before** the forced-password-change flag, which is only set inside `IssueSessionAsync`.

---

# Admin desktop

## 10. Creating a taxi company provisions its login

**What it does.** When an administrator adds a taxi company, the system also creates the Company user account it will sign in with, using a password the administrator types, and marks that account as needing a password change on first login.

**Why we built it this way.** A company cannot sign in without an account, and generating a secret the admin then has to relay is worse than letting them set one — provided the owner is forced to replace it immediately.

**How it works.** Name, contact phone and login uniqueness are checked first, then user + role + company are written in one transaction.

**Code**

`PickMeUp.Services/TaxiCompanyService.cs:171-185`
```csharp
        public override async Task<TaxiCompanyResponse> InsertAsync(TaxiCompanyInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            await EnsureCityExistsAsync(request.CityId);
            await EnsureCompanyNameIsFreeAsync(request.Name, null);
            await EnsureContactPhoneIsFreeAsync(request.ContactPhone, null);

            var loginEmail = string.IsNullOrWhiteSpace(request.LoginEmail)
                ? request.ContactEmail
                : request.LoginEmail;

            await EnsureLoginIsFreeAsync(request.LoginUsername, loginEmail);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
```

`PickMeUp.Services/TaxiCompanyService.cs:197-213`
```csharp
                Username = request.LoginUsername,
                PasswordSalt = salt,
                PasswordHash = _cryptoService.GenerateHash(request.LoginPassword, salt),
                IsActive = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = companyRole.Id,
                DateAssigned = DateTime.UtcNow
            });
```

**Defense notes**
- `MustChangePassword = true` is what forces the gate screen on the company's first sign-in, so the admin-chosen password lives only until then.
- The whole thing is one transaction — a half-created company with no login cannot exist.

## 11. Deleting a company revokes every login it owns

**What it does.** Soft-deleting a taxi company also closes the owner's account **and** every one of its drivers: each is deactivated, soft-deleted, marked resigned, and has their refresh tokens destroyed.

**Why we built it this way.** Deleting a company used to leave its people able to sign in and keep working for a company that no longer existed.

**How it works.** All affected accounts are collected first, their refresh tokens deleted in one statement, then every account and driver row is stamped, inside a single transaction.

**Code**

`PickMeUp.Services/TaxiCompanyService.cs:338-375`
```csharp
        public override async Task DeleteAsync(int id)
        {
            var company = await _dbContext.TaxiCompanies.FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"TaxiCompany with id {id} not found.");

            var deletedAt = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var drivers = await _dbContext.Drivers
                .Where(d => d.TaxiCompanyId == company.Id)
                .ToListAsync();

            var accountIds = drivers.Select(d => d.UserId).Append(company.UserId).ToList();

            await _dbContext.RefreshTokens
                .IgnoreQueryFilters()
                .Where(t => accountIds.Contains(t.UserId))
                .ExecuteDeleteAsync();

            var accounts = await _dbContext.Users
                .Where(u => accountIds.Contains(u.Id))
                .ToListAsync();

            foreach (var account in accounts)
            {
                account.IsActive = false;
                account.IsDeleted = true;
                account.DeletedAt = deletedAt;
            }

            foreach (var driver in drivers)
            {
                driver.IsActive = false;
                driver.IsDeleted = true;
                driver.DeletedAt = deletedAt;
                driver.ResignedAt ??= deletedAt;
                driver.UpdatedAt = deletedAt;
```

**Defense notes**
- This is a soft delete (NFR-12): no rows are destroyed, so historical orders, payments and reports still resolve their company and driver names.
- Login is blocked because `GetByUsernameAsync` reads `_dbContext.Users`, which carries the global `!IsDeleted` filter — the account simply stops being found.
- `ResignedAt ??= deletedAt` preserves an earlier resignation date rather than overwriting it.

## 12. User management — uniqueness rules

**What it does.** Username, email and phone are each unique across live accounts, checked in the service with readable messages and enforced in the database.

**Why we built it this way.** The service check produces a message an examiner can read; the partial unique index closes the race between two simultaneous registrations and is filtered so a soft-deleted account releases its username again.

**Code**

`PickMeUp.Services/UserService.cs:176-186`
```csharp
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new BusinessException($"Email '{request.Email}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new BusinessException($"Username '{request.Username}' is already in use.");
            }

            await EnsurePhoneIsFreeAsync(NormalizePhone(request.PhoneNumber), null);
```

**Defense notes**
- The indexes are filtered on `"IsDeleted" = false`, so reusing a deleted account's username is allowed and duplicates between live rows are not.
- The phone index additionally filters `"PhoneNumber" IS NOT NULL`, so the many accounts without a phone do not collide.

## 13. PDF reports (admin-only)

**What it does.** Two PDF reports for administrators: orders grouped by period, and the top five companies by completed rides with their average rating and revenue.

**Why we built it this way.** FR-A6 requires PDF reports, and NFR-11 requires role restriction — these endpoints are `Admin` only.

**How it works.** The query aggregates in the database (rides grouped by company, ratings grouped separately) and the result is rendered by QuestPDF.

**Code**

`PickMeUp.WebAPI/Controllers/ReportsController.cs:47-52`
```csharp
    [Authorization(Roles.Admin)]
    [HttpGet("TopCompanies")]
    [Produces("application/pdf")]
    public async Task<IActionResult> TopCompanies([FromQuery] ReportRangeSearch search)
    {
        return Pdf(await _service.GenerateTopCompaniesAsync(search));
```

`PickMeUp.Services/ReportService.cs:123-159`
```csharp
        public async Task<ReportFile> GenerateTopCompaniesAsync(ReportRangeSearch search)
        {
            var range = ReportRange.Of(search);
            var completed = (int)OrderStatusEnum.Completed;

            var rides = _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.TaxiCompanyId != null
                    && o.OrderStatusId == completed
                    && o.CompletedAt >= range.From
                    && o.CompletedAt < range.ToExclusive)
                .Select(o => new
                {
                    CompanyId = o.TaxiCompanyId!.Value,
                    Fare = o.Fare ?? 0m,
                    IsPaid = _dbContext.Payments.Any(p => p.OrderId == o.Id && p.IsPaid)
                })
                .GroupBy(x => x.CompanyId)
                .Select(g => new
                {
                    CompanyId = g.Key,
                    CompletedRides = g.Count(),
                    Revenue = g.Sum(x => x.IsPaid ? x.Fare : 0m)
                });

            var ratings = _dbContext.Reviews
                .AsNoTracking()
                .Where(r => r.TargetCompanyId != null
                    && r.CreatedAt >= range.From
                    && r.CreatedAt < range.ToExclusive)
                .GroupBy(r => r.TargetCompanyId!.Value)
                .Select(g => new
                {
                    CompanyId = g.Key,
                    Average = g.Average(r => (double)r.Rating),
                    ReviewCount = g.Count()
                });
```

The desktop screen opens on a range that actually contains data:

`UI/pickmeup_desktop/lib/screens/admin/pdf_reports_screen.dart:353-356`
```dart
DateTime _defaultReportStart() {
  final now = DateTime.now();
  return DateTime(now.year, now.month - 5, 1);
}
```

**Defense notes**
- Revenue counts a ride only when it is both `Completed` **and** settled by a paid payment — an unpaid completed ride adds to the ride count but not to revenue.
- A ride is attributed to the period by `CompletedAt`, a review by `CreatedAt`; both ranges are half-open (`>= From`, `< ToExclusive`) so a day is never double-counted.
- The default window is the last six months because the demo data is older than 30 days; a 30-day default showed all zeros.

## 14. Support tickets — admin resolution

**What it does.** An administrator moves a ticket between Open / InProgress / Resolved and replies to it. Resolving stamps who resolved it and when; moving it back clears that.

**Why we built it this way.** The audit of who closed a complaint matters, and reopening must not leave a stale "resolved by" name behind.

**Code**

`PickMeUp.Services/SupportTicketService.cs:96-121`
```csharp
            var userId = RequireUserId();
            var ticket = await LoadTicketAsync(id);

            if (ticket.SupportTicketStatusId == (int)request.Status)
            {
                throw new BusinessException($"This ticket is already {request.Status}.");
            }

            ticket.SupportTicketStatusId = (int)request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (request.Status == SupportTicketStatusEnum.Resolved)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
                ticket.ResolvedByUserId = userId;
            }
            else
            {
                ticket.ResolvedAt = null;
                ticket.ResolvedByUserId = null;
            }

            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(ticket.Id);
        }
```

**Defense notes**
- Setting the status it already has is rejected with a readable message rather than silently writing a no-op.
- The resolver is taken from the token (`RequireUserId()`), not from the request body.

## 15. Reference data with caching

**What it does.** Cities, vehicle categories and the three status tables are full CRUD, served from an in-memory cache that is invalidated whenever that entity type changes.

**Why we built it this way.** FR-X6 requires full CRUD for reference data, and the grading list requires `IMemoryCache` for per-request data. These lists are read constantly (dropdowns) and change rarely.

**Code**

`PickMeUp.Services/BaseReferenceService.cs:14-44`
```csharp
    public abstract class BaseReferenceService<TEntity, TResponse, TSearch, TInsertRequest, TUpdateRequest>
        : BaseCRUDService<TEntity, TResponse, TSearch, TInsertRequest, TUpdateRequest>
        where TEntity : class
        where TSearch : ReferenceSearchObject
    {
        private static readonly ConcurrentDictionary<Type, CancellationTokenSource> _invalidationTokens = new();
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(30);

        private readonly IMemoryCache _cache;

        protected BaseReferenceService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<TInsertRequest> insertValidator,
            IValidator<TUpdateRequest> updateValidator,
            IMemoryCache cache)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cache = cache;
        }

        protected abstract string EntityDisplayName { get; }

        public override async Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null)
        {
            var cacheKey = BuildCacheKey(search);

            if (_cache.TryGetValue(cacheKey, out PageResult<TResponse>? cached) && cached != null)
            {
                return cached;
            }
```

**Defense notes**
- The cache key includes the search object, so a filtered list and an unfiltered list do not collide.
- Invalidation is per entity type via a `CancellationTokenSource`, so editing a city drops city entries only — not vehicle categories.

---

# Passenger mobile

## 16. Sign-up asks for a home city

**What it does.** Registration requires choosing the city you ride in, from a dropdown loaded from the database.

**Why we built it this way.** The home city drives which companies a passenger sees and what the recommender offers. It also satisfies the UI rule that dropdowns come from the database and no DB id is typed by hand.

**Code**

`PickMeUp.Services/Validators/UserInsertValidator.cs:33-35`
```csharp
            RuleFor(x => x.CityId)
                .NotNull().WithMessage("Select the city you ride in from the list.")
                .GreaterThan(0).WithMessage("Select the city you ride in from the list.");
```

`PickMeUp.Services/UserService.cs:188-191`
```csharp
            if (!await _dbContext.Cities.AnyAsync(c => c.Id == request.CityId && c.IsActive))
            {
                throw new BusinessException("The selected city does not exist. Pick a city from the list.");
            }
```

**Defense notes**
- Validated twice on purpose: FluentValidation gives the field-level message under the control, the service check confirms the city actually exists and is active.

## 17. Companies and recommendations are scoped to the home city

**What it does.** A passenger only ever sees companies operating in their registered home city, and cannot widen that by passing a different `CityId`.

**Why we built it this way.** Offering a Mostar passenger a Sarajevo company produces an order nobody can serve.

**How it works.** `GetAllAsync` resolves the caller's home city before the base pipeline runs; `ApplyFilters` then applies it ahead of any client-supplied filter, so the client filter can only narrow further.

**Code**

`PickMeUp.Services/TaxiCompanyService.cs:108-131`
```csharp
        public override async Task<PageResult<TaxiCompanyResponse>> GetAllAsync(TaxiCompanySearch? search = null)
        {
            _passengerHomeCityId = await ResolvePassengerHomeCityIdAsync();
            return await base.GetAllAsync(search);
        }

        private async Task<int?> ResolvePassengerHomeCityIdAsync()
        {
            if (!_userAccessor.IsInRole(Roles.Passenger))
            {
                return null;
            }

            var userId = _userAccessor.GetUserId();

            if (userId == null)
            {
                return null;
            }

            return await _dbContext.Users
                .Where(u => u.Id == userId.Value)
                .Select(u => u.CityId)
                .FirstOrDefaultAsync();
```

`PickMeUp.Services/TaxiCompanyService.cs:134-139`
```csharp
        protected override IQueryable<TaxiCompany> ApplyFilters(IQueryable<TaxiCompany> query, TaxiCompanySearch? search)
        {
            if (_passengerHomeCityId.HasValue)
            {
                query = query.Where(t => t.CityId == _passengerHomeCityId.Value);
            }
```

**Defense notes**
- The scope applies only to the Passenger role — an admin listing companies is unaffected.
- A passenger sending `?CityId=2` for a different city gets an **empty** list rather than that city's companies, because the home-city predicate is ANDed in first.

## 18. Order screen — live quote then pay

**What it does.** As soon as pickup and destination are set, the screen asks the server for the price and shows the total. The order button then places the order and opens the payment sheet in one action.

**Why we built it this way.** The displayed number is the number charged, so it must come from the server. Ordering and paying became one step because an unpaid ride is one no driver will ever see.

**How it works.** Every change to pickup, destination or company calls `_refreshQuote`; a monotonically increasing token discards responses that arrive out of order. `_submit` creates the order, then hands straight to the payment flow.

**Code**

`UI/pickmeup_mobile/lib/screens/passenger/order_taxi_screen.dart:113-142`
```dart
  Future<void> _refreshQuote() async {
    final trip = _tripPayload();

    if (trip == null) {
      setState(() {
        _quote = null;
        _isQuoting = false;
      });
      return;
    }

    final token = ++_quoteToken;
    setState(() => _isQuoting = true);

    try {
      final quote = await _orders.quote(trip);

      if (!mounted || token != _quoteToken) return;

      setState(() {
        _quote = quote;
        _isQuoting = false;
      });
    } catch (_) {
      if (!mounted || token != _quoteToken) return;

      setState(() {
        _quote = null;
        _isQuoting = false;
      });
```

`UI/pickmeup_mobile/lib/screens/passenger/order_taxi_screen.dart:299-321`
```dart
    setState(() => _isSubmitting = true);

    Order order;

    try {
      order = await _orders.insert(_tripPayload()!);
    } catch (error) {
      if (!mounted) return;

      setState(() => _isSubmitting = false);
      ApiErrorHandler.surface(context, error);
      return;
    }

    if (!mounted) return;

    setState(() {
      _placed = order;
      _isPaid = false;
    });

    await _payForPlacedRide();
  }
```

**Defense notes**
- `_quoteToken` guards against a stale quote overwriting a newer one when the user moves the pin quickly.
- The same `_tripPayload()` object is sent to both `quote` and `insert`, which is why the quoted price and the created order's fare always match.
- If the payment sheet is cancelled the order still exists, unpaid and undispatched, and the confirmation card offers a **PAY NOW** button instead of stranding it.

## 19. Stripe payment sheet flow

**What it does.** Creates a payment intent on the server, opens Stripe's native sheet, then waits for the server to confirm the webhook arrived before declaring the ride paid.

**Why we built it this way.** The client must not decide that payment succeeded — Stripe's sheet returning is not proof. The flow polls our own API until `IsPaid` is true, which only the verified webhook can set.

**Code**

`UI/pickmeup_mobile/lib/services/ride_payment_flow.dart:43-92`
```dart
  static Future<RidePaymentResult> pay(
    PaymentProvider provider,
    int orderId,
  ) async {
    PaymentIntentDetails intent;

    try {
      intent = await provider.createIntent(orderId);
    } on ApiClientException catch (e) {
      return RidePaymentResult(RidePaymentOutcome.failed, e.message);
    } catch (_) {
      return const RidePaymentResult(
        RidePaymentOutcome.failed,
        'Could not start the payment. Please try again.',
      );
    }

    try {
      await Stripe.instance.initPaymentSheet(
        paymentSheetParameters: SetupPaymentSheetParameters(
          paymentIntentClientSecret: intent.clientSecret ?? '',
          merchantDisplayName: 'PickMeUp',
          style: ThemeMode.light,
        ),
      );

      await Stripe.instance.presentPaymentSheet();
    } on StripeException catch (e) {
      if (e.error.code == FailureCode.Canceled) {
        return const RidePaymentResult(
          RidePaymentOutcome.cancelled,
          'Payment cancelled. The ride has not been sent to drivers yet.',
        );
      }

      return RidePaymentResult(
        RidePaymentOutcome.declined,
        e.error.localizedMessage ??
            e.error.message ??
            'That card was declined. Try another card.',
      );
    } catch (_) {
      return const RidePaymentResult(
        RidePaymentOutcome.failed,
        'The payment sheet could not be opened. Please try again.',
      );
    }

    return await awaitConfirmation(provider, orderId);
  }
```

**Defense notes**
- The client only ever sends `orderId` — the amount comes from the server's stored fare.
- Cancelling, a decline and a transport failure are three distinct outcomes with distinct messages, none of which mark the ride paid.
- `awaitConfirmation` polls `Payments/ByOrder`; the ride is declared paid only when the server says `IsPaid`, i.e. after the signed webhook landed.

## 20. Live ride status over SignalR

**What it does.** The passenger's ride status screen updates itself when the ride changes — accepted, picked up, completed, cancelled — without pulling to refresh.

**Why we built it this way.** FR-X5 requires real-time notifications. The screen reuses the app's single SignalR connection rather than opening its own.

**How it works.** The screen subscribes to `NotificationStore`, remembers the newest notification id it has seen, and reloads when a newer one arrives. The hub connection itself is authenticated by the JWT passed in the query string, because browsers and WebSocket clients cannot set an `Authorization` header on the socket handshake.

**Code**

`UI/pickmeup_mobile/lib/screens/passenger/ride_status_screen.dart:44-57`
```dart
  @override
  void initState() {
    super.initState();
    _load();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      final store = context.read<NotificationStore>();
      _store = store;
      _lastSeenNotificationId = store.items.isEmpty
          ? null
          : store.items.first.id;
      store.addListener(_onNotifications);
    });
  }
```

`PickMeUp.WebAPI/Program.cs:251-261`
```csharp
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
```

The hub is mapped at `PickMeUp.WebAPI/Program.cs:354` as `/hubs/notifications`.

**Defense notes**
- The query-string token is accepted **only** for paths starting `/hubs` — every ordinary REST endpoint still requires the `Authorization` header.
- The listener is removed in `dispose()`, so leaving the screen does not leak a subscription.
- Notifications are also persisted, so the bell badge and history survive an app restart; the live push is an optimisation on top, not the source of truth.
