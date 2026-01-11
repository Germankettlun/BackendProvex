namespace ProvexApi.Entities.Login
{
    public class Module
    {
        public int ModuleId { get; set; }
        public string Type { get; set; } = null!;      // "CATALOGUE" | "MENU"
        public string Name { get; set; } = null!;      // código interno
        public string Label { get; set; } = null!;      // texto a mostrar
        public int? ParentId { get; set; }
        public Module? Parent { get; set; }
        public string? Route { get; set; }
        public string? Component { get; set; }
        public string? Icon { get; set; }
        public bool IsHidden { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }

        public ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();
        public ICollection<UserModule> UserModules { get; set; } = new List<UserModule>();
        public ICollection<Module> Children { get; set; } = new List<Module>();
    }
}
