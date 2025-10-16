
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto.Authentication;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Mapping;
using ProvexBackendAPI.Services.IServices;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
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
        private readonly ISemanaVigenteProvider _semanaProvider;


        public AuthService(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<Guid>> roleManager, ITokenService tokenService, IUserService userService,ISemanaVigenteProvider semanaProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userService = userService;
            _tokenService = tokenService;
            _semanaProvider = semanaProvider;

        }
        public async Task<AuthenticationDto.LoginResponseDto> Login(AuthenticationDto.LoginDto loginDto)
        {

            if (loginDto is null) throw new ArgumentNullException(nameof(loginDto));

            if (string.IsNullOrWhiteSpace(loginDto.Username))
                throw new ValidationException("El username es requerido.");

            if (string.IsNullOrWhiteSpace(loginDto.Password))
                throw new ValidationException("El password es requerido.");

            var input = loginDto.Username.Trim();
            ApplicationUser? user = input.Contains('@')
                ? await _userManager.FindByEmailAsync(input)
                : await _userManager.FindByNameAsync(input);
            
            var semana = await _semanaProvider.GetAsync(codigoEmpresa: "PRX", codigoTemporada: null, soloVigente: true);

            if (user is null)
            throw new InvalidCredentialException("Usuario o contraseña inválidos.");

            // Verificar contraseña ( Identity )
            var check = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

            if (check.IsLockedOut)
                throw new UnauthorizedAccessException("locked_out");
            if (check.IsNotAllowed)
                throw new UnauthorizedAccessException("not_allowed");
            if (check.RequiresTwoFactor)
                throw new UnauthorizedAccessException("requires_2fa");
            if (!check.Succeeded)
                throw new InvalidCredentialException("Usuario o contraseña inválidos.");

            var token = await _tokenService.GenerateAsync(
            user,
            rolesProvider: async u => await _userManager.GetRolesAsync(u)
            );

            
            //var userDto = _mapper.Map<UserDataDto>(user);
            var userDto = user.ToUserDataDto();

            return new LoginResponseDto
            {
                //Token = handler.WriteToken(token),
                Token = token.Token,
                User = userDto,
                ExpiresAt = token.ExpiresAtUtc,
                AnoBaseSemanaVigente = semana?.AnioBase,
                SemanaBaseSemanaVigente = semana is null ? null : semana.SemanaBase
               
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

            //return _mapper.Map<UserDataDto>(createdUser);
            return createdUser.ToUserDataDto();
           


        }


    }
}
