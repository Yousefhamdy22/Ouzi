using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> AuthenticateAsync(string email, string password);
        Task<AuthResult> RefreshTokenAsync(string expiredToken, string refreshToken);
        Task<bool> RevokeTokensAsync(Guid userId);

    }
}
