using PickMeUp.Common.Services.Configuration;
using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Access;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Services;
using PickMeUp.Services.Database;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PickMeUp.WebAPI.Services.AccessManager
{
    public class AccessManager : IAccessManager
    {
        private readonly IUserService _userService;
        private readonly JwtOptions _jwtOptions;
        private readonly ICryptoService _cryptoService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AccessManager(IUserService userService, IOptions<JwtOptions> jwtOptions, ICryptoService cryptoService, IRefreshTokenService refreshTokenService)
        {
            _userService = userService;
            _jwtOptions = jwtOptions.Value;
            _cryptoService = cryptoService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request)
        {
            var user = await _userService.GetByUsernameAsync(request.Username);

            if (user == null || !_cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password))
            {
                throw new BusinessException("Invalid username or password.");
            }

            var accessToken = GenerateToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.InsertAsync(refreshToken);

            return new UserLoginResponse
            {
                Accesstoken = accessToken,
                Refreshtoken = refreshTokenValue
            };
        }

        public async Task<UserLoginResponse> LoginWithRefreshTokenAsync(RefreshAccessTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new BusinessException("Refresh token is required");
            }

            var refreshToken = await _refreshTokenService.GetStoredTokenAsync(request.RefreshToken);

            if (refreshToken == null)
            {
                throw new BusinessException("Invalid refresh token");
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new BusinessException("Refresh token has expired");
            }

            var user = await _userService.GetWithRoleByIdAsync(refreshToken.UserId);

            if (user == null)
            {
                throw new BusinessException("User not found");
            }

            if (!user.IsActive)
            {
                throw new BusinessException("User is not active");
            }

            await _refreshTokenService.DeleteAllUserRefreshTokensAsync(user.Id);

            var accessToken = GenerateToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var token = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.InsertAsync(token);

            return new UserLoginResponse
            {
                Accesstoken = accessToken,
                Refreshtoken = refreshTokenValue
            };

        }

        public Task LogoutAsync(int userId) =>
            _refreshTokenService.DeleteAllUserRefreshTokensAsync(userId);

        private string GenerateToken(UserResponse user)
        {
            var secretKey = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimNames.Id, user.Id.ToString()),
                    new Claim(ClaimNames.FirstName, user.FirstName ?? string.Empty),
                    new Claim(ClaimNames.LastName, user.LastName ?? string.Empty),
                    new Claim(ClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(ClaimNames.Role, user.Role ?? "user"),
                    new Claim(ClaimNames.IsActive, user.IsActive.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.DurationInMinutes),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randombytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randombytes);
        }

    }
}
