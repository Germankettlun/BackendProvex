using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Data.Models.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Repository
{
    public class AuthenticationRepository
    {

        //public async Task<LoginResponseDto> Login(LoginDto userLoginDto)
        //{
        //    if (string.IsNullOrEmpty(userLoginDto.Username))
        //    {
        //        return new LoginResponseDto()
        //        {
        //            Token = "",
        //            User = null,
        //            Message = "El Username es requerido"
        //        };
        //    }

        //    var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u => u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
        //    if (user == null)
        //    {
        //        return new LoginResponseDto()
        //        {
        //            Token = "",
        //            User = null,
        //            Message = "Username no encontrado"
        //        };
        //    }
        //    if (userLoginDto.Password == null)
        //    {
        //        return new LoginResponseDto()
        //        {
        //            Token = "",
        //            User = null,
        //            Message = "Password requerido"
        //        };
        //    }
        //    bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

        //    if (!isValid)
        //    {
        //        return new LoginResponseDto()
        //        {
        //            Token = "",
        //            User = null,
        //            Message = "Credenciales son incorrectas"
        //        };
        //    }

        //    //Generar JWT
        //    var handlerToken = new JwtSecurityTokenHandler();
        //    if (string.IsNullOrEmpty(secretKey))
        //    {
        //        throw new InvalidOperationException("SecretKey no esta configurada");
        //    }
        //    var roles = await _userManager.GetRolesAsync(user);
        //    var key = System.Text.Encoding.UTF8.GetBytes(secretKey);
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new[]
        //        {
        //        new Claim("id", user.Id.ToString()),
        //        new Claim("username", user.UserName ?? string.Empty),
        //        new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),

        //    }
        //        ),
        //        //Vendrá de configuración
        //        Expires = DateTime.UtcNow.AddHours(2),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    var token = handlerToken.CreateToken(tokenDescriptor);
        //    return new LoginResponseDto()
        //    {
        //        Token = handlerToken.WriteToken(token),
        //        /* User = new UserRegisterDto()
        //        {
        //            Username = user.Username,
        //            Name = user.Name,
        //            Role = user.Role,
        //            Password = user.Password ?? ""
        //        },
        //         */
        //        User = _mapper.Map<UserDataDto>(user),
        //        Message = "Usuario logueado correctamente."

        //    };

        //}

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

        //public async Task<UserDataDto> Register(CreateUserDto createUserDto)
        //{
        //    if (string.IsNullOrEmpty(createUserDto.Username))
        //    {
        //        throw new ArgumentNullException("El username es requerido");
        //    }

        //    if (createUserDto.Password == null)
        //    {
        //        throw new ArgumentNullException("La password es requerida");
        //    }

        //    var user = new ApplicationUser()
        //    {
        //        UserName = createUserDto.Username,
        //        Email = createUserDto.Username,
        //        NormalizedEmail = createUserDto.Username.ToUpper(),
        //        Name = createUserDto.Name
        //    };
        //    var result = await _userManager.CreateAsync(user, createUserDto.Password);
        //    if (result.Succeeded)
        //    {
        //        var userRole = createUserDto.Role ?? "User";
        //        var roleExists = await _roleManager.RoleExistsAsync(userRole);
        //        if (!roleExists)
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole<Guid>(userRole));
        //        }
        //        await _userManager.AddToRoleAsync(user, userRole);
        //        var createdUser = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == createUserDto.Username);
        //        return _mapper.Map<UserDataDto>(createdUser);
        //    }
        //    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        //    throw new ApplicationException($"No se pudo crear el registro: {errors}");
        //}

    }
}


