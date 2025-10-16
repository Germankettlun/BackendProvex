using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Dto.Authentication;
using ProvexBackendAPI.Dto.Users;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;
using System.Collections.Generic;
using System.Linq;

namespace ProvexBackendAPI.Helpers.Mapping
{
    public static class UserProfile
    {

        //CreateMap<ApplicationUser, UserDataDto>().ReverseMap();
        //CreateMap<ApplicationUser, UserDto>().ReverseMap();

        // ApplicationUser -> UserDataDto
        public static UserDataDto ToUserDataDto(this ApplicationUser u) => new()
        {
            Username = u.UserName,
            Name = u.Name
        };

        // ApplicationUser -> UserDto
        public static UserDto ToUserDto(this ApplicationUser u) => new()
        {
            Username = u.UserName,
            Name = u.Name
        };

        // Helpers para colecciones (opcional)
        public static IEnumerable<UserDto> ToUserDtos(this IEnumerable<ApplicationUser> users)
            => users.Select(u => u.ToUserDto());

        public static IEnumerable<UserDataDto> ToUserDataDtos(this IEnumerable<ApplicationUser> users)
            => users.Select(u => u.ToUserDataDto());

    }
}
