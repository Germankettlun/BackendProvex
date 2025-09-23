using System.Linq.Expressions;

namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IReadRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        IQueryable<T> Query(bool asNoTracking = true);
    }
}
