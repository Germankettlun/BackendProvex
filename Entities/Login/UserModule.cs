namespace ProvexApi.Entities.Login
{
    public class UserModule
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int ModuleId { get; set; }
        public Module Module { get; set; } = null!;
    }
}
