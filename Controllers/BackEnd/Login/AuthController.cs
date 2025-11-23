using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProvexApi.Data;
using ProvexApi.Data.DTOs;            // LoginRequestDto, RefreshTokenRequest
using ProvexApi.Models.API;          // ApiResponse, ResultEnum
using ProvexApi.Entities.Login;      // User, UserRole, RoleModule, Module
using ProvexApi.Services.Token;
using ProvexApi.Data.DTOs.API;

namespace ProvexApi.Controllers.BackEnd.Login
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ExtranetDBContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(
            ExtranetDBContext context,
            ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// 1) LOGIN: genera y guarda AccessToken + RefreshToken
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    status = ResultEnum.FAIL,
                    message = "Datos de login inválidos."
                });
            }

            // Cargamos el usuario con sus roles
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username == loginRequest.Username &&
                    u.IsActive
                );

            if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    status = ResultEnum.FAIL,
                    message = "Usuario o contraseña incorrectos."
                });
            }

            // Generamos tokens
            var (accessToken, refreshToken) = _tokenService.CreateToken(
                clientId: user.Id.ToString(),
                requestType: "login_web",
                expirationMinutes: out int expirationMinutes
            );

            // 1) Extraemos los RoleIds
            var roleIds = user.UserRoles
                              .Select(ur => ur.RoleId)
                              .ToList();

            // 2) Obtenemos todos los módulos de esos roles
            var modules = await _context.RoleModules
                .Where(rm => roleIds.Contains(rm.RoleId))
                .Include(rm => rm.Module)
                .Select(rm => rm.Module)
                .Where(m => m.IsEnabled && !m.IsHidden)
                .Distinct()
                .ToListAsync();

            // 3) Mapear a un DTO anónimo (o podrías crear un ModuleDto)
            var moduleList = modules.Select(m => new
            {
                m.ModuleId,
                m.Type,
                m.Name,
                m.Label,
                m.ParentId,
                m.Route,
                m.Component,
                m.Icon,
                m.IsHidden,
                m.SortOrder,
                m.IsEnabled   
            }).ToList();

            // 4) Empaquetar la respuesta
            var data = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpirationMinutes = expirationMinutes,
                User = new
                {
                    user.Id,
                    user.Username,
                    user.Email
                },
                Modules = moduleList
            };

            return Ok(new ApiResponse<object>
            {
                status = ResultEnum.SUCCESS,
                data = data,
                message = "Login exitoso."
            });
        }

        /// <summary>
        /// 2) REFRESH: rota refreshToken antiguo y emite nuevos tokens
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult Refresh([FromBody] RefreshTokenRequest dto)
        {
            try
            {
                var (newAccessToken, newRefreshToken) = _tokenService.RefreshToken(dto.RefreshToken);

                var data = new
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };

                return Ok(new ApiResponse<object>
                {
                    status = ResultEnum.SUCCESS,
                    data = data,
                    message = "Token renovado."
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    status = ResultEnum.FAIL,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// 3) LOGOUT: revoca (desactiva) el refreshToken actual
        /// </summary>
        [HttpPost("logout")]
        [Authorize(Policy = "LocalOnly")]
        public IActionResult Logout([FromBody] RefreshTokenRequest dto)
        {
            _tokenService.RevokeRefreshToken(dto.RefreshToken);

            return Ok(new ApiResponse<object>
            {
                status = ResultEnum.SUCCESS,
                message = "Sesión cerrada correctamente."
            });
        }

        // Utilizado para verificar la contraseña
        private bool VerifyPassword(string password, string storedHash)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var inputBytes = Encoding.UTF8.GetBytes(password);
            var hashedBytes = sha.ComputeHash(inputBytes);
            var hashBase64 = Convert.ToBase64String(hashedBytes);

            return hashBase64 == storedHash;
        }
    }
}
