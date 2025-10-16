using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Interfsces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(RestaurantDbContext context) : base(context) { }

        public async Task<ApplicationUser> GetUserWithAddressesAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser> GetUserWithCartAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

       
    }
}
