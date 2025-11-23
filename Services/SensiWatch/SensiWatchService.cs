using Microsoft.EntityFrameworkCore;
using ProvexApi.Data;
using ProvexApi.Entities.SensiWatch;
using ProvexApi.Models.SensiWatch;

namespace ProvexApi.Services.SensiWatch
{
    /// <summary>
    /// Servicio para procesar y almacenar datos de termógrafos SensiWatch
    /// </summary>
    public class SensiWatchService : ISensiWatchService
    {
        private readonly ProvexDbContext _context;
        private readonly ILogger<SensiWatchService> _logger;

        public SensiWatchService(ProvexDbContext context, ILogger<SensiWatchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessDeviceReportAsync(DeviceReportDto report)
        {
            try
            {
                _logger.LogInformation("[SensiWatch] Procesando reporte de dispositivo: {DeviceSerial}",
                    report.DeviceSerial);

                // 1. Buscar o crear dispositivo
                var device = await GetOrCreateDeviceAsync(report.DeviceIdentity);

                // 2. Buscar o crear trip
                Trip? trip = null;
                if (report.SwpTrip != null)
                {
                    trip = await GetOrCreateTripAsync(report.SwpTrip);
                }

                // 3. Crear evento
                var evt = new ThermographEvent
                {
                    DeviceId = device.DeviceId,
                    TripId = trip?.TripId,
                    EventType = "Report",
                    CustomerShipmentId = report.DeviceIdentity?.CustomerShipmentId
                };

                // Establecer timestamp del dispositivo (último sensor o ubicación)
                if (report.Sensors?.Any() == true || report.Locations?.Any() == true)
                {
                    var deviceTimes = new List<DateTime>();
                    
                    if (report.Sensors != null)
                    {
                        deviceTimes.AddRange(report.Sensors
                            .Where(s => s.Timestamp != null)
                            .Select(s => s.Timestamp!.DeviceDateTime));
                    }

                    if (report.Locations != null)
                    {
                        deviceTimes.AddRange(report.Locations
                            .Where(l => l.Timestamp != null)
                            .Select(l => l.Timestamp!.DeviceDateTime));
                    }

                    if (deviceTimes.Any())
                    {
                        evt.DeviceTimestamp = deviceTimes.Max();
                    }
                }

                // Guardar ubicación si existe
                var lastLocation = report.LastLocation;
                if (lastLocation != null)
                {
                    evt.Latitude = lastLocation.Latitude;
                    evt.Longitude = lastLocation.Longitude;
                    evt.LocationAddress = lastLocation.Address;
                }

                _context.ThermographEvents.Add(evt);
                await _context.SaveChangesAsync();

                // 4. Guardar lecturas de sensores
                if (report.Sensors?.Any() == true)
                {
                    await SaveSensorReadingsAsync(evt.EventId, report.Sensors);
                }

                // 5. Actualizar última actividad del dispositivo
                device.LastSeen = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("[SensiWatch] Reporte procesado exitosamente. EventId: {EventId}, Sensores: {SensorCount}",
                    evt.EventId, report.Sensors?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error procesando reporte de dispositivo: {DeviceSerial}",
                    report.DeviceSerial);
                throw;
            }
        }

        public async Task ProcessDeviceActivationAsync(DeviceActivationDto activation)
        {
            try
            {
                _logger.LogInformation("[SensiWatch] Procesando activación de dispositivo: {DeviceSerial}",
                    activation.DeviceIdentity?.SensitechSerialNumber);

                var device = await GetOrCreateDeviceAsync(activation.DeviceIdentity);

                var evt = new ThermographEvent
                {
                    DeviceId = device.DeviceId,
                    EventType = "Activation",
                    ActivationTime = activation.ActivationDateTime,
                    DeviceTimestamp = activation.ActivationDateTime,
                    CustomerShipmentId = activation.DeviceIdentity?.CustomerShipmentId
                };

                _context.ThermographEvents.Add(evt);
                device.LastSeen = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("[SensiWatch] Activación procesada exitosamente. EventId: {EventId}",
                    evt.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error procesando activación de dispositivo: {DeviceSerial}",
                    activation.DeviceIdentity?.SensitechSerialNumber);
                throw;
            }
        }

        public async Task<List<TemperatureReadingDto>> GetTemperatureReadingsAsync(DateTime startDate, DateTime endDate, string? deviceSerial = null)
        {
            var query = _context.ThermographSensorReadings
                .Include(r => r.Event)
                .ThenInclude(e => e.Device)
                .Include(r => r.Event)
                .ThenInclude(e => e.Trip)
                .Where(r => r.SensorType.ToLower() == "temperature" &&
                           r.DeviceTime >= startDate &&
                           r.DeviceTime <= endDate);

            if (!string.IsNullOrEmpty(deviceSerial))
            {
                query = query.Where(r => r.Event.Device.SerialNumber == deviceSerial);
            }

            return await query
                .OrderBy(r => r.DeviceTime)
                .Select(r => new TemperatureReadingDto
                {
                    ReadingId = r.SensorReadingId,
                    DeviceSerial = r.Event.Device.SerialNumber,
                    InternalTripId = r.Event.Trip != null ? r.Event.Trip.InternalTripId : null,
                    Temperature = r.SensorValue,
                    DeviceTime = r.DeviceTime ?? DateTime.MinValue,
                    ReceiveTime = r.ReceiveTime ?? DateTime.MinValue,
                    Latitude = r.Event.Latitude,
                    Longitude = r.Event.Longitude,
                    LocationAddress = r.Event.LocationAddress
                })
                .ToListAsync();
        }

        public async Task<TripSummaryDto?> GetTripSummaryAsync(string internalTripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Events)
                .ThenInclude(e => e.Device)
                .Include(t => t.Events)
                .ThenInclude(e => e.SensorReadings)
                .FirstOrDefaultAsync(t => t.InternalTripId == internalTripId);

            if (trip == null)
                return null;

            var temperatureReadings = trip.Events
                .SelectMany(e => e.SensorReadings)
                .Where(r => r.IsTemperature)
                .ToList();

            var locations = trip.Events
                .Where(e => e.Latitude.HasValue && e.Longitude.HasValue)
                .OrderBy(e => e.DeviceTimestamp)
                .Select(e => new LocationPointDto
                {
                    Latitude = e.Latitude!.Value,
                    Longitude = e.Longitude!.Value,
                    Timestamp = e.DeviceTimestamp ?? e.ReceivedAt,
                    Address = e.LocationAddress
                })
                .ToList();

            return new TripSummaryDto
            {
                InternalTripId = trip.InternalTripId ?? string.Empty,
                TrailerId = trip.TrailerId,
                DeviceSerial = trip.Events.FirstOrDefault()?.Device.SerialNumber,
                StartTime = trip.Events.Min(e => e.DeviceTimestamp),
                EndTime = trip.Events.Max(e => e.DeviceTimestamp),
                TotalReadings = temperatureReadings.Count,
                MinTemperature = temperatureReadings.Any() ? temperatureReadings.Min(r => r.SensorValue) : null,
                MaxTemperature = temperatureReadings.Any() ? temperatureReadings.Max(r => r.SensorValue) : null,
                AvgTemperature = temperatureReadings.Any() ? temperatureReadings.Average(r => r.SensorValue) : null,
                Route = locations
            };
        }

