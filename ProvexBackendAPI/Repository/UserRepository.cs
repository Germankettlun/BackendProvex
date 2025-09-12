using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Data;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProvexBackendAPI.Repository
{

    

    public class UserRepository : IUserRepository
    {

        public readonly AppDbContext _db;
        private string? secretKey;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;


        public UserRepository(AppDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IMapper mapper)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public ApplicationUser? GetUser(Guid id)
        {
            return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
        }

        public ICollection<ApplicationUser> GetUsers()
        {
            return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
        }

        public bool IsUniqueUser(string username)
        {
            var normalized = username.ToUpper().Trim();
            return !_db.Users.Any(u => u.NormalizedUserName == normalized);
        }

        public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
        {
            if (string.IsNullOrEmpty(userLoginDto.Username))
            {
                return new UserLoginResponseDto()
                {
                    Token = "",
                    User = null,
                    Message = "El Username es requerido"
                };
            }

            var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u => u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
            if (user == null)
            {
                return new UserLoginResponseDto()
                {
                    Token = "",
                    User = null,
                    Message = "Username no encontrado"
                };
            }
            if (userLoginDto.Password == null)
            {
                return new UserLoginResponseDto()
                {
                    Token = "",
                    User = null,
                    Message = "Password requerido"
                };
            }
            bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
           
            if (!isValid)
            {
                return new UserLoginResponseDto()
                {
                    Token = "",
                    User = null,
                    Message = "Credenciales son incorrectas"
                };
            }

            //Generar JWT
            var handlerToken = new JwtSecurityTokenHandler();
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("SecretKey no esta configurada");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var key = System.Text.Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),

            }
                ),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = handlerToken.CreateToken(tokenDescriptor);
            return new UserLoginResponseDto()
            {
                Token = handlerToken.WriteToken(token),
                /* User = new UserRegisterDto()
                {
                    Username = user.Username,
                    Name = user.Name,
                    Role = user.Role,
                    Password = user.Password ?? ""
                },
                 */
                User = _mapper.Map<UserDataDto>(user),
                Message = "Usuario logueado correctamente."

            };

        }

        /* public async Task<User> Register(CreateUserDto createUserDto)
        {
            var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
            var user = new User()
            {
                Username = createUserDto.Username ?? "No Username",
                Name = createUserDto.Name,
                Role = createUserDto.Role,
                Password = encriptedPassword
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        } */

        public async Task<UserDataDto> Register(CreateUserDto createUserDto)
        {
            if (string.IsNullOrEmpty(createUserDto.Username))
            {
                throw new ArgumentNullException("El username es requerido");
            }

            if (createUserDto.Password == null)
            {
                throw new ArgumentNullException("La password es requerida");
            }

            var user = new ApplicationUser()
            {
                UserName = createUserDto.Username,
                Email = createUserDto.Username,
                NormalizedEmail = createUserDto.Username.ToUpper(),
                Name = createUserDto.Name
            };
            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (result.Succeeded)
            {
                var userRole = createUserDto.Role ?? "User";
                var roleExists = await _roleManager.RoleExistsAsync(userRole);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(userRole));
                }
                await _userManager.AddToRoleAsync(user, userRole);
                var createdUser = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == createUserDto.Username);
                return _mapper.Map<UserDataDto>(createdUser);
            }
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"No se pudo crear el registro: {errors}");
        }
    }
}
