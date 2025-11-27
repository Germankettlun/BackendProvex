
using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto.Authentication;
using ProvexBackendAPI.Exceptions;
using ProvexBackendAPI.Helpers.Mapping;
using ProvexBackendAPI.Services.IServices;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProvexBackendAPI.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITokenService tokenService;
        private readonly IUserService userService;
        private readonly ITemporadasService temporada;



        public AuthService(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<Guid>> roleManager, ITokenService tokenService, IUserService userService, ITemporadasService temporada)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            this.userService = userService;
            this.tokenService = tokenService;
            this.temporada = temporada;


        }
        public async Task<AuthenticationDto.LoginResponseDto> Login(AuthenticationDto.LoginDto loginDto)
        {

            if (loginDto is null || string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                throw new BadRequestException("Usuario y contraseña son obligatorios.");

            var input = loginDto.Username.Trim();
            ApplicationUser? user = input.Contains('@')
                ? await _userManager.FindByEmailAsync(input)
                : await _userManager.FindByNameAsync(input);
            
           
            if (user is null)
                throw new UnauthorizedException("Usuario o contraseña incorrectos.");

            // Verificar contraseña ( Identity )
            var check = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

            if (!check.Succeeded)
                throw new UnauthorizedException("Usuario o contraseña incorrectos.");

            var roles = await _userManager.GetRolesAsync(user);

            var token = await tokenService.GenerateTokenAsync(user.UserName!,roles.ToList());

            var semana = await temporada.GetSemanaAsync(codigoEmpresa: "PRX", codigoTemporada: null, soloVigente: true);


            //var userDto = _mapper.Map<UserDataDto>(user);
            var userDto = user.ToUserDataDto();

            return new LoginResponseDto
            {
                Token = token.Token,
                User = userDto,
                ExpiresAt = token.ExpiresAtUtc,
                AnoBaseSemanaVigente = semana?.AnioBase,
                SemanaBaseSemanaVigente = semana is null ? null : semana.SemanaBase
               
            };


        }


        public async Task<UserDataDto> Register(CreateUserDto createUserDto)
        {
            try { 
            
            if (createUserDto is null)
                throw new ApplicationException("Datos de registro requeridos.");

            var username = createUserDto.Username?.Trim();

            if (string.IsNullOrWhiteSpace(username))
                throw new ApplicationException("El username es requerido.");

            if (string.IsNullOrWhiteSpace(createUserDto.Password))
                throw new ApplicationException("La password es requerida.");

            var exists = await userService.IsUniqueUser(username);

                if (!exists)
                throw new ApplicationException("El usuario ya existe.");


            
            var user = new ApplicationUser()
            {   
                UserName = username,
                Email = username,
                NormalizedEmail = username.ToUpper(),
                Name = createUserDto.Name
            };

            user.Id = Guid.NewGuid();

            const string DefaultRole = "User";

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
            {
                throw new ApplicationException("Error al crear el usuario.");
            }
                // Si no especifica rol, usamos el por defecto
                var requestedRole = string.IsNullOrWhiteSpace(createUserDto.Role) ? DefaultRole : createUserDto.Role.Trim();

                // Si el rol no existe, degradamos al rol por defecto
                var roleExists = await _roleManager.RoleExistsAsync(requestedRole);

                if (!roleExists)
                {

                    requestedRole = DefaultRole;
                }

                var addToRole = await _userManager.AddToRoleAsync(user, requestedRole);
                if (!addToRole.Succeeded)
                {
                    throw new ApplicationException("No se pudo asignar el rol al usuario.");
                }
           

            var createdUser = await _userManager.FindByNameAsync(createUserDto.Username);
            if (createdUser == null)
                throw new ApplicationException("Error al crear el usuario.");

            //return _mapper.Map<UserDataDto>(createdUser);
            return createdUser.ToUserDataDto();

            }
            catch (Exception)
            {
                throw new Exception("Error al crear usuario.");
            }

        }

        public async Task ResetPasswordByUserNameAsync(AdminResetPasswordByUserNameRequest request)
        {
            try { 

            if (string.IsNullOrWhiteSpace(request.UserName))
                throw new ArgumentException("userName es obligatorio.", nameof(request.UserName));

            if (!string.IsNullOrWhiteSpace(request.ConfirmNewPassword) &&
               request.NewPassword != request.ConfirmNewPassword)
                throw new ArgumentException("Las contraseñas ingresadas no coinciden.");

            var normalizedUserName = request.UserName.Trim();

            var user = await _userManager.FindByNameAsync(normalizedUserName);
            if (user is null)
                throw new InvalidOperationException("Error al restablecer la contraseña.");

            // Validar contraseña con las reglas de Identity
            foreach (var validator in _userManager.PasswordValidators)
            {
                var validationResult = await validator.ValidateAsync(_userManager, user, request.NewPassword);
                if (!validationResult.Succeeded)
                {
                    var errors = string.Join(" | ", validationResult.Errors.Select(e => e.Description));
                    throw new ValidationException("Error al restablecer la contraseña.");
                }
            }

            // Generar token y resetear
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo restablecer la contraseña: {errors}");
            }

            await _userManager.UpdateSecurityStampAsync(user);

            }
            catch (Exception)
            {
                throw new Exception("Error al restablecer contraseña.");
            }
        }


    }
}
