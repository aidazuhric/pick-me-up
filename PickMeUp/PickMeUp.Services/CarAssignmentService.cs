using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public class CarAssignmentService
        : BaseCompanyScopedService<CarAssignment, CarAssignmentResponse, CarAssignmentSearch, CarAssignmentInsertRequest, CarAssignmentUpdateRequest>,
          ICarAssignmentService
    {
        private static readonly DateTime OpenEnded = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        public CarAssignmentService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<CarAssignmentInsertRequest> insertValidator,
            IValidator<CarAssignmentUpdateRequest> updateValidator,
            IAuthenticatedUserAccessor userAccessor)
            : base(dbContext, mapper, insertValidator, updateValidator, userAccessor)
        {
        }

        protected override Task<IQueryable<CarAssignment>> IncludeRelatedEntitiesAsync(
            CarAssignmentSearch? search, IQueryable<CarAssignment> query)
        {
            IQueryable<CarAssignment> result = query
                .Include(a => a.Car).ThenInclude(c => c.VehicleCategory)
                .Include(a => a.Car).ThenInclude(c => c.TaxiCompany)
                .Include(a => a.Driver).ThenInclude(d => d.User);

            return Task.FromResult(result);
        }

        protected override IQueryable<CarAssignment> ApplyCompanyScope(IQueryable<CarAssignment> query, int companyId) =>
            query.Where(a => a.Car.TaxiCompanyId == companyId);

        protected override int GetOwningCompanyId(CarAssignment entity) => entity.Car.TaxiCompanyId;

        protected override IQueryable<CarAssignment> ApplyEntityFilters(
            IQueryable<CarAssignment> query, CarAssignmentSearch? search)
        {
            if (search == null)
            {
                return query;
            }

            if (search.CarId.HasValue)
            {
                query = query.Where(a => a.CarId == search.CarId.Value);
            }

            if (search.DriverId.HasValue)
            {
                query = query.Where(a => a.DriverId == search.DriverId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Plate))
            {
                var pattern = $"%{EscapeLikePattern(search.Plate)}%";
                query = query.Where(a => EF.Functions.ILike(a.Car.Plate, pattern));
            }

            if (!string.IsNullOrWhiteSpace(search.DriverName))
            {
                var pattern = $"%{EscapeLikePattern(search.DriverName)}%";
                query = query.Where(a => EF.Functions.ILike(a.Driver.User.FirstName, pattern)
                                      || EF.Functions.ILike(a.Driver.User.LastName, pattern));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(a => a.IsActive == search.IsActive.Value);
            }

            if (search.OngoingOnly == true)
            {
                query = query.Where(a => a.AssignedTo == null);
            }

            if (search.ActiveFrom.HasValue)
            {
                var from = search.ActiveFrom.Value;
                query = query.Where(a => (a.AssignedTo ?? OpenEnded) > from);
            }

            if (search.ActiveTo.HasValue)
            {
                var to = search.ActiveTo.Value;
                query = query.Where(a => a.AssignedFrom < to);
            }

            return query;
        }

        public override async Task<CarAssignmentResponse> InsertAsync(CarAssignmentInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var companyId = await RequireCallerCompanyIdAsync();

            await EnsureCarBelongsToCompanyAsync(request.CarId, companyId);
            await EnsureDriverBelongsToCompanyAsync(request.DriverId, companyId);

            await EnsureNoOverlapAsync(request.CarId, request.DriverId, request.AssignedFrom, request.AssignedTo, null);

            var assignment = new CarAssignment
            {
                CarId = request.CarId,
                DriverId = request.DriverId,
                AssignedFrom = request.AssignedFrom,
                AssignedTo = request.AssignedTo,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.CarAssignments.Add(assignment);
            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(assignment.Id);
        }

        public override async Task<CarAssignmentResponse> UpdateAsync(int id, CarAssignmentUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var assignment = await LoadOwnedAssignmentAsync(id);
            var companyId = await GetCallerCompanyIdAsync() ?? assignment.Car.TaxiCompanyId;

            var carId = request.CarId ?? assignment.CarId;
            var driverId = request.DriverId ?? assignment.DriverId;
            var from = request.AssignedFrom ?? assignment.AssignedFrom;
            var to = request.AssignedTo ?? assignment.AssignedTo;

            if (to.HasValue && to.Value <= from)
            {
                throw new BusinessException("End of the assignment must be later than its start.");
            }

            if (request.CarId.HasValue)
            {
                await EnsureCarBelongsToCompanyAsync(carId, companyId);
            }

            if (request.DriverId.HasValue)
            {
                await EnsureDriverBelongsToCompanyAsync(driverId, companyId);
            }

            await EnsureNoOverlapAsync(carId, driverId, from, to, id);

            assignment.CarId = carId;
            assignment.DriverId = driverId;
            assignment.AssignedFrom = from;
            assignment.AssignedTo = to;

            if (request.IsActive.HasValue)
            {
                assignment.IsActive = request.IsActive.Value;
            }

            assignment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(assignment.Id);
        }

        public async Task<CarAssignmentResponse> CloseAsync(int id)
        {
            var assignment = await LoadOwnedAssignmentAsync(id);

            if (assignment.AssignedTo != null)
            {
                throw new BusinessException("This assignment already has an end date.");
            }

            assignment.AssignedTo = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(assignment.Id);
        }

        private async Task EnsureNoOverlapAsync(int carId, int driverId, DateTime from, DateTime? to, int? excludeId)
        {
            var end = to ?? OpenEnded;

            var carConflict = await _dbContext.CarAssignments
                .Include(a => a.Driver).ThenInclude(d => d.User)
                .Where(a => a.IsActive && a.CarId == carId)
                .Where(a => excludeId == null || a.Id != excludeId.Value)
                .Where(a => a.AssignedFrom < end && from < (a.AssignedTo ?? OpenEnded))
                .FirstOrDefaultAsync();

            if (carConflict != null)
            {
                throw new BusinessException(
                    $"This car is already assigned to {carConflict.Driver.User.FirstName} {carConflict.Driver.User.LastName} " +
                    $"for an overlapping period starting {carConflict.AssignedFrom:yyyy-MM-dd}.");
            }

            var driverConflict = await _dbContext.CarAssignments
                .Include(a => a.Car)
                .Where(a => a.IsActive && a.DriverId == driverId)
                .Where(a => excludeId == null || a.Id != excludeId.Value)
                .Where(a => a.AssignedFrom < end && from < (a.AssignedTo ?? OpenEnded))
                .FirstOrDefaultAsync();

            if (driverConflict != null)
            {
                throw new BusinessException(
                    $"This driver already has vehicle {driverConflict.Car.Plate} assigned " +
                    $"for an overlapping period starting {driverConflict.AssignedFrom:yyyy-MM-dd}.");
            }
        }

        private async Task EnsureCarBelongsToCompanyAsync(int carId, int companyId)
        {
            var owned = await _dbContext.Cars.AnyAsync(c => c.Id == carId && c.TaxiCompanyId == companyId);

            if (!owned)
            {
                throw new KeyNotFoundException($"Car with id {carId} not found.");
            }
        }

        private async Task EnsureDriverBelongsToCompanyAsync(int driverId, int companyId)
        {
            var owned = await _dbContext.Drivers.AnyAsync(d => d.Id == driverId && d.TaxiCompanyId == companyId);

            if (!owned)
            {
                throw new KeyNotFoundException($"Driver with id {driverId} not found.");
            }
        }

        private async Task<CarAssignment> LoadOwnedAssignmentAsync(int id)
        {
            var assignment = await _dbContext.CarAssignments
                .Include(a => a.Car)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new KeyNotFoundException($"CarAssignment with id {id} not found.");

            if (!CallerIsAdmin)
            {
                var companyId = await GetCallerCompanyIdAsync();

                if (companyId == null || assignment.Car.TaxiCompanyId != companyId.Value)
                {
                    throw new KeyNotFoundException($"CarAssignment with id {id} not found.");
                }
            }

            return assignment;
        }

        private async Task<CarAssignmentResponse> LoadResponseAsync(int id)
        {
            var assignment = await _dbContext.CarAssignments
                .AsNoTracking()
                .Include(a => a.Car).ThenInclude(c => c.VehicleCategory)
                .Include(a => a.Car).ThenInclude(c => c.TaxiCompany)
                .Include(a => a.Driver).ThenInclude(d => d.User)
                .FirstAsync(a => a.Id == id);

            return _mapper.Map<CarAssignmentResponse>(assignment);
        }
    }
}
