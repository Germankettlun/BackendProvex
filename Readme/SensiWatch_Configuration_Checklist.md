# ? Checklist de Configuración SensiWatch - ACTUALIZADO

## ?? INFORMACIÓN PENDIENTE DE SENSITECH

### Credenciales de API (CRÍTICO)
- [ ] **Subscription Key**: _Pendiente de Sensitech_
- [ ] **Username**: _Pendiente - Usuario específico para API_  
- [ ] **Password**: _Pendiente - Contraseña para API_
- [ ] **Program ID**: _Pendiente - ID numérico del programa_
- [ ] **Environment**: _¿Sandbox disponible?_

### Master Data IDs (REQUERIDO para Trip Creation)
- [ ] **Location IDs**: _Lista completa con nombres/códigos_
- [ ] **Product IDs**: _Productos permitidos en programa_
- [ ] **Trip Template ID**: _¿Usar 0 o ID específico?_
- [ ] **Required Fields**: _Campos obligatorios vs opcionales_

### Configuración de Push (CRÍTICO)
- [ ] **URL Base confirmada**: https://api.provexsa.cl/api/sensiwatch
- [ ] **Endpoints confirmados**: /device/activation y /device/report
- [ ] **Tipo de autenticación**: _¿Basic Auth, API Key, OAuth?_
- [ ] **Credenciales exactas**: _Usuario/contraseña para Basic Auth_
- [ ] **IP de SensiWatch confirmada**: _20.124.145.167_
- [ ] **Nuestra IP para whitelist**: _Para calls salientes_
- [ ] **Frecuencia de mensajes**: _Volumen esperado_
- [ ] **Límites de tamaño**: _Máximo por request_
- [ ] **Timeout limits**: _Tiempo máximo de respuesta_
- [ ] **Retry policy**: _Política de reintentos_

## ?? CONFIGURACIÓN TÉCNICA NUESTRA

### Prerrequisitos Técnicos
- [x] Certificado HTTPS válido configurado
- [x] Dominio público accesible
- [ ] **Firewall configurado**: _Permitir IP 20.124.145.167_
- [x] Base de datos actualizada con migración SensiWatch
- [ ] **Variables de entorno**: _Configurar credenciales cuando las recibamos_

### Implementación Completada
- [x] **Modelos y DTOs**: DeviceIdentity, SensorReading, etc.
- [x] **Entidades de BD**: ThermographDevice, Trip, Events, Readings
- [x] **Servicios**: SensiWatchService, SensiWatchApiClient
- [x] **Controlador**: SensiWatchController con endpoints push
- [x] **Migración de BD**: AddSensiWatchTables aplicada
- [x] **Configuración DI**: Servicios registrados en Program.cs

### Configuración de Seguridad (PENDIENTE DE INFORMACIÓN)
- [ ] **Credenciales Basic Auth**: _Actualizar cuando Sensitech confirme_
- [ ] **Whitelist de IP**: _Implementar en firewall/NSG_
- [ ] **Logs de seguridad**: _Configurados para monitoring_

## ?? PRUEBAS PENDIENTES

### Pruebas de Conectividad
- [x] Health check general: GET /health
- [x] Health check SensiWatch: GET /health/sensiwatch  
- [ ] **Prueba con credenciales reales**: _Cuando las recibamos_
- [ ] **Prueba de endpoint activation**: _Con datos de Sensitech_
- [ ] **Prueba de endpoint report**: _Con formato real_

### Coordinación con Sensitech (CRÍTICO)
- [ ] **Envío de información**: _Email con template creado_
- [ ] **Respuesta recibida**: _Con todas las credenciales_
- [ ] **Configuración aplicada**: _Credenciales en producción_
- [ ] **Pruebas coordinadas**: _Con dispositivos reales_
- [ ] **Activación programada**: _Ventana de go-live_
- [ ] **Contacto técnico**: _Para soporte 24/7_

## ?? MONITOREO Y ALERTAS (POST-ACTIVACIÓN)

### Logs y Monitoring
- [x] **Logs estructurados**: _Prefijo [SensiWatch] implementado_
- [ ] **Alertas por errores**: _Configurar cuando esté activo_
- [ ] **Dashboard de dispositivos**: _Implementar con datos reales_
- [ ] **Reportes de temperatura**: _Desarrollar con requirements reales_

### Métricas de Performance
- [ ] **Tiempo de respuesta**: _< 30 segundos para push endpoints_
- [ ] **Tasa de éxito**: _Procesamiento de reportes_
- [ ] **Volumen de datos**: _Dispositivos activos y lecturas/día_
- [ ] **Availability**: _Uptime de endpoints push_

## ?? ACCIONES INMEDIATAS REQUERIDAS

### 1. ENVÍO A SENSITECH (HOY)
- [ ] **Enviar email**: _Usar template Email_Template_Sensitech.md_
- [ ] **Adjuntar documentación**: _Implementation Guide_
- [ ] **Solicitar call técnica**: _Para aclarar detalles_

### 2. PREPARACIÓN TÉCNICA (ESTA SEMANA)
- [ ] **Configurar firewall**: _Permitir IP de SensiWatch_
- [ ] **Preparar environment variables**: _Template para credenciales_
- [ ] **Configurar monitoring**: _Logs y alertas básicas_

### 3. POST-RESPUESTA SENSITECH (SIGUIENTE SEMANA)
- [ ] **Aplicar configuración**: _Credenciales y settings recibidos_
- [ ] **Ejecutar pruebas**: _Con datos reales proporcionados_
- [ ] **Coordinar go-live**: _Ventana de activación_

---

## ?? ARCHIVOS DE REFERENCIA

- `SensiWatch_Implementation_Guide.md` - Guía técnica completa
- `SensiWatch_Consultas_Sensitech.md` - Lista detallada de consultas
- `Email_Template_Sensitech.md` - Template para envío
- `appsettings.SensiWatch.json` - Configuración de ejemplo
- `Scripts/SensiWatch_CreateTables.sql` - Script de BD
- `Deploy-SensiWatch.ps1` - Script de despliegue

---

**ESTADO ACTUAL**: ? **IMPLEMENTACIÓN COMPLETA** - ? **ESPERANDO INFORMACIÓN DE SENSITECH**

**PRÓXIMO PASO CRÍTICO**: ?? **ENVIAR EMAIL A SENSITECH CON CONSULTAS**

---
Fecha de actualización: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Estado: Implementación completa, esperando configuración de Sensitech