using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Access;
using PickMeUp.Model.Constants;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using PickMeUp.Services.Orders;
using PickMeUp.Services.Storage;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PickMeUp.Services
{
    public class UserService : BaseCRUDService<User, UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest>, IUserService
    {
        private const string ProfileStorageArea = "user-photos";

        private readonly ICryptoService _cryptoService;
        private readonly IImageFileStore _imageFileStore;
        private readonly IAuthenticatedUserAccessor _userAccessor;
        private readonly IValidator<UserProfileUpdateRequest> _profileValidator;
        private readonly IValidator<UserPasswordChangeRequest> _passwordChangeValidator;

        public UserService(PickMeUpDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<UserInsertRequest> insertValidator, IValidator<UserUpdateRequest> updateValidator, IValidator<UserProfileUpdateRequest> profileValidator, IValidator<UserPasswordChangeRequest> passwordChangeValidator, ICryptoService cryptoService, IImageFileStore imageFileStore, IAuthenticatedUserAccessor userAccessor)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cryptoService = cryptoService;
            _imageFileStore = imageFileStore;
            _userAccessor = userAccessor;
            _profileValidator = profileValidator;
            _passwordChangeValidator = passwordChangeValidator;
        }

        public async Task<UserResponse> GetOwnAsync()
        {
            return await GetWithRoleByIdAsync(RequireOwnUserId())
                ?? throw new BusinessException("Your account could not be found.");
        }

        public async Task<UserResponse> UpdateOwnAsync(UserProfileUpdateRequest request)
        {
            await _profileValidator.ValidateAndThrowAsync(request);

            var userId = RequireOwnUserId();
            var user = await LoadUserAsync(userId);

            var email = request.Email.Trim();

            var taken = await _dbContext.Users
                .AnyAsync(u => u.Id != userId && u.Email == email);

            if (taken)
            {
                throw new BusinessException("That email address is already in use.");
            }

            var phone = NormalizePhone(request.PhoneNumber);
            await EnsurePhoneIsFreeAsync(phone, userId);

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = email;
            user.PhoneNumber = phone;
            await SaveAccountAsync();

            return await GetOwnAsync();
        }

        public Task<string> StoreOwnProfileImageAsync(Stream content, string fileName, string contentType)
        {
            return StoreProfileImageAsync(RequireOwnUserId(), content, fileName, contentType);
        }

        private int RequireOwnUserId()
        {
            return _userAccessor.GetUserId()
                ?? throw new BusinessException("The caller could not be identified.");
        }

        public async Task<string> StoreProfileImageAsync(int id, Stream content, string fileName, string contentType)
        {
            var user = await LoadUserAsync(id);

            var storedPath = await _imageFileStore.SaveAsync(ProfileStorageArea, id, content, fileName, contentType);

            user.ProfileImagePath = storedPath;
            await _dbContext.SaveChangesAsync();

            return storedPath;
        }

        public async Task<(Stream Content, string ContentType, string FileName)> GetProfileImageAsync(int id)
        {
            await EnsureProfileImageIsVisibleAsync(id);

            var user = await LoadUserAsync(id);

            if (string.IsNullOrWhiteSpace(user.ProfileImagePath))
            {
                throw new KeyNotFoundException($"User with id {id} has no profile image.");
            }

            return _imageFileStore.Open(user.ProfileImagePath);
        }

        private async Task EnsureProfileImageIsVisibleAsync(int id)
        {
            if (_userAccessor.IsInRole(Roles.Admin))
            {
                return;
            }

            var callerId = RequireOwnUserId();

            if (callerId == id)
            {
                return;
            }

            var sharedRide = await _dbContext.Orders.AnyAsync(o =>
                (o.PassengerId == callerId && o.Driver != null && o.Driver.UserId == id)
                || (o.PassengerId == id && o.Driver != null && o.Driver.UserId == callerId));

            if (sharedRide)
            {
                return;
            }

            var callerCompanies = CompanyIdsOf(callerId);
            var sharedCompany = await CompanyIdsOf(id).AnyAsync(companyId => callerCompanies.Contains(companyId));

            if (sharedCompany)
            {
                return;
            }

            throw new KeyNotFoundException($"User with id {id} not found.");
        }

        private IQueryable<int> CompanyIdsOf(int userId) =>
            _dbContext.TaxiCompanies
                .Where(t => t.UserId == userId)
                .Select(t => t.Id)
                .Concat(_dbContext.Drivers
                    .Where(d => d.UserId == userId)
                    .Select(d => d.TaxiCompanyId));

        private async Task<User> LoadUserAsync(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            return user;
        }

        protected override Task<IQueryable<User>> IncludeRelatedEntitiesAsync(UserSearch? search, IQueryable<User> query)
        {
            IQueryable<User> result = query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).Include(u => u.City);
            return Task.FromResult(result);
        }

        protected override IQueryable<User> ApplyFilters(IQueryable<User> query, UserSearch? search)
        {
            if (search == null)
            {
                return query;
            }

            if (!string.IsNullOrWhiteSpace(search.Email))
            {
                var pattern = $"%{EscapeLikePattern(search.Email)}%";
                query = query.Where(u => EF.Functions.ILike(u.Email, pattern));
            }

            if (!string.IsNullOrWhiteSpace(search.Username))
            {
                var pattern = $"%{EscapeLikePattern(search.Username)}%";
                query = query.Where(u => EF.Functions.ILike(u.Username, pattern));
            }

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                var pattern = $"%{EscapeLikePattern(search.Name)}%";
                query = query.Where(u => EF.Functions.ILike(u.FirstName, pattern)
                                      || EF.Functions.ILike(u.LastName, pattern));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == search.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Role))
            {
                var role = search.Role.Trim();
                query = query.Where(u => u.UserRoles.Any(ur => EF.Functions.ILike(ur.Role.Name, role)));
            }

            return query;
        }

        protected override User MapInsertRequestToEntity(UserInsertRequest request)
        {
            var entity = base.MapInsertRequestToEntity(request);

            var salt = _cryptoService.GenerateSalt();
            entity.PasswordSalt = salt;
            entity.PasswordHash = _cryptoService.GenerateHash(request.Password, salt);

            return entity;
        }

        public override async Task<UserResponse> InsertAsync(UserInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new BusinessException($"Email '{request.Email}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new BusinessException($"Username '{request.Username}' is already in use.");
            }

            await EnsurePhoneIsFreeAsync(NormalizePhone(request.PhoneNumber), null);

            if (!await _dbContext.Cities.AnyAsync(c => c.Id == request.CityId && c.IsActive))
            {
                throw new BusinessException("The selected city does not exist. Pick a city from the list.");
            }

            var passengerRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Passenger)
                ?? throw new BusinessException($"Role '{Roles.Passenger}' is missing from the database.");

            var entity = MapInsertRequestToEntity(request);
            entity.CreatedAt = DateTime.UtcNow;
            entity.PhoneNumber = NormalizePhone(entity.PhoneNumber);

            _dbContext.Users.Add(entity);
            await SaveAccountAsync();

            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = entity.Id,
                RoleId = passengerRole.Id,
                DateAssigned = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(entity).Reference(u => u.City).LoadAsync();

            var response = _mapper.Map<UserResponse>(entity);
            response.Role = passengerRole.Name;

            return response;
        }

        public override async Task<UserResponse> UpdateAsync(int id, UserUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var entity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
            {
                throw new BusinessException($"Email '{request.Email}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
            {
                throw new BusinessException($"Username '{request.Username}' is already in use.");
            }

            MapUpdateRequestToEntity(request, entity);
            entity.PhoneNumber = NormalizePhone(entity.PhoneNumber);

            await EnsurePhoneIsFreeAsync(entity.PhoneNumber, id);

            _dbContext.Users.Update(entity);
            await SaveAccountAsync();

            return _mapper.Map<UserResponse>(entity);
        }

        private static string? NormalizePhone(string? phone) =>
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

        private async Task EnsurePhoneIsFreeAsync(string? phone, int? excludeUserId)
        {
            if (phone == null)
            {
                return;
            }

            var taken = await _dbContext.Users
                .AnyAsync(u => u.PhoneNumber == phone && (excludeUserId == null || u.Id != excludeUserId));

            if (taken)
            {
                throw new BusinessException($"Phone number '{phone}' is already in use.");
            }
        }

        private async Task SaveAccountAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation())
            {
                throw DuplicateAccountDetail(exception);
            }
        }

        private static BusinessException DuplicateAccountDetail(DbUpdateException exception) =>
            exception.ConstraintName() switch
            {
                "IX_Users_Username" => new BusinessException("That username is already in use."),
                "IX_Users_Email" => new BusinessException("That email address is already in use."),
                "IX_Users_PhoneNumber" => new BusinessException("That phone number is already in use."),
                _ => new BusinessException("Those account details are already in use.")
            };

        public Task MarkSignedInAsync(int userId) =>
            _dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastLoginAt, DateTime.UtcNow));

        public async Task<UserSensitiveResponse?> GetByUsernameAsync(string username)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.City)
                .FirstOrDefaultAsync(u => u.Username == username);

            UserSensitiveResponse? response = null;

            if (user != null)
            {
                response = _mapper.Map<UserSensitiveResponse>(user);
                response.Role = user.UserRoles.FirstOrDefault()?.Role.Name ?? string.Empty;
            }

            return response;
        }

        public async Task<UserResponse?> GetWithRoleByIdAsync(int id)
        {
            var user = await _dbContext.Users
               .AsNoTracking()
               .Include(u => u.UserRoles)
               .ThenInclude(ur => ur.Role)
               .Include(u => u.City)
               .FirstOrDefaultAsync(u => u.Id == id);

            UserResponse? response = null;

            if (user != null)
            {
                response = _mapper.Map<UserResponse>(user);
                response.Role = user.UserRoles.FirstOrDefault()?.Role.Name ?? string.Empty;
            }

            return response;
        }

        public override async Task<UserResponse> GetByIdAsync(int id)
        {
            var response = await GetWithRoleByIdAsync(id);
            if (response == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            return response;
        }

        public async Task DeleteOwnAccountAsync()
        {
            await DeleteAccountAsync(RequireOwnUserId());
        }

        public override async Task DeleteAsync(int id)
        {
            await DeleteAccountAsync(id);
        }

        private async Task DeleteAccountAsync(int userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"User with id {userId} not found.");

            await EnsureAccountCanBeClosedAsync(userId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            await _dbContext.RefreshTokens
                .IgnoreQueryFilters()
                .Where(t => t.UserId == userId)
                .ExecuteDeleteAsync();

            var closedAt = DateTime.UtcNow;

            user.IsActive = false;
            user.IsDeleted = true;
            user.DeletedAt = closedAt;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task EnsureAccountCanBeClosedAsync(int userId)
        {
            var ridingAsPassenger = await _dbContext.Orders
                .AnyAsync(o => o.PassengerId == userId
                    && DispatchRules.ActiveRideStatuses.Contains(o.OrderStatusId));

            if (ridingAsPassenger)
            {
                throw new BusinessException(
                    "There is a ride under way on this account. It has to finish or be cancelled first.");
            }

            var driverId = await _dbContext.Drivers
                .Where(d => d.UserId == userId)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();

            if (driverId == null)
            {
                return;
            }

            var drivingARide = await _dbContext.Orders.AnyAsync(DispatchRules.ActiveRideOf(driverId.Value));

            if (drivingARide)
            {
                throw new BusinessException(
                    "This driver is on a ride. It has to be completed before the account is closed.");
            }

            var onShift = await _dbContext.Shifts
                .AnyAsync(s => s.DriverId == driverId.Value && s.EndedAt == null);

            if (onShift)
            {
                throw new BusinessException(
                    "This driver still has an open shift. Stop the shift before closing the account.");
            }
        }

        public async Task ChangePasswordAsync(UserPasswordChangeRequest request)
        {
            await _passwordChangeValidator.ValidateAndThrowAsync(request);

            var targetId = _userAccessor.IsInRole(Roles.Admin) ? request.Id : RequireOwnUserId();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == targetId);

            if (user == null)
                throw new BusinessException("User not found.");

            if (!_cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password))
                throw new BusinessException("Wrong credential.");

            if (!request.NewPassword.Equals(request.ConfirmNewPassword))
                throw new BusinessException("Password confirmation doesn't match new password.");

            user.PasswordSalt = _cryptoService.GenerateSalt();
            user.PasswordHash = _cryptoService.GenerateHash(request.NewPassword, user.PasswordSalt);
            user.MustChangePassword = false;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
