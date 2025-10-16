using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Interfsces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItems
    {
        public CartItemRepository(RestaurantDbContext context) : base(context)
        {
        }

        public Task<CartItem> GetByProductIdAsync(int cartId, int productId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<CartItem>> GetItemsByCartIdAsync(int cartId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByProductIdAsync(int cartId, int productId)
        {
            throw new NotImplementedException();
        }
    }
}
