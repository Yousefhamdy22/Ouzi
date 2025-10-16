using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task<string?> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokensAsync(string userId);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    }
}
