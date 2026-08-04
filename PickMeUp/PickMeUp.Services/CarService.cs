using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using PickMeUp.Services.Storage;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public class CarService
        : BaseCompanyScopedService<Car, CarResponse, CarSearch, CarInsertRequest, CarUpdateRequest>,
          ICarService
    {
        private const string PhotoStorageArea = "car-photos";

        private readonly IImageFileStore _imageFileStore;

        public CarService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<CarInsertRequest> insertValidator,
            IValidator<CarUpdateRequest> updateValidator,
            IAuthenticatedUserAccessor userAccessor,
            IImageFileStore imageFileStore)
            : base(dbContext, mapper, insertValidator, updateValidator, userAccessor)
        {
            _imageFileStore = imageFileStore;
        }

        protected override Task<IQueryable<Car>> IncludeRelatedEntitiesAsync(CarSearch? search, IQueryable<Car> query)
        {
            IQueryable<Car> result = query.Include(c => c.VehicleCategory).Include(c => c.TaxiCompany);
            return Task.FromResult(result);
        }

        protected override IQueryable<Car> ApplyCompanyScope(IQueryable<Car> query, int companyId) =>
            query.Where(c => c.TaxiCompanyId == companyId);

        protected override int GetOwningCompanyId(Car entity) => entity.TaxiCompanyId;

        protected override IQueryable<Car> ApplyEntityFilters(IQueryable<Car> query, CarSearch? search)
        {
            if (search == null)
            {
                return query;
            }

            if (!string.IsNullOrWhiteSpace(search.Plate))
            {
                var pattern = $"%{EscapeLikePattern(search.Plate)}%";
                query = query.Where(c => EF.Functions.ILike(c.Plate, pattern));
            }

            if (!string.IsNullOrWhiteSpace(search.Model))
            {
                var pattern = $"%{EscapeLikePattern(search.Model)}%";
                query = query.Where(c => EF.Functions.ILike(c.Model, pattern)
                                      || EF.Functions.ILike(c.Make, pattern));
            }

            if (search.VehicleCategoryId.HasValue)
            {
                query = query.Where(c => c.VehicleCategoryId == search.VehicleCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.CategoryName))
            {
                var pattern = $"%{EscapeLikePattern(search.CategoryName)}%";
                query = query.Where(c => EF.Functions.ILike(c.VehicleCategory.Name, pattern));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == search.IsActive.Value);
            }

            return query;
        }

        public override async Task<CarResponse> InsertAsync(CarInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var companyId = await RequireCallerCompanyIdAsync();
            await EnsureCategoryExistsAsync(request.VehicleCategoryId);

            var car = new Car
            {
                Make = request.Make,
                Model = request.Model,
                Plate = request.Plate,
                Year = request.Year,
                Seats = request.Seats,
                VehicleCategoryId = request.VehicleCategoryId,
                TaxiCompanyId = companyId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Cars.Add(car);
            await SaveTranslatingDuplicatePlateAsync(request.Plate);

            await LoadNamesAsync(car);

            return _mapper.Map<CarResponse>(car);
        }

        public override async Task<CarResponse> UpdateAsync(int id, CarUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var car = await LoadOwnedCarAsync(id);

            if (request.VehicleCategoryId.HasValue)
            {
                await EnsureCategoryExistsAsync(request.VehicleCategoryId.Value);
            }

            MapUpdateRequestToEntity(request, car);
            car.UpdatedAt = DateTime.UtcNow;

            await SaveTranslatingDuplicatePlateAsync(request.Plate ?? car.Plate);
            await LoadNamesAsync(car);

            return _mapper.Map<CarResponse>(car);
        }

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

        public async Task<string> StorePhotoAsync(int id, Stream content, string fileName, string contentType)
        {
            var car = await LoadOwnedCarAsync(id);

            var storedPath = await _imageFileStore.SaveAsync(PhotoStorageArea, id, content, fileName, contentType);

            car.PhotoImagePath = storedPath;
            car.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return storedPath;
        }

        public async Task<(Stream Content, string ContentType, string FileName)> GetPhotoAsync(int id)
        {
            var car = await LoadOwnedCarAsync(id);

            if (string.IsNullOrWhiteSpace(car.PhotoImagePath))
            {
                throw new KeyNotFoundException($"Car with id {id} has no photo.");
            }

            return _imageFileStore.Open(car.PhotoImagePath);
        }

        private async Task<Car> LoadOwnedCarAsync(int id)
        {
            var car = await _dbContext.Cars
                .Include(c => c.VehicleCategory)
                .Include(c => c.TaxiCompany)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException($"Car with id {id} not found.");

            if (!CallerIsAdmin)
            {
                var companyId = await GetCallerCompanyIdAsync();

                if (companyId == null || car.TaxiCompanyId != companyId.Value)
                {
                    throw new KeyNotFoundException($"Car with id {id} not found.");
                }
            }

            return car;
        }

        private async Task SaveTranslatingDuplicatePlateAsync(string plate)
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation())
            {
                throw new BusinessException($"A vehicle with registration plate '{plate}' already exists.");
            }
        }

        private async Task EnsureCategoryExistsAsync(int categoryId)
        {
            if (!await _dbContext.VehicleCategories.AnyAsync(c => c.Id == categoryId))
            {
                throw new BusinessException("The selected vehicle category does not exist. Pick one from the list.");
            }
        }

        private async Task LoadNamesAsync(Car car)
        {
            await _dbContext.Entry(car).Reference(c => c.VehicleCategory).LoadAsync();
            await _dbContext.Entry(car).Reference(c => c.TaxiCompany).LoadAsync();
        }
    }
}
