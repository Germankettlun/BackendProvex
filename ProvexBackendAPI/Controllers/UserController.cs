
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Data.Models;
using static ProvexBackendAPI.Dto.Users.UsersDto;
using ProvexBackendAPI.Services.IServices;


namespace ProvexBackendAPI.Controllers
{

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // [ApiVersion("1.0")]
    // [ApiVersion("2.0")]
    [ApiVersionNeutral]
    //[Authorize]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
      
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]

        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _userService.GetUsers();
            return Ok(users);
        }

        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _userService.GetUser(id);
            if (user == null)
            {
                return NotFound($"El usuario con el id {id} no existe");
            }
            return Ok(user);
        }

        [HttpGet("is-unique")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> IsUnique([FromQuery] string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("username is required.");

            var isUnique = await _userService.IsUniqueUser(username);
            return Ok(isUnique);
        }




    }
}
