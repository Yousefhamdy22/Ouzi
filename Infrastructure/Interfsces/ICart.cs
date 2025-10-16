using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface ICart : IGenaricRepository<Cart>
    {

        Task<Cart> GetByUserIdAsync(Guid userId);
        Task<bool> ExistsForUserAsync(Guid userId);

        Task<Cart> GetByUserIdWithItemsAsync(Guid userId);

        Task UpdateAsync(Cart cart);
    }
}
