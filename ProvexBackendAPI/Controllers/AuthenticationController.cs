using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ProvexBackendAPI.Dto.Authentication.AuthenticationDto;
using static ProvexBackendAPI.Dto.Users.UsersDto;

namespace ProvexBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {


        //[HttpPost("Login", Name = "LoginUser")]
        //[AllowAnonymous]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> LoginUser([FromBody] LoginDto userLoginDtoDto)
        //{
        //    if (userLoginDtoDto == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var user = await _userRepository.Login(userLoginDtoDto);
        //    if (user == null)
        //    {
        //        return Unauthorized();
        //    }
        //    return Ok(user);
        //}

        //[HttpPost(Name = "RegisterUser")]
        //[AllowAnonymous]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        //{
        //    if (createUserDto == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (string.IsNullOrWhiteSpace(createUserDto.Username))
        //    {
        //        return BadRequest("Username es requerido");
        //    }

        //    if (!_userRepository.IsUniqueUser(createUserDto.Username))
        //    {
        //        return BadRequest("El usuario ya existe");
        //    }

        //    var result = await _userRepository.Register(createUserDto);
        //    if (result == null)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario");
        //    }
        //    return CreatedAtRoute("GetUser", result);
        //}
    }



}
