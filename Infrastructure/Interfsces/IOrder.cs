using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface IOrder : IGenaricRepository<Order>
    {

        public Task<Order> GetByUserIdAsync (Guid userId);
        public Task<Order> GetByInvoiceIdAsync(string invoiceId);

        Task<Order> GetByIdWithItemsAsync(int orderId);

    }
}
