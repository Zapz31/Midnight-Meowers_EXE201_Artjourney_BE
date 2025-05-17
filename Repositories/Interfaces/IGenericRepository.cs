using Repositories.Queries;
using System.Linq.Expressions;

namespace Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        public Task<T> CreateAsync(T entity);
        public Task CreateAllAsync(List<T> entities);
        public Task UpdateAsync(T entity);
        public Task DeleteAsync(T entity);

        public Task<int> ExecuteDeleteAsync(QueryOptions<T> options);
        public Task DeleteAllAsync(List<T> entities);
        public IQueryable<T> Get(QueryOptions<T> options);
        public Task<IEnumerable<T>> GetAllAsync(QueryOptions<T> options);
        public Task<T?> GetSingleAsync(QueryOptions<T> options);
        public Task<bool> AnyAsync(QueryOptions<T> options);
        public Task<TResult?> MaxAsync<TResult>(QueryOptions<T> options, Expression<Func<T, TResult>> selector);
        public Task<TResult?> MinAsync<TResult>(QueryOptions<T> options, Expression<Func<T, TResult>> selector);
        public Task<int> CountAsync(QueryOptions<T> options);
        public Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector);
        public Task<double?> SumAsync(Expression<Func<T, double?>> selector); // optional overload
        public Task<int?> SumAsync(Expression<Func<T, int?>> selector);
        public Task<long?> SumAsync(Expression<Func<T, long?>> selector);
    }
}