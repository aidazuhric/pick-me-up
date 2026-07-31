using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Access;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
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
        private readonly ICryptoService _cryptoService;
        public UserService(PickMeUpDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<UserInsertRequest> insertValidator, IValidator<UserUpdateRequest> updateValidator, ICryptoService cryptoService)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cryptoService = cryptoService;
        }

        protected override Task<IQueryable<User>> IncludeRelatedEntitiesAsync(UserSearch? search, IQueryable<User> query)
        {
            IQueryable<User> result = query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
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

            return query;
        }

        protected override User MapInsertRequestToEntity(UserInsertRequest request)
        {
            var entity = base.MapInsertRequestToEntity(request);

            var salt = _cryptoService.GenerateSlat();
            entity.PasswordSalt = salt;
            entity.PasswordHash = _cryptoService.GenerateHash(request.Password, salt);

            return entity;
        }

        public override async Task<UserResponse> InsertAsync(UserInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ClinetException($"Email '{request.Email}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new ClinetException($"Username '{request.Username}' is already in use.");
            }

            var entity = MapInsertRequestToEntity(request);
            entity.CreatedAt = DateTime.UtcNow;

            _dbContext.Users.Add(entity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<UserResponse>(entity);
        }

        public override async Task<UserResponse> UpdateAsync(int id, UserUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var entity = await _dbContext.Users.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
            {
                throw new ClinetException($"Email '{request.Email}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
            {
                throw new ClinetException($"Username '{request.Username}' is already in use.");
            }

            MapUpdateRequestToEntity(request, entity);

            _dbContext.Users.Update(entity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<UserResponse>(entity);
        }

        public override async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            _dbContext.Users.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserSensitveResponse?> GetByUsernameAsync(string username)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            UserSensitveResponse? response = null;

            if (user != null)
            {
                response = _mapper.Map<UserSensitveResponse>(user);
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

        public async Task ChangePasswordAsync(UserPasswordChangeRequest request)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == request.Id);

            if (user == null)
                throw new ClinetException("User not found.");

            if (!_cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password))
                throw new ClinetException("Wrong credential.");

            if (!request.NewPassword.Equals(request.ConfirmNewPassword))
                throw new ClinetException("Password confirmation doesn't match new password.");

            user.PasswordSalt = _cryptoService.GenerateSlat();
            user.PasswordHash = _cryptoService.GenerateHash(request.NewPassword, user.PasswordSalt);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
