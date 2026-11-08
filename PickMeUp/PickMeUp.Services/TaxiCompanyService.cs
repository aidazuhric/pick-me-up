using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Constants;
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
    public class TaxiCompanyService
        : BaseCRUDService<TaxiCompany, TaxiCompanyResponse, TaxiCompanySearch, TaxiCompanyInsertRequest, TaxiCompanyUpdateRequest>,
          ITaxiCompanyService
    {
        private const string LogoStorageArea = "company-logos";

        private readonly ICryptoService _cryptoService;
        private readonly IImageFileStore _imageFileStore;
        private readonly IAuthenticatedUserAccessor _userAccessor;

        private int? _passengerHomeCityId;

        public TaxiCompanyService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<TaxiCompanyInsertRequest> insertValidator,
            IValidator<TaxiCompanyUpdateRequest> updateValidator,
            ICryptoService cryptoService,
            IImageFileStore imageFileStore,
            IAuthenticatedUserAccessor userAccessor)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cryptoService = cryptoService;
            _imageFileStore = imageFileStore;
            _userAccessor = userAccessor;
        }

        public async Task<TaxiCompanyResponse> GetOwnAsync()
        {
            var company = await LoadOwnCompanyAsync();

            return _mapper.Map<TaxiCompanyResponse>(company);
        }

        public async Task<TaxiCompanyDetailsResponse> GetDetailsAsync(int id)
        {
            var details = await _dbContext.TaxiCompanies
                .AsNoTracking()
                .Where(t => t.Id == id && t.IsActive)
                .Select(t => new TaxiCompanyDetailsResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    CityName = t.City.Name,
                    HqAddress = t.HqAddress,
                    ContactPhone = t.ContactPhone,
                    StartingPrice = t.StartingPrice,
                    PricePerKm = t.PricePerKm,
                    LogoImageUrl = t.LogoImagePath == null ? null : "TaxiCompanies/" + t.Id + "/Logo",
                    VehicleCount = _dbContext.Cars.Count(c => c.TaxiCompanyId == t.Id && c.IsActive),
                    ReviewCount = _dbContext.Reviews.Count(r => r.TargetCompanyId == t.Id),
                    AverageRating = _dbContext.Reviews
                        .Where(r => r.TargetCompanyId == t.Id)
                        .Average(r => (double?)r.Rating)
                })
                .FirstOrDefaultAsync();

            return details ?? throw new KeyNotFoundException($"Taxi company with id {id} not found.");
        }

        public async Task<TaxiCompanyResponse> UpdateOwnAsync(TaxiCompanyUpdateRequest request)
        {
            request.IsActive = null;

            var company = await LoadOwnCompanyAsync();

            return await UpdateAsync(company.Id, request);
        }

        public async Task<string> StoreOwnLogoAsync(Stream content, string fileName, string contentType)
        {
            var company = await LoadOwnCompanyAsync();

            return await StoreLogoAsync(company.Id, content, fileName, contentType);
        }

        private async Task<TaxiCompany> LoadOwnCompanyAsync()
        {
            var userId = _userAccessor.GetUserId()
                ?? throw new BusinessException("The caller could not be identified.");

            return await _dbContext.TaxiCompanies
                .Include(t => t.City)
                .FirstOrDefaultAsync(t => t.UserId == userId)
                ?? throw new BusinessException("This account does not manage a taxi company.");
        }

        protected override Task<IQueryable<TaxiCompany>> IncludeRelatedEntitiesAsync(
            TaxiCompanySearch? search, IQueryable<TaxiCompany> query)
        {
            IQueryable<TaxiCompany> result = query.Include(t => t.City);
            return Task.FromResult(result);
        }

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
        }

        protected override IQueryable<TaxiCompany> ApplyFilters(IQueryable<TaxiCompany> query, TaxiCompanySearch? search)
        {
            if (_passengerHomeCityId.HasValue)
            {
                query = query.Where(t => t.CityId == _passengerHomeCityId.Value);
            }

            if (search == null)
            {
                return query;
            }

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                var pattern = $"%{EscapeLikePattern(search.Name)}%";
                query = query.Where(t => EF.Functions.ILike(t.Name, pattern));
            }

            if (search.CityId.HasValue)
            {
                query = query.Where(t => t.CityId == search.CityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.CityName))
            {
                var pattern = $"%{EscapeLikePattern(search.CityName)}%";
                query = query.Where(t => EF.Functions.ILike(t.City.Name, pattern));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == search.IsActive.Value);
            }

            return query;
        }

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

            var companyRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Company)
                ?? throw new BusinessException($"Role '{Roles.Company}' is missing from the database.");

            var salt = _cryptoService.GenerateSalt();

            var user = new User
            {
                FirstName = request.OwnerName,
                LastName = request.Name,
                Email = loginEmail,
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

            var company = new TaxiCompany
            {
                Name = request.Name,
                HqAddress = request.HqAddress,
                CityId = request.CityId,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                OwnerName = request.OwnerName,
                StartingPrice = request.StartingPrice,
                PricePerKm = request.PricePerKm,
                IsActive = request.IsActive,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.TaxiCompanies.Add(company);

            try
            {
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation())
            {
                await transaction.RollbackAsync();
                throw DuplicateCompanyDetail(exception, request.Name, request.ContactPhone);
            }

            await _dbContext.Entry(company).Reference(c => c.City).LoadAsync();

            var response = _mapper.Map<TaxiCompanyResponse>(company);
            response.InitialLoginUsername = user.Username;

            return response;
        }

        public override async Task<TaxiCompanyResponse> UpdateAsync(int id, TaxiCompanyUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var company = await _dbContext.TaxiCompanies
                .Include(t => t.City)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"TaxiCompany with id {id} not found.");

            if (request.CityId.HasValue)
            {
                await EnsureCityExistsAsync(request.CityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                await EnsureCompanyNameIsFreeAsync(request.Name, id);
            }

            if (!string.IsNullOrWhiteSpace(request.ContactPhone))
            {
                await EnsureContactPhoneIsFreeAsync(request.ContactPhone, id);
            }

            MapUpdateRequestToEntity(request, company);
            company.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation())
            {
                throw DuplicateCompanyDetail(exception, company.Name, company.ContactPhone);
            }

            await _dbContext.Entry(company).Reference(c => c.City).LoadAsync();

            return _mapper.Map<TaxiCompanyResponse>(company);
        }

        public async Task<string> StoreLogoAsync(int id, Stream content, string fileName, string contentType)
        {
            var company = await _dbContext.TaxiCompanies.FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"TaxiCompany with id {id} not found.");

            var storedPath = await _imageFileStore.SaveAsync(LogoStorageArea, id, content, fileName, contentType);

            company.LogoImagePath = storedPath;
            company.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return storedPath;
        }

        public async Task<(Stream Content, string ContentType, string FileName)> GetLogoAsync(int id)
        {
            var company = await _dbContext.TaxiCompanies.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"TaxiCompany with id {id} not found.");

            if (string.IsNullOrWhiteSpace(company.LogoImagePath))
            {
                throw new KeyNotFoundException($"TaxiCompany with id {id} has no logo.");
            }

            return _imageFileStore.Open(company.LogoImagePath);
        }

        private async Task EnsureCityExistsAsync(int cityId)
        {
            if (!await _dbContext.Cities.AnyAsync(c => c.Id == cityId))
            {
                throw new BusinessException("The selected city does not exist. Pick a city from the list.");
            }
        }

        private async Task EnsureCompanyNameIsFreeAsync(string name, int? excludingId)
        {
            var taken = await _dbContext.TaxiCompanies
                .AnyAsync(t => t.Name == name && (excludingId == null || t.Id != excludingId));

            if (taken)
            {
                throw new BusinessException($"A taxi company named '{name}' already exists.");
            }
        }

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
            }

            company.IsDeleted = true;
            company.DeletedAt = deletedAt;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task EnsureContactPhoneIsFreeAsync(string phone, int? excludingId)
        {
            var taken = await _dbContext.TaxiCompanies
                .AnyAsync(t => t.ContactPhone == phone && (excludingId == null || t.Id != excludingId));

            if (taken)
            {
                throw new BusinessException($"Contact phone '{phone}' is already used by another taxi company.");
            }
        }

        private static BusinessException DuplicateCompanyDetail(
            DbUpdateException exception, string name, string contactPhone) =>
            exception.ConstraintName() switch
            {
                "IX_TaxiCompanies_Name" =>
                    new BusinessException($"A taxi company named '{name}' already exists."),
                "IX_TaxiCompanies_ContactPhone" =>
                    new BusinessException($"Contact phone '{contactPhone}' is already used by another taxi company."),
                "IX_Users_Username" =>
                    new BusinessException("That login username is already in use."),
                "IX_Users_Email" =>
                    new BusinessException("That login email address is already in use."),
                "IX_Users_PhoneNumber" =>
                    new BusinessException("That phone number is already in use."),
                _ => new BusinessException("Those company details are already in use.")
            };

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

    }
}
