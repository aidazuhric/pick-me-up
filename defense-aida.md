# PickMeUp — Defense Dossier: Aida Zuhrić

**Responsibility split.** Per `CLAUDE.md`, Aida defends the **Company desktop** app (the live fleet map, vehicle and driver CRUD, car assignments, the company side of support, and the company profile), the **Driver mobile** app (starting and ending a shift with breaks, the ride-requests list and the heads-up accept/decline banner, the active ride and its transitions, completed rides, driver reviews and location reporting), and the **shared backend she personally defends**: `ShiftService` and dispatch eligibility, the atomic accept that decides which driver wins a ride, one-driver-one-ride, driver location updates, the SignalR dispatch of orders to drivers, the backend behind the heads-up banner, and the Worker with its RabbitMQ consumption, retry policy and branded email templates. Work owned by Eldar — the pay-before-dispatch `IsPrepaid` condition inside the availability query, the company-delete cascade that closes driver accounts, fares, payments, reviews and the recommender — is flagged as his where it touches these flows rather than claimed here.

All excerpts below are verbatim from the repository at the stated paths and line numbers.

## Table of contents

**Backend Aida defends**

1. [Dispatch eligibility — one predicate, reused everywhere](#1-dispatch-eligibility--one-predicate-reused-everywhere)
2. [Starting and stopping a shift](#2-starting-and-stopping-a-shift)
3. [Breaks take a driver out of dispatch](#3-breaks-take-a-driver-out-of-dispatch)
4. [First driver to accept wins — atomic claim](#4-first-driver-to-accept-wins--atomic-claim)
5. [Declining re-queues the ride to everyone else](#5-declining-re-queues-the-ride-to-everyone-else)
6. [One driver, one ride](#6-one-driver-one-ride)
7. [Pushing a ride to the right drivers](#7-pushing-a-ride-to-the-right-drivers)
8. [SignalR hub — per-user groups](#8-signalr-hub--per-user-groups)
9. [Driver location reporting](#9-driver-location-reporting)
10. [Worker — RabbitMQ consumption with exponential backoff](#10-worker--rabbitmq-consumption-with-exponential-backoff)
11. [Branded emails with a plain-text twin](#11-branded-emails-with-a-plain-text-twin)

**Company desktop** 12. [Company scope is resolved from the token](#12-company-scope-is-resolved-from-the-token) 13. [Adding a driver provisions their login](#13-adding-a-driver-provisions-their-login) 14. [Vehicles — delete is blocked while assigned](#14-vehicles--delete-is-blocked-while-assigned) 15. [Fleet map — stale positions are shown as stale](#15-fleet-map--stale-positions-are-shown-as-stale)

**Driver mobile** 16. [My shift — start, break, end](#16-my-shift--start-break-end) 17. [Heads-up banner — a ride arrives wherever you are](#17-heads-up-banner--a-ride-arrives-wherever-you-are) 18. [Banner accept reuses the same claim](#18-banner-accept-reuses-the-same-claim) 19. [Active ride — picked up, then completed](#19-active-ride--picked-up-then-completed) 20. [Completing a ride counts towards the shift](#20-completing-a-ride-counts-towards-the-shift)

---

# Backend Aida defends

## 1. Dispatch eligibility — one predicate, reused everywhere

**What it does.** Decides whether a driver is currently reachable by dispatch: they must have an open shift and must not be on a break.

**Why we built it this way.** The same rule is needed in three places — the driver's available-orders query, the push that notifies drivers, and the eligibility card on the driver's own screen. Writing it three times would guarantee they drift apart. It is a single `Expression`, so EF translates it into SQL rather than pulling shifts into memory.

**How it works.** `DispatchEligibleShift` is an expression tree over `Shift`; `QueryDispatchEligibleDriverIds` turns it into an `IQueryable<int>` that other queries compose into their own `WHERE`.

**Code**

`PickMeUp.Services/ShiftService.cs:29-39`
```csharp
        public static Expression<Func<Shift, bool>> OpenShift =>
            shift => shift.EndedAt == null;

        public static Expression<Func<Shift, bool>> DispatchEligibleShift =>
            shift => shift.EndedAt == null && !shift.Breaks.Any(b => b.EndedAt == null);

        public IQueryable<int> QueryDispatchEligibleDriverIds() =>
            _dbContext.Shifts.Where(DispatchEligibleShift).Select(s => s.DriverId).Distinct();

        public Task<bool> IsDriverDispatchEligibleAsync(int driverId) =>
            _dbContext.Shifts.Where(s => s.DriverId == driverId).AnyAsync(DispatchEligibleShift);
```

**Defense notes**
- `QueryDispatchEligibleDriverIds` returns an **`IQueryable`, not a list** — it is never executed on its own, it is composed into the caller's query and runs as one SQL statement. That is how the no-N+1 rule is honoured here.
- "On shift" and "dispatch eligible" are deliberately two different predicates: a driver on a break still has an open shift, so `OpenShift` is true while `DispatchEligibleShift` is false.

## 2. Starting and stopping a shift

**What it does.** A driver opens a shift when they start working and closes it when they finish. Only one shift can be open at a time, and stopping a shift also closes a break left open.

**Why we built it this way.** The shift is what makes a driver visible to dispatch, so its lifecycle has to be unambiguous. Two open shifts would make "am I working?" unanswerable; a break left dangling would keep the driver out of dispatch forever after their shift ended.

**How it works.** `StartShiftAsync` resolves the caller's driver id from the token, refuses if a shift is already open, then inserts one. `StopShiftAsync` closes any open break first, then stamps `EndedAt`.

**Code**

`PickMeUp.Services/ShiftService.cs:71-119`
```csharp
        public async Task<ShiftResponse> StartShiftAsync()
        {
            var driverId = await RequireCallerDriverIdAsync();

            var alreadyOpen = await _dbContext.Shifts.Where(s => s.DriverId == driverId).AnyAsync(OpenShift);

            if (alreadyOpen)
            {
                throw new BusinessException("You already have an open shift. Stop it before starting a new one.");
            }

            var shift = new Shift
            {
                DriverId = driverId,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Shifts.Add(shift);
            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(shift.Id);
        }

        public async Task<ShiftResponse> StopShiftAsync(int id)
        {
            var shift = await LoadOwnedShiftForWriteAsync(id);

            if (shift.EndedAt != null)
            {
                throw new BusinessException("This shift has already been stopped.");
            }

            var now = DateTime.UtcNow;

            var openBreak = shift.Breaks.FirstOrDefault(b => b.EndedAt == null);
            if (openBreak != null)
            {
                openBreak.EndedAt = now;
                openBreak.UpdatedAt = now;
            }

            shift.EndedAt = now;
            shift.UpdatedAt = now;

            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(shift.Id);
        }
```

**Defense notes**
- The driver id comes from `RequireCallerDriverIdAsync()`, never from the request — a driver cannot open a shift for somebody else.
- `LoadOwnedShiftForWriteAsync(id)` is what stops a driver stopping another driver's shift; the ownership check is in the load, not bolted on afterwards.
- Every timestamp is `DateTime.UtcNow`, per the grading rule.

## 3. Breaks take a driver out of dispatch

**What it does.** A driver on shift can start and stop a break. While the break is open they receive no new ride requests, but their shift keeps running.

**Why we built it this way.** A driver having lunch should not be offered rides, but forcing them to end and restart the shift would corrupt the shift statistics the company sees.

**How it works.** A `ShiftBreak` row with `EndedAt == null` is what `DispatchEligibleShift` looks for. Starting a second break while one is open is refused, as is stopping a break when none is running.

**Code**

`PickMeUp.Services/ShiftService.cs:121-165`
```csharp
        public async Task<ShiftResponse> StartBreakAsync(int id)
        {
            var shift = await LoadOwnedShiftForWriteAsync(id);

            if (shift.EndedAt != null)
            {
                throw new BusinessException("This shift has ended. Start a new shift before taking a break.");
            }

            if (shift.Breaks.Any(b => b.EndedAt == null))
            {
                throw new BusinessException("You are already on a break.");
            }

            var now = DateTime.UtcNow;

            _dbContext.ShiftBreaks.Add(new ShiftBreak
            {
                ShiftId = shift.Id,
                StartedAt = now,
                CreatedAt = now
            });

            shift.UpdatedAt = now;
            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(shift.Id);
        }

        public async Task<ShiftResponse> StopBreakAsync(int id)
        {
            var shift = await LoadOwnedShiftForWriteAsync(id);

            var openBreak = shift.Breaks.FirstOrDefault(b => b.EndedAt == null)
                ?? throw new BusinessException("You are not currently on a break.");

            var now = DateTime.UtcNow;
            openBreak.EndedAt = now;
            openBreak.UpdatedAt = now;
            shift.UpdatedAt = now;

            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(shift.Id);
        }
```

**Defense notes**
- Nothing about the break is stored on the driver or the shift as a flag — eligibility is derived from the break rows, so there is no second copy of the truth to go stale.
- A break cannot be started on an ended shift, which is why the `StopShiftAsync` auto-close in section 2 matters: it prevents an orphaned open break.

## 4. First driver to accept wins — atomic claim

**What it does.** When several drivers are offered the same ride and tap Accept at the same moment, exactly one of them gets it. The others are told it is gone.

**Why we built it this way.** Read-then-write would let two drivers both read `DriverId == null` and both write themselves in. The claim therefore has to be a single conditional UPDATE that the database itself serialises.

**How it works.** `ExecuteUpdateAsync` runs one `UPDATE … WHERE Id = @id AND OrderStatusId = Pending AND DriverId IS NULL`. The database reports how many rows it changed; `0` means somebody else already claimed it, and the transaction is rolled back.

**Code**

`PickMeUp.Services/OrderService.cs:155-209`
```csharp
        public async Task<OrderResponse> AcceptAsync(int id, OrderAcceptRequest request)
        {
            var driver = await RequireEligibleDriverAsync();
            var order = await LoadForTransitionAsync(id);

            EnsureOrderIsOfferedTo(order, driver);

            var target = _stateMachine.ResolveTarget(OrderTransition.Accept, CurrentStatus(order));

            var carId = request.CarId ?? await ResolveAssignedCarIdAsync(driver.Id);

            if (carId == null)
            {
                throw new BusinessException("You have no vehicle assigned. Ask your company to assign one first.");
            }

            await EnsureCarBelongsToDriverAsync(carId.Value, driver);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var from = CurrentStatus(order);
            var companyWasUnassigned = order.TaxiCompanyId == null;
            var acceptedAt = DateTime.UtcNow;

            var claimed = await _dbContext.Orders
                .Where(o => o.Id == order.Id
                    && o.OrderStatusId == (int)OrderStatusEnum.Pending
                    && o.DriverId == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.DriverId, driver.Id)
                    .SetProperty(o => o.CarId, carId)
                    .SetProperty(o => o.TaxiCompanyId, o => o.TaxiCompanyId ?? driver.TaxiCompanyId)
                    .SetProperty(o => o.AcceptedAt, acceptedAt)
                    .SetProperty(o => o.OrderStatusId, (int)target));

            if (claimed == 0)
            {
                await transaction.RollbackAsync();
                throw new BusinessException("This order has already been accepted by a driver.");
            }

            await RecordHistoryAsync(order.Id, from, target, driver.UserId, "Accepted by driver");
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            if (companyWasUnassigned)
            {
                await _interactions.RecordAsync(
                    order.PassengerId, driver.TaxiCompanyId, InteractionSignalType.Ordered, 1m, order.Id);
            }

            await _dispatcher.OrderStatusChangedAsync(order.Id, from.ToString(), target.ToString());

            return await LoadResponseAsync(order.Id);
        }
```

**Defense notes**
- The race is settled by the `WHERE` clause, not by application code — this was verified by firing two concurrent accepts at the same paid order three times; each round produced exactly one winner, one readable refusal, one driver assignment and one audit row.
- The loser gets a `BusinessException`, which the global `ExceptionFilter` turns into a 400 with a real message, not a 500.
- A general order (no company chosen) picks up the winning driver's company via `TaxiCompanyId ?? driver.TaxiCompanyId`, so the ride belongs to whoever took it.
- The `IsPrepaid` condition that decides which orders are offered at all is **Eldar's** pay-before-dispatch work; this method consumes it through `EnsureOrderIsOfferedTo`.

## 5. Declining re-queues the ride to everyone else

**What it does.** A driver can decline a request. It disappears from their own list and is offered to the remaining eligible drivers.

**Why we built it this way.** A declined ride must not bounce straight back to the same driver, and it must not be lost either. Recording the refusal as a row means the exclusion is permanent for that driver and survives a restart.

**How it works.** An `OrderRejection` row is written, then availability is pushed again with that driver excluded. `QueryAvailableFor` reads the same table, so the ride never reappears for them.

**Code**

`PickMeUp.Services/OrderService.cs:211-243`
```csharp
        public async Task<OrderResponse> RejectAsync(int id)
        {
            var driver = await RequireEligibleDriverAsync();
            var order = await LoadForTransitionAsync(id);

            EnsureOrderIsOfferedTo(order, driver);

            if (CurrentStatus(order) != OrderStatusEnum.Pending)
            {
                throw new BusinessException("Only a pending order can be declined.");
            }

            var already = await _dbContext.OrderRejections
                .AnyAsync(r => r.OrderId == id && r.DriverId == driver.Id);

            if (already)
            {
                throw new BusinessException("You have already declined this order.");
            }

            _dbContext.OrderRejections.Add(new OrderRejection
            {
                OrderId = id,
                DriverId = driver.Id,
                RejectedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();

            await _dispatcher.OrderBecameAvailableAsync(id, driver.Id);

            return await LoadResponseAsync(id);
        }
```

**Defense notes**
- The order's status is untouched — it stays `Pending`, so it remains claimable by anybody else. Declining is not cancelling.
- `OrderBecameAvailableAsync(id, driver.Id)` re-pushes to everyone **except** the driver who just declined.
- Only a pending order can be declined, so a driver cannot decline a ride they have already accepted as a way of abandoning it.

## 6. One driver, one ride

**What it does.** A driver who is already carrying a passenger is offered no further requests until that ride is completed or cancelled.

**Why we built it this way.** A driver can physically only serve one ride at a time, and a request they cannot take is noise that delays the passenger who is waiting.

**How it works.** The availability query carries a `NOT EXISTS` against the driver's own orders in `Confirmed` or `InProgress`. The same condition is repeated in the push so a busy driver is not even notified.

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

**Defense notes**
- This one query is the single source of "what may this driver see", and every driver-facing list goes through it — the requests screen, the heads-up banner's re-check, and the accept guard.
- The `o.IsPrepaid` line is **Eldar's** pay-before-dispatch rule, not part of the one-ride-per-driver work; the rest of the predicate is the dispatch logic defended here.
- Because it is a query and not a UI filter, a driver cannot reach a hidden ride by crafting a request by hand.

## 7. Pushing a ride to the right drivers

**What it does.** When a ride becomes available, a notification is pushed only to the drivers who could actually take it: right company, on shift, not on a break, not already busy, and not having declined it.

**Why we built it this way.** Broadcasting to every driver and filtering in the app would leak other companies' rides to the client and would wake drivers who cannot act.

**How it works.** One query intersects all the conditions and projects to `UserId`; if nobody qualifies it returns early rather than calling the notification layer with an empty list. The whole push is wrapped in a `try` so a notification failure can never break the order flow that triggered it.

**Code**

`PickMeUp.Services/Orders/OrderNotificationDispatcher.cs:145-175`
```csharp
        private async Task PushAvailabilityAsync(Order order, int? excludeDriverId)
        {
            try
            {
                var eligibleDriverIds = _shiftService.QueryDispatchEligibleDriverIds();

                var recipients = await _dbContext.Drivers
                    .Where(d => d.ResignedAt == null)
                    .Where(d => order.TaxiCompanyId == null || d.TaxiCompanyId == order.TaxiCompanyId)
                    .Where(d => excludeDriverId == null || d.Id != excludeDriverId.Value)
                    .Where(d => eligibleDriverIds.Contains(d.Id))
                    .Where(d => !_dbContext.OrderRejections.Any(r => r.OrderId == order.Id && r.DriverId == d.Id))
                    .Where(d => !_dbContext.Orders.Any(active => active.DriverId == d.Id
                        && (active.OrderStatusId == (int)OrderStatusEnum.Confirmed
                            || active.OrderStatusId == (int)OrderStatusEnum.InProgress)))
                    .Select(d => d.UserId)
                    .ToListAsync();

                if (recipients.Count == 0)
                {
                    return;
                }

                await _notifications.DeliverAsync(recipients, new NotificationMessage
                {
                    Title = "New ride available",
                    Body = $"Pick-up at {order.PickupAddress}.",
                    OrderId = order.Id,
                    Category = NotificationCategories.OrderAvailable
                });
            }
```

**Defense notes**
- The recipient set mirrors `QueryAvailableFor` exactly, so a driver is never notified about a ride their own list would hide.
- The push is best-effort by design: it is inside a `try` and logged on failure, because a dropped notification must not roll back an accepted order or a confirmed payment.
- Under the pay-before-dispatch flow this runs from the **payment webhook** rather than at order creation — that trigger point is **Eldar's** work; the recipient selection is the part defended here.

## 8. SignalR hub — per-user groups

**What it does.** Holds the live connection each app keeps open, and routes a notification to one specific user's devices.

**Why we built it this way.** FR-X5 asks for real-time notifications. Putting every connection of one user into a group named after their id means a push is addressed, not broadcast — no client ever receives another user's traffic.

**How it works.** The hub requires a valid JWT. On connect it reads the user id from the token and joins `user-{id}`; a connection with no identifiable user is aborted.

**Code**

`PickMeUp.WebAPI/Hubs/NotificationHub.cs:12-42`
```csharp
[Authorize]
public class NotificationHub : Hub
{
    public const string ReceiveNotification = "ReceiveNotification";

    private readonly IAuthenticatedUserAccessor _userAccessor;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(IAuthenticatedUserAccessor userAccessor, ILogger<NotificationHub> logger)
    {
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public static string GroupFor(int userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId();

        if (userId == null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(userId.Value));
        _logger.LogInformation("User {UserId} connected to notifications", userId.Value);

        await base.OnConnectedAsync();
    }
```

**Defense notes**
- The group name is derived from the **token**, not from anything the client sends, so a client cannot subscribe to another user's group.
- `Context.Abort()` on a missing user id means an unauthenticated socket is dropped rather than left idle.
- The same hub serves every role; who receives what is decided by the recipient query in section 7, not by the transport.

## 9. Driver location reporting

**What it does.** A driver's app reports its current position; the company's fleet map reads the latest position of every driver.

**Why we built it this way.** One row per driver, overwritten in place, rather than an append-only trail: the fleet map only ever needs "where is this car now", and a growing history table would need pruning for no benefit.

**How it works.** Coordinates are range-checked, the driver id comes from the token, and the row is inserted on first report and updated thereafter.

**Code**

`PickMeUp.Services/DriverService.cs:219-251`
```csharp
        public async Task<DriverLocationResponse> ReportOwnLocationAsync(DriverLocationUpdateRequest request)
        {
            if (request.Latitude is < -90 or > 90)
            {
                throw new BusinessException("Latitude must be between -90 and 90.");
            }

            if (request.Longitude is < -180 or > 180)
            {
                throw new BusinessException("Longitude must be between -180 and 180.");
            }

            var driverId = await RequireCallerDriverIdAsync();

            var location = await _dbContext.DriverLocations
                .FirstOrDefaultAsync(l => l.DriverId == driverId);

            if (location == null)
            {
                location = new DriverLocation { DriverId = driverId };
                _dbContext.DriverLocations.Add(location);
            }

            location.Latitude = request.Latitude;
            location.Longitude = request.Longitude;
            location.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var rows = await QueryFleetLocations(l => l.DriverId == driverId);

            return rows.Single();
        }
```

**Defense notes**
- A driver can only ever write their **own** position — the id is resolved from the token, and the request DTO carries no driver id.
- Coordinates are validated server-side, so a broken GPS reading cannot put a car at latitude 500.
- `UpdatedAt` is what the fleet map uses to decide whether a position is still trustworthy — see section 15.

## 10. Worker — RabbitMQ consumption with exponential backoff

**What it does.** A separate Worker service consumes the events the API publishes and sends the emails. A failing send is retried four times with growing delays before the message is given up on.

**Why we built it this way.** NFR-1 requires the Worker to be its own project and container, and NFR-4 fixes the retry schedule at 1s → 2s → 4s → 8s with every failure logged. Retrying in-process inside the API would block a request and would not survive an API restart.

**How it works.** `SendWithBackoffAsync` loops over the delay table; success acknowledges the message, exhausting the retries logs an error and nacks it without requeue so a permanently broken message cannot loop forever.

**Code**

`PickMeUp.Worker/NotificationConsumer.cs:89-125`
```csharp
        private async Task<bool> SendWithBackoffAsync(EventEnvelope envelope)
        {
            for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
            {
                try
                {
                    await _emailSender.SendAsync(envelope.RecipientEmail, envelope.RecipientName,
                        envelope.Subject, envelope.Body, envelope.HtmlBody, CancellationToken.None);

                    _logger.LogInformation("Delivered event {EventId} to {Recipient} on attempt {Attempt}",
                        envelope.EventId, envelope.RecipientEmail, attempt + 1);

                    return true;
                }
                catch (Exception exception)
                {
                    if (attempt == RetryDelays.Length)
                    {
                        _logger.LogError(exception,
                            "Giving up on event {EventId} for {Recipient} after {Attempts} attempts.",
                            envelope.EventId, envelope.RecipientEmail, attempt + 1);

                        return false;
                    }

                    var delay = RetryDelays[attempt];

                    _logger.LogWarning(exception,
                        "Attempt {Attempt} to deliver event {EventId} failed. Retrying in {Delay}s.",
                        attempt + 1, envelope.EventId, delay.TotalSeconds);

                    await Task.Delay(delay);
                }
            }

            return false;
        }
```

The Worker falls back to writing the message into its log when no SMTP host is configured, so the demo still works without a mail server:

`PickMeUp.Worker/Program.cs:10-21`
```csharp
var smtpHost = EnvironmentReader.Optional("SMTP_HOST");

if (string.IsNullOrWhiteSpace(smtpHost))
{
    builder.Services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
}
else
{
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
}

builder.Services.AddHostedService<NotificationConsumer>();
```

**Defense notes**
- The loop runs `RetryDelays.Length + 1` times — one first attempt plus four retries — which is why the log says "after 5 attempts" when it gives up.
- Failures are nacked **without requeue**, so a message that can never be delivered does not spin forever; it is logged instead.
- In the demo SMTP points at Mailpit, so the mail is really sent and can be opened in a browser; with SMTP blank it goes to the Worker log instead.

## 11. Branded emails with a plain-text twin

**What it does.** Every outgoing message is a branded HTML email with a plain-text alternative attached, so clients that refuse HTML still show something readable.

**Why we built it this way.** A one-line plain-text message looks like spam for something as sensitive as a sign-in code. Describing the content once and rendering both parts from that description means the HTML and the text can never disagree.

**How it works.** `IntegrationEvent` makes each event declare an `EmailContent`; `Body` and `HtmlBody` are both derived from it. The layout is table-based with inline CSS only, which is what mail clients tolerate.

**Code**

`PickMeUp.Model/Messaging/IntegrationEvent.cs:1-25`
```csharp
using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public abstract class IntegrationEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public string RecipientEmail { get; set; } = string.Empty;

        public string RecipientName { get; set; } = string.Empty;

        public abstract string RoutingKey { get; }

        public abstract string Subject { get; }

        protected abstract EmailContent Content { get; }

        public string Body => EmailTemplate.RenderText(Content);

        public string HtmlBody => EmailTemplate.RenderHtml(Subject, Content);
    }
}
```

`PickMeUp.Model/Emails/EmailTemplate.cs:20-36`
```csharp
            var html = new StringBuilder();

            html.Append("<!doctype html><html><head><meta charset=\"utf-8\">");
            html.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
            html.Append($"<title>{Escape(subject)}</title></head>");
            html.Append($"<body style=\"margin:0;padding:0;background:{PageBackground};\">");

            html.Append($"<span style=\"display:none;max-height:0;overflow:hidden;opacity:0;\">{Escape(content.Intro)}</span>");

            html.Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background:{PageBackground};\">");
            html.Append("<tr><td align=\"center\" style=\"padding:24px 12px;\">");
            html.Append("<table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:100%;max-width:600px;\">");

            html.Append($"<tr><td style=\"background:{Brand};border-radius:10px 10px 0 0;padding:20px 28px;\">");
            html.Append($"<span style=\"font-family:{FontStack};font-size:20px;font-weight:700;color:#FFFFFF;letter-spacing:0.4px;\">PickMeUp</span>");
            html.Append($"<span style=\"font-family:{FontStack};font-size:13px;color:#D7DBEA;padding-left:10px;\">taxi, when you need it</span>");
            html.Append("</td></tr>");
```

**Defense notes**
- Seven message types share this one layout: the two-factor code, the password reset, order created / status changed / cancelled, and payment confirmed / refunded.
- Every interpolated value goes through `Escape` (`WebUtility.HtmlEncode`), so a name or an address containing `<` cannot break the markup.
- Table-based layout with inline styles and no external CSS or images is deliberate — that is the subset mail clients render reliably.

---

# Company desktop

## 12. Company scope is resolved from the token

**What it does.** Everything the company app reads and writes — vehicles, drivers, assignments, shifts, orders — is limited to that company, without the app ever sending a company id.

**Why we built it this way.** If the client passed its own company id, forging another company's id would expose their fleet. Resolving it server-side from the signed-in user makes that impossible.

**How it works.** `GetCallerCompanyIdAsync` looks for a company owned by this user; failing that, for a driver row belonging to them. Company-scoped services then apply that id to every query.

**Code**

`PickMeUp.Services/BaseCompanyScopedService.cs:98-121`
```csharp
        protected async Task<int?> GetCallerCompanyIdAsync()
        {
            var userId = _userAccessor.GetUserId();

            if (userId == null)
            {
                return null;
            }

            var ownedCompanyId = await _dbContext.TaxiCompanies
                .Where(t => t.UserId == userId.Value)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();

            if (ownedCompanyId != null)
            {
                return ownedCompanyId;
            }

            return await _dbContext.Drivers
                .Where(d => d.UserId == userId.Value && d.ResignedAt == null)
                .Select(d => (int?)d.TaxiCompanyId)
                .FirstOrDefaultAsync();
        }
```

**Defense notes**
- One method serves both roles: a Company user resolves through the company they own, a Driver through their employment — so a driver reading a shift list is scoped to their own company too.
- `ResignedAt == null` means a former driver stops resolving to their old company the moment they leave.
- `TaxiCompanies` carries a global `!IsDeleted` filter, so a deleted company resolves to `null` and its screens go empty rather than showing stale data.

## 13. Adding a driver provisions their login

**What it does.** When a company adds a driver, the driver's user account is created at the same time with a password the company types, and the driver is forced to replace it on first sign-in.

**Why we built it this way.** A driver cannot work without an account, and the company is the only party who knows the new hire. Forcing the change on first login means the company-chosen password is short-lived.

**How it works.** The login is checked for uniqueness, then user, role and driver rows are written in one transaction.

**Code**

`PickMeUp.Services/DriverService.cs:79-100`
```csharp
            await _insertValidator.ValidateAndThrowAsync(request);

            var companyId = await RequireCallerCompanyIdAsync();

            await EnsureLoginIsFreeAsync(request.LoginUsername, request.Email);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var driverRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Driver)
                ?? throw new BusinessException($"Role '{Roles.Driver}' is missing from the database.");

            var salt = _cryptoService.GenerateSlat();

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Username = request.LoginUsername,
                PhoneNumber = request.PhoneNumber,
                PasswordSalt = salt,
                PasswordHash = _cryptoService.GenerateHash(request.LoginPassword, salt),
```

**Defense notes**
- The driver is attached to `RequireCallerCompanyIdAsync()`, so a company can only ever create drivers for itself.
- The password is never stored — only a PBKDF2 hash with a per-user salt, the same helper used for every other account.
- The forced first-login password change is driven by `MustChangePassword` on the created user, mirroring how admin-created company accounts behave.

## 14. Vehicles — delete is blocked while assigned

**What it does.** A company manages its vehicles as full CRUD, but a vehicle that is still assigned to a driver cannot be deleted.

**Why we built it this way.** Deleting a car out from under a live assignment would leave a driver holding a vehicle that no longer exists, and would break the accept flow, which needs the driver's assigned car.

**How it works.** `DeleteAsync` first re-checks ownership, then refuses if any assignment row references the car, with a message that says what to do about it.

**Code**

`PickMeUp.Services/CarService.cs:130-147`
```csharp
        public override async Task DeleteAsync(int id)
        {
            await LoadOwnedCarAsync(id);
            await EnsureCarHasNoAssignmentsAsync(id);

            await base.DeleteAsync(id);
        }

        private async Task EnsureCarHasNoAssignmentsAsync(int carId)
        {
            var assigned = await _dbContext.CarAssignments.AnyAsync(a => a.CarId == carId);

            if (assigned)
            {
                throw new BusinessException(
                    "This vehicle is still assigned to a driver and cannot be removed. Remove its assignments first.");
            }
        }
```

**Defense notes**
- `LoadOwnedCarAsync(id)` runs first, so one company cannot probe for another company's car ids through the delete endpoint.
- The refusal is a `BusinessException` — a 400 with an actionable message, which is what the UI rules require rather than a bare "Bad request".
- The base `DeleteAsync` performs a **soft** delete, so historical orders that reference the car still resolve its plate.

## 15. Fleet map — stale positions are shown as stale

**What it does.** The company's fleet map plots each driver's last reported position, and marks a position as stale when it has not been refreshed recently.

**Why we built it this way.** A map that shows a car confidently at a location it left an hour ago is worse than one that admits it does not know. Drivers lose signal, close the app, or end their shift.

**How it works.** The age of `UpdatedAt` is compared against a threshold; a driver who has never reported is treated as stale too, and the age is rendered in words.

**Code**

`UI/pickmeup_desktop/lib/screens/company/fleet_map_screen.dart:69-84`
```dart
  static bool isStale(DateTime? updatedAt) {
    if (updatedAt == null) {
      return true;
    }

    return DateTime.now().toUtc().difference(updatedAt.toUtc()).inMinutes >
        _staleAfterMinutes;
  }

  static String lastSeen(DateTime? updatedAt) {
    if (updatedAt == null) {
      return 'never reported';
    }

    final minutes =
        DateTime.now().toUtc().difference(updatedAt.toUtc()).inMinutes;
```

**Defense notes**
- Both sides of the comparison are converted with `.toUtc()`, so the map is correct regardless of the desktop's time zone.
- A `null` timestamp is deliberately treated as stale rather than hidden — the company still sees the driver, with "never reported" against their name.
- The positions themselves come from the per-driver row described in section 9; the map only reads, it never writes.

---

# Driver mobile

## 16. My shift — start, break, end

**What it does.** The driver's shift screen starts a shift, takes and ends a break, and stops the shift. This is what makes them visible to dispatch.

**Why we built it this way.** These four actions map one-to-one onto the four server endpoints, so the screen holds no state machine of its own — the server decides what is legal and the app renders the result.

**How it works.** `ShiftProvider` posts to the four routes; every call returns the refreshed shift, so the screen re-renders from the server's answer rather than guessing.

**Code**

`UI/pickmeup_mobile/lib/providers/shift_provider.dart:24-30`
```dart
  Future<Shift> start() => _post('Start');

  Future<Shift> stop(int shiftId) => _post('$shiftId/Stop');

  Future<Shift> startBreak(int shiftId) => _post('$shiftId/Break/Start');

  Future<Shift> stopBreak(int shiftId) => _post('$shiftId/Break/Stop');
```

**Defense notes**
- No shift id is sent to `start()` — the server derives the driver from the token, which is why a driver cannot open a shift for a colleague.
- Each method returns the updated `Shift`, so the "you are on a break" state shown in the app is the server's, not a local flag that could drift.
- The rules behind these calls — one open shift, no double break — are enforced server-side in sections 2 and 3, not in the app.

## 17. Heads-up banner — a ride arrives wherever you are

**What it does.** When a new paid ride is dispatched to a driver, a card drops over whatever screen they are on, showing the passenger, distance, fare and both addresses, with Accept and Decline.

**Why we built it this way.** A request is worth money and another driver can take it, so waiting for the driver to remember to open the requests screen loses rides. The banner reuses the notification connection the app already holds rather than opening a second one.

**How it works.** The overlay subscribes to the notification store's push stream, filtered to the `order-available` category. On a push it re-queries the server's own availability endpoint — which re-checks every eligibility rule — and only shows the card if the ride is genuinely still offered to this driver.

**Code**

`UI/pickmeup_mobile/lib/widgets/driver_request_overlay.dart:38-47`
```dart
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;

      _subscription = context
          .read<NotificationStore>()
          .pushes
          .where((n) => n.category == DriverRequestOverlay.availableCategory)
          .listen(_onAvailablePush);
    });
  }
```

`UI/pickmeup_mobile/lib/widgets/driver_request_overlay.dart:62-100`
```dart
    if (_queue.any((o) => o.id == orderId)) {
      return;
    }

    final offered = await _readOfferedRide(orderId);

    if (offered == null || !mounted) {
      return;
    }

    setState(() {
      _queue = [
        offered,
        ..._queue.where((o) => o.id != orderId),
      ].take(DriverRequestOverlay.queueLimit).toList();
    });
  }

  Future<Order?> _readOfferedRide(int orderId) async {
    try {
      final result = await _orders.get(
        filter: {
          'AvailableForMe': true,
          'Page': 1,
          'PageSize': 20,
          'SortBy': 'CreatedAt desc',
        },
      );

      for (final order in result.items ?? const <Order>[]) {
        if (order.id == orderId) {
          return order;
        }
      }
    } catch (_) {
      return null;
    }

    return null;
```

**Defense notes**
- Eligibility is **not** re-implemented in the app: `AvailableForMe=true` runs the server's `QueryAvailableFor`, so the banner can only ever show a ride the driver could legitimately accept.
- The push carries only an order id; all displayed detail comes from that authenticated call, so a forged push cannot put false information on screen.
- Two pushes do not stack: the newest goes to the front, duplicates are dropped, and the queue is capped — the card shows one ride with a "+n waiting" count.
- It listens to a broadcast stream on the existing `NotificationStore`; no second SignalR connection is opened.

## 18. Banner accept reuses the same claim

**What it does.** Accepting from the banner takes the ride and moves the driver straight to the active-ride screen. Declining dismisses it and returns the ride to the other drivers.

**Why we built it this way.** The banner must not be a second, weaker way to take a ride. It calls the identical provider method the requests screen calls, so the atomic claim of section 4 applies unchanged.

**How it works.** `_orders.accept(order.id!)` hits the same endpoint; a refusal from a lost race arrives as an `ApiClientException` whose message is shown and the card is dropped.

**Code**

`UI/pickmeup_mobile/lib/widgets/driver_request_overlay.dart:107-144`
```dart
  Future<void> _accept(Order order) async {
    setState(() => _isBusy = true);

    try {
      final accepted = await _orders.accept(order.id!);

      if (!mounted) return;

      _drop(order.id!);
      AppMessages.success(
        context,
        'Ride #${accepted.id} is yours. The passenger has been told.',
      );

      await Navigator.of(context).push(
        MaterialPageRoute<void>(
          builder: (_) => Scaffold(
            appBar: AppBar(title: const Text('Active ride')),
            body: const ActiveRideScreen(),
          ),
        ),
      );
    } on ApiClientException catch (e) {
      if (!mounted) return;
      _drop(order.id!);
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }
```

**Defense notes**
- Losing the race is not an error state in the app: the card is dropped and the server's message — "This order has already been accepted by a driver." — is shown as-is.
- A transport failure is handled separately from a business refusal, so a dropped connection does not silently discard the ride from the queue.
- The buttons are disabled while `_isBusy`, so a double tap cannot send two accepts.

## 19. Active ride — picked up, then completed

**What it does.** The driver's active-ride screen drives the ride forward: confirmed → picked up → completed, one button at a time.

**Why we built it this way.** Each transition is a server call through the centralized state machine, so the app cannot invent an illegal jump such as completing a ride that was never picked up.

**How it works.** The screen calls `pickup` and `complete` on the order provider; the server resolves the legal target status, records the audit row, and pushes the change to the passenger.

**Code**

`PickMeUp.Services/OrderService.cs:245-269`

```csharp
        public async Task<OrderResponse> PickupAsync(int id)
        {
            var driver = await RequireEligibleDriverAsync();
            var order = await LoadForTransitionAsync(id);

            EnsureOrderIsOfferedTo(order, driver);
            EnsureNotAnotherDriversOrder(order, driver, id);

            var from = CurrentStatus(order);
            var target = _stateMachine.ResolveTarget(OrderTransition.Pickup, from);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            order.PickedUpAt = DateTime.UtcNow;
            ApplyStatus(order, target);

            await RecordHistoryAsync(order.Id, from, target, driver.UserId, "Passenger picked up");
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _dispatcher.OrderStatusChangedAsync(order.Id, from.ToString(), target.ToString());

            return await LoadResponseAsync(order.Id);
        }

```

**Defense notes**
- `_stateMachine.ResolveTarget` is the single place transitions are decided (FR-X1) — no controller and no screen contains a status rule.
- `EnsureNotAnotherDriversOrder` means a driver cannot advance a ride that belongs to a colleague, even with a hand-crafted request.
- Every transition writes an `OrderStatusHistory` row with who, when and why, which is the audit trail NFR-16/NFR-17 ask for.

## 20. Completing a ride counts towards the shift

**What it does.** Finishing a ride stamps it completed, increments the ride counter on the driver's open shift, and frees the driver to receive new requests.

**Why we built it this way.** The shift counter is what the company's shift report shows, and it has to move in the same transaction as the completion so the two can never disagree.

**How it works.** Status, timestamp, shift counter and audit row are written inside one transaction; afterwards a `Completed` signal is recorded and the passenger is notified.

**Code**

`PickMeUp.Services/OrderService.cs:270-301`
```csharp
        public async Task<OrderResponse> CompleteAsync(int id)
        {
            var driver = await RequireEligibleDriverAsync();
            var order = await LoadForTransitionAsync(id);

            EnsureOrderIsOfferedTo(order, driver);
            EnsureNotAnotherDriversOrder(order, driver, id);

            var from = CurrentStatus(order);
            var target = _stateMachine.ResolveTarget(OrderTransition.Complete, from);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            order.CompletedAt = DateTime.UtcNow;
            ApplyStatus(order, target);

            await IncrementShiftOrderCountAsync(driver.Id);
            await RecordHistoryAsync(order.Id, from, target, driver.UserId, "Ride completed");

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            if (order.TaxiCompanyId.HasValue)
            {
                await _interactions.RecordAsync(
                    order.PassengerId, order.TaxiCompanyId.Value, InteractionSignalType.Completed, 1m, order.Id);
            }

            await _dispatcher.OrderStatusChangedAsync(order.Id, from.ToString(), target.ToString());

            return await LoadResponseAsync(order.Id);
        }
```

**Defense notes**
- Completing is what releases the driver: the one-ride-per-driver condition in section 6 tests for `Confirmed` or `InProgress`, so the moment the status becomes `Completed` new requests start arriving again.
- No fare is computed here — the price was fixed when the order was placed, which is **Eldar's** pay-before-dispatch design; this method only records that the ride happened.
- The `Completed` interaction signal feeds **Eldar's** recommender; it is written here because this is where the fact becomes true.