        public async Task<List<DeviceStatusDto>> GetActiveDevicesAsync()
        {
            var devices = await _context.ThermographDevices
                .Include(d => d.Events.OrderByDescending(e => e.ReceivedAt).Take(1))
                .ThenInclude(e => e.SensorReadings)
                .Include(d => d.Events)
                .ThenInclude(e => e.Trip)
                .ToListAsync();

            return devices.Select(d =>
            {
                var lastEvent = d.Events.OrderByDescending(e => e.ReceivedAt).FirstOrDefault();
                var lastTemperature = lastEvent?.SensorReadings
                    .Where(r => r.IsTemperature)
                    .OrderByDescending(r => r.DeviceTime)
                    .FirstOrDefault()?.SensorValue;
                var batteryLevel = lastEvent?.SensorReadings
                    .Where(r => r.IsBattery)
                    .OrderByDescending(r => r.DeviceTime)
                    .FirstOrDefault()?.SensorValue;

                return new DeviceStatusDto
                {
                    SerialNumber = d.SerialNumber,
                    DeviceName = d.DeviceName,
                    LastSeen = d.LastSeen,
                    Status = GetDeviceStatus(d.LastSeen),
                    CurrentTripId = lastEvent?.Trip?.InternalTripId,
                    LastTemperature = lastTemperature,
                    BatteryLevel = batteryLevel
                };
            }).ToList();
        }

