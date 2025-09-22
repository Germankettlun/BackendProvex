namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IGenericRepository<T> : IReadRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
