using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexApi.Entities.SensiWatch
{
    /// <summary>
    /// Entidad que representa una lectura individual de sensor
    /// </summary>
    [Table("SensiWatch_SensorReadings")]
    public class ThermographSensorReading
    {
        [Key]
        public long SensorReadingId { get; set; }

        public long EventId { get; set; }

        [Required]
        [StringLength(50)]
        public string SensorType { get; set; } = string.Empty;

        public double SensorValue { get; set; }

        public DateTime? DeviceTime { get; set; }

        public DateTime? ReceiveTime { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        /// <summary>
        /// Navegación al evento
        /// </summary>
        [ForeignKey("EventId")]
        public virtual ThermographEvent Event { get; set; } = null!;

        /// <summary>
        /// Indica si es una lectura de temperatura
        /// </summary>
        public bool IsTemperature => SensorType.ToLower() == "temperature";

        /// <summary>
        /// Indica si es una lectura de humedad
        /// </summary>
        public bool IsHumidity => SensorType.ToLower() == "humidity";

        /// <summary>
        /// Indica si es una lectura de batería
        /// </summary>
        public bool IsBattery => SensorType.ToLower() == "battery";
    }
}