using ProvexBackendAPI.Data.Models.Users;


namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetUsers();
        Task<ApplicationUser?> GetUser(Guid id);
        Task<ApplicationUser?> GetUserByUsername(string username);
        Task<bool> IsUniqueUser(string username);
        Task<int> SaveChangesAsync();

    }
}
