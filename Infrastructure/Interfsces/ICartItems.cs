using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface ICartItems : IGenaricRepository<CartItem>
    {

        Task<CartItem> GetByProductIdAsync(int cartId, int productId);
        Task<IReadOnlyList<CartItem>> GetItemsByCartIdAsync(int cartId);
        Task RemoveByProductIdAsync(int cartId, int productId);
    }
}
