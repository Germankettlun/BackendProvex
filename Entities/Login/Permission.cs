using Microsoft.Graph.Models;

namespace ProvexApi.Entities.Login
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
