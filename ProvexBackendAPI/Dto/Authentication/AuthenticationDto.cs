using ProvexBackendAPI.Dto.Users;
using System.ComponentModel.DataAnnotations;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Dto.Authentication
{
    public class AuthenticationDto
    {
        public class LoginDto
        {
            [Required(ErrorMessage = "El username es requerido")]
            public string? Username { get; set; }
            [Required(ErrorMessage = "El password es requerido")]
            public string? Password { get; set; }
        }

        public class LoginResponseDto
        {
            public UserDataDto? User { get; set; }
            public string? Token { get; set; }
            public DateTimeOffset? ExpiresAt { get; set; }
            public int? AnoBaseSemanaVigente { get; set; }
            public string? SemanaBaseSemanaVigente { get; set; }
          

        }

        public class UserRegisterDto
        {
            public UserDataDto? User { get; set; }
            public string? Token { get; set; }
            public string? Message { get; set; }
        }

        public class AdminResetPasswordByUserNameRequest
        {
            public string UserName { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string? ConfirmNewPassword { get; set; }
        }


    }
}
