using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProvexApi.Data.DTOs.API
{
    public class CreateUserDto
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } // plaintext

        [Required]
        public int RoleId { get; set; }

        [Required]
        public string Status { get; set; } // "ENABLE" o "DISABLE"

        public IFormFile? Avatar { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Code { get; set; }
        public string? About { get; set; }
        public List<int>? ModuleIds { get; set; }
    }
}
