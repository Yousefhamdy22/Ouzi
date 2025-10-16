using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Net.Sockets;
using Infrastructure.Data;
using Domain.Exceptions;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly RestaurantDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _transaction;

        public IGenaricRepository<Product> Products { get; private set; }

        public IGenaricRepository<Category> Categories { get; private set; }

        public IGenaricRepository<Cart> Carts { get; private set; }

        public IGenaricRepository<CartItem> CartItems { get; private set; }

        public IGenaricRepository<Order> Orders { get; private set; }

        public IGenaricRepository<OrderItem> OrderItems { get; private set; }

        public IGenaricRepository<Address> Addresses { get; private set; }

        public IGenaricRepository<ApplicationUser> ApplicationUsers { get; private set; }

        public UnitOfWork(
            RestaurantDbContext context,
            ILogger<UnitOfWork> logger
            )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Products = new GenericRepository<Product>(context);
            Categories = new GenericRepository<Category>(context);
            Carts = new GenericRepository<Cart>(context);
            CartItems = new GenericRepository<CartItem>(context);
            Orders = new GenericRepository<Order>(context);
            OrderItems = new GenericRepository<OrderItem>(context);
            Addresses = new GenericRepository<Address>(context);
            ApplicationUsers = new GenericRepository<ApplicationUser>(context);


        }
        public async Task<int> CommitAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Database error occurred while committing changes");
                throw new ServiceUnavailableException("Database service is unavailable", ex);
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Network error occurred while connecting to database");
                throw new ServiceUnavailableException("Database service is unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while committing changes");
                throw;
            }
        }


        public async Task RollbackAsync()
        {
            try
            {
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during rollback");
                throw;
            }
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.CommitAsync(); 
                    await _transaction.DisposeAsync(); 
                }
                finally
                {
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.RollbackAsync(); 
                    await _transaction.DisposeAsync(); 
                }
                finally
                {
                    _transaction = null;
                }
            }
        }
        public void Dispose()
        {
            //  Dispose(true);
            GC.SuppressFinalize(this);
        }


    }
}
