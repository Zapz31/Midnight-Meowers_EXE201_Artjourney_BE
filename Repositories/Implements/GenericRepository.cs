using DAOs;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System.Linq.Expressions;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    public async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task CreateAllAsync(List<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public Task DeleteAsync(T entity)
    {
        if (_context.Entry<T>(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public Task DeleteAllAsync(List<T> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public IQueryable<T> Get(QueryOptions<T> options)
    {
        IQueryable<T> query = _dbSet;

        if (options.Tracked == false)
        {
            query = query.AsNoTracking();
        }

        if (options.IncludeProperties?.Any() ?? false)
        {
            foreach (var includeProperty in options.IncludeProperties)
            {
                query = query.Include(includeProperty);
            }
        }

        if (options.Predicate != null)
        {
            query = query.Where(options.Predicate);
        }

        if (options.OrderBy != null)
        {
            query = options.OrderBy(query);
        }

        return query;
    }

    public Task UpdateAsync(T entity)
    {

        if (_context.Entry<T>(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Update(entity);

        return Task.CompletedTask;
    }

    public async Task<int> ExecuteRawSqlAsync(string sql, params object[] parameters)
    {
        return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    public async Task<IEnumerable<T>> GetAllAsync(QueryOptions<T> options)
    {
        return await Get(options).ToListAsync();
    }

    public async Task<T?> GetSingleAsync(QueryOptions<T> options)
    {
        return await Get(options).FirstOrDefaultAsync();
    }

    public async Task<bool> AnyAsync(QueryOptions<T> options)
    {
        if (options.Predicate != null)
        {
            var result = await _dbSet.AnyAsync(options.Predicate);
            return result;
        }
        return false;
    }

    public async Task<TResult?> MaxAsync<TResult>(QueryOptions<T> options, Expression<Func<T, TResult>> selector)
    {
        var query = Get(options); 
        return await query.MaxAsync(selector);
    }

    public async Task<TResult?> MinAsync<TResult>(QueryOptions<T> options, Expression<Func<T, TResult>> selector)
    {
        var query = Get(options);
        return await query.MaxAsync(selector);
    }

    public async Task<int> CountAsync(QueryOptions<T> options)
    {
        var query = Get(options);
        return await query.CountAsync();
    }

    public async Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
    {
        return await _dbSet.SumAsync(selector);
    }

    public async Task<double?> SumAsync(Expression<Func<T, double?>> selector)
    {
        return await _dbSet.SumAsync(selector);
    }

    public async Task<int?> SumAsync(Expression<Func<T, int?>> selector)
    {
        return await _dbSet.SumAsync(selector);
    }

    public async Task<long?> SumAsync(Expression<Func<T, long?>> selector)
    {
        return await _dbSet.SumAsync(selector);
    }

    public async Task<int> ExecuteDeleteAsync(QueryOptions<T> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.Predicate == null)
        {
            throw new ArgumentNullException(nameof(options.Predicate),
                "Predicate cannot null");
        }

        IQueryable<T> query = _dbSet;
        query = query.Where(options.Predicate);
        return await query.ExecuteDeleteAsync();
    }
}