using ProvexBackendAPI.Data.Models;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IGenericService<T> where T : BaseEntity
    {
        Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
        Task<T?> GetAsync(Guid id, CancellationToken ct = default);
        Task<T> CreateAsync(T entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(Guid id, Action<T> mutate, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
