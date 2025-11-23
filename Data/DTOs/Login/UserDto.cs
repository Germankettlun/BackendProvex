using ProvexApi.Controllers.BackEnd.Login;
using System;
using System.Collections.Generic;

namespace ProvexApi.Data.DTOs.API
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Code { get; set; }
        public string? About { get; set; }

        // Un solo rol por usuario (ajusta a lista si usas multirol)
        public RoleDto? Role { get; set; }

        // Módulos asociados como lista de IDs
        public List<int> ModuleIds { get; set; }
    }
}