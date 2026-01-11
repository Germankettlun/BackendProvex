using System.Data;

namespace ProvexApi.Entities.Login
{
    public class RoleModule
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public int ModuleId { get; set; }
        public Module Module { get; set; } = null!;
    }
}
