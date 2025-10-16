using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface ICategory : IGenaricRepository<Category>
    {
        public Task<bool>AnyAsync(Expression<Func<Category, bool>> predicate);
    }
}
