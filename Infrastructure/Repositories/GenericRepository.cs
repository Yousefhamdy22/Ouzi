using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenaricRepository<T> where T : class
    {
        protected readonly RestaurantDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(RestaurantDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }


        #region CURD OPrations
        public async Task<T> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }



        public async Task DeleteAsync(T entity)
        {
           
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<T> GetQueryable(bool tracking = false)
        {
            return tracking ? _dbSet : _dbSet.AsNoTracking();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(predicate)
                .ToListAsync();
        }
        public IQueryable<T> GetAsNoTracking()
        {
            return _dbSet.AsNoTracking();
        }

        public IQueryable<T> GetAsTracking()
        {
            return _dbSet;
        }

        #endregion


        #region Pagunation

        //public async Task<PaginatedResult<T>> GetPagedAsync(
        //   int pageNumber,
        //   int pageSize,
        //   Expression<Func<T, bool>> filter = null,
        //   Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        //   string includeProperties = "")
        //{
        //    IQueryable<T> query = _dbSet;

        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }

        //    foreach (var includeProperty in includeProperties.Split
        //        (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        query = query.Include(includeProperty);
        //    }

        //    if (orderBy != null)
        //    {
        //        query = orderBy(query);
        //    }

        //    int totalRecords = await query.CountAsync();
        //    var data = await query.Skip((pageNumber - 1) * pageSize)
        //                         .Take(pageSize)
        //                         .ToListAsync();

        //    return new PaginatedResult<T>(data, pageNumber, pageSize, totalRecords);
        //}

        //public async Task<PaginatedResult<TResult>> GetPagedProjectionAsync<TResult>(
        //    int pageNumber,
        //    int pageSize,
        //    Expression<Func<T, TResult>> selector,
        //    Expression<Func<T, bool>> filter = null,
        //    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        //    string includeProperties = "")
        //{
        //    IQueryable<T> query = _dbSet;

        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }

        //    foreach (var includeProperty in includeProperties.Split
        //        (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        query = query.Include(includeProperty);
        //    }

        //    if (orderBy != null)
        //    {
        //        query = orderBy(query);
        //    }

        //    int totalRecords = await query.CountAsync();
        //    var data = await query.Skip((pageNumber - 1) * pageSize)
        //                         .Take(pageSize)
        //                         .Select(selector)
        //                         .ToListAsync();

        //    return new PaginatedResult<TResult>(data, pageNumber, pageSize, totalRecords);
        //}

        #endregion

    }
}
