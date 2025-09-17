using AutoMapper;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto.Users;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UsersDto.UserDto?> GetUser(Guid id)
        {
            var entity = await _userRepository.GetUser(id);
            return entity is null ? null : _mapper.Map<UserDto>(entity);
        }

        public async Task<List<UsersDto.UserDto>> GetUsers()
        {
            var users = await _userRepository.GetUsers();
            return _mapper.Map<List<UserDto>>(users);
        }

        public Task<bool> IsUniqueUser(string username)
        {
           return _userRepository.IsUniqueUser(username);
        }
    }
}
