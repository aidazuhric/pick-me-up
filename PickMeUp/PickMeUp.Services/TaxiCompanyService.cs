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
using System.Security.Cryptography;

namespace PickMeUp.Services
{
    public class TaxiCompanyService
        : BaseCRUDService<TaxiCompany, TaxiCompanyResponse, TaxiCompanySearch, TaxiCompanyInsertRequest, TaxiCompanyUpdateRequest>,
          ITaxiCompanyService
    {
        private const string LogoStorageArea = "company-logos";

        private readonly ICryptoService _cryptoService;
        private readonly IImageFileStore _imageFileStore;

        public TaxiCompanyService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<TaxiCompanyInsertRequest> insertValidator,
            IValidator<TaxiCompanyUpdateRequest> updateValidator,
            ICryptoService cryptoService,
            IImageFileStore imageFileStore)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cryptoService = cryptoService;
            _imageFileStore = imageFileStore;
        }

        protected override Task<IQueryable<TaxiCompany>> IncludeRelatedEntitiesAsync(
            TaxiCompanySearch? search, IQueryable<TaxiCompany> query)
        {
            IQueryable<TaxiCompany> result = query.Include(t => t.City);
            return Task.FromResult(result);
        }

        protected override IQueryable<TaxiCompany> ApplyFilters(IQueryable<TaxiCompany> query, TaxiCompanySearch? search)
        {
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

            var loginEmail = string.IsNullOrWhiteSpace(request.LoginEmail)
                ? request.ContactEmail
                : request.LoginEmail;

            await EnsureLoginIsFreeAsync(request.LoginUsername, loginEmail);

            var generatedPassword = string.IsNullOrWhiteSpace(request.LoginPassword)
                ? GenerateTemporaryPassword()
                : null;
            var password = generatedPassword ?? request.LoginPassword!;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var companyRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Company)
                ?? throw new BusinessException($"Role '{Roles.Company}' is missing from the database.");

            var salt = _cryptoService.GenerateSlat();

            var user = new User
            {
                FirstName = request.OwnerName,
                LastName = request.Name,
                Email = loginEmail,
                Username = request.LoginUsername,
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
                throw new BusinessException(
                    $"A taxi company named '{request.Name}' already exists. Nothing was created.");
            }

            await _dbContext.Entry(company).Reference(c => c.City).LoadAsync();

            var response = _mapper.Map<TaxiCompanyResponse>(company);
            response.InitialLoginUsername = user.Username;
            response.InitialLoginPassword = generatedPassword;

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

            MapUpdateRequestToEntity(request, company);
            company.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
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
