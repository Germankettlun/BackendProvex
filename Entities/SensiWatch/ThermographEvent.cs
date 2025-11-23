using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexApi.Entities.SensiWatch
{
    /// <summary>
    /// Entidad que representa un evento de termógrafo (activación o reporte)
    /// </summary>
    [Table("SensiWatch_Events")]
    public class ThermographEvent
    {
        [Key]
        public long EventId { get; set; }

        public int DeviceId { get; set; }

        public int? TripId { get; set; }

        [Required]
        [StringLength(20)]
        public string EventType { get; set; } = string.Empty; // "Activation" o "Report"

        public DateTime? DeviceTimestamp { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ActivationTime { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [StringLength(255)]
        public string? LocationAddress { get; set; }

        [StringLength(100)]
        public string? CustomerShipmentId { get; set; }

        /// <summary>
        /// Navegación al dispositivo
        /// </summary>
        [ForeignKey("DeviceId")]
        public virtual ThermographDevice Device { get; set; } = null!;

        /// <summary>
        /// Navegación al trip
        /// </summary>
        [ForeignKey("TripId")]
        public virtual Trip? Trip { get; set; }

        /// <summary>
        /// Navegación a las lecturas de sensores
        /// </summary>
        public virtual ICollection<ThermographSensorReading> SensorReadings { get; set; } = new List<ThermographSensorReading>();
    }
}