
using Microsoft.EntityFrameworkCore;
using ProvexBackendAPI.Data;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Repository.IRepository;


namespace ProvexBackendAPI.Repository
{

    

    public class UserRepository : IUserRepository
    {

        public readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ApplicationUser?> GetUser(Guid id)
        {
            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<ApplicationUser>> GetUsers()
        {
            return await _db.Users.OrderBy(u => u.UserName).AsNoTracking().ToListAsync();
        }

        public async Task<bool> IsUniqueUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return true; 

          
            var normalized = username.Trim().ToUpperInvariant();

            var exists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.NormalizedUserName == normalized);

            return !exists; // true = es único; false = ya existe
        }

        public Task<int> SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
