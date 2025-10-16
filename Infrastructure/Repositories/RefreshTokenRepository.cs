using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Interfsces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly RestaurantDbContext _context;

        public RefreshTokenRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task SaveRefreshTokenAsync(string userId, string token, DateTime expiryTime)
        {
            // Revoke any existing tokens for this user
            await RevokeRefreshTokensAsync(userId);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Token = token,
                Expires = expiryTime,

                Created = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<string?> ValidateRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);



            return refreshToken.UserId;
        }

        public async Task RevokeRefreshTokensAsync(string userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.Expires >= DateTime.UtcNow)
                .ToListAsync();

            await _context.SaveChangesAsync();
        }
    }
}
