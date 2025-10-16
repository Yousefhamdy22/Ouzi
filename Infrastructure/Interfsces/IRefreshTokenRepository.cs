using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface IRefreshTokenRepository
    {
        Task SaveRefreshTokenAsync(string userId, string token, DateTime expiryTime);
        Task<string?> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokensAsync(string userId);
    }
}
