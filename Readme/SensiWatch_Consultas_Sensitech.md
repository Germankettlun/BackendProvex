# ?? CONSULTAS CRÍTICAS PARA SENSITECH

## ?? CONFIGURACIÓN DE API

### 1. Credenciales de Acceso
- [ ] **Subscription Key**: Clave de suscripción del Developer Portal
- [ ] **Username/Password**: Credenciales específicas para API (diferentes al portal web)
- [ ] **Program ID**: Identificador numérico del programa asignado
- [ ] **Environment**: ¿Sandbox/Testing disponible antes de producción?

### 2. Master Data IDs
- [ ] **Location IDs**: IDs numéricos de ubicaciones (origen/destino)
- [ ] **Product IDs**: IDs de productos permitidos en el programa
- [ ] **Carrier IDs**: IDs de transportistas si es requerido
- [ ] **Trip Template ID**: ¿Usar 0 (default) o ID específico?

## ?? CONFIGURACIÓN DE PUSH ENDPOINTS

### 1. URL y Autenticación
**Nuestra URL base propuesta**: `https://api.provexsa.cl/api/sensiwatch`

**Preguntas críticas**:
- [ ] **¿Confirman esta URL base?**: https://api.provexsa.cl/api/sensiwatch
- [ ] **¿Qué tipo de autenticación prefieren?**:
  - [ ] HTTP Basic Auth (actual implementación)
  - [ ] API Key en header
  - [ ] OAuth 2.0
  - [ ] Custom token
- [ ] **¿Cuáles serán las credenciales exactas?**: Usuario/contraseña para Basic Auth
- [ ] **¿Necesitan IP whitelist de nuestro servidor?**: Para calls salientes a su API

### 2. Endpoints de Push
**Endpoints que implementamos**:
- `POST /api/sensiwatch/device/activation` - Activaciones de dispositivo
- `POST /api/sensiwatch/device/report` - Reportes de datos

**Consultas**:
- [ ] **¿Confirman estos paths?**: /device/activation y /device/report
- [ ] **¿Envían otros tipos de mensajes?**: Además de activation/report
- [ ] **¿Cuál es la frecuencia esperada?**: Mensajes por hora/día
- [ ] **¿Hay límites de tamaño?**: Máximo de datos por request

### 3. Formato de Datos
- [ ] **¿El formato JSON actual es correcto?**: Según documentación v4
- [ ] **¿Hay campos adicionales específicos de nuestro programa?**
- [ ] **¿Los timestamps vienen en milisegundos Unix?**: Confirmación
- [ ] **¿Las temperaturas vienen en Fahrenheit?**: Confirmación de unidades

### 4. Manejo de Errores y Reintentos
- [ ] **¿Cuánto tiempo esperan respuesta?**: Límite de timeout
- [ ] **¿Cuántos reintentos hacen?**: Si no reciben 200 OK
- [ ] **¿Por cuánto tiempo reintentan?**: Política de reintentos
- [ ] **¿Necesitan algún formato específico de error?**: En respuestas 4xx/5xx

## ??? CONFIGURACIÓN DE TRIPS

### 1. Creación de Trips via API
**Datos que necesitamos**:
- [ ] **Location IDs disponibles**: Lista completa con nombres/códigos
- [ ] **Product IDs disponibles**: Lista de productos para trips
- [ ] **Campos obligatorios**: ¿Qué campos son requeridos vs opcionales?
- [ ] **Validaciones**: ¿Qué validaciones aplican a InternalTripID?

### 2. Dispositivos y Asignación
- [ ] **¿Cómo se asignan dispositivos a trips?**: En creación o separado
- [ ] **¿Dispositivos pueden cambiar de trip?**: Durante el viaje
- [ ] **¿Lista de seriales disponibles?**: Para validación
- [ ] **¿Hay límite de dispositivos por trip?**

## ?? SEGURIDAD Y COMPLIANCE

### 1. Certificados y HTTPS
- [ ] **¿Requieren certificado específico?**: CA particular
- [ ] **¿Verifican certificate pinning?**
- [ ] **¿IP fija de SensiWatch para whitelist?**: 20.124.145.167 confirmado

### 2. Logs y Auditoría
- [ ] **¿Requieren logs específicos?**: Para auditoría
- [ ] **¿Necesitan confirmación de recepción detallada?**: Más allá de 200 OK
- [ ] **¿Hay compliance requirements?**: GDPR, CCPA, etc.

## ? TIMELINE Y ACTIVACIÓN

### 1. Proceso de Activación
- [ ] **¿Cuál es el proceso para activar push?**: Pasos específicos
- [ ] **¿Hay environment de testing?**: Antes de producción
- [ ] **¿Ventana de mantenimiento requerida?**: Para activación
- [ ] **¿Contacto técnico 24/7?**: Para soporte post-activación

### 2. Testing y Validación
- [ ] **¿Proporcionan datos de prueba?**: Dispositivos/trips de test
- [ ] **¿Cómo validamos la integración?**: Proceso de QA
- [ ] **¿SLA de respuesta para issues?**: Post go-live

## ?? MONITOREO Y REPORTES

### 1. Métricas y Alertas
- [ ] **¿Qué métricas monitoreamos?**: KPIs importantes
- [ ] **¿Notificaciones por problemas?**: Email/SMS alerts
- [ ] **¿Dashboard de status disponible?**: Para monitoring

### 2. Datos Históricos
- [ ] **¿Acceso a datos históricos via API?**: Para sincronización inicial
- [ ] **¿Backup de datos requerido?**: Por nuestra parte
- [ ] **¿Retención de datos especificada?**: Políticas de storage

---

## ?? **CONTACTOS REQUERIDOS**

### Technical Contacts
- [ ] **API Support**: Email/teléfono para issues técnicos
- [ ] **Account Manager**: Contacto comercial/contractual  
- [ ] **Emergency Contact**: 24/7 para production issues

### Documentation Access
- [ ] **Developer Portal Access**: Credenciales específicas
- [ ] **API Documentation**: Versión específica para nuestro programa
- [ ] **Change Notifications**: ¿Cómo nos notifican cambios en API?

---
**Fecha**: $(Get-Date -Format 'yyyy-MM-dd')
**Estado**: Pendiente respuesta de Sensitech
**Prioridad**: ALTA - Requerido para activación