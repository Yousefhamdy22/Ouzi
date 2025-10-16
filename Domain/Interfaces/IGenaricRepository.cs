using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGenaricRepository<T> where T : class
    {
        // Basic CRUD operations
        Task<T> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T id);

        // Queryable operations
        IQueryable<T> GetQueryable(bool tracking = false);
        IQueryable<T> GetAsNoTracking();
        IQueryable<T> GetAsTracking();

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Pagination methods
        //Task<PaginatedResult<T>> GetPagedAsync(
        //    int pageNumber,
        //    int pageSize,
        //    Expression<Func<T, bool>> filter = null,
        //    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        //    string includeProperties = "");

        //Task<PaginatedResult<TResult>> GetPagedProjectionAsync<TResult>(
        //    int pageNumber,
        //    int pageSize,
        //    Expression<Func<T, TResult>> selector,
        //    Expression<Func<T, bool>> filter = null,
        //    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        //    string includeProperties = "");
    }


}
