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
        private readonly IPaymentService _payments;
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
            IPaymentService payments,
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
            _payments = payments;
            _insertValidator = insertValidator;
            _cancelValidator = cancelValidator;
        }

        public async Task<OrderResponse> CreateAsync(OrderInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var userId = RequireUserId();

            var cityId = await ResolveNearestCityIdAsync(request.PickupLat, request.PickupLng);

            await RequireOperatingCompanyAsync(request.TaxiCompanyId, cityId);

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

            var companyName = await RequireOperatingCompanyAsync(request.TaxiCompanyId, cityId);

            return new OrderQuoteResponse
            {
                DistanceKm = probe.DistanceKm,
                Fare = await ComputeFareAsync(probe),
                IsGeneralOrder = request.TaxiCompanyId == null,
                CompanyName = companyName
            };
        }

        public async Task<OrderResponse> AcceptAsync(int id, OrderAcceptRequest request)
        {
            var driver = await RequireEligibleDriverAsync();
            var order = await LoadForTransitionAsync(id);

            await _shiftService.EnsureDriverMayTakeNewWorkAsync(driver.Id);

            EnsureOrderIsOfferedTo(order, driver);
            await EnsureDriverIsFreeAsync(driver.Id);

            var target = _stateMachine.ResolveTarget(OrderTransition.Accept, CurrentStatus(order));

            EnsureOrderIsAcceptableBy(order, driver);

            var carId = request.CarId ?? await ResolveAssignedCarIdAsync(driver.Id);

            if (carId == null)
            {
                throw new BusinessException("You have no vehicle assigned. Ask your company to assign one first.");
            }

            await EnsureCarIsAssignedToDriverAsync(carId.Value, driver);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var from = CurrentStatus(order);
            var companyWasUnassigned = order.TaxiCompanyId == null;
            var acceptedAt = DateTime.UtcNow;

            var claimed = await _dbContext.Orders
                .Where(DispatchRules.AcceptableBy(driver.Id, driver.TaxiCompanyId, driver.TaxiCompany?.CityId))
                .Where(o => o.Id == order.Id)
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

            var refund = RefundOutcome.NotNeeded;

            if (order.IsPrepaid && from != OrderStatusEnum.Completed)
            {
                refund = await _payments.RefundCancelledOrderAsync(order.Id, $"Ride cancelled: {request.Reason}");
            }

            await _dispatcher.OrderCancelledAsync(order.Id, request.Reason, userId);

            var response = await LoadResponseAsync(order.Id);
            response.RefundStatus = refund.ToString();

            return response;
        }

        public async Task<RideDriverLocationResponse?> GetRideDriverLocationAsync(int id)
        {
            var userId = RequireUserId();

            var ride = await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.PassengerId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.DriverId,
                    o.OrderStatusId,
                    DriverName = o.Driver == null ? null : o.Driver.User.FirstName + " " + o.Driver.User.LastName,
                    CarPlate = o.Car == null ? null : o.Car.Plate
                })
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Order with id {id} not found.");

            if (ride.DriverId == null || !DispatchRules.ActiveRideStatuses.Contains(ride.OrderStatusId))
            {
                return null;
            }

            var location = await _dbContext.DriverLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.DriverId == ride.DriverId.Value);

            if (location == null)
            {
                return null;
            }

            return new RideDriverLocationResponse
            {
                OrderId = ride.Id,
                DriverName = ride.DriverName,
                CarPlate = ride.CarPlate,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                UpdatedAt = location.UpdatedAt
            };
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

            await EnsureVisibleAsync(order, id);

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

                return query.Where(DispatchRules.VisibleToDriver(driverId, companyId, cityId));
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
            var operationalDriverIds = _shiftService.QueryDriverIdsAllowedNewWork();

            return query
                .Where(DispatchRules.AcceptableBy(driverId, companyId, cityId))
                .Where(o => eligibleDriverIds.Contains(driverId)
                    && operationalDriverIds.Contains(driverId)
                    && !_dbContext.Orders.Any(active => active.DriverId == driverId
                        && DispatchRules.ActiveRideStatuses.Contains(active.OrderStatusId)));
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
            return await _dbContext.CarAssignments
                .Where(DispatchRules.AssignmentActiveAt(DateTime.UtcNow))
                .Where(a => a.DriverId == driverId)
                .Select(a => (int?)a.CarId)
                .FirstOrDefaultAsync();
        }

        private async Task EnsureCarIsAssignedToDriverAsync(int carId, Driver driver)
        {
            var assigned = await _dbContext.CarAssignments
                .Where(DispatchRules.AssignmentActiveAt(DateTime.UtcNow))
                .AnyAsync(a => a.DriverId == driver.Id && a.CarId == carId);

            if (!assigned)
            {
                throw new BusinessException(
                    "That vehicle is not assigned to you right now. Ask your company to assign it first.");
            }
        }

        private static void EnsureOrderIsOfferedTo(Order order, Driver driver)
        {
            var offered = DispatchRules.IsOfferedTo(
                order.TaxiCompanyId, order.CityId, driver.TaxiCompanyId, driver.TaxiCompany?.CityId);

            if (!offered)
            {
                throw new KeyNotFoundException($"Order with id {order.Id} not found.");
            }
        }

        private static void EnsureOrderIsAcceptableBy(Order order, Driver driver)
        {
            var acceptable = DispatchRules.IsAcceptableBy(
                order, driver.Id, driver.TaxiCompanyId, driver.TaxiCompany?.CityId);

            if (acceptable)
            {
                return;
            }

            if (!order.IsPrepaid)
            {
                throw new BusinessException(
                    "This ride has not been paid for yet, so it cannot be accepted.");
            }

            if (order.Rejections.Any(r => r.DriverId == driver.Id))
            {
                throw new BusinessException("You have already declined this ride.");
            }

            throw new BusinessException("This order has already been accepted by a driver.");
        }

        private async Task EnsureDriverIsFreeAsync(int driverId)
        {
            var onAnotherRide = await _dbContext.Orders.AnyAsync(DispatchRules.ActiveRideOf(driverId));

            if (onAnotherRide)
            {
                throw new BusinessException("You are already on a ride. Finish it before accepting another.");
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
                if (order.OrderStatusId != (int)OrderStatusEnum.Pending || order.DriverId != null)
                {
                    throw new BusinessException(
                        "A driver has already taken this ride, so it can no longer be cancelled here. "
                        + "Contact support if you need it stopped.");
                }

                return;
            }

            var isAssignedDriver = order.DriverId != null
                && await _dbContext.Drivers.AnyAsync(d => d.Id == order.DriverId && d.UserId == userId);

            if (!isAssignedDriver)
            {
                throw new KeyNotFoundException($"Order with id {id} not found.");
            }
        }

        private async Task<string?> RequireOperatingCompanyAsync(int? companyId, int? pickupCityId)
        {
            if (companyId == null)
            {
                return null;
            }

            var company = await _dbContext.TaxiCompanies
                .Where(t => t.Id == companyId.Value && t.IsActive)
                .Select(t => new { t.Name, t.CityId })
                .FirstOrDefaultAsync()
                ?? throw new BusinessException("The selected taxi company is not available.");

            if (pickupCityId.HasValue && company.CityId != pickupCityId.Value)
            {
                throw new BusinessException("This company does not operate in the pickup city.");
            }

            return company.Name;
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
                .Include(d => d.TaxiCompany)
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

        private async Task EnsureVisibleAsync(Order order, int id)
        {
            var scope = _scope!;

            if (scope.IsAdmin
                || (scope.PassengerUserId.HasValue && order.PassengerId == scope.PassengerUserId.Value)
                || (scope.CompanyId.HasValue && order.TaxiCompanyId == scope.CompanyId.Value))
            {
                return;
            }

            if (scope.DriverId.HasValue)
            {
                var visibleToDriver = await _dbContext.Orders
                    .Where(DispatchRules.VisibleToDriver(
                        scope.DriverId.Value, scope.DriverCompanyId!.Value, scope.DriverCompanyCityId))
                    .AnyAsync(o => o.Id == id);

                if (visibleToDriver)
                {
                    return;
                }
            }

            throw new KeyNotFoundException($"Order with id {id} not found.");
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
