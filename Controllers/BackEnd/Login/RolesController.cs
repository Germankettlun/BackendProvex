using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProvexApi.Data;
using ProvexApi.Entities.Login;
using ProvexApi.Models.API; // ApiResponse, ResultEnum
using System.Linq;

namespace ProvexApi.Controllers.BackEnd.Login
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly ExtranetDBContext _context;
        public RolesController(ExtranetDBContext context)
        {
            _context = context;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _context.Roles
                .Include(r => r.RoleModules)
                .ThenInclude(rm => rm.Module)
                .Select(r => new
                {
                    id = r.RoleId,
                    name = r.Name,
                    label = r.Description,
                    status = "ENABLE", // o como manejes el estado
                    permission = r.RoleModules.Select(rm => new {
                        id = rm.Module.ModuleId.ToString(),
                        parentId = rm.Module.ParentId != null ? rm.Module.ParentId.ToString() : null,
                        name = rm.Module.Name,
                        label = rm.Module.Label,
                        type = rm.Module.Type,
                        route = rm.Module.Route,
                        order = rm.Module.SortOrder,
                        icon = rm.Module.Icon,
                        component = rm.Module.Component,
                        hide = rm.Module.IsHidden,
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                status = ResultEnum.SUCCESS,
                data = roles
            });
        }

        // GET: api/roles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RoleModules)
                .ThenInclude(rm => rm.Module)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
                return NotFound(new ApiResponse<object> { status = ResultEnum.FAIL, message = "Role not found" });

            var data = new
            {
                id = role.RoleId,
                name = role.Name,
                label = role.Description,
                status = "ENABLE",
                modules = role.RoleModules.Select(rm => rm.Module.ModuleId.ToString()).ToList()
            };

            return Ok(new ApiResponse<object> { status = ResultEnum.SUCCESS, data = data });
        }

        // POST: api/roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleDto dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Label
            };

            // Relacionar módulos (permissions)
            if (dto.ModulesIds != null && dto.ModulesIds.Any())
            {
                role.RoleModules = dto.ModulesIds
                    .Select(pid => new RoleModule
                    {
                        ModuleId = int.Parse(pid)
                    })
                    .ToList();
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object> { status = ResultEnum.SUCCESS, data = new { role.RoleId } });
        }

        // PUT: api/roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleDto dto)
        {
            var role = await _context.Roles
                .Include(r => r.RoleModules)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
                return NotFound(new ApiResponse<object> { status = ResultEnum.FAIL, message = "Role not found" });

            role.Name = dto.Name;
            role.Description = dto.Label;

            // Actualiza los módulos
            role.RoleModules.Clear();
            if (dto.ModulesIds != null && dto.ModulesIds.Any())
            {
                role.RoleModules = dto.ModulesIds
                    .Select(pid => new RoleModule
                    {
                        RoleId = id,
                        ModuleId = int.Parse(pid)
                    })
                    .ToList();
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<object> { status = ResultEnum.SUCCESS });
        }

        // DELETE: api/roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound(new ApiResponse<object> { status = ResultEnum.FAIL, message = "Role not found" });

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object> { status = ResultEnum.SUCCESS });
        }
    }

    // DTO de entrada
    public class RoleDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public List<string>? ModulesIds { get; set; }
    }
}
