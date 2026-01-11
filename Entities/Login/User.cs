using System;
using System.ComponentModel.DataAnnotations;

namespace ProvexApi.Entities.Login
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;


        // Navegación a UserRoles y UserModules
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserModule> UserModules { get; set; } = new List<UserModule>();
    }
}
