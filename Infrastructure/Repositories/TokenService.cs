using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfsces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<ApplicationUser>  _userManager;
        private readonly byte[] _jwtSecret;

       
        public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _userManager = userManager;

            // Validate the JWT secret key
            var secretKey = _configuration["jwtSettings:secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException(
                    nameof(secretKey),
                    "JWT Secret Key is missing in appsettings.json under 'jwtSettings:secret'");
            }

            _jwtSecret = Encoding.ASCII.GetBytes(secretKey);
        }


        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;

            var accessTokenMinutes = Convert.ToDouble(_configuration["jwtSettings:AccessTokenExpiration"]);
            var expiration = now.AddMinutes(accessTokenMinutes);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
           
          
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));             
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = now,
                Expires = expiration, 
                Issuer = _configuration["jwtSettings:Issuer"],
                Audience = _configuration["jwtSettings:Audience"], 
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtSettings:Secret"])), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

           
            var decodedToken = tokenHandler.ReadJwtToken(tokenString);
           
            return tokenString;
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            var expiryDays = _configuration["jwtSettings:RefreshTokenExpirationDays"]
                ?? throw new ArgumentNullException("jwtSettings:RefreshTokenExpirationDays is missing");

            var expiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(expiryDays));
            await _refreshTokenRepository.SaveRefreshTokenAsync(userId, refreshToken, expiryTime);
        }


        public async Task<string?> ValidateRefreshTokenAsync(string token)
        {
            return await _refreshTokenRepository.ValidateRefreshTokenAsync(token);
        }

        public async Task RevokeRefreshTokensAsync(string userId)
        {
            await _refreshTokenRepository.RevokeRefreshTokensAsync(userId);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_jwtSecret),
                ValidateLifetime = false
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

    }
}

