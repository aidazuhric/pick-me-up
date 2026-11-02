using PickMeUp.Model.Constants;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using PickMeUp.Services.Orders;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PickMeUp.Services
{
    public class ShiftService : BaseReadService<Shift, ShiftResponse, ShiftSearch>, IShiftService
    {
        private readonly IAuthenticatedUserAccessor _userAccessor;

        private int? _scopedDriverId;
        private int? _scopedCompanyId;
        private bool _scopeResolved;

        public ShiftService(
            MapsterMapper.IMapper mapper,
            PickMeUpDbContext dbContext,
            IAuthenticatedUserAccessor userAccessor)
            : base(mapper, dbContext)
        {
            _userAccessor = userAccessor;
        }

        public static Expression<Func<Shift, bool>> OpenShift =>
            shift => shift.EndedAt == null;

        public static Expression<Func<Shift, bool>> DispatchEligibleShift =>
            shift => shift.EndedAt == null && !shift.Breaks.Any(b => b.EndedAt == null);

        public IQueryable<int> QueryDispatchEligibleDriverIds() =>
            _dbContext.Shifts.Where(DispatchEligibleShift).Select(s => s.DriverId).Distinct();

        public Task<bool> IsDriverDispatchEligibleAsync(int driverId) =>
            _dbContext.Shifts.Where(s => s.DriverId == driverId).AnyAsync(DispatchEligibleShift);

        public IQueryable<int> QueryDriverIdsAllowedNewWork() =>
            _dbContext.Drivers.Where(DispatchRules.OperationalDriver).Select(d => d.Id);

        public async Task EnsureDriverMayTakeNewWorkAsync(int driverId)
        {
            var allowed = await QueryDriverIdsAllowedNewWork().AnyAsync(id => id == driverId);

            if (!allowed)
            {
                throw new BusinessException(
                    "Your driver account is not active. Ask your company to reactivate it before taking work.");
            }
        }

        public async Task<DriverEligibilityResponse> GetEligibilityAsync(int driverId)
        {
            await ResolveScopeAsync();

            var driver = await ScopedDrivers()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == driverId)
                ?? throw new KeyNotFoundException($"Driver with id {driverId} not found.");

            var openShift = await _dbContext.Shifts
                .Include(s => s.Breaks)
                .Where(s => s.DriverId == driverId)
                .FirstOrDefaultAsync(OpenShift);

            var onBreak = openShift != null && openShift.Breaks.Any(b => b.EndedAt == null);
            var eligible = openShift != null && !onBreak;

            return new DriverEligibilityResponse
            {
                DriverId = driverId,
                DriverName = $"{driver.User.FirstName} {driver.User.LastName}",
                HasOpenShift = openShift != null,
                IsOnBreak = onBreak,
                IsDispatchEligible = eligible,
                Reason = eligible
                    ? "On shift and available."
                    : openShift == null
                        ? "No open shift."
                        : "Currently on a break."
            };
        }

        public async Task<ShiftResponse> StartShiftAsync()
        {
            var driverId = await RequireCallerDriverIdAsync();

            await EnsureDriverMayTakeNewWorkAsync(driverId);

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

            await EnsureNoActiveRideAsync(shift.DriverId, "stopping your shift");

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

            await EnsureNoActiveRideAsync(shift.DriverId, "taking a break");

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

        protected override Task<IQueryable<Shift>> IncludeRelatedEntitiesAsync(ShiftSearch? search, IQueryable<Shift> query)
        {
            IQueryable<Shift> result = query
                .Include(s => s.Breaks)
                .Include(s => s.Driver).ThenInclude(d => d.User)
                .Include(s => s.Driver).ThenInclude(d => d.TaxiCompany);

            return Task.FromResult(result);
        }

        protected override IQueryable<Shift> ApplyFilters(IQueryable<Shift> query, ShiftSearch? search)
        {
            query = ApplyReadScope(query);

            if (search == null)
            {
                return query;
            }

            if (search.DriverId.HasValue)
            {
                query = query.Where(s => s.DriverId == search.DriverId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.DriverName))
            {
                var pattern = $"%{EscapeLikePattern(search.DriverName)}%";
                query = query.Where(s => EF.Functions.ILike(s.Driver.User.FirstName, pattern)
                                      || EF.Functions.ILike(s.Driver.User.LastName, pattern));
            }

            if (!string.IsNullOrWhiteSpace(search.CompanyName))
            {
                var pattern = $"%{EscapeLikePattern(search.CompanyName)}%";
                query = query.Where(s => EF.Functions.ILike(s.Driver.TaxiCompany.Name, pattern));
            }

            if (search.OngoingOnly == true)
            {
                query = query.Where(OpenShift);
            }

            var startedFrom = search.StartedFrom ?? PeriodStart(search.Period);
            if (startedFrom.HasValue)
            {
                query = query.Where(s => s.StartedAt >= startedFrom.Value);
            }

            if (search.StartedTo.HasValue)
            {
                query = query.Where(s => s.StartedAt <= search.StartedTo.Value);
            }

            return query;
        }

        public override async Task<PageResult<ShiftResponse>> GetAllAsync(ShiftSearch? search = null)
        {
            await ResolveScopeAsync();
            return await base.GetAllAsync(search);
        }

        public override async Task<ShiftResponse> GetByIdAsync(int id)
        {
            await ResolveScopeAsync();

            var shift = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"Shift with id {id} not found.");

            EnsureVisible(shift, id);

            return _mapper.Map<ShiftResponse>(shift);
        }

        private static DateTime? PeriodStart(ShiftPeriod? period) => period switch
        {
            ShiftPeriod.Day => DateTime.UtcNow.AddDays(-1),
            ShiftPeriod.Week => DateTime.UtcNow.AddDays(-7),
            ShiftPeriod.Month => DateTime.UtcNow.AddMonths(-1),
            _ => null
        };

        private IQueryable<Driver> ScopedDrivers()
        {
            IQueryable<Driver> drivers = _dbContext.Drivers;

            if (_userAccessor.IsInRole(Roles.Admin))
            {
                return drivers;
            }

            if (_scopedDriverId.HasValue)
            {
                return drivers.Where(d => d.Id == _scopedDriverId.Value);
            }

            if (_scopedCompanyId.HasValue)
            {
                return drivers.Where(d => d.TaxiCompanyId == _scopedCompanyId.Value);
            }

            return drivers.Where(d => false);
        }

        private IQueryable<Shift> ApplyReadScope(IQueryable<Shift> query)
        {
            if (_userAccessor.IsInRole(Roles.Admin))
            {
                return query;
            }

            if (_scopedDriverId.HasValue)
            {
                return query.Where(s => s.DriverId == _scopedDriverId.Value);
            }

            if (_scopedCompanyId.HasValue)
            {
                return query.Where(s => s.Driver.TaxiCompanyId == _scopedCompanyId.Value);
            }

            return query.Where(s => false);
        }

        private void EnsureVisible(Shift shift, int id)
        {
            if (_userAccessor.IsInRole(Roles.Admin))
            {
                return;
            }

            var visible = _scopedDriverId.HasValue
                ? shift.DriverId == _scopedDriverId.Value
                : _scopedCompanyId.HasValue && shift.Driver.TaxiCompanyId == _scopedCompanyId.Value;

            if (!visible)
            {
                throw new KeyNotFoundException($"Shift with id {id} not found.");
            }
        }

        private async Task ResolveScopeAsync()
        {
            if (_scopeResolved)
            {
                return;
            }

            var userId = _userAccessor.GetUserId();

            if (userId != null)
            {
                _scopedDriverId = await _dbContext.Drivers
                    .Where(d => d.UserId == userId.Value && d.ResignedAt == null)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();

                if (_scopedDriverId == null)
                {
                    _scopedCompanyId = await _dbContext.TaxiCompanies
                        .Where(t => t.UserId == userId.Value)
                        .Select(t => (int?)t.Id)
                        .FirstOrDefaultAsync();
                }
            }

            _scopeResolved = true;
        }

        private async Task<int> RequireCallerDriverIdAsync()
        {
            var userId = _userAccessor.GetUserId();

            var driverId = userId == null
                ? null
                : await _dbContext.Drivers
                    .Where(d => d.UserId == userId.Value && d.ResignedAt == null)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();

            return driverId ?? throw new BusinessException("This account is not an active driver.");
        }

        private async Task EnsureNoActiveRideAsync(int driverId, string action)
        {
            var onRide = await _dbContext.Orders.AnyAsync(DispatchRules.ActiveRideOf(driverId));

            if (onRide)
            {
                throw new BusinessException($"Finish your active ride before {action}.");
            }
        }

        private async Task<Shift> LoadOwnedShiftForWriteAsync(int id)
        {
            var driverId = await RequireCallerDriverIdAsync();

            var shift = await _dbContext.Shifts
                .Include(s => s.Breaks)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Shift with id {id} not found.");

            if (shift.DriverId != driverId)
            {
                throw new KeyNotFoundException($"Shift with id {id} not found.");
            }

            return shift;
        }

        private async Task<ShiftResponse> LoadResponseAsync(int id)
        {
            var shift = await _dbContext.Shifts
                .AsNoTracking()
                .Include(s => s.Breaks)
                .Include(s => s.Driver).ThenInclude(d => d.User)
                .Include(s => s.Driver).ThenInclude(d => d.TaxiCompany)
                .FirstAsync(s => s.Id == id);

            return _mapper.Map<ShiftResponse>(shift);
        }
    }
}
