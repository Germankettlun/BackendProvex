using ProvexApi.Models.SensiWatch;

namespace ProvexApi.Services.SensiWatch
{
    /// <summary>
    /// Interfaz para el servicio de SensiWatch que maneja el procesamiento de datos de termógrafos
    /// </summary>
    public interface ISensiWatchService
    {
        /// <summary>
        /// Procesa un reporte de dispositivo recibido desde SensiWatch
        /// </summary>
        /// <param name="report">Datos del reporte</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task ProcessDeviceReportAsync(DeviceReportDto report);

        /// <summary>
        /// Procesa una activación de dispositivo recibida desde SensiWatch
        /// </summary>
        /// <param name="activation">Datos de activación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task ProcessDeviceActivationAsync(DeviceActivationDto activation);

        /// <summary>
        /// Obtiene las lecturas de temperatura por rango de fechas
        /// </summary>
        /// <param name="startDate">Fecha de inicio</param>
        /// <param name="endDate">Fecha de fin</param>
        /// <param name="deviceSerial">Serial del dispositivo (opcional)</param>
        /// <returns>Lista de lecturas de temperatura</returns>
        Task<List<TemperatureReadingDto>> GetTemperatureReadingsAsync(DateTime startDate, DateTime endDate, string? deviceSerial = null);

        /// <summary>
        /// Obtiene el resumen de un trip específico
        /// </summary>
        /// <param name="internalTripId">ID interno del trip</param>
        /// <returns>Resumen del trip</returns>
        Task<TripSummaryDto?> GetTripSummaryAsync(string internalTripId);

        /// <summary>
        /// Obtiene los dispositivos activos
        /// </summary>
        /// <returns>Lista de dispositivos activos</returns>
        Task<List<DeviceStatusDto>> GetActiveDevicesAsync();
    }
}