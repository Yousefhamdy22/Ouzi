using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Interfsces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    class ProductRepository : GenericRepository<Product>, IProduct
    {
        public ProductRepository(RestaurantDbContext context) : base(context)
        {

        }
       
    }
}
