using ProvexBackendAPI.Dto.Users;
using System.ComponentModel.DataAnnotations;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Dto.Authentication
{
    public class AuthenticationDto
    {
        public class LoginDto
        {
            [Required(ErrorMessage = "El campo username es requerido")]
            public string? Username { get; set; }
            [Required(ErrorMessage = "El campo password es requerido")]
            public string? Password { get; set; }
        }

        public class LoginResponseDto
        {
            public UserDataDto? User { get; set; }
            public string? Token { get; set; }
            public DateTimeOffset? ExpiresAt { get; set; }
      
        }

        public class UserRegisterDto
        {
            public UserDataDto? User { get; set; }
            public string? Token { get; set; }
            public string? Message { get; set; }
        }


    }
}
