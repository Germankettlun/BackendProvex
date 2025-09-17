using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto.Authentication;
using ProvexBackendAPI.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        //private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthService(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<Guid>> roleManager, ITokenService tokenService, IConfiguration config, IMapper mapper, IUserService userService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userService = userService;
            _tokenService = tokenService;
            //_config = config;
            _mapper = mapper;
        }
        public async Task<AuthenticationDto.LoginResponseDto> Login(AuthenticationDto.LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Username))
            {
                return new LoginResponseDto
                {
                    Token = "",
                    User = null,
                    ExpiresAt = null,
                    Message = "El Username es requerido"
                };
            }

            if (string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return new LoginResponseDto
                {
                    Token = "",
                    User = null,
                    ExpiresAt = null,
                    Message = "Password requerido"
                };
            }

            var input = loginDto.Username.Trim();
            ApplicationUser? user = input.Contains('@')
                ? await _userManager.FindByEmailAsync(input)
                : await _userManager.FindByNameAsync(input);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Token = "",
                    User = null,
                    ExpiresAt = null,
                    Message = "Username no encontrado"
                };
            }

            // Verificar contraseña ( Identity )
            var check = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
            if (!check.Succeeded)
            {
                return new LoginResponseDto
                {
                    Token = "",
                    User = null,
                    ExpiresAt = null,
                    Message = "Credenciales son incorrectas"
                };
            }

            // ====== Generar JWT ======
            //        var jwtSec = _config.GetSection("Jwt");
            //        var secretKey = jwtSec["Key"];
            //        if (string.IsNullOrWhiteSpace(secretKey))
            //            throw new InvalidOperationException("JWT:Key no está configurada");

            //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            //        var roles = await _userManager.GetRolesAsync(user);

            //        var claims = new List<Claim>
            //{
            //        new Claim("id", user.Id.ToString()),
            //        new Claim("username", user.UserName ?? string.Empty),
            //        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            //        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //};
            //        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            //        var minutes = int.TryParse(jwtSec["AccessTokenMinutes"], out var m) ? m : 120;
            //        var expires = DateTime.UtcNow.AddMinutes(minutes);

            //        var tokenDescriptor = new SecurityTokenDescriptor
            //        {
            //            Subject = new ClaimsIdentity(claims),
            //            Expires = expires,
            //            Issuer = jwtSec["Issuer"],
            //            Audience = jwtSec["Audience"],
            //            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            //        };

            //        var handler = new JwtSecurityTokenHandler();
            //        var token = handler.CreateToken(tokenDescriptor);

            var token = await _tokenService.GenerateAsync(
            user,
            rolesProvider: async u => await _userManager.GetRolesAsync(u)
            );

            // Map a tu DTO de usuario (AutoMapper)
            var userDto = _mapper.Map<UserDataDto>(user);

            return new LoginResponseDto
            {
                //Token = handler.WriteToken(token),
                Token = token.Token,
                User = userDto,
                ExpiresAt = token.ExpiresAtUtc,
                Message = "Usuario logueado correctamente."
            };


        }


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

            var exists = await _userService.IsUniqueUser(createUserDto.Username);

                if (!exists)
                throw new ApplicationException("El usuario ya existe.");


            // Mapear DTO -> Identity user
            var user = new ApplicationUser()
            {   
                UserName = createUserDto.Username,
                Email = createUserDto.Username,
                NormalizedEmail = createUserDto.Username.ToUpper(),
                Name = createUserDto.Name
            };
            user.Id = Guid.NewGuid();
           

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (result.Succeeded)
            {
                var userRole = string.IsNullOrWhiteSpace(createUserDto.Role) ? "User" : createUserDto.Role;

                var roleExists = await _roleManager.RoleExistsAsync(userRole);
                if (!roleExists)
                {
                    // OJO: usamos IdentityRole<Guid> (no el IdentityRole por defecto de string)
                    var identityRole = new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = userRole,
                        NormalizedName = userRole.ToUpperInvariant()
                    };
                    var roleCreate = await _roleManager.CreateAsync(identityRole);
                    if (!roleCreate.Succeeded)
                    {
                        var errs = string.Join(" | ", roleCreate.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new ApplicationException($"No se pudo crear el rol '{userRole}'. {errs}");
                    }
                }

                var addToRole = await _userManager.AddToRoleAsync(user, userRole);
                if (!addToRole.Succeeded)
                {
                    var errs = string.Join(" | ", addToRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new ApplicationException($"No se pudo asignar el rol '{userRole}' al usuario. {errs}");
                }
            }

            var createdUser = await _userManager.FindByNameAsync(createUserDto.Username);
            if (createdUser == null)
                throw new ApplicationException("No se pudo recuperar el usuario recién creado.");

            return _mapper.Map<UserDataDto>(createdUser);


        }


    }
}
