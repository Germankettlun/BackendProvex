using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IAuthService
    {
        Task<UserDataDto> Register(CreateUserDto createUserDto);
        Task<LoginResponseDto> Login(LoginDto loginDto);

        Task ResetPasswordByUserNameAsync(AdminResetPasswordByUserNameRequest request);
    }
}
