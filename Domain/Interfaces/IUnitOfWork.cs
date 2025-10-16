using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRepository<Product> Products { get; }
        IGenaricRepository<Category> Categories { get; }
        IGenaricRepository<Cart> Carts { get; }
        IGenaricRepository<CartItem> CartItems { get; }
        IGenaricRepository<Order> Orders { get; }
        IGenaricRepository<OrderItem> OrderItems { get; }
        IGenaricRepository<Address> Addresses { get; }
        IGenaricRepository<ApplicationUser> ApplicationUsers { get; }



        Task<int> CommitAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task RollbackAsync();
    
    }
}
