# Guía de Despliegue a Producción - API Provex Backend

## ?? Problemas Detectados y Solucionados

### 1. ? Error 404 en todos los endpoints
**Causa**: Swagger solo estaba habilitado en Development.

**Solución**: 
- ? Swagger ahora está disponible en `/swagger` en producción
- ? Endpoint de health check disponible en `/api/v1/meta/healthz` (MetaController)
- ? Los controladores siguen funcionando normalmente

### 2. ? Error "Failed to determine the https port for redirect"
**Causa**: El código llamaba `UseHttpsRedirection()` incluso cuando solo HTTP estaba configurado (puerto 8083).

**Solución**:
- ? HTTPS redirection ahora es condicional
- ? Solo se activa si HTTPS está configurado
- ? Permite ejecutar en HTTP puro sin errores

### 3. ?? Faltaba configuración de producción
**Causa**: No existía `appsettings.Production.json`.

**Solución**:
- ? Creado archivo con configuración específica de producción
- ? Puerto 8083 configurado explícitamente
- ? Logging optimizado para producción

---

## ?? Pasos para Desplegar

### Opción 1: IIS (Recomendado para Windows Server)

1. **Configurar el Application Pool**:
   ```
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated
   - Identity: ApplicationPoolIdentity (o cuenta de servicio)
   ```

2. **Configurar la aplicación en IIS**:
   - Physical path: `C:\inetpub\wwwroot\ProvexBackend` (o tu ruta)
   - Binding: HTTP en puerto 8083
   - **NO habilitar "Require SSL"** (ya que usas HTTP puro)

3. **Instalar .NET 9 Hosting Bundle**:
   ```powershell
   # Descargar de: https://dotnet.microsoft.com/download/dotnet/9.0
   # Buscar: ASP.NET Core Runtime 9.0.x - Windows Hosting Bundle
   ```

4. **Publicar la aplicación**:
   ```powershell
   cd ProvexBackendAPI
   dotnet publish -c Release -o C:\inetpub\wwwroot\ProvexBackend
   ```

5. **Configurar la variable de entorno**:
   - En el Application Pool > Advanced Settings > Environment Variables
   - Agregar: `ASPNETCORE_ENVIRONMENT = Production`

6. **Reiniciar IIS**:
   ```powershell
   iisreset
   ```

### Opción 2: Kestrel Standalone (Servicio de Windows)

1. **Publicar como autónomo**:
   ```powershell
   dotnet publish -c Release -o C:\Services\ProvexBackend
   ```

2. **Crear servicio de Windows**:
   ```powershell
   # Instalar NSSM (Non-Sucking Service Manager)
   # O usar sc.exe
   
   sc create ProvexBackendAPI binPath="C:\Services\ProvexBackend\ProvexBackendAPI.exe" start=auto
   sc description ProvexBackendAPI "API Backend de Provex"
   
   # Configurar variables de entorno en el registro:
   # HKLM\SYSTEM\CurrentControlSet\Services\ProvexBackendAPI\Environment
   # ASPNETCORE_ENVIRONMENT=Production
   ```

3. **Iniciar el servicio**:
   ```powershell
   sc start ProvexBackendAPI
   ```

---

## ? Verificación Post-Despliegue

### 1. Health Check
```powershell
# Endpoint de salud (MetaController)
Invoke-RestMethod -Uri "http://localhost:8083/api/v1/meta/healthz" -Method Get

# Debería retornar:
# {
#   "status": "ok",
#   "pipelineVersion": "...",
#   "commitHash": "...",
#   "environment": "Production",
#   "isLocal": false
# }
```

### 2. Swagger UI
Abrir en navegador: `http://localhost:8083/swagger`

### 3. Test de CORS (desde red interna)
```powershell
# Desde otra máquina en la red 10.115.x.x
Invoke-RestMethod -Uri "http://10.115.1.253:8083/api/v1/meta/healthz" -Method Get
```

---

## ?? Configuración CORS en Producción

La configuración actual permite:
- ? Red interna: `10.115.x.x` (HTTP/HTTPS)
- ? Localhost: `localhost`, `127.0.0.1`
- ? Dominios: `*.provexsa.cl`, `*.provex.com` (solo HTTPS)

Para verificar qué orígenes se permiten, revisar los logs de la consola:
```
?? CORS: Modo RESTRINGIDO activado (Production)
?? CORS: Evaluando origen: http://10.115.1.100:3000
   ? PERMITIDO - Red interna 10.115.x.x
```

---

## ?? Troubleshooting

### Error: "HTTP Error 500.31 - Failed to load ASP.NET Core runtime"
**Solución**: Instalar .NET 9 Hosting Bundle y reiniciar IIS

### Error: "Unable to connect to database"
**Solución**: Verificar que el servidor pueda acceder a `10.115.1.252:1433`
```powershell
Test-NetConnection -ComputerName 10.115.1.252 -Port 1433
```

### Error: CORS bloqueado
**Solución**: Revisar logs de la aplicación para ver qué origen fue rechazado

### Los logs no aparecen
**Solución**: Habilitar stdout en `web.config`:
```xml
<aspNetCore processPath="dotnet" 
            arguments=".\ProvexBackendAPI.dll" 
            stdoutLogEnabled="true" 
            stdoutLogFile=".\logs\stdout" />
```

---

## ?? Monitoreo

### Logs de IIS
Ubicación: `C:\inetpub\logs\LogFiles\W3SVC1\`

### Logs de la aplicación
Los logs de consola (`Console.WriteLine`) aparecerán en:
- IIS: Event Viewer > Application
- Kestrel: stdout (si está configurado en web.config)

### Endpoints de monitoreo
```bash
# Health check con información de deployment
curl http://localhost:8083/api/v1/meta/healthz

# Swagger (para verificar endpoints disponibles)
http://localhost:8083/swagger
```

---

## ?? Seguridad

### Recomendaciones adicionales:
1. **Firewall**: Solo abrir puerto 8083 para red interna
2. **HTTPS**: Configurar certificado SSL en IIS si es posible
3. **Secrets**: Mover JWT Key a variables de entorno o Azure Key Vault
4. **Logging**: Configurar logging estructurado (Serilog, NLog)

---

## ?? Archivos Modificados

1. ? `Program.cs` - HTTPS redirection condicional, Swagger en producción
2. ? `appsettings.Production.json` - Configuración de producción creada

## ?? Próximos Pasos Recomendados

1. Configurar logging estructurado (Serilog)
2. Implementar Application Insights para monitoreo
3. Configurar certificado SSL para HTTPS
4. Implementar rate limiting
5. Agregar métricas de performance (Prometheus/Grafana)
