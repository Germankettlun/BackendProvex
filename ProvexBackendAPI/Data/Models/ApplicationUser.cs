using Microsoft.AspNetCore.Identity;

namespace ProvexBackendAPI.Data.Models

{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }
    }
}
