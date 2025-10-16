using Microsoft.EntityFrameworkCore;
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

        public Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
        => _genericRepository.GetAllAsync(ct);

        public Task<T?> GetAsync(Guid id, CancellationToken ct = default)
            => _genericRepository.GetByIdAsync(id, ct);

        public async Task<T> CreateAsync(T entity, CancellationToken ct = default)
        {
            await _genericRepository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> UpdateAsync(Guid id, Action<T> mutate, CancellationToken ct = default)
        {
            var entity = await _genericRepository.GetByIdAsync(id, ct);
            if (entity is null) return false;

            mutate(entity);
            _genericRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _genericRepository.GetByIdAsync(id, ct);
            if (entity is null) return false;

            _genericRepository.Remove(entity); // hará soft-delete si T : BaseEntity
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }
    }
}
