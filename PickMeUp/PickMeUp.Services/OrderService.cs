using PickMeUp.Model.Constants;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using PickMeUp.Services.Orders;
using PickMeUp.Services.Recommendations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public class OrderService : BaseReadService<Order, OrderResponse, OrderSearch>, IOrderService
    {
        private readonly IAuthenticatedUserAccessor _userAccessor;
        private readonly IOrderStateMachine _stateMachine;
        private readonly IFareCalculator _fareCalculator;
        private readonly IShiftService _shiftService;
        private readonly IOrderNotificationDispatcher _dispatcher;
        private readonly IInteractionRecorder _interactions;
        private readonly IValidator<OrderInsertRequest> _insertValidator;
        private readonly IValidator<OrderCancelRequest> _cancelValidator;

        private OrderScope? _scope;

        public OrderService(
            MapsterMapper.IMapper mapper,
            PickMeUpDbContext dbContext,
            IAuthenticatedUserAccessor userAccessor,
            IOrderStateMachine stateMachine,
            IFareCalculator fareCalculator,
            IShiftService shiftService,
            IOrderNotificationDispatcher dispatcher,
            IInteractionRecorder interactions,
            IValidator<OrderInsertRequest> insertValidator,
            IValidator<OrderCancelRequest> cancelValidator)
            : base(mapper, dbContext)
        {
            _userAccessor = userAccessor;
            _stateMachine = stateMachine;
            _fareCalculator = fareCalculator;
            _shiftService = shiftService;
            _dispatcher = dispatcher;
            _interactions = interactions;
            _insertValidator = insertValidator;
            _cancelValidator = cancelValidator;
        }

        public async Task<OrderResponse> CreateAsync(OrderInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var userId = RequireUserId();

            var cityId = await ResolveNearestCityIdAsync(request.PickupLat, request.PickupLng);

            if (request.TaxiCompanyId.HasValue)
            {
                var company = await _dbContext.TaxiCompanies
                    .Where(t => t.Id == request.TaxiCompanyId.Value && t.IsActive)
                    .Select(t => new { t.Id, t.CityId })
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    throw new BusinessException("The selected taxi company is not available.");
                }

                if (cityId.HasValue && company.CityId != cityId.Value)
                {
                    throw new BusinessException("This company does not operate in the pickup city.");
                }
            }

            var order = new Order
            {
                PassengerId = userId,
                CityId = cityId,
                TaxiCompanyId = request.TaxiCompanyId,
                PickupLat = request.PickupLat,
                PickupLng = request.PickupLng,
                PickupAddress = request.PickupAddress,
                DestinationLat = request.DestinationLat,
                DestinationLng = request.DestinationLng,
                DestinationAddress = request.DestinationAddress,
                OrderStatusId = (int)OrderStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };

            order.DistanceKm = DeriveDistanceKm(order);

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

        public async Task<OrderResponse> CompleteAsync(int id)
        {
            var driver = await RequireEligibleDriverAsync();
            var order = await LoadForTransitionAsync(id);

            EnsureOrderIsOfferedTo(order, driver);
            EnsureNotAnotherDriversOrder(order, driver, id);

            var from = CurrentStatus(order);
            var target = _stateMachine.ResolveTarget(OrderTransition.Complete, from);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            order.DistanceKm = DeriveDistanceKm(order);
            order.Fare = await ComputeFareAsync(order);
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

        public async Task<OrderResponse> CancelAsync(int id, OrderCancelRequest request)
        {
            await _cancelValidator.ValidateAndThrowAsync(request);
            _stateMachine.EnsureCancellationReason(request.Reason);

            var userId = RequireUserId();
            var order = await LoadForTransitionAsync(id);

            await EnsureCanCancelAsync(order, userId, id);

            var from = CurrentStatus(order);
            var target = _stateMachine.ResolveTarget(OrderTransition.Cancel, from);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            order.CancelledAt = DateTime.UtcNow;
            order.CancellationReason = request.Reason;
            ApplyStatus(order, target);

            await RecordHistoryAsync(order.Id, from, target, userId, request.Reason);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _dispatcher.OrderCancelledAsync(order.Id, request.Reason, userId);

            return await LoadResponseAsync(order.Id);
        }

        public async Task<bool> DriverHasActiveOrderAsync(int driverId) =>
            await _dbContext.Orders.AnyAsync(o => o.DriverId == driverId
                && (o.OrderStatusId == (int)OrderStatusEnum.Confirmed
                    || o.OrderStatusId == (int)OrderStatusEnum.InProgress));

        protected override Task<IQueryable<Order>> IncludeRelatedEntitiesAsync(OrderSearch? search, IQueryable<Order> query)
        {
            IQueryable<Order> result = query
                .Include(o => o.Passenger)
                .Include(o => o.OrderStatus)
                .Include(o => o.TaxiCompany)
                .Include(o => o.Car)
                .Include(o => o.Driver).ThenInclude(d => d!.User)
                .Include(o => o.StatusHistory).ThenInclude(h => h.ToStatus)
                .Include(o => o.StatusHistory).ThenInclude(h => h.FromStatus)
                .Include(o => o.StatusHistory).ThenInclude(h => h.ChangedByUser);

            return Task.FromResult(result);
        }

        protected override IQueryable<Order> ApplyFilters(IQueryable<Order> query, OrderSearch? search)
        {
            query = ApplyReadScope(query, search);

            if (search == null)
            {
                return query;
            }

            if (search.Status.HasValue)
            {
                query = query.Where(o => o.OrderStatusId == (int)search.Status.Value);
            }

            if (search.TaxiCompanyId.HasValue)
            {
                query = query.Where(o => o.TaxiCompanyId == search.TaxiCompanyId.Value);
            }

            if (search.DriverId.HasValue)
            {
                query = query.Where(o => o.DriverId == search.DriverId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.PassengerName))
            {
                var pattern = $"%{EscapeLikePattern(search.PassengerName)}%";
                query = query.Where(o => EF.Functions.ILike(o.Passenger.FirstName, pattern)
                                      || EF.Functions.ILike(o.Passenger.LastName, pattern));
            }

            var createdFrom = search.CreatedFrom ?? PeriodStart(search.Period);
            if (createdFrom.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= createdFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= search.CreatedTo.Value);
            }

            return query;
        }

        public override async Task<PageResult<OrderResponse>> GetAllAsync(OrderSearch? search = null)
        {
            await ResolveScopeAsync();
            return await base.GetAllAsync(search);
        }

        public override async Task<OrderResponse> GetByIdAsync(int id)
        {
            await ResolveScopeAsync();

            var order = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"Order with id {id} not found.");

            EnsureVisible(order, id);

            return _mapper.Map<OrderResponse>(order);
        }

        private IQueryable<Order> ApplyReadScope(IQueryable<Order> query, OrderSearch? search)
        {
            var scope = _scope!;

            if (scope.IsAdmin)
            {
                return query;
            }

            if (scope.PassengerUserId.HasValue && !scope.DriverId.HasValue && !scope.CompanyId.HasValue)
            {
                return query.Where(o => o.PassengerId == scope.PassengerUserId.Value);
            }

            if (scope.DriverId.HasValue)
            {
                var driverId = scope.DriverId.Value;
                var companyId = scope.DriverCompanyId!.Value;
                var cityId = scope.DriverCompanyCityId;

                if (search?.AvailableForMe == true)
                {
                    return QueryAvailableFor(query, driverId, companyId, cityId);
                }

                return query.Where(o => o.DriverId == driverId
                    || (o.OrderStatusId == (int)OrderStatusEnum.Pending
                        && (o.TaxiCompanyId == null || o.TaxiCompanyId == companyId)
                        && (o.CityId == null || o.CityId == cityId)
                        && !o.Rejections.Any(r => r.DriverId == driverId)));
            }

            if (scope.CompanyId.HasValue)
            {
                return query.Where(o => o.TaxiCompanyId == scope.CompanyId.Value);
            }

            return query.Where(o => false);
        }

        private IQueryable<Order> QueryAvailableFor(IQueryable<Order> query, int driverId, int companyId, int? cityId)
        {
            var eligibleDriverIds = _shiftService.QueryDispatchEligibleDriverIds();

            return query.Where(o => o.OrderStatusId == (int)OrderStatusEnum.Pending
                && o.DriverId == null
                && (o.TaxiCompanyId == null || o.TaxiCompanyId == companyId)
                && (o.CityId == null || o.CityId == cityId)
                && !o.Rejections.Any(r => r.DriverId == driverId)
                && eligibleDriverIds.Contains(driverId));
        }

        private static DateTime? PeriodStart(ShiftPeriod? period) => period switch
        {
            ShiftPeriod.Day => DateTime.UtcNow.AddDays(-1),
            ShiftPeriod.Week => DateTime.UtcNow.AddDays(-7),
            ShiftPeriod.Month => DateTime.UtcNow.AddMonths(-1),
            _ => null
        };

        private static OrderStatusEnum CurrentStatus(Order order) => (OrderStatusEnum)order.OrderStatusId;

        private static void ApplyStatus(Order order, OrderStatusEnum status)
        {
            order.OrderStatusId = (int)status;
            order.UpdatedAt = DateTime.UtcNow;
        }

        private Task RecordHistoryAsync(int orderId, OrderStatusEnum? from, OrderStatusEnum to, int userId, string? reason)
        {
            _dbContext.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                FromStatusId = from.HasValue ? (int)from.Value : null,
                ToStatusId = (int)to,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Reason = reason
            });

            return Task.CompletedTask;
        }

        private async Task<decimal> ComputeFareAsync(Order order)
        {
            var companyId = order.TaxiCompanyId
                ?? throw new BusinessException("The order has no company, so no price list applies.");

            var pricing = await _dbContext.TaxiCompanies
                .Where(t => t.Id == companyId)
                .Select(t => new { t.StartingPrice, t.PricePerKm })
                .FirstOrDefaultAsync()
                ?? throw new BusinessException("The company price list could not be found.");

            return _fareCalculator.Calculate(pricing.StartingPrice, pricing.PricePerKm, order.DistanceKm);
        }

        private async Task IncrementShiftOrderCountAsync(int driverId)
        {
            var shift = await _dbContext.Shifts
                .Where(s => s.DriverId == driverId && s.EndedAt == null)
                .FirstOrDefaultAsync();

            if (shift != null)
            {
                shift.OrdersHandled += 1;
                shift.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task<int?> ResolveAssignedCarIdAsync(int driverId)
        {
            var now = DateTime.UtcNow;

            return await _dbContext.CarAssignments
                .Where(a => a.DriverId == driverId && a.IsActive)
                .Where(a => a.AssignedFrom <= now && (a.AssignedTo == null || a.AssignedTo > now))
                .Select(a => (int?)a.CarId)
                .FirstOrDefaultAsync();
        }

        private async Task EnsureCarBelongsToDriverAsync(int carId, Driver driver)
        {
            var ok = await _dbContext.Cars.AnyAsync(c => c.Id == carId && c.TaxiCompanyId == driver.TaxiCompanyId);

            if (!ok)
            {
                throw new BusinessException("That vehicle does not belong to your company.");
            }
        }

        private static void EnsureOrderIsOfferedTo(Order order, Driver driver)
        {
            var offered = order.TaxiCompanyId == null || order.TaxiCompanyId == driver.TaxiCompanyId;

            if (!offered)
            {
                throw new KeyNotFoundException($"Order with id {order.Id} not found.");
            }
        }

        private static void EnsureNotAnotherDriversOrder(Order order, Driver driver, int id)
        {
            if (order.DriverId != null && order.DriverId != driver.Id)
            {
                throw new KeyNotFoundException($"Order with id {id} not found.");
            }
        }

        private async Task EnsureCanCancelAsync(Order order, int userId, int id)
        {
            if (_userAccessor.IsInRole(Roles.Admin))
            {
                return;
            }

            if (order.PassengerId == userId)
            {
                return;
            }

            var isAssignedDriver = order.DriverId != null
                && await _dbContext.Drivers.AnyAsync(d => d.Id == order.DriverId && d.UserId == userId);

            if (!isAssignedDriver)
            {
                throw new KeyNotFoundException($"Order with id {id} not found.");
            }
        }

        private async Task<int?> ResolveNearestCityIdAsync(double latitude, double longitude)
        {
            var cities = await _dbContext.Cities
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.Latitude, c.Longitude })
                .ToListAsync();

            if (cities.Count == 0)
            {
                return null;
            }

            return cities
                .OrderBy(c => GeoDistance.KilometresBetween(latitude, longitude, c.Latitude, c.Longitude))
                .First()
                .Id;
        }

        private decimal? DeriveDistanceKm(Order order)
        {
            if (!order.DestinationLat.HasValue || !order.DestinationLng.HasValue)
            {
                return null;
            }

            var kilometres = GeoDistance.KilometresBetween(
                order.PickupLat, order.PickupLng, order.DestinationLat.Value, order.DestinationLng.Value);

            return _fareCalculator.RoundDistance((decimal)kilometres);
        }

        private async Task<Order> LoadForTransitionAsync(int id) =>
            await _dbContext.Orders
                .Include(o => o.Rejections)
                .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Order with id {id} not found.");

        private async Task<Driver> RequireEligibleDriverAsync()
        {
            var userId = RequireUserId();

            var driver = await _dbContext.Drivers
                .FirstOrDefaultAsync(d => d.UserId == userId && d.ResignedAt == null)
                ?? throw new BusinessException("This account is not an active driver.");

            if (!await _shiftService.IsDriverDispatchEligibleAsync(driver.Id))
            {
                throw new BusinessException(
                    "You are not on an active shift, or you are on a break. Start your shift to take orders.");
            }

            return driver;
        }

        private int RequireUserId() =>
            _userAccessor.GetUserId() ?? throw new BusinessException("The caller could not be identified.");

        private void EnsureVisible(Order order, int id)
        {
            var scope = _scope!;

            if (scope.IsAdmin)
            {
                return;
            }

            var visible =
                (scope.PassengerUserId.HasValue && order.PassengerId == scope.PassengerUserId.Value)
                || (scope.DriverId.HasValue && (order.DriverId == scope.DriverId.Value
                    || (order.OrderStatusId == (int)OrderStatusEnum.Pending
                        && (order.TaxiCompanyId == null || order.TaxiCompanyId == scope.DriverCompanyId))))
                || (scope.CompanyId.HasValue && order.TaxiCompanyId == scope.CompanyId.Value);

            if (!visible)
            {
                throw new KeyNotFoundException($"Order with id {id} not found.");
            }
        }

        private async Task ResolveScopeAsync()
        {
            if (_scope != null)
            {
                return;
            }

            var userId = _userAccessor.GetUserId();
            var scope = new OrderScope { IsAdmin = _userAccessor.IsInRole(Roles.Admin) };

            if (userId != null && !scope.IsAdmin)
            {
                var driver = await _dbContext.Drivers
                    .Where(d => d.UserId == userId.Value && d.ResignedAt == null)
                    .Select(d => new { d.Id, d.TaxiCompanyId, CityId = (int?)d.TaxiCompany.CityId })
                    .FirstOrDefaultAsync();

                if (driver != null)
                {
                    scope.DriverId = driver.Id;
                    scope.DriverCompanyId = driver.TaxiCompanyId;
                    scope.DriverCompanyCityId = driver.CityId;
                }
                else
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
            }

            _scope = scope;
        }

        private async Task<OrderResponse> LoadResponseAsync(int id)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .Include(o => o.Passenger)
                .Include(o => o.OrderStatus)
                .Include(o => o.TaxiCompany)
                .Include(o => o.Car)
                .Include(o => o.Driver).ThenInclude(d => d!.User)
                .Include(o => o.StatusHistory).ThenInclude(h => h.ToStatus)
                .Include(o => o.StatusHistory).ThenInclude(h => h.FromStatus)
                .Include(o => o.StatusHistory).ThenInclude(h => h.ChangedByUser)
                .FirstAsync(o => o.Id == id);

            return _mapper.Map<OrderResponse>(order);
        }

        private sealed class OrderScope
        {
            public bool IsAdmin { get; set; }
            public int? PassengerUserId { get; set; }
            public int? DriverId { get; set; }
            public int? DriverCompanyId { get; set; }
            public int? DriverCompanyCityId { get; set; }
            public int? CompanyId { get; set; }
        }
    }
}
