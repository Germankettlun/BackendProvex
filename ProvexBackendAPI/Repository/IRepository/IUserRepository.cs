using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Dto;

namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        ICollection<ApplicationUser> GetUsers();
        ApplicationUser? GetUser(Guid id);
        bool IsUniqueUser(string username);
        Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
        Task<UserDataDto> Register(CreateUserDto createUserDto);

    }
}
