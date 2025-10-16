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
    class OrderRepository : GenericRepository<Order>, IOrder
    {
        private readonly RestaurantDbContext _context;
        public OrderRepository(RestaurantDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdWithItemsAsync(int orderId)
        {
            return await _context.Orders
                   .AsNoTracking() // Important to avoid tracking issues
                   .Include(o => o.OrderItems)
                       //.ThenInclude(oi => oi.ProductId) // Include product details if needed
                   .Include(o => o.DeliveryAddress) // Include address if it's a navigation property
                   .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public Task<Order> GetByInvoiceIdAsync(string invoiceId)
        {
            throw new NotImplementedException();
        }

        public Task<Order>GetByUserIdAsync(Guid userId)
        {
           var order = 
                _context.Orders.Where(o=> o.UserId == userId);
            return Task.FromResult(order.FirstOrDefault());


        }
    }
}
