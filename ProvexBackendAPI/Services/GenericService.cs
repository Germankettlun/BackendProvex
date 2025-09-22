using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;

namespace ProvexBackendAPI.Services
{
    public class GenericService<T> : IGenericService<T> where T : BaseEntity
    {
        private readonly IGenericRepository<T> _genericRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GenericService(IGenericRepository<T> genericRepository, IUnitOfWork unitOfWork)
        {
            _genericRepository = genericRepository;
            _unitOfWork = unitOfWork;
        }

        public Task<T> CreateAsync(T entity, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Guid id, Action<T> mutate, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
