// /Controllers/API/AdminController.cs

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using ProvexApi.Models.API;
using ProvexApi.Services.Token;
using ProvexApi.Services.Logs;
using ProvexApi.Data.DTOs.API;

namespace ProvexApi.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ApiCallLogger _callLogger;

        public AdminController(ITokenService tokenService, ApiCallLogger callLogger)
        {
            _tokenService = tokenService;
            _callLogger = callLogger;
        }

        /// <summary>
        /// 1) Genera y guarda AccessToken + RefreshToken
        /// </summary>
        [HttpPost("GenerateToken")]
        public IActionResult GenerateToken([FromBody] TokenRequest request)
        {
            try
            {
                //  ► desempaquetamos la tupla
                var (accessToken, refreshToken) = _tokenService.CreateToken(
                    request.ClientId,
                    request.RequestType,
                    out int expirationMinutes
                );

                var data = new
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpirationMinutes = expirationMinutes
                };

                return Ok(new ApiResponse<object>
                {
                    status = ResultEnum.SUCCESS,
                    data = data,
                    message = "Token generado correctamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    status = ResultEnum.FAIL,
                    message = $"Error al generar el token: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 2) Valida un token y devuelve sus Claims
        /// </summary>
        [HttpPost("ValidateToken")]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
        {
            if (_tokenService.ValidateToken(request.Token, out ClaimsPrincipal principal))
            {
                var claims = principal.Claims
                                      .Select(c => new { c.Type, c.Value });
                return Ok(new ApiResponse<object>
                {
                    status = ResultEnum.SUCCESS,
                    data = new { Valid = true, Claims = claims },
                    message = "Token válido."
                });
            }

            return Unauthorized(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                data = new { Valid = false },
                message = "Token inválido."
            });
        }

        /// <summary>
        /// 3) Refresca el AccessToken usando el RefreshToken
        /// </summary>
        [HttpPost("RefreshToken")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                //  ► desempaquetamos los nuevos tokens
                var (newAccessToken, newRefreshToken) = _tokenService.RefreshToken(request.RefreshToken);

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
    }
}
