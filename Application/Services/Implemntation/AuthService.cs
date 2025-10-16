using Application.DTOs;
using Application.Services.interfaces;
using Domain.Entities;
using Infrastructure.Interfsces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;


namespace Application.Services.Implemntation
{
    public class AuthService : IAuthService
    {


        #region DI

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly ILogger<AuthService> _logger;
        #endregion

        #region ctors

       
        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepo,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _refreshTokenRepo = refreshTokenRepo;
            _logger = logger;
        }
        #endregion

        #region Authentication


        public async Task<AuthResult> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Authentication failed: User not found for email {Email}", email);
                    return AuthResult.Fail("Invalid credentials");
                }

                if (!await _userManager.CheckPasswordAsync(user, password))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for user {UserId}", user.Id);
                    return AuthResult.Fail("Invalid credentials");
                }

                var token = await _tokenService.GenerateTokenAsync(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

                _logger.LogInformation("User {UserId} authenticated successfully", user.Id);

                return AuthResult.Success(
                    token: token,
                    refreshToken: refreshToken,
                    user: user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for email {Email}", email);
                return AuthResult.Fail("An error occurred during authentication");
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string expiredToken, string refreshToken)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(expiredToken);
                if (principal == null)
                {
                    _logger.LogWarning("Refresh token failed: Invalid expired token");
                    return AuthResult.Fail("Invalid token");
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Refresh token failed: Missing user ID in token");
                    return AuthResult.Fail("Invalid token");
                }

                var validUserId = await _tokenService.ValidateRefreshTokenAsync(refreshToken);
                if (string.IsNullOrEmpty(validUserId) || validUserId != userId)
                {
                    _logger.LogWarning("Refresh token failed: Invalid refresh token for user {UserId}", userId);
                    return AuthResult.Fail("Invalid refresh token");
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Refresh token failed: User {UserId} not found", user);
                    return AuthResult.Fail("User not found");
                }

                // Generate new tokens
                var newToken = await _tokenService.GenerateTokenAsync(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Revoke old refresh token and save new one
                await _tokenService.RevokeRefreshTokensAsync(userId);
                await _tokenService.SaveRefreshTokenAsync(userId, newRefreshToken);

                _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

                return AuthResult.Success(
                    token: newToken,
                    refreshToken: newRefreshToken,
                    user: user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return AuthResult.Fail("An error occurred during token refresh");
            }
        }

        public async Task<bool> RevokeTokensAsync(Guid userId)
        {
            try
            {
                await _tokenService.RevokeRefreshTokensAsync(userId.ToString());
                _logger.LogInformation("Tokens revoked for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking tokens for user {UserId}", userId);
                return false;
            }
        }
        #endregion

    }
}
