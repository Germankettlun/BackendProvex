using System.ComponentModel.DataAnnotations;

namespace ProvexBackendAPI.Dto.Users
{
    public class UsersDto
    {
        public class CreateUserDto
        {
            [Required(ErrorMessage = "El campo username es requerido")]
            public string? Username { get; set; }
            [Required(ErrorMessage = "El campo name es requerido")]
            public string? Name { get; set; }
            [Required(ErrorMessage = "El campo password es requerido")]
            public string? Password { get; set; }
            [Required(ErrorMessage = "El campo role es requerido")]
            public string? Role { get; set; }
        }

        public class UserDataDto
        {
           // public string? Id { get; set; }
            public string? Username { get; set; }
            public string? Name { get; set; }
        }

        public class UserDto
        {
           // public string Id { get; set; } = string.Empty;

            public string? Name { get; set; }
            public string? Username { get; set; }
        }
    }
}
