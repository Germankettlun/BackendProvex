using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProvexApi.Data;
using ProvexApi.Entities.Login;
using ProvexApi.Models.API;       // ApiResponse, ResultEnum

namespace ProvexApi.Controllers.BackEnd.Login
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class ModulesController : ControllerBase
    {
        private readonly ExtranetDBContext _context;
        public ModulesController(ExtranetDBContext context)
        {
            _context = context;
        }

        // GET: api/modules
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _context.Modules
               .OrderBy(m => m.SortOrder)
               .Select(m => new {
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
               })
               .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    status = ResultEnum.SUCCESS,
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    status = ResultEnum.FAIL,
                    message = $"Error al obtener modules : {ex.Message}"
                });
            }          
        }

        // POST: api/modules
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Module dto)
        {
            var mod = new Module
            {
                Type = dto.Type,
                Name = dto.Name,
                Label = dto.Label,
                ParentId = dto.ParentId,
                Route = dto.Route,
                Component = dto.Component,
                Icon = dto.Icon,
                IsHidden = dto.IsHidden,
                SortOrder = dto.SortOrder,
                IsEnabled = true
            };
            _context.Modules.Add(mod);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Module>
            {
                status = ResultEnum.SUCCESS,
                data = mod
            });
        }

        // PUT: api/modules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Module dto)
        {
            var mod = await _context.Modules.FindAsync(id);
            if (mod == null) return NotFound(new ApiResponse<object> { status = ResultEnum.FAIL, message = "Módulo no encontrado." });

            mod.Label = dto.Label;
            mod.ParentId = dto.ParentId;
            mod.Route = dto.Route;
            mod.Component = dto.Component;
            mod.Icon = dto.Icon;
            mod.IsHidden = dto.IsHidden;
            mod.SortOrder = dto.SortOrder;
            mod.IsEnabled = dto.IsEnabled;

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<Module>
            {
                status = ResultEnum.SUCCESS,
                data = mod
            });
        }

        // DELETE: api/modules/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mod = await _context.Modules.FindAsync(id);
            if (mod == null) return NotFound(new ApiResponse<object> {status = ResultEnum.FAIL, message = "Módulo no encontrado." });

            _context.Modules.Remove(mod);
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<object> {status = ResultEnum.SUCCESS, message = "Módulo eliminado." });
        }
    }
}
