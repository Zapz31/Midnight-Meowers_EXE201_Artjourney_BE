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
        public Task DeleteAllAsync(List<T> entities);
        public IQueryable<T> Get(QueryOptions<T> options);
        public Task<IEnumerable<T>> GetAllAsync(QueryOptions<T> options);
        public Task<T?> GetSingleAsync(QueryOptions<T> options);
        public Task<bool> AnyAsync(QueryOptions<T> options);
        public Task<TResult?> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);
    }
}