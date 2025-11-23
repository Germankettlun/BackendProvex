using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProvexApi.Data;
using ProvexApi.Data.DTOs.API;
using ProvexApi.Entities.Login;
using ProvexApi.Models.API; // ApiResponse<T> y ResultEnum
using System.Data;
using System.Security.Claims;

[Authorize(Policy = "LocalOnly")]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ExtranetDBContext _context;
    private readonly IWebHostEnvironment _env;

    public UserController(ExtranetDBContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null)
    {
        var query = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserModules)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

        var total = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Status = u.IsActive ? "ENABLE" : "DISABLE",
            AvatarUrl = null,
            Role = u.UserRoles.Select(ur => ur.Role == null ? null : new RoleDto { RoleId = ur.Role.RoleId, Name = ur.Role.Name, Label = ur.Role.Description }).FirstOrDefault(),
            ModuleIds = u.UserModules.Select(um => um.ModuleId).ToList()
        });

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            data = new { data = result, total },
            message = "OK"
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserModules)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return Ok(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                data = null,
                message = "No se encontró el usuario"
            });

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Status = user.IsActive ? "ENABLE" : "DISABLE",
            AvatarUrl = null,
            Role = user.UserRoles.Select(ur => ur.Role == null ? null : new RoleDto { RoleId = ur.Role.RoleId, Name = ur.Role.Name, Label = ur.Role.Description }).FirstOrDefault(),
            ModuleIds = user.UserModules.Select(um => um.ModuleId).ToList()
        };

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            data = dto,
            message = "OK"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateUserDto dto)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = dto.Status == "ENABLE",
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Roles
        if (dto.RoleId != 0)
        {
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = dto.RoleId });
        }

        // Modules
        if (dto.ModuleIds != null && dto.ModuleIds.Any())
        {
            foreach (var mid in dto.ModuleIds)
                _context.UserModules.Add(new UserModule { UserId = user.Id, ModuleId = mid });
        }
        else if (dto.RoleId != 0) // Hereda módulos del rol si no se seleccionan explícitamente
        {
            var roleModuleIds = await _context.RoleModules
                .Where(rm => rm.RoleId == dto.RoleId)
                .Select(rm => rm.ModuleId)
                .ToListAsync();

            foreach (var mid in roleModuleIds)
                _context.UserModules.Add(new UserModule { UserId = user.Id, ModuleId = mid });
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            data = new { user.Id },
            message = "Usuario creado correctamente"
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateUserDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserModules)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return Ok(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                data = null,
                message = "Usuario no encontrado"
            });
        }

        // Actualiza campos...
        user.Username = dto.Username ?? user.Username;
        user.Email = dto.Email ?? user.Email;
        if (!string.IsNullOrEmpty(dto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        if (dto.Status != null)
            user.IsActive = dto.Status == "ENABLE";

        // Actualiza roles
        user.UserRoles.Clear();
        if (dto.RoleId.HasValue && dto.RoleId.Value != 0)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = dto.RoleId.Value });

        // Actualiza módulos
        user.UserModules.Clear();
        if (dto.ModuleIds != null && dto.ModuleIds.Any())
            foreach (var mid in dto.ModuleIds)
                user.UserModules.Add(new UserModule { UserId = user.Id, ModuleId = mid });

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            data = new { user.Id },
            message = "Usuario actualizado correctamente"
        });
    }

    [HttpPut("{id}/modules")]
    public async Task<IActionResult> UpdateModules(Guid id, [FromForm] string moduleIds)
    {
        // moduleIds debe venir como JSON string (ej: "[1,2,3]")
        if (string.IsNullOrWhiteSpace(moduleIds))
        {
            return BadRequest(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                message = "No se recibieron módulos"
            });
        }

        var user = await _context.Users
            .Include(u => u.UserModules)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                message = "Usuario no encontrado"
            });
        }

        // Borra todos los módulos actuales
        _context.UserModules.RemoveRange(user.UserModules);

        // Deserializa los nuevos módulos y asigna
        var modulesToAdd = System.Text.Json.JsonSerializer.Deserialize<List<int>>(moduleIds);
        if (modulesToAdd != null && modulesToAdd.Any())
        {
            foreach (var mid in modulesToAdd)
                _context.UserModules.Add(new UserModule { UserId = user.Id, ModuleId = mid });
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            message = "Módulos del usuario actualizados correctamente"
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserModules)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return Ok(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                data = null,
                message = "Usuario no encontrado"
            });
        }

        // Elimina relaciones hijas primero
        _context.UserRoles.RemoveRange(user.UserRoles);
        _context.UserModules.RemoveRange(user.UserModules);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            data = null,
            message = "Usuario eliminado correctamente"
        });
    }

    [HttpGet("{id}/modules")]
    public async Task<IActionResult> GetUserModules(Guid id)
    {
        var moduleIds = await _context.UserModules
            .Where(um => um.UserId == id)
            .Select(um => um.ModuleId)
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            data = moduleIds,
            message = "OK"
        });
    }

    [HttpGet("modules")]
    public async Task<IActionResult> GetModulesForCurrentUser()
    {
        var userIdStr =
        User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst("userId")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier"))?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
        {
            return Unauthorized(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                message = "El token no corresponde a un usuario válido."
            });
        }

        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(new ApiResponse<object>
            {
                status = ResultEnum.FAIL,
                message = "No se pudo determinar el usuario actual"
            });

        // Trae SOLO los módulos asignados a ese usuario
        var modules = await _context.UserModules
            .Where(um => um.UserId == userId)
            .Include(um => um.Module)
            .Select(um => new
            {
                um.Module.ModuleId,
                um.Module.Type,
                um.Module.Name,
                um.Module.Label,
                um.Module.ParentId,
                um.Module.Route,
                um.Module.Component,
                um.Module.Icon,
                um.Module.IsHidden,
                um.Module.SortOrder,
                um.Module.IsEnabled
            })
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            status = ResultEnum.SUCCESS,
            data = modules,
            message = "OK"
        });
    }
}
