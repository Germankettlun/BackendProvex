Asunto: Integración SensiWatch API - Información Requerida para Activación

Estimado equipo de Sensitech,

Saludos desde Provex. Hemos completado la implementación técnica de la integración con SensiWatch API en nuestro backend .NET 9 y necesitamos la siguiente información para proceder con la activación:

## ?? CREDENCIALES DE API REQUERIDAS

1. **Subscription Key**: Para acceso al Developer Portal
2. **Username/Password**: Credenciales específicas para API calls (diferentes al portal web)
3. **Program ID**: Identificador numérico del programa asignado a Provex
4. **Environment**: ¿Tienen sandbox/testing disponible antes de producción?

## ?? MASTER DATA IDs

Para crear trips via API necesitamos:
- **Location IDs**: Lista de ubicaciones disponibles (origen/destino) con sus IDs numéricos
- **Product IDs**: Lista de productos permitidos en nuestro programa
- **Trip Template ID**: ¿Debemos usar 0 (default) o tienen un ID específico?

## ?? CONFIGURACIÓN DE PUSH ENDPOINTS

**Nuestra URL base**: `https://api.provexsa.cl/api/sensiwatch`

**Endpoints implementados**:
- `POST /api/sensiwatch/device/activation`
- `POST /api/sensiwatch/device/report`

**Consultas sobre Push**:
1. ¿Confirman nuestra URL base y endpoints?
2. ¿Qué tipo de autenticación prefieren para los push? (actualmente HTTP Basic Auth)
3. ¿Cuáles serán las credenciales exactas para autenticación?
4. ¿Cuál es la frecuencia esperada de mensajes?
5. ¿Hay límites de tamaño por request?
6. ¿Cuánto tiempo esperan respuesta antes de timeout?
7. ¿Cuál es su política de reintentos si no reciben HTTP 200?

## ?? CONFIGURACIÓN DE SEGURIDAD

1. **Certificado HTTPS**: ¿Requieren CA específica o cualquier certificado válido?
2. **IP Whitelist**: Confirmación de IP fija de SensiWatch: 20.124.145.167
3. **Whitelist de nuestra IP**: ¿Necesitan nuestra IP para calls salientes a su API?

## ??? CREACIÓN DE TRIPS

1. ¿Qué campos son obligatorios vs opcionales en trip creation?
2. ¿Hay validaciones específicas para InternalTripID?
3. ¿Cómo se asignan dispositivos a trips? ¿En creation o separado?
4. ¿Lista de device serials disponibles para validación?

## ? PROCESO DE ACTIVACIÓN

1. ¿Cuáles son los pasos específicos para activar el push service?
2. ¿Proporcionan dispositivos/trips de prueba para testing?
3. ¿Hay ventana de mantenimiento requerida?
4. ¿Contacto técnico 24/7 para soporte post-activación?

## ?? INFORMACIÓN DE CONTACTO

Por favor proporcionen:
- **Technical Support**: Email/teléfono para issues técnicos
- **Account Manager**: Contacto comercial
- **Emergency Contact**: Para production issues

## ?? INFORMACIÓN TÉCNICA DE NUESTRA IMPLEMENTACIÓN

- **Backend**: .NET 9 / ASP.NET Core
- **Base de Datos**: SQL Server con tablas optimizadas para SensiWatch
- **Hosting**: Azure/IIS con certificado SSL válido
- **Endpoints**: Responden < 30 segundos con HTTP 200
- **Procesamiento**: Asíncrono para no bloquear push calls
- **Logging**: Estructurado con prefijo [SensiWatch]

Hemos seguido la documentación de Push API v4 y REST API. Nuestro sistema está listo para recibir:
- Device Activation messages
- Device Report messages con sensors/locations/trip info

Una vez recibamos esta información, podremos completar la configuración y coordinar las pruebas de conectividad.

Quedamos atentos a su respuesta.

Saludos cordiales,

**[Tu Nombre]**
**[Tu Cargo]**
**Provex**
**Email**: [tu-email@provexsa.cl]
**Teléfono**: [tu-teléfono]

---

**Archivos adjuntos sugeridos**:
- SensiWatch_Implementation_Guide.md
- SensiWatch_Consultas_Sensitech.md