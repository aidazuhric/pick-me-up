using System.Linq.Expressions;
using PickMeUp.Model.Constants;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using PickMeUp.Services.Orders;
using PickMeUp.Services.Payments;
using PickMeUp.Common.Services.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PickMeUp.Services
{
    public class PaymentService : BaseReadService<Payment, PaymentResponse, PaymentSearch>, IPaymentService
    {
        private const string PaymentSucceededEvent = "payment_intent.succeeded";
        private const string AlreadyPaidMessage = "This ride has already been paid for.";

        private readonly IAuthenticatedUserAccessor _userAccessor;
        private readonly IStripeGateway _stripeGateway;
        private readonly IPaymentNotificationDispatcher _dispatcher;
        private readonly IOrderNotificationDispatcher _orderDispatcher;
        private readonly StripeOptions _stripeOptions;
        private readonly ILogger<PaymentService> _logger;

        private PaymentScope? _scope;

        public PaymentService(
            MapsterMapper.IMapper mapper,
            PickMeUpDbContext dbContext,
            IAuthenticatedUserAccessor userAccessor,
            IStripeGateway stripeGateway,
            IPaymentNotificationDispatcher dispatcher,
            IOrderNotificationDispatcher orderDispatcher,
            IOptions<StripeOptions> stripeOptions,
            ILogger<PaymentService> logger)
            : base(mapper, dbContext)
        {
            _userAccessor = userAccessor;
            _stripeGateway = stripeGateway;
            _dispatcher = dispatcher;
            _orderDispatcher = orderDispatcher;
            _stripeOptions = stripeOptions.Value;
            _logger = logger;
        }

        public PaymentConfigResponse GetConfiguration()
        {
            return new PaymentConfigResponse
            {
                PublishableKey = _stripeOptions.PublishableKey,
                IsLiveGateway = !string.IsNullOrWhiteSpace(_stripeOptions.SecretKey)
            };
        }

        public async Task<PaymentIntentResponse> CreateIntentAsync(PaymentIntentRequest request)
        {
            var userId = RequireUserId();

            var order = await _dbContext.Orders
                .Include(o => o.TaxiCompany)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId)
                ?? throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");

            if (!_userAccessor.IsInRole(Roles.Admin) && order.PassengerId != userId)
            {
                throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");
            }

            if (order.IsPrepaid)
            {
                throw new BusinessException(AlreadyPaidMessage);
            }

            if (order.OrderStatusId != (int)OrderStatusEnum.Pending)
            {
                throw new BusinessException("Only a ride that is still waiting for a driver can be paid for.");
            }

            var amount = order.Fare
                ?? throw new BusinessException("This ride has no fare yet, so it cannot be paid for.");

            var payment = await ClaimPaymentForAsync(order.Id, amount);

            if (payment.IsPaid)
            {
                throw new BusinessException(AlreadyPaidMessage);
            }

            var decision = await DecideIntentAsync(payment, order.Id, amount);

            if (decision.AlreadySucceeded)
            {
                _logger.LogWarning(
                    "The intent for order {OrderId} already succeeded at Stripe but was never confirmed. "
                    + "Settling it now instead of charging a second time.",
                    order.Id);

                await SettleAsync(payment);

                throw new BusinessException(AlreadyPaidMessage);
            }

            return Describe(payment, decision.ClientSecret!);
        }

        private async Task<IntentDecision> DecideIntentAsync(Payment payment, int orderId, decimal amount)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await LockPaymentAsync(payment.Id);
                await _dbContext.Entry(payment).ReloadAsync();

                if (payment.IsPaid)
                {
                    await transaction.CommitAsync();
                    return IntentDecision.Succeeded;
                }

                if (payment.StripePaymentIntentId != null)
                {
                    var state = await _stripeGateway.ReadIntentAsync(payment.StripePaymentIntentId);

                    if (state != null && state.IsSucceeded)
                    {
                        await transaction.CommitAsync();
                        return IntentDecision.Succeeded;
                    }

                    if (state != null && state.IsReusable)
                    {
                        _logger.LogInformation(
                            "Reusing open intent {IntentId} for order {OrderId}.",
                            state.PaymentIntentId, orderId);

                        await transaction.CommitAsync();
                        return IntentDecision.Open(state.ClientSecret);
                    }
                }

                StripeIntent intent;

                try
                {
                    intent = await StartIntentAsync(payment, orderId, amount);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    await MarkIntentFailedAsync(payment, orderId);
                    throw;
                }

                await transaction.CommitAsync();

                return IntentDecision.Open(intent.ClientSecret);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        private Task LockPaymentAsync(int paymentId) =>
            _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT \"Id\" FROM \"Payments\" WHERE \"Id\" = {0} FOR UPDATE", paymentId);

        private async Task MarkIntentFailedAsync(Payment payment, int orderId)
        {
            payment.PaymentStatusId = (int)PaymentStatusEnum.Failed;
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogError(
                "Stripe would not start a payment for order {OrderId}; payment {PaymentId} is marked failed "
                + "so the passenger can try again without a stale row in the way.",
                orderId, payment.Id);
        }

        private sealed class IntentDecision
        {
            public bool AlreadySucceeded { get; private init; }

            public string? ClientSecret { get; private init; }

            public static IntentDecision Succeeded => new() { AlreadySucceeded = true };

            public static IntentDecision Open(string clientSecret) => new() { ClientSecret = clientSecret };
        }

        private async Task<Payment> ClaimPaymentForAsync(int orderId, decimal amount)
        {
            var existing = await ReadCurrentPaymentAsync(LivePaymentFor(orderId));

            if (existing != null)
            {
                return existing;
            }

            var payment = new Payment
            {
                OrderId = orderId,
                AmountCharged = amount,
                Currency = "BAM",
                PaymentStatusId = (int)PaymentStatusEnum.Unpaid,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                _dbContext.Entry(payment).State = EntityState.Detached;

                var winner = await ReadCurrentPaymentAsync(LivePaymentFor(orderId));

                if (winner == null)
                {
                    throw;
                }

                _logger.LogInformation(exception,
                    "Another request was already starting a payment for order {OrderId}; "
                    + "continuing with payment {PaymentId}.",
                    orderId, winner.Id);

                return winner;
            }

            return payment;
        }

        private async Task<StripeIntent> StartIntentAsync(Payment payment, int orderId, decimal amount)
        {
            payment.IntentAttempt += 1;
            payment.AmountCharged = amount;
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var key = $"order-{orderId}-payment-{payment.Id}-attempt-{payment.IntentAttempt}";

            StripeIntent intent;

            try
            {
                intent = await _stripeGateway.CreateIntentAsync(amount, payment.Currency, orderId, key);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Stripe refused to start a payment for order {OrderId} on attempt {Attempt}.",
                    orderId, payment.IntentAttempt);

                throw;
            }

            _logger.LogInformation(
                "Started intent {IntentId} for order {OrderId} ({Amount} {Currency}), attempt {Attempt}.",
                intent.PaymentIntentId, orderId, amount, payment.Currency, payment.IntentAttempt);

            payment.StripePaymentIntentId = intent.PaymentIntentId;
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return intent;
        }

        private static Expression<Func<Payment, bool>> LivePaymentFor(int orderId) =>
            payment => payment.OrderId == orderId
                && payment.PaymentStatusId != (int)PaymentStatusEnum.Refunded
                && payment.PaymentStatusId != (int)PaymentStatusEnum.Failed;

        private Task<Payment?> ReadCurrentPaymentAsync(Expression<Func<Payment, bool>> predicate) =>
            _dbContext.Payments
                .Include(p => p.Refunds)
                .Where(predicate)
                .OrderByDescending(p => p.IsPaid)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync();

        private static PaymentIntentResponse Describe(Payment payment, string clientSecret) =>
            new()
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                AmountCharged = payment.AmountCharged,
                Currency = payment.Currency,
                ClientSecret = clientSecret
            };

        public async Task<bool> HandleWebhookAsync(string payload, string signatureHeader)
        {
            StripeWebhookEvent stripeEvent;

            try
            {
                stripeEvent = _stripeGateway.ReadWebhook(payload, signatureHeader);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Rejected a Stripe webhook: the signature could not be verified. "
                    + "Check that STRIPE_WEBHOOK_SECRET matches the secret of the listener that sent it.");
                throw;
            }

            _logger.LogInformation(
                "Stripe webhook accepted: signature valid, type {Type}, intent {IntentId}, order {OrderId}.",
                stripeEvent.Type, stripeEvent.PaymentIntentId, stripeEvent.OrderId);

            if (stripeEvent.Type != PaymentSucceededEvent)
            {
                _logger.LogInformation("Ignoring Stripe event {Type}", stripeEvent.Type);
                return false;
            }

            var payment = await FindPaymentForEventAsync(stripeEvent);

            if (payment == null)
            {
                _logger.LogWarning(
                    "No payment matches Stripe intent {IntentId} or order {OrderId}; nothing was confirmed.",
                    stripeEvent.PaymentIntentId, stripeEvent.OrderId);
                return false;
            }

            if (payment.IsPaid)
            {
                _logger.LogInformation(
                    "Payment {PaymentId} for order {OrderId} is already paid; webhook replay had no effect.",
                    payment.Id, payment.OrderId);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(stripeEvent.PaymentIntentId)
                && payment.StripePaymentIntentId != stripeEvent.PaymentIntentId)
            {
                _logger.LogWarning(
                    "Payment {PaymentId} was recorded against intent {StoredIntentId} but order {OrderId} was paid "
                    + "with {IntentId}; settling against the intent that actually charged.",
                    payment.Id, payment.StripePaymentIntentId, payment.OrderId, stripeEvent.PaymentIntentId);

                payment.StripePaymentIntentId = stripeEvent.PaymentIntentId;
            }

            await SettleAsync(payment);

            _logger.LogInformation(
                "Confirmed payment {PaymentId}: order {OrderId} is prepaid and visible to drivers.",
                payment.Id, payment.OrderId);

            return true;
        }

        private async Task<Payment?> FindPaymentForEventAsync(StripeWebhookEvent stripeEvent)
        {
            if (!string.IsNullOrWhiteSpace(stripeEvent.PaymentIntentId))
            {
                var byIntent = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripeEvent.PaymentIntentId);

                if (byIntent != null)
                {
                    return byIntent;
                }
            }

            if (stripeEvent.OrderId == null)
            {
                return null;
            }

            return await ReadCurrentPaymentAsync(p => p.OrderId == stripeEvent.OrderId!.Value);
        }

        private async Task SettleAsync(Payment payment)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var paidAt = DateTime.UtcNow;

            payment.IsPaid = true;
            payment.PaymentStatusId = (int)PaymentStatusEnum.Paid;
            payment.PaidAt = paidAt;
            payment.UpdatedAt = paidAt;

            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            var cancelled = order != null && order.OrderStatusId == (int)OrderStatusEnum.Cancelled;

            if (cancelled)
            {
                payment.RefundRequired = true;
            }
            else if (order != null && !order.IsPrepaid)
            {
                order.IsPrepaid = true;
                order.PrepaidAt = paidAt;
                order.UpdatedAt = paidAt;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _dispatcher.PaymentConfirmedAsync(payment.Id);

            if (cancelled)
            {
                _logger.LogWarning(
                    "Payment {PaymentId} confirmed for order {OrderId}, which is already cancelled. "
                    + "The ride stays cancelled and the money is owed back.",
                    payment.Id, payment.OrderId);

                await RefundOwedAsync(payment, "Payment arrived after the ride was cancelled");
                return;
            }

            if (order != null && order.OrderStatusId == (int)OrderStatusEnum.Pending)
            {
                await _orderDispatcher.OrderBecameAvailableAsync(order.Id, excludeDriverId: null);
            }
        }

        public async Task<PaymentResponse> RefundAsync(int id, PaymentRefundRequest request)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Payment with id {id} not found.");

            if (!payment.IsPaid)
            {
                throw new BusinessException("This payment has not been charged, so there is nothing to refund.");
            }

            var alreadyRefunded = await RecordedRefundTotalAsync(payment.Id);
            var remaining = payment.AmountCharged - alreadyRefunded;

            if (remaining <= 0m)
            {
                throw new BusinessException("The full charged amount has already been refunded.");
            }

            var amount = request.Amount ?? remaining;

            if (amount <= 0m)
            {
                throw new BusinessException("The refund amount must be greater than zero.");
            }

            if (amount > remaining)
            {
                throw new BusinessException(
                    $"At most {remaining} {payment.Currency} can still be refunded for this payment.");
            }

            var outcome = await ApplyRefundAsync(payment, alreadyRefunded, amount, request.Reason);

            if (outcome == RefundOutcome.Pending)
            {
                throw new BusinessException(
                    "The refund could not be completed and is still owed. It is recorded and can be retried safely.");
            }

            return await LoadResponseAsync(payment.Id);
        }

        public async Task<RefundOutcome> RefundCancelledOrderAsync(int orderId, string reason)
        {
            var payment = await ReadCurrentPaymentAsync(p => p.OrderId == orderId && p.IsPaid);

            if (payment == null)
            {
                return RefundOutcome.NotNeeded;
            }

            return await RefundOwedAsync(payment, reason);
        }

        private async Task<RefundOutcome> RefundOwedAsync(Payment payment, string reason)
        {
            var alreadyRefunded = await RecordedRefundTotalAsync(payment.Id);
            var remaining = payment.AmountCharged - alreadyRefunded;

            if (remaining <= 0m)
            {
                await ClearRefundOwedAsync(payment);
                return RefundOutcome.Refunded;
            }

            return await ApplyRefundAsync(payment, alreadyRefunded, remaining, reason);
        }

        private async Task<RefundOutcome> ApplyRefundAsync(
            Payment payment, decimal alreadyRefunded, decimal amount, string? reason)
        {
            var key = RefundKey(payment.Id, alreadyRefunded, amount);

            StripeRefund stripeRefund;

            try
            {
                stripeRefund = await _stripeGateway.RefundAsync(
                    payment.StripePaymentIntentId ?? string.Empty, amount, reason, key);
            }
            catch (Exception exception)
            {
                await MarkRefundOwedAsync(payment, exception,
                    "Stripe would not refund {Amount} on payment {PaymentId}");
                return RefundOutcome.Pending;
            }

            try
            {
                await RecordRefundAsync(payment, stripeRefund, reason);
            }
            catch (Exception exception)
            {
                await MarkRefundOwedAsync(payment, exception,
                    "Stripe refunded {Amount} on payment {PaymentId} but it could not be written down");
                return RefundOutcome.Pending;
            }

            await _dispatcher.PaymentRefundedAsync(payment.Id, stripeRefund.Amount);

            return RefundOutcome.Refunded;
        }

        private async Task RecordRefundAsync(Payment payment, StripeRefund stripeRefund, string? reason)
        {
            var alreadyWritten = await _dbContext.PaymentRefunds
                .AnyAsync(r => r.StripeRefundId == stripeRefund.RefundId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (!alreadyWritten)
            {
                _dbContext.PaymentRefunds.Add(new PaymentRefund
                {
                    PaymentId = payment.Id,
                    RefundedAmount = stripeRefund.Amount,
                    StripeRefundId = stripeRefund.RefundId,
                    Reason = reason,
                    RefundedAt = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation(
                    "Stripe refund {RefundId} was already written down for payment {PaymentId}; "
                    + "the retry changed nothing.",
                    stripeRefund.RefundId, payment.Id);
            }

            var refundedTotal = await RecordedRefundTotalAsync(payment.Id);
            var full = refundedTotal >= payment.AmountCharged;

            payment.PaymentStatusId = full
                ? (int)PaymentStatusEnum.Refunded
                : (int)PaymentStatusEnum.PartiallyRefunded;

            if (full)
            {
                payment.RefundRequired = false;
            }

            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task MarkRefundOwedAsync(Payment payment, Exception exception, string message)
        {
            _logger.LogError(exception,
                message + "; the refund is marked as owed so it can be retried without paying twice.",
                payment.AmountCharged, payment.Id);

            payment.RefundRequired = true;
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private async Task ClearRefundOwedAsync(Payment payment)
        {
            if (!payment.RefundRequired)
            {
                return;
            }

            payment.RefundRequired = false;
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private Task<decimal> RecordedRefundTotalAsync(int paymentId) =>
            _dbContext.PaymentRefunds
                .Where(r => r.PaymentId == paymentId)
                .SumAsync(r => r.RefundedAmount);

        private static string RefundKey(int paymentId, decimal alreadyRefunded, decimal amount) =>
            $"payment-{paymentId}-refund-from-{MinorUnits(alreadyRefunded)}-amount-{MinorUnits(amount)}";

        private static long MinorUnits(decimal value) =>
            (long)Math.Round(value * 100m, MidpointRounding.AwayFromZero);

        public async Task<PaymentResponse> GetByOrderAsync(int orderId)
        {
            await ResolveScopeAsync();

            var query = await IncludeRelatedEntitiesAsync(null, _dbContext.Payments.AsNoTracking());

            var payment = await query
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.IsPaid)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No payment found for order {orderId}.");

            EnsureVisible(payment, orderId);

            return _mapper.Map<PaymentResponse>(payment);
        }

        public async Task<List<PayableOrderResponse>> GetPayableOrdersAsync()
        {
            var userId = RequireUserId();

            return await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.PassengerId == userId)
                .Where(o => o.OrderStatusId == (int)OrderStatusEnum.Pending)
                .Where(o => !o.IsPrepaid)
                .Where(o => o.Fare != null)
                .Where(o => !_dbContext.Payments.Any(p => p.OrderId == o.Id
                    && (p.IsPaid
                        || (p.PaymentStatusId != (int)PaymentStatusEnum.Unpaid
                            && p.PaymentStatusId != (int)PaymentStatusEnum.Failed))))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new PayableOrderResponse
                {
                    OrderId = o.Id,
                    CompanyName = o.TaxiCompany == null ? null : o.TaxiCompany.Name,
                    DriverName = o.Driver == null ? null : o.Driver.User.FirstName + " " + o.Driver.User.LastName,
                    PickupAddress = o.PickupAddress,
                    DestinationAddress = o.DestinationAddress,
                    DistanceKm = o.DistanceKm,
                    Fare = o.Fare!.Value,
                    CompletedAt = o.CompletedAt
                })
                .ToListAsync();
        }

        protected override Task<IQueryable<Payment>> IncludeRelatedEntitiesAsync(PaymentSearch? search, IQueryable<Payment> query)
        {
            IQueryable<Payment> result = query
                .Include(p => p.PaymentStatus)
                .Include(p => p.Refunds)
                .Include(p => p.Order).ThenInclude(o => o.Passenger)
                .Include(p => p.Order).ThenInclude(o => o.TaxiCompany)
                .Include(p => p.Order).ThenInclude(o => o.Driver).ThenInclude(d => d!.User);

            return Task.FromResult(result);
        }

        protected override IQueryable<Payment> ApplyFilters(IQueryable<Payment> query, PaymentSearch? search)
        {
            query = ApplyReadScope(query);

            if (search == null)
            {
                return query;
            }

            if (search.OrderId.HasValue)
            {
                query = query.Where(p => p.OrderId == search.OrderId.Value);
            }

            if (search.Status.HasValue)
            {
                query = query.Where(p => p.PaymentStatusId == (int)search.Status.Value);
            }

            if (search.IsPaid.HasValue)
            {
                query = query.Where(p => p.IsPaid == search.IsPaid.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.PassengerName))
            {
                var pattern = $"%{EscapeLikePattern(search.PassengerName)}%";
                query = query.Where(p => EF.Functions.ILike(p.Order.Passenger.FirstName, pattern)
                                      || EF.Functions.ILike(p.Order.Passenger.LastName, pattern));
            }

            var createdFrom = search.CreatedFrom ?? PeriodStart(search.Period);
            if (createdFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= createdFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= search.CreatedTo.Value);
            }

            return query;
        }

        public override async Task<PageResult<PaymentResponse>> GetAllAsync(PaymentSearch? search = null)
        {
            await ResolveScopeAsync();
            return await base.GetAllAsync(search);
        }

        public override async Task<PaymentResponse> GetByIdAsync(int id)
        {
            await ResolveScopeAsync();

            var payment = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"Payment with id {id} not found.");

            EnsureVisible(payment, id);

            return _mapper.Map<PaymentResponse>(payment);
        }

        private IQueryable<Payment> ApplyReadScope(IQueryable<Payment> query)
        {
            var scope = _scope!;

            if (scope.IsAdmin)
            {
                return query;
            }

            if (scope.CompanyId.HasValue)
            {
                return query.Where(p => p.Order.TaxiCompanyId == scope.CompanyId.Value);
            }

            if (scope.PassengerUserId.HasValue)
            {
                return query.Where(p => p.Order.PassengerId == scope.PassengerUserId.Value);
            }

            return query.Where(p => false);
        }

        private void EnsureVisible(Payment payment, int id)
        {
            var scope = _scope!;

            var visible = scope.IsAdmin
                || (scope.CompanyId.HasValue && payment.Order.TaxiCompanyId == scope.CompanyId.Value)
                || (scope.PassengerUserId.HasValue && payment.Order.PassengerId == scope.PassengerUserId.Value);

            if (!visible)
            {
                throw new KeyNotFoundException($"Payment with id {id} not found.");
            }
        }

        private async Task ResolveScopeAsync()
        {
            if (_scope != null)
            {
                return;
            }

            var userId = _userAccessor.GetUserId();
            var scope = new PaymentScope { IsAdmin = _userAccessor.IsInRole(Roles.Admin) };

            if (userId != null && !scope.IsAdmin)
            {
                scope.CompanyId = await _dbContext.TaxiCompanies
                    .Where(t => t.UserId == userId.Value)
                    .Select(t => (int?)t.Id)
                    .FirstOrDefaultAsync();

                if (scope.CompanyId == null)
                {
                    scope.PassengerUserId = userId;
                }
            }

            _scope = scope;
        }

        private static DateTime? PeriodStart(ShiftPeriod? period) => period switch
        {
            ShiftPeriod.Day => DateTime.UtcNow.AddDays(-1),
            ShiftPeriod.Week => DateTime.UtcNow.AddDays(-7),
            ShiftPeriod.Month => DateTime.UtcNow.AddMonths(-1),
            _ => null
        };

        private int RequireUserId() =>
            _userAccessor.GetUserId() ?? throw new BusinessException("The caller could not be identified.");

        private async Task<PaymentResponse> LoadResponseAsync(int id)
        {
            var payment = await _dbContext.Payments
                .AsNoTracking()
                .Include(p => p.PaymentStatus)
                .Include(p => p.Refunds)
                .Include(p => p.Order).ThenInclude(o => o.Passenger)
                .Include(p => p.Order).ThenInclude(o => o.TaxiCompany)
                .Include(p => p.Order).ThenInclude(o => o.Driver).ThenInclude(d => d!.User)
                .FirstAsync(p => p.Id == id);

            return _mapper.Map<PaymentResponse>(payment);
        }

        private sealed class PaymentScope
        {
            public bool IsAdmin { get; set; }
            public int? CompanyId { get; set; }
            public int? PassengerUserId { get; set; }
        }
    }
}
