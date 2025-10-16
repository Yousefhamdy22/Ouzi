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
    public class OrderItemsRepository : GenericRepository<OrderItem>, IOrderItems
    {
        public OrderItemsRepository(RestaurantDbContext context) : base(context)
        {
        }



    }
}