        private async Task<ThermographDevice> GetOrCreateDeviceAsync(DeviceIdentity? deviceIdentity)
        {
            if (deviceIdentity?.SensitechSerialNumber == null)
                throw new ArgumentException("Device serial number is required");

            var device = await _context.ThermographDevices
                .FirstOrDefaultAsync(d => d.SerialNumber == deviceIdentity.SensitechSerialNumber);

            if (device == null)
            {
                device = new ThermographDevice
                {
                    SerialNumber = deviceIdentity.SensitechSerialNumber,
                    IMEI = deviceIdentity.Imei,
                    PlatformId = deviceIdentity.PlatformId,
                    DeviceName = deviceIdentity.DeviceName,
                    OrgUnit = deviceIdentity.OrgUnit
                };

                _context.ThermographDevices.Add(device);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[SensiWatch] Nuevo dispositivo creado: {SerialNumber}", device.SerialNumber);
            }

            return device;
        }

        private async Task<Trip> GetOrCreateTripAsync(SwpTripInfo swpTrip)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.SwpTripId == swpTrip.TripId);

            if (trip == null)
            {
                trip = new Trip
                {
                    SwpTripId = swpTrip.TripId,
                    TripGuid = swpTrip.TripGuid,
                    InternalTripId = swpTrip.InternalTripId,
                    TrailerId = swpTrip.TrailerId,
                    PoNumber = swpTrip.Destinations?.LastOrDefault()?.PoNumber,
                    OrderNumber = swpTrip.Destinations?.LastOrDefault()?.OrderNumber
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[SensiWatch] Nuevo trip creado: {InternalTripId}", trip.InternalTripId);
            }

            return trip;
        }

        private async Task SaveSensorReadingsAsync(long eventId, List<SensorReading> sensors)
        {
            foreach (var sensor in sensors)
            {
                if (sensor.ValueAsDouble.HasValue && sensor.Timestamp != null)
                {
                    var reading = new ThermographSensorReading
                    {
                        EventId = eventId,
                        SensorType = sensor.SensorId ?? "unknown",
                        SensorValue = sensor.ValueAsDouble.Value,
                        DeviceTime = sensor.Timestamp.DeviceDateTime,
                        ReceiveTime = sensor.Timestamp.ReceiveDateTime,
                        Unit = DetermineUnit(sensor.SensorId)
                    };

                    _context.ThermographSensorReadings.Add(reading);
                }
            }

            await _context.SaveChangesAsync();
        }

        private static string? DetermineUnit(string? sensorType)
        {
            return sensorType?.ToLower() switch
            {
                "temperature" => "°F",
                "humidity" => "%",
                "battery" => "%",
                "light" => "lux",
                "rssi" => "dBm",
                _ => null
            };
        }

        private static string GetDeviceStatus(DateTime? lastSeen)
        {
            if (!lastSeen.HasValue)
                return "Unknown";

            var timeSinceLastSeen = DateTime.UtcNow - lastSeen.Value;

            return timeSinceLastSeen.TotalHours switch
            {
                < 1 => "Active",
                < 24 => "Recent",
                < 168 => "Inactive", // 7 días
                _ => "Offline"
            };
        }
    }
}