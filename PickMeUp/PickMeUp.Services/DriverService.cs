using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Constants;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace PickMeUp.Services
{
    public class DriverService
        : BaseCompanyScopedService<Driver, DriverResponse, DriverSearch, DriverInsertRequest, DriverUpdateRequest>,
          IDriverService
    {
        private readonly ICryptoService _cryptoService;

        public DriverService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<DriverInsertRequest> insertValidator,
            IValidator<DriverUpdateRequest> updateValidator,
            IAuthenticatedUserAccessor userAccessor,
            ICryptoService cryptoService)
            : base(dbContext, mapper, insertValidator, updateValidator, userAccessor)
        {
            _cryptoService = cryptoService;
        }

        protected override Task<IQueryable<Driver>> IncludeRelatedEntitiesAsync(
            DriverSearch? search, IQueryable<Driver> query)
        {
            IQueryable<Driver> result = query.Include(d => d.User).Include(d => d.TaxiCompany);
            return Task.FromResult(result);
        }

        protected override IQueryable<Driver> ApplyCompanyScope(IQueryable<Driver> query, int companyId) =>
            query.Where(d => d.TaxiCompanyId == companyId);

        protected override int GetOwningCompanyId(Driver entity) => entity.TaxiCompanyId;

        protected override IQueryable<Driver> ApplyEntityFilters(IQueryable<Driver> query, DriverSearch? search)
        {
            if (search == null)
            {
                return query.Where(d => d.ResignedAt == null);
            }

            if (search.IncludeResigned != true)
            {
                query = query.Where(d => d.ResignedAt == null);
            }

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                var pattern = $"%{EscapeLikePattern(search.Name)}%";
                query = query.Where(d => EF.Functions.ILike(d.User.FirstName, pattern)
                                      || EF.Functions.ILike(d.User.LastName, pattern));
            }

            if (!string.IsNullOrWhiteSpace(search.CompanyName))
            {
                var pattern = $"%{EscapeLikePattern(search.CompanyName)}%";
                query = query.Where(d => EF.Functions.ILike(d.TaxiCompany.Name, pattern));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(d => d.IsActive == search.IsActive.Value);
            }

            return query;
        }

        public override async Task<DriverResponse> InsertAsync(DriverInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var companyId = await RequireCallerCompanyIdAsync();

            await EnsureLoginIsFreeAsync(request.LoginUsername, request.Email);

            var generatedPassword = string.IsNullOrWhiteSpace(request.LoginPassword)
                ? GenerateTemporaryPassword()
                : null;
            var password = generatedPassword ?? request.LoginPassword!;

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
                PasswordHash = _cryptoService.GenerateHash(password, salt),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = driverRole.Id,
                DateAssigned = DateTime.UtcNow
            });

            var driver = new Driver
            {
                UserId = user.Id,
                TaxiCompanyId = companyId,
                HireDate = request.HireDate,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Drivers.Add(driver);

            try
            {
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation())
            {
                await transaction.RollbackAsync();
                throw new BusinessException("That login is already taken. Nothing was created.");
            }

            await _dbContext.Entry(driver).Reference(d => d.User).LoadAsync();
            await _dbContext.Entry(driver).Reference(d => d.TaxiCompany).LoadAsync();

            var response = _mapper.Map<DriverResponse>(driver);
            response.InitialLoginUsername = user.Username;
            response.InitialLoginPassword = generatedPassword;

            return response;
        }

        public override async Task<DriverResponse> UpdateAsync(int id, DriverUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var driver = await LoadOwnedDriverAsync(id);

            if (request.FirstName != null)
            {
                driver.User.FirstName = request.FirstName;
            }

            if (request.LastName != null)
            {
                driver.User.LastName = request.LastName;
            }

            if (request.PhoneNumber != null)
            {
                driver.User.PhoneNumber = request.PhoneNumber;
            }

            if (request.HireDate.HasValue)
            {
                driver.HireDate = request.HireDate.Value;
            }

            if (request.IsActive.HasValue)
            {
                driver.IsActive = request.IsActive.Value;
            }

            driver.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<DriverResponse>(driver);
        }

        public override async Task DeleteAsync(int id)
        {
            var driver = await LoadOwnedDriverAsync(id);

            await EnsureDriverHasNoOpenShiftAsync(id);
            await EnsureDriverHasNoActiveOrderAsync(id);
            EnsureDriverHasResigned(driver);
            await EnsureDriverHasNoAssignmentsAsync(id);

            await base.DeleteAsync(id);
        }

        public async Task<DriverResponse> ResignAsync(int id)
        {
            await EnsureDriverMayOnlyResignSelfAsync(id);

            var driver = await LoadOwnedDriverAsync(id);

            if (driver.ResignedAt != null)
            {
                throw new BusinessException("This driver has already resigned.");
            }

            await EnsureDriverHasNoOpenShiftAsync(id);
            await EnsureDriverHasNoActiveOrderAsync(id);

            driver.ResignedAt = DateTime.UtcNow;
            driver.IsActive = false;
            driver.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return _mapper.Map<DriverResponse>(driver);
        }

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

        public Task<List<DriverLocationResponse>> GetFleetLocationsAsync()
        {
            return QueryFleetLocations(null);
        }

        private async Task<List<DriverLocationResponse>> QueryFleetLocations(
            System.Linq.Expressions.Expression<Func<DriverLocation, bool>>? extra)
        {
            IQueryable<DriverLocation> query = _dbContext.DriverLocations.AsNoTracking();

            if (!CallerIsAdmin)
            {
                var companyId = await GetCallerCompanyIdAsync();

                if (companyId == null)
                {
                    return new List<DriverLocationResponse>();
                }

                query = query.Where(l => l.Driver.TaxiCompanyId == companyId.Value);
            }

            if (extra != null)
            {
                query = query.Where(extra);
            }

            var openShifts = _dbContext.Shifts.Where(s => s.EndedAt == null);

            return await query
                .Select(l => new DriverLocationResponse
                {
                    DriverId = l.DriverId,
                    DriverName = l.Driver.User.FirstName + " " + l.Driver.User.LastName,
                    CompanyName = l.Driver.TaxiCompany.Name,
                    CarPlate = _dbContext.CarAssignments
                        .Where(a => a.DriverId == l.DriverId && a.IsActive && a.AssignedTo == null)
                        .Select(a => a.Car.Plate)
                        .FirstOrDefault(),
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    UpdatedAt = l.UpdatedAt,
                    IsOnShift = openShifts.Any(s => s.DriverId == l.DriverId),
                    IsOnBreak = openShifts.Any(s => s.DriverId == l.DriverId
                        && s.Breaks.Any(b => b.EndedAt == null)),
                    IsDispatchEligible = openShifts.Any(s => s.DriverId == l.DriverId
                        && !s.Breaks.Any(b => b.EndedAt == null))
                })
                .OrderBy(l => l.DriverName)
                .ToListAsync();
        }

        private async Task<int> RequireCallerDriverIdAsync()
        {
            var userId = _userAccessor.GetUserId()
                ?? throw new BusinessException("The caller could not be identified.");

            var driverId = await _dbContext.Drivers
                .Where(d => d.UserId == userId && d.ResignedAt == null)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();

            return driverId
                ?? throw new BusinessException("This account is not an active driver.");
        }

        private async Task EnsureDriverMayOnlyResignSelfAsync(int id)
        {
            if (!_userAccessor.IsInRole(Roles.Driver) || CallerIsAdmin)
            {
                return;
            }

            var userId = _userAccessor.GetUserId();

            var ownDriverId = userId == null
                ? null
                : await _dbContext.Drivers
                    .Where(d => d.UserId == userId.Value)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();

            if (ownDriverId != id)
            {
                throw new KeyNotFoundException($"Driver with id {id} not found.");
            }
        }

        private async Task<Driver> LoadOwnedDriverAsync(int id)
        {
            var driver = await _dbContext.Drivers
                .Include(d => d.User)
                .Include(d => d.TaxiCompany)
                .FirstOrDefaultAsync(d => d.Id == id)
                ?? throw new KeyNotFoundException($"Driver with id {id} not found.");

            if (!CallerIsAdmin)
            {
                var companyId = await GetCallerCompanyIdAsync();

                if (companyId == null || driver.TaxiCompanyId != companyId.Value)
                {
                    throw new KeyNotFoundException($"Driver with id {id} not found.");
                }
            }

            return driver;
        }

        private async Task EnsureDriverHasNoOpenShiftAsync(int driverId)
        {
            var onShift = await _dbContext.Shifts.AnyAsync(s => s.DriverId == driverId && s.EndedAt == null);

            if (onShift)
            {
                throw new BusinessException(
                    "This driver is currently on an open shift. Wait for the shift to end before removing them.");
            }
        }

        private async Task EnsureDriverHasNoActiveOrderAsync(int driverId)
        {
            var busy = await _dbContext.Orders.AnyAsync(o => o.DriverId == driverId
                && (o.OrderStatusId == (int)OrderStatusEnum.Confirmed
                    || o.OrderStatusId == (int)OrderStatusEnum.InProgress));

            if (busy)
            {
                throw new BusinessException(
                    "This driver has a ride in progress. Wait for it to finish before removing them.");
            }
        }

        private async Task EnsureDriverHasNoAssignmentsAsync(int driverId)
        {
            var assigned = await _dbContext.CarAssignments.AnyAsync(a => a.DriverId == driverId);

            if (assigned)
            {
                throw new BusinessException(
                    "This driver still has a vehicle assigned and cannot be removed. Remove their assignments first.");
            }
        }

        private static void EnsureDriverHasResigned(Driver driver)
        {
            if (driver.ResignedAt == null)
            {
                throw new BusinessException(
                    "This driver is still employed. Record their resignation before removing them.");
            }
        }

        private async Task EnsureLoginIsFreeAsync(string username, string email)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == username))
            {
                throw new BusinessException($"Username '{username}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Email == email))
            {
                throw new BusinessException($"Email '{email}' is already in use.");
            }
        }

        private static string GenerateTemporaryPassword() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(9)).Replace("/", "x").Replace("+", "y");
    }
}
