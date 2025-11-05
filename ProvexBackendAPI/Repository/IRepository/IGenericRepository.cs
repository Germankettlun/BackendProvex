using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IGenericRepository<T> : IReadRepository<T> where T : class
    {
        // Lectura
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        IQueryable<T> Query(bool asNoTracking = true);

        // Escritura
        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task SpVoid(string query, SqlParameter[] parameters);
    }
}
