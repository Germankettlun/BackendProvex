using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ProvexBackendAPI.Exceptions;
using ProvexBackendAPI.Services;
using ProvexBackendAPI.Services.IServices;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersionNeutral]

    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService auth;

        public AuthenticationController(IAuthService auth)
        {
            this.auth = auth;

        }

        
        [HttpPost("Login", Name = "LoginUser")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto userLoginDtoDto)
        {

            var user = await auth.Login(userLoginDtoDto);
            return Ok(user);
        }

        [HttpPost("Register", Name = "RegisterUser")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {

             var result = await auth.Register(createUserDto);
            return Ok(result);
        }

        [HttpPost("admin/resetPassword")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordByUserNameRequest request)
        {

            await auth.ResetPasswordByUserNameAsync(request);

            return NoContent(); 
        }
    }



}
