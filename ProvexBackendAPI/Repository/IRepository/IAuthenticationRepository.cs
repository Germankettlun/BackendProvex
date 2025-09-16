using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IAuthenticationRepository
    {
        Task<LoginResponseDto> Login(LoginDto userLoginDto);
        Task<UserDataDto> Register(CreateUserDto createUserDto);
    }
}
