using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexApi.Entities.SensiWatch
{
    /// <summary>
    /// Entidad que representa un dispositivo termógrafo de SensiWatch
    /// </summary>
    [Table("SensiWatch_Devices")]
    public class ThermographDevice
    {
        [Key]
        public int DeviceId { get; set; }

        [Required]
        [StringLength(50)]
        public string SerialNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string? IMEI { get; set; }

        [StringLength(50)]
        public string? PlatformId { get; set; }

        [StringLength(100)]
        public string? DeviceName { get; set; }

        [StringLength(50)]
        public string? OrgUnit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastSeen { get; set; }

        /// <summary>
        /// Navegación a los eventos del dispositivo
        /// </summary>
        public virtual ICollection<ThermographEvent> Events { get; set; } = new List<ThermographEvent>();
    }
}