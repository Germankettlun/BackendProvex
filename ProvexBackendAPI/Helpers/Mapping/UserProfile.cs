using AutoMapper;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Dto.Authentication;
using ProvexBackendAPI.Dto.Users;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Helpers.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, CreateUserDto>().ReverseMap();
            CreateMap<User, LoginDto>().ReverseMap();
            CreateMap<User, LoginResponseDto>().ReverseMap();
            CreateMap<ApplicationUser, UserDataDto>().ReverseMap();
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
        }
    }
}
