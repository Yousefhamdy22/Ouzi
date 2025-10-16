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
    class CategoryRepository : GenericRepository<Category>, ICategory
    {
        private readonly RestaurantDbContext _context;
        public CategoryRepository(RestaurantDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate)
        {
          return await _context.Categories.AnyAsync(predicate);

        }

       
    }
   
}
