using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexApi.Entities.SensiWatch
{
    /// <summary>
    /// Entidad que representa un trip/viaje en SensiWatch
    /// </summary>
    [Table("SensiWatch_Trips")]
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        public int SwpTripId { get; set; }

        public Guid TripGuid { get; set; }

        [StringLength(100)]
        public string? InternalTripId { get; set; }

        [StringLength(100)]
        public string? TrailerId { get; set; }

        [StringLength(100)]
        public string? PoNumber { get; set; }

        [StringLength(100)]
        public string? OrderNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Navegación a los eventos del trip
        /// </summary>
        public virtual ICollection<ThermographEvent> Events { get; set; } = new List<ThermographEvent>();
    }
}