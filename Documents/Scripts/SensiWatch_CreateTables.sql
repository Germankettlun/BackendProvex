-- Script SQL para crear las tablas de SensiWatch
-- Creado para .NET 9 - ProvexApi
-- Fecha: $(Get-Date)

-- =============================================================================
-- TABLA: SensiWatch_Devices
-- Descripción: Almacena información de dispositivos termógrafo de SensiWatch
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SensiWatch_Devices' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[SensiWatch_Devices] (
        [DeviceId] INT IDENTITY(1,1) NOT NULL,
        [SerialNumber] NVARCHAR(50) NOT NULL,
        [IMEI] NVARCHAR(20) NULL,
        [PlatformId] NVARCHAR(50) NULL,
        [DeviceName] NVARCHAR(100) NULL,
        [OrgUnit] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastSeen] DATETIME2 NULL,
        
        CONSTRAINT [PK_SensiWatch_Devices] PRIMARY KEY CLUSTERED ([DeviceId] ASC),
        CONSTRAINT [UK_SensiWatch_Devices_SerialNumber] UNIQUE ([SerialNumber])
    );
    
    PRINT 'Tabla SensiWatch_Devices creada exitosamente.'
END
ELSE
BEGIN
    PRINT 'Tabla SensiWatch_Devices ya existe.'
END
GO

-- =============================================================================
-- TABLA: SensiWatch_Trips
-- Descripción: Almacena información de trips/viajes en SensiWatch
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SensiWatch_Trips' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[SensiWatch_Trips] (
        [TripId] INT IDENTITY(1,1) NOT NULL,
        [SwpTripId] INT NOT NULL,
        [TripGuid] UNIQUEIDENTIFIER NOT NULL,
        [InternalTripId] NVARCHAR(100) NULL,
        [TrailerId] NVARCHAR(100) NULL,
        [PoNumber] NVARCHAR(100) NULL,
        [OrderNumber] NVARCHAR(100) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CompletedAt] DATETIME2 NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
        
        CONSTRAINT [PK_SensiWatch_Trips] PRIMARY KEY CLUSTERED ([TripId] ASC),
        CONSTRAINT [UK_SensiWatch_Trips_SwpTripId] UNIQUE ([SwpTripId])
    );
    
    -- Índices para optimizar consultas
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_Trips_InternalTripId] 
        ON [dbo].[SensiWatch_Trips] ([InternalTripId] ASC)
        WHERE [InternalTripId] IS NOT NULL;
    
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_Trips_Status] 
        ON [dbo].[SensiWatch_Trips] ([Status] ASC);
    
    PRINT 'Tabla SensiWatch_Trips creada exitosamente.'
END
ELSE
BEGIN
    PRINT 'Tabla SensiWatch_Trips ya existe.'
END
GO

-- =============================================================================
-- TABLA: SensiWatch_Events
-- Descripción: Almacena eventos de termógrafo (activaciones y reportes)
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SensiWatch_Events' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[SensiWatch_Events] (
        [EventId] BIGINT IDENTITY(1,1) NOT NULL,
        [DeviceId] INT NOT NULL,
        [TripId] INT NULL,
        [EventType] NVARCHAR(20) NOT NULL, -- 'Activation' o 'Report'
        [DeviceTimestamp] DATETIME2 NULL,
        [ReceivedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ActivationTime] DATETIME2 NULL,
        [Latitude] FLOAT NULL,
        [Longitude] FLOAT NULL,
        [LocationAddress] NVARCHAR(255) NULL,
        [CustomerShipmentId] NVARCHAR(100) NULL,
        
        CONSTRAINT [PK_SensiWatch_Events] PRIMARY KEY CLUSTERED ([EventId] ASC),
        CONSTRAINT [FK_SensiWatch_Events_Device] 
            FOREIGN KEY ([DeviceId]) REFERENCES [dbo].[SensiWatch_Devices]([DeviceId]) 
            ON DELETE CASCADE,
        CONSTRAINT [FK_SensiWatch_Events_Trip] 
            FOREIGN KEY ([TripId]) REFERENCES [dbo].[SensiWatch_Trips]([TripId]) 
            ON DELETE SET NULL,
        CONSTRAINT [CK_SensiWatch_Events_EventType] 
            CHECK ([EventType] IN ('Activation', 'Report'))
    );
    
    -- Índices para optimizar consultas por dispositivo y fecha
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_Events_DeviceId_ReceivedAt] 
        ON [dbo].[SensiWatch_Events] ([DeviceId] ASC, [ReceivedAt] DESC);
    
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_Events_TripId_DeviceTimestamp] 
        ON [dbo].[SensiWatch_Events] ([TripId] ASC, [DeviceTimestamp] ASC)
        WHERE [TripId] IS NOT NULL;
    
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_Events_EventType_ReceivedAt] 
        ON [dbo].[SensiWatch_Events] ([EventType] ASC, [ReceivedAt] DESC);
    
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_Events_CustomerShipmentId] 
        ON [dbo].[SensiWatch_Events] ([CustomerShipmentId] ASC)
        WHERE [CustomerShipmentId] IS NOT NULL;
    
    PRINT 'Tabla SensiWatch_Events creada exitosamente.'
END
ELSE
BEGIN
    PRINT 'Tabla SensiWatch_Events ya existe.'
END
GO

-- =============================================================================
-- TABLA: SensiWatch_SensorReadings
-- Descripción: Almacena lecturas individuales de sensores
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SensiWatch_SensorReadings' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[SensiWatch_SensorReadings] (
        [SensorReadingId] BIGINT IDENTITY(1,1) NOT NULL,
        [EventId] BIGINT NOT NULL,
        [SensorType] NVARCHAR(50) NOT NULL, -- 'temperature', 'humidity', 'battery', 'light', etc.
        [SensorValue] FLOAT NOT NULL,
        [DeviceTime] DATETIME2 NULL,
        [ReceiveTime] DATETIME2 NULL,
        [Unit] NVARCHAR(50) NULL, -- '°F', '%', 'lux', etc.
        
        CONSTRAINT [PK_SensiWatch_SensorReadings] PRIMARY KEY CLUSTERED ([SensorReadingId] ASC),
        CONSTRAINT [FK_SensiWatch_SensorReadings_Event] 
            FOREIGN KEY ([EventId]) REFERENCES [dbo].[SensiWatch_Events]([EventId]) 
            ON DELETE CASCADE
    );
    
    -- Índices para optimizar consultas por tipo de sensor y tiempo
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_SensorReadings_SensorType_DeviceTime] 
        ON [dbo].[SensiWatch_SensorReadings] ([SensorType] ASC, [DeviceTime] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_SensorReadings_EventId_SensorType] 
        ON [dbo].[SensiWatch_SensorReadings] ([EventId] ASC, [SensorType] ASC);
    
    -- Índice especializado para lecturas de temperatura (consulta más común)
    CREATE NONCLUSTERED INDEX [IX_SensiWatch_SensorReadings_Temperature] 
        ON [dbo].[SensiWatch_SensorReadings] ([DeviceTime] ASC, [SensorValue] ASC)
        WHERE [SensorType] = 'temperature';
    
    PRINT 'Tabla SensiWatch_SensorReadings creada exitosamente.'
END
ELSE
BEGIN
    PRINT 'Tabla SensiWatch_SensorReadings ya existe.'
END
GO

-- =============================================================================
-- VISTAS AUXILIARES PARA CONSULTAS COMUNES
-- =============================================================================

-- Vista para obtener temperaturas con información del dispositivo y trip
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_SensiWatch_TemperatureReadings')
    DROP VIEW [dbo].[VW_SensiWatch_TemperatureReadings]
GO

CREATE VIEW [dbo].[VW_SensiWatch_TemperatureReadings]
AS
SELECT 
    sr.SensorReadingId,
    sr.SensorValue AS Temperature,
    sr.DeviceTime,
    sr.ReceiveTime,
    sr.Unit,
    d.SerialNumber AS DeviceSerial,
    d.DeviceName,
    e.EventId,
    e.ReceivedAt AS EventReceivedAt,
    e.Latitude,
    e.Longitude,
    e.LocationAddress,
    e.CustomerShipmentId,
    t.InternalTripId,
    t.TrailerId,
    t.Status AS TripStatus,
    -- Información calculada
    DATEDIFF(MINUTE, sr.DeviceTime, sr.ReceiveTime) AS TransmissionDelayMinutes
FROM [dbo].[SensiWatch_SensorReadings] sr
INNER JOIN [dbo].[SensiWatch_Events] e ON sr.EventId = e.EventId
INNER JOIN [dbo].[SensiWatch_Devices] d ON e.DeviceId = d.DeviceId
LEFT JOIN [dbo].[SensiWatch_Trips] t ON e.TripId = t.TripId
WHERE sr.SensorType = 'temperature'
GO

