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

            var existing = await _dbContext.Payments
                .Where(p => p.OrderId == order.Id)
                .FirstOrDefaultAsync(p => p.IsPaid || p.StripePaymentIntentId != null);

            if (existing != null)
            {
                if (existing.IsPaid)
                {
                    throw new BusinessException(AlreadyPaidMessage);
                }

                var state = await _stripeGateway.ReadIntentAsync(existing.StripePaymentIntentId!);

                if (state != null && state.IsSucceeded)
                {
                    _logger.LogWarning(
                        "Intent {IntentId} for order {OrderId} already succeeded at Stripe but was never confirmed. "
                        + "Settling it now instead of charging a second time.",
                        state.PaymentIntentId, order.Id);

                    await SettleAsync(existing);

                    throw new BusinessException(AlreadyPaidMessage);
                }

                if (state != null && state.IsReusable)
                {
                    _logger.LogInformation(
                        "Reusing open intent {IntentId} for order {OrderId}.", state.PaymentIntentId, order.Id);

                    return new PaymentIntentResponse
                    {
                        PaymentId = existing.Id,
                        OrderId = order.Id,
                        AmountCharged = existing.AmountCharged,
                        Currency = existing.Currency,
                        ClientSecret = state.ClientSecret
                    };
                }

                var replacement = await _stripeGateway.CreateIntentAsync(amount, existing.Currency, order.Id);

                _logger.LogInformation(
                    "Intent {PreviousIntentId} for order {OrderId} is no longer usable; replacing it with {IntentId}.",
                    existing.StripePaymentIntentId, order.Id, replacement.PaymentIntentId);

                existing.AmountCharged = amount;
                existing.StripePaymentIntentId = replacement.PaymentIntentId;
                existing.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return new PaymentIntentResponse
                {
                    PaymentId = existing.Id,
                    OrderId = order.Id,
                    AmountCharged = existing.AmountCharged,
                    Currency = existing.Currency,
                    ClientSecret = replacement.ClientSecret
                };
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                AmountCharged = amount,
                Currency = "BAM",
                PaymentStatusId = (int)PaymentStatusEnum.Unpaid,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            var intent = await _stripeGateway.CreateIntentAsync(amount, payment.Currency, order.Id);

            payment.StripePaymentIntentId = intent.PaymentIntentId;
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Started intent {IntentId} for order {OrderId} ({Amount} {Currency}).",
                intent.PaymentIntentId, order.Id, amount, payment.Currency);

            return new PaymentIntentResponse
            {
                PaymentId = payment.Id,
                OrderId = order.Id,
                AmountCharged = payment.AmountCharged,
                Currency = payment.Currency,
                ClientSecret = intent.ClientSecret
            };
        }

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

            return await _dbContext.Payments
                .Where(p => p.OrderId == stripeEvent.OrderId.Value)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();
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
        }

        public async Task<PaymentResponse> RefundAsync(int id, PaymentRefundRequest request)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Payment with id {id} not found.");

            if (!payment.IsPaid)
            {
                throw new BusinessException("This payment has not been charged, so there is nothing to refund.");
            }

            var alreadyRefunded = payment.Refunds.Sum(r => r.RefundedAmount);
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

            var stripeRefund = await _stripeGateway.RefundAsync(
                payment.StripePaymentIntentId ?? string.Empty, amount, request.Reason);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.PaymentRefunds.Add(new PaymentRefund
            {
                PaymentId = payment.Id,
                RefundedAmount = stripeRefund.Amount,
                StripeRefundId = stripeRefund.RefundId,
                Reason = request.Reason,
                RefundedAt = DateTime.UtcNow
            });

            var refundedTotal = alreadyRefunded + stripeRefund.Amount;

            payment.PaymentStatusId = refundedTotal >= payment.AmountCharged
                ? (int)PaymentStatusEnum.Refunded
                : (int)PaymentStatusEnum.PartiallyRefunded;
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _dispatcher.PaymentRefundedAsync(payment.Id, stripeRefund.Amount);

            return await LoadResponseAsync(payment.Id);
        }

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

            payment.PaymentStatusId = (int)PaymentStatusEnum.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _dispatcher.PaymentRefundedAsync(payment.Id, stripeRefund.Amount);

            return true;
        }

        public async Task<PaymentResponse> GetByOrderAsync(int orderId)
        {
            await ResolveScopeAsync();

            var query = await IncludeRelatedEntitiesAsync(null, _dbContext.Payments.AsNoTracking());

            var payment = await query.FirstOrDefaultAsync(p => p.OrderId == orderId)
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
                .Where(o => !_dbContext.Payments.Any(p => p.OrderId == o.Id && (p.IsPaid || p.PaymentStatusId != (int)PaymentStatusEnum.Unpaid)))
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
