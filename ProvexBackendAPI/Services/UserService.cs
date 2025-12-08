
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto.Users;
using ProvexBackendAPI.Helpers.Mapping;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
     

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
          
        }

        public async Task<UsersDto.UserDto?> GetUser(Guid id)
        {
            var entity = await _userRepository.GetUser(id);
            //return entity is null ? null : _mapper.Map<UserDto>(entity);
            return entity is null ? null : entity.ToUserDto();
        }

        public async Task<List<UsersDto.UserDto>> GetUsers()
        {
            var users = await _userRepository.GetUsers();
            //return _mapper.Map<List<UserDto>>(users);
            return users.ToUserDtos().ToList();
        }

        public Task<bool> IsUniqueUser(string username)
        {

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username es obligatorio.", nameof(username));

            return _userRepository.IsUniqueUser(username);
        }
    }
}