-- Vista para resumen de dispositivos activos
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_SensiWatch_ActiveDevices')
    DROP VIEW [dbo].[VW_SensiWatch_ActiveDevices]
GO

CREATE VIEW [dbo].[VW_SensiWatch_ActiveDevices]
AS
SELECT 
    d.DeviceId,
    d.SerialNumber,
    d.DeviceName,
    d.CreatedAt,
    d.LastSeen,
    -- Último evento
    last_event.EventId AS LastEventId,
    last_event.EventType AS LastEventType,
    last_event.ReceivedAt AS LastEventTime,
    last_event.CustomerShipmentId AS CurrentShipmentId,
    -- Trip actual
    t.InternalTripId AS CurrentTripId,
    t.TrailerId AS CurrentTrailerId,
    t.Status AS CurrentTripStatus,
    -- Ubicación actual
    last_event.Latitude AS CurrentLatitude,
    last_event.Longitude AS CurrentLongitude,
    last_event.LocationAddress AS CurrentAddress,
    -- Última temperatura
    temp.Temperature AS LastTemperature,
    temp.TemperatureTime,
    -- Estado del dispositivo
    CASE 
        WHEN d.LastSeen IS NULL THEN 'Unknown'
        WHEN DATEDIFF(HOUR, d.LastSeen, GETUTCDATE()) < 1 THEN 'Active'
        WHEN DATEDIFF(HOUR, d.LastSeen, GETUTCDATE()) < 24 THEN 'Recent'
        WHEN DATEDIFF(DAY, d.LastSeen, GETUTCDATE()) < 7 THEN 'Inactive'
        ELSE 'Offline'
    END AS DeviceStatus
FROM [dbo].[SensiWatch_Devices] d
-- Último evento
OUTER APPLY (
    SELECT TOP 1 EventId, EventType, ReceivedAt, CustomerShipmentId, TripId, Latitude, Longitude, LocationAddress
    FROM [dbo].[SensiWatch_Events] e
    WHERE e.DeviceId = d.DeviceId
    ORDER BY e.ReceivedAt DESC
) last_event
-- Trip actual
LEFT JOIN [dbo].[SensiWatch_Trips] t ON last_event.TripId = t.TripId
-- Última temperatura
OUTER APPLY (
    SELECT TOP 1 sr.SensorValue AS Temperature, sr.DeviceTime AS TemperatureTime
    FROM [dbo].[SensiWatch_Events] e2
    INNER JOIN [dbo].[SensiWatch_SensorReadings] sr ON e2.EventId = sr.EventId
    WHERE e2.DeviceId = d.DeviceId AND sr.SensorType = 'temperature'
    ORDER BY sr.DeviceTime DESC
) temp
GO

-- =============================================================================
-- STORED PROCEDURES PARA OPERACIONES COMUNES
-- =============================================================================

-- Procedimiento para obtener resumen de trip
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_SensiWatch_GetTripSummary')
    DROP PROCEDURE [dbo].[SP_SensiWatch_GetTripSummary]
GO

CREATE PROCEDURE [dbo].[SP_SensiWatch_GetTripSummary]
    @InternalTripId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        -- Información del trip
        t.TripId,
        t.InternalTripId,
        t.TrailerId,
        t.CreatedAt,
        t.CompletedAt,
        t.Status,
        
        -- Información del dispositivo
        d.SerialNumber AS DeviceSerial,
        d.DeviceName,
        
        -- Estadísticas temporales
        MIN(e.DeviceTimestamp) AS StartTime,
        MAX(e.DeviceTimestamp) AS EndTime,
        DATEDIFF(HOUR, MIN(e.DeviceTimestamp), MAX(e.DeviceTimestamp)) AS DurationHours,
        
        -- Estadísticas de eventos
        COUNT(DISTINCT e.EventId) AS TotalEvents,
        COUNT(DISTINCT CASE WHEN e.EventType = 'Report' THEN e.EventId END) AS ReportEvents,
        COUNT(DISTINCT CASE WHEN e.EventType = 'Activation' THEN e.EventId END) AS ActivationEvents,
        
        -- Estadísticas de lecturas de temperatura
        COUNT(DISTINCT sr.SensorReadingId) AS TotalTemperatureReadings,
        MIN(CASE WHEN sr.SensorType = 'temperature' THEN sr.SensorValue END) AS MinTemperature,
        MAX(CASE WHEN sr.SensorType = 'temperature' THEN sr.SensorValue END) AS MaxTemperature,
        AVG(CASE WHEN sr.SensorType = 'temperature' THEN sr.SensorValue END) AS AvgTemperature,
        
        -- Estadísticas de ubicación
        COUNT(DISTINCT CASE WHEN e.Latitude IS NOT NULL THEN e.EventId END) AS LocationPoints
        
    FROM [dbo].[SensiWatch_Trips] t
    LEFT JOIN [dbo].[SensiWatch_Events] e ON t.TripId = e.TripId
    LEFT JOIN [dbo].[SensiWatch_Devices] d ON e.DeviceId = d.DeviceId
    LEFT JOIN [dbo].[SensiWatch_SensorReadings] sr ON e.EventId = sr.EventId
    WHERE t.InternalTripId = @InternalTripId
    GROUP BY t.TripId, t.InternalTripId, t.TrailerId, t.CreatedAt, t.CompletedAt, t.Status,
             d.SerialNumber, d.DeviceName
