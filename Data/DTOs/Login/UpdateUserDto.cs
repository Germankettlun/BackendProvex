using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ProvexApi.Data.DTOs.API
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public string? Status { get; set; }
        public IFormFile? Avatar { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Code { get; set; }
        public string? About { get; set; }
        public List<int>? ModuleIds { get; set; }
    }
}
