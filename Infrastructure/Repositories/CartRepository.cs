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
    class CartRepository : GenericRepository<Cart>, ICart
    {
        private readonly RestaurantDbContext _context;
        public CartRepository(RestaurantDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<bool> ExistsForUserAsync(Guid userId)
        {
            return _context.Carts.AnyAsync(c => c.UserId == userId);
        }

        public async Task<Cart> GetByUserIdAsync(Guid userId)
        {
            var cart =  await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart;

        }

        public async Task UpdateAsync(Cart cart)
        {
            _context.Carts.Attach(cart);
            _context.Entry(cart).State = EntityState.Modified;

            // Also mark all cart items as modified if they exist
            foreach (var item in cart.Items)
            {
                _context.Entry(item).State = item.Id == 0 ?
                    EntityState.Added : EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<Cart> GetByUserIdWithItemsAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.Items) // Ensure cart items are loaded
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

    }
}