END
GO

-- =============================================================================
-- FUNCIONES AUXILIARES
-- =============================================================================

-- Función para convertir timestamps Unix a DateTime
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FN_SensiWatch_UnixToDateTime' AND type = 'FN')
    DROP FUNCTION [dbo].[FN_SensiWatch_UnixToDateTime]
GO

CREATE FUNCTION [dbo].[FN_SensiWatch_UnixToDateTime](@UnixTimestamp BIGINT)
RETURNS DATETIME2
AS
BEGIN
    RETURN DATEADD(MILLISECOND, @UnixTimestamp % 1000, DATEADD(SECOND, @UnixTimestamp / 1000, '1970-01-01 00:00:00'))
END
GO

-- =============================================================================
-- CONFIGURACIÓN DE PERMISOS Y SEGURIDAD
-- =============================================================================

-- Crear rol específico para SensiWatch si no existe
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'SensiWatchRole')
BEGIN
    CREATE ROLE [SensiWatchRole]
    PRINT 'Rol SensiWatchRole creado.'
END

-- Otorgar permisos al rol
GRANT SELECT, INSERT, UPDATE ON [dbo].[SensiWatch_Devices] TO [SensiWatchRole]
GRANT SELECT, INSERT, UPDATE ON [dbo].[SensiWatch_Trips] TO [SensiWatchRole]
GRANT SELECT, INSERT, UPDATE ON [dbo].[SensiWatch_Events] TO [SensiWatchRole]
GRANT SELECT, INSERT, UPDATE ON [dbo].[SensiWatch_SensorReadings] TO [SensiWatchRole]
GRANT SELECT ON [dbo].[VW_SensiWatch_TemperatureReadings] TO [SensiWatchRole]
GRANT SELECT ON [dbo].[VW_SensiWatch_ActiveDevices] TO [SensiWatchRole]
GRANT EXECUTE ON [dbo].[SP_SensiWatch_GetTripSummary] TO [SensiWatchRole]

PRINT 'Permisos otorgados al rol SensiWatchRole.'

-- =============================================================================
-- DATOS DE PRUEBA (OPCIONAL - COMENTAR EN PRODUCCIÓN)
-- =============================================================================

/*
-- Dispositivo de prueba
INSERT INTO [dbo].[SensiWatch_Devices] (SerialNumber, DeviceName, IMEI, PlatformId, OrgUnit)
VALUES ('TEST001', 'Dispositivo de Prueba', '123456789012345', 'TEST_PLATFORM', 'TEST_ORG');

-- Trip de prueba
INSERT INTO [dbo].[SensiWatch_Trips] (SwpTripId, TripGuid, InternalTripId, TrailerId, Status)
VALUES (999999, NEWID(), 'TRIP_TEST_001', 'TRAILER_TEST', 'Active');

PRINT 'Datos de prueba insertados (comentar en producción).'
*/

PRINT '========================================='
PRINT 'Script de SensiWatch completado exitosamente.'
PRINT 'Tablas creadas: 4'
PRINT 'Vistas creadas: 2' 
PRINT 'Procedimientos creados: 1'
PRINT 'Funciones creadas: 1'
PRINT 'Roles creados: 1'
PRINT '========================================='