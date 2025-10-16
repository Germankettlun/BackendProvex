using System;
using System.Collections.Generic;
using System.Linq;
using ProvexBackendAPI.Data.Models.Users;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Helpers.Mapping
{

    public static class AuthenticationProfile 
    {
        // User -> CreateUserDto (útil si devuelves algo tras crear)
        public static CreateUserDto ToCreateUserDto(this User e) => new()
        {
            Username = e.Username,
            Name = e.Name,
            Role = e.Role
        };

        // CreateUserDto -> User (alta)
        public static User ToNewUserEntity(this CreateUserDto dto) => new()
        {
            Username = dto.Username,
            Name = dto.Name,
            Role = dto.Role
           
        };

        // --- User -> UserDataDto ---
        public static UserDataDto ToUserDataDto(this User u) => new()
        {
            // OJO: ajusta Username vs UserName según tu modelo real.
            Username = u.Username,  // o u.UserName
            Name = u.Name
        };

        public static LoginDto ToLoginDto(this User e) => new()
        {
            Username = e.Username
        };

        // --- User -> LoginResponseDto (inyectando token y expiración) ---
        public static LoginResponseDto ToLoginResponseDto(
            this User u,
            string token,
            DateTimeOffset? expiresAt = null) => new()
            {
                User = u.ToUserDataDto(),
                Token = token,
                ExpiresAt = expiresAt
            };

        // Helpers para colecciones (por comodidad)
        public static IEnumerable<CreateUserDto> ToCreateUserDtos(this IEnumerable<User> users)
            => users.Select(u => u.ToCreateUserDto());
    }
}
