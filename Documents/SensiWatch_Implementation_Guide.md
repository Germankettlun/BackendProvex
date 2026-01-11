# Guía de Implementación - Integración SensiWatch con ProvexApi

## ?? Resumen de Implementación

Se ha implementado exitosamente la integración completa de SensiWatch con el backend .NET 9, incluyendo:

### ? Componentes Implementados

1. **Modelos y DTOs**
   - `DeviceIdentity` - Identidad del dispositivo termógrafo
   - `DeviceLocation` - Ubicación geográfica
   - `SensorReading` - Lecturas de sensores
   - `DeviceActivationDto` - Activaciones de dispositivo
   - `DeviceReportDto` - Reportes de datos
   - `SensiWatchDtos` - DTOs para respuestas de API

2. **Entidades de Base de Datos**
   - `ThermographDevice` - Dispositivos termógrafo
   - `Trip` - Viajes/envíos
   - `ThermographEvent` - Eventos de dispositivos
   - `ThermographSensorReading` - Lecturas de sensores

3. **Servicios**
   - `SensiWatchService` - Procesamiento de datos entrantes
   - `SensiWatchApiClient` - Cliente para API saliente
   - Interfaces correspondientes

4. **Controlador**
   - `SensiWatchController` - Endpoints para push y consultas

5. **Base de Datos**
   - Migración EF Core generada: `AddSensiWatchTables`
   - Script SQL completo con índices optimizados
   - Vistas y procedimientos almacenados

## ??? Estructura de Base de Datos

### Tablas Principales
- `SensiWatch_Devices` - Dispositivos termógrafo
- `SensiWatch_Trips` - Viajes/envíos  
- `SensiWatch_Events` - Eventos (activaciones/reportes)
- `SensiWatch_SensorReadings` - Lecturas de sensores

### Vistas Optimizadas
- `VW_SensiWatch_TemperatureReadings` - Lecturas de temperatura con joins
- `VW_SensiWatch_ActiveDevices` - Estado de dispositivos activos

### Procedimientos Almacenados
- `SP_SensiWatch_GetTripSummary` - Resumen completo de trip

## ?? Endpoints Implementados

### Endpoints de Push (Para SensiWatch)
- `POST /api/sensiwatch/device/activation` - Recibir activaciones
- `POST /api/sensiwatch/device/report` - Recibir reportes de datos

### Endpoints de Consulta (Para la aplicación)
- `GET /api/sensiwatch/temperature-readings` - Obtener lecturas de temperatura
- `GET /api/sensiwatch/trip/{tripId}/summary` - Resumen de trip
- `GET /api/sensiwatch/devices/status` - Estado de dispositivos
- `GET /api/sensiwatch/system-info` - Información del sistema

### Health Checks
- `GET /health/sensiwatch` - Estado de conectividad con SensiWatch

## ?? Configuración Requerida

### 1. Variables de Entorno (Producción)
```bash
SENSIWATCH__SUBSCRIPTIONKEY=your_subscription_key_here
SENSIWATCH__USERNAME=your_sensiwatch_username
SENSIWATCH__PASSWORD=your_sensiwatch_password
SENSIWATCH__PROGRAMID=your_program_id_number
```

### 2. appsettings.json
```json
{
  "SensiWatch": {
    "BaseUrl": "https://developer.api.sensiwatch.com",
    "SubscriptionKey": "your_subscription_key",
    "Username": "your_username",
    "Password": "your_password",
    "ProgramId": 12345
  }
}
```

## ?? Configuración de Seguridad

### 1. Certificado HTTPS
- Requerido certificado válido emitido por CA reconocida
- SensiWatch rechaza certificados autofirmados

### 2. Autenticación Push
- HTTP Basic Auth en header Authorization
- Credenciales configurables en `ValidateAuthenticationHeader()`
- Por defecto: `sensitech:secreto123` (CAMBIAR EN PRODUCCIÓN)

### 3. Whitelist de IP
- Permitir solo IP de SensiWatch: `20.124.145.167`
- Configurar en firewall/NSG/Security Groups

## ?? Scripts de Base de Datos

### Ejecutar Migración EF Core
```bash
dotnet ef database update --context ProvexDbContext
```

### Ejecutar Script SQL Directo
```sql
-- Usar el archivo: Scripts/SensiWatch_CreateTables.sql
-- Incluye tablas, índices, vistas y procedimientos optimizados
```

## ?? Pruebas de Integración

### 1. Verificar Conectividad
```bash
curl https://tu-dominio.com/health/sensiwatch
```

### 2. Probar Endpoint de Activación
```bash
curl -X POST https://tu-dominio.com/api/sensiwatch/device/activation \
  -H "Authorization: Basic c2Vuc2l0ZWNoOnNlY3JldG8xMjM=" \
  -H "Content-Type: application/json" \
  -d '{
    "deviceIdentity": {
      "sensitechSerialNumber": "TEST001",
      "deviceName": "Test Device"
    },
    "activationTime": 1696356000000
  }'
```

### 3. Probar Endpoint de Reporte
```bash
curl -X POST https://tu-dominio.com/api/sensiwatch/device/report \
  -H "Authorization: Basic c2Vuc2l0ZWNoOnNlY3JldG8xMjM=" \
  -H "Content-Type: application/json" \
  -d '{
    "deviceIdentity": {
      "sensitechSerialNumber": "TEST001"
    },
    "sensors": [
      {
        "sensorId": "temperature", 
        "value": "23.5",
        "timestamp": {
          "deviceTime": 1696356000000,
          "receiveTime": 1696356030000
        }
      }
    ]
  }'
```

## ?? Consultas de Ejemplo

### Obtener Temperaturas por Rango
```bash
curl "https://tu-dominio.com/api/sensiwatch/temperature-readings?startDate=2024-01-01&endDate=2024-01-31" \
  -H "Authorization: Bearer your_jwt_token"
```

### Obtener Resumen de Trip
```bash
curl "https://tu-dominio.com/api/sensiwatch/trip/TRIP-123/summary" \
  -H "Authorization: Bearer your_jwt_token"
```

### Estado de Dispositivos
```bash
curl "https://tu-dominio.com/api/sensiwatch/devices/status" \
  -H "Authorization: Bearer your_jwt_token"
```

## ?? Logging y Monitoreo

### Logs de SensiWatch
- Prefijo `[SensiWatch]` para identificar logs relacionados
- Log estructurado con información de IP, dispositivo, trip
- Separación entre logs de push vs consultas

### Métricas Importantes
- Tiempo de respuesta < 30 segundos para endpoints push
- Tasa de éxito de procesamiento de reportes
- Cantidad de dispositivos activos
- Volumen de lecturas de temperatura por día

## ?? Próximos Pasos

### 1. Coordinación con Sensitech
- Proporcionar URL base HTTPS: `https://tu-dominio.com/api/sensiwatch`
- Configurar credenciales de autenticación Basic Auth
- Solicitar activación del push service

### 2. Monitoreo en Producción
- Configurar alertas para errores de conectividad
- Monitorear volumen de datos recibidos
- Validar integridad de datos de temperatura

### 3. Funcionalidades Adicionales
- Dashboard para visualizar datos de termógrafos
- Alertas por temperatura fuera de rango
- Reportes de cumplimiento de cadena de frío
- Integración con sistema de trazabilidad existente

## ?? Notas Importantes

1. **Respuesta Rápida**: Los endpoints de push SIEMPRE deben responder OK dentro de 30 segundos
2. **Procesamiento Asíncrono**: El procesamiento pesado se hace en background para no bloquear SensiWatch
3. **Manejo de Errores**: Los endpoints push siempre responden 200 OK para evitar reintentos innecesarios
4. **Escalabilidad**: La estructura está optimizada para manejar miles de lecturas por día
5. **Compatibilidad**: Implementación compatible con .NET 9 y Entity Framework Core

## ? Estado de Compilación

- ? Proyecto compila sin errores
- ? Migración de BD generada exitosamente  
- ? Servicios registrados en DI container
- ? Endpoints configurados correctamente
- ? Documentación Swagger actualizada

La integración está lista para despliegue y configuración con Sensitech.