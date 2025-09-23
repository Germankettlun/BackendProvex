namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
