
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
    [ApiVersionNeutral]
    [Authorize]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
      
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _userService.GetUsers();
            return Ok(users);
        }

        [HttpGet("{id}", Name = "GetUser")]
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
        public async Task<ActionResult<bool>> IsUnique([FromQuery] string username)
        {
            var isUnique = await _userService.IsUniqueUser(username);
            return Ok(isUnique);
        }




    }
}
