# ?? Guía de Pruebas - Endpoints de Versión

## ?? Endpoints Implementados

### 1. GET /config/version - Información de Versión

#### Desde Swagger UI
1. Abre http://localhost:5000/swagger
2. Busca la sección **Config**
3. Expande `GET /config/version`
4. Click en **"Try it out"**
5. Click en **"Execute"**

#### Desde el Navegador
```
http://localhost:5000/config/version
```

#### Con cURL (Terminal/PowerShell)
```bash
curl http://localhost:5000/config/version
```

#### Con PowerShell
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/config/version" -Method Get
```

#### Con Postman
- **Method:** GET
- **URL:** `http://localhost:5000/config/version`
- **Headers:** Ninguno necesario

#### Respuesta Esperada (Desarrollo)
```json
{
  "version": "1.0.0-dev",
  "commit": "local",
  "commitFull": "local",
  "environment": "Development",
  "buildDate": "2025-01-19 18:30:45 UTC",
  "machineName": "DESKTOP-ABC123",
  "frameworkVersion": "9.0.0"
}
```

#### Respuesta Esperada (Producción)
```json
{
  "version": "1.2.345",
  "commit": "abc123d",
  "commitFull": "abc123def456789abcdef0123456789abcdef01",
  "environment": "Production",
  "buildDate": "2025-01-19 14:30:45 UTC",
  "machineName": "PROD-SERVER-01",
  "frameworkVersion": "9.0.0"
}
```

---

### 2. GET /config/health - Health Check Extendido

#### Desde el Navegador
```
http://localhost:5000/config/health
```

#### Con cURL
```bash
curl http://localhost:5000/config/health
```

#### Con PowerShell
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/config/health" -Method Get
```

#### Respuesta Esperada
```json
{
  "status": "healthy",
  "timestamp": "2025-01-19 18:45:30 UTC",
  "version": "1.0.0-dev",
  "environment": "Development",
  "uptime": "00.00:15:23",
  "processId": 12345,
  "workingSet": "156 MB",
  "assemblyLocation": "C:\\Users\\...\\BackendProvex\\bin\\Debug\\net9.0\\ProvexApi.dll"
}
```

---

### 3. POST /config/reload - Recargar Configuración (?? Requiere Autenticación)

#### Con cURL + Basic Auth
```bash
curl -X POST http://localhost:5000/config/reload \
  -u username:password
```

#### Con PowerShell + Basic Auth
```powershell
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("username:password"))
Invoke-RestMethod -Uri "http://localhost:5000/config/reload" -Method Post -Headers @{
    Authorization = "Basic $credentials"
}
```

#### Respuesta Exitosa
```json
{
  "message": "Configuración recargada exitosamente"
}
```

---

## ? Validación de Compatibilidad Dev/Prod

### Estado de la Validación

| Aspecto | Estado | Detalles |
|---------|--------|----------|
| **Compilación** | ? PASS | Sin errores de compilación |
| **ConfigController** | ? PASS | Controller implementado correctamente |
| **Endpoint /config/version** | ? PASS | AllowAnonymous, accesible sin auth |
| **Endpoint /config/health** | ? PASS | AllowAnonymous, accesible sin auth |
| **Endpoint /config/reload** | ? PASS | Requiere BasicAuth, seguro |
| **index.html** | ? PASS | JavaScript actualizado, manejo de errores |
| **appsettings.json** | ? PASS | Puerto corregido (5000) |
| **Kestrel Config** | ? PASS | Compatible Dev/Prod |
| **CORS** | ? PASS | Configurado para Dev y Prod |

### Pruebas de Compatibilidad

#### ? Desarrollo (localhost)
- Puerto: 5000 (HTTP)
- Config: `appsettings.json` ? `App:Version` = "1.0.0-dev"
- CORS: Permite localhost:3001, localhost:3002
- Swagger: Habilitado en todos los entornos

#### ? Producción (api.provexsa.cl)
- Puerto: Configurado por IIS/Azure
- Config: Variables de entorno `BUILD_VERSION`, `BUILD_SOURCEVERSION`
- CORS: Permite solo https://api.provexsa.cl
- Swagger: Habilitado (puede deshabilitarse si lo deseas)

### Cambios Realizados - Análisis de Impacto

| Archivo | Cambio | Impacto Dev | Impacto Prod |
|---------|--------|-------------|--------------|
| `ConfigController.cs` | Nuevo controller | ? Ninguno | ? Ninguno - Endpoint nuevo |
| `index.html` | Script actualizado | ? Mejora visualización | ? Mejora visualización |
| `appsettings.json` | Puerto 0?5000 | ? Corrige error | ?? No afecta (IIS maneja puerto) |
| `appsettings.json` | Agregado App:Version | ? Nuevo valor default | ?? Ignorado (usa env vars) |

### ?? Consideraciones Importantes

1. **appsettings.json - Puerto en Producción:**
   - En producción con IIS, el puerto lo maneja IIS/Azure App Service
   - La configuración de Kestrel en `appsettings.json` puede ser ignorada
   - ? Sin impacto negativo

2. **Variables de Entorno en Producción:**
   - Debes configurar `BUILD_VERSION` y `BUILD_SOURCEVERSION` en producción
   - Si no existen, usará el número de versión del ensamblado (Assembly.Version)
   - ? Fallback seguro implementado

3. **CORS:**
   - Dev: `http://localhost:3001`, `http://localhost:3002`
   - Prod: `https://api.provexsa.cl`
   - ? Correctamente configurado por entorno

4. **Swagger en Producción:**
   - Actualmente habilitado en todos los entornos
   - Si deseas deshabilitarlo en producción, modifica `Program.cs`:
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI(...);
   }
   ```

---

## ?? Plan de Pruebas

### Pruebas en Desarrollo (LOCAL)

```powershell
# 1. Verificar que la app esté corriendo
netstat -ano | findstr :5000

# 2. Probar endpoint de versión
Invoke-RestMethod -Uri "http://localhost:5000/config/version" | ConvertTo-Json

# 3. Probar health check
Invoke-RestMethod -Uri "http://localhost:5000/config/health" | ConvertTo-Json

# 4. Probar página principal
Start-Process "http://localhost:5000"

# 5. Probar Swagger
Start-Process "http://localhost:5000/swagger"
```

### Pruebas en Producción (DESPUÉS DEL DEPLOY)

```powershell
# 1. Verificar endpoint de versión
Invoke-RestMethod -Uri "https://api.provexsa.cl/config/version" | ConvertTo-Json

# 2. Verificar que muestre la versión del build
# Debe mostrar algo como "1.2.345" en lugar de "1.0.0-dev"

# 3. Verificar página principal
Start-Process "https://api.provexsa.cl"

# 4. Verificar que la versión se muestre en la UI
# Abre el navegador y verifica que NO aparezca "__VERSION__"
```

---

## ?? Troubleshooting

### Problema: "No se puede acceder a localhost:0"
**Causa:** Puerto 0 en appsettings.json  
**Solución:** ? Ya corregido - Ahora usa puerto 5000

### Problema: La versión muestra "__VERSION__" en producción
**Causa:** Variables de entorno no configuradas  
**Solución:** Configurar `BUILD_VERSION` y `BUILD_SOURCEVERSION` en IIS/Azure

### Problema: Error 401 en /config/reload
**Causa:** Endpoint requiere autenticación  
**Solución:** Usar BasicAuth o remover la autorización si no es necesaria

### Problema: CORS error en producción
**Causa:** Frontend en dominio no permitido  
**Solución:** Agregar el dominio en `Program.cs` ? CORS configuration

---

## ?? Checklist Pre-Deploy a Producción

- [ ] ? Compilación exitosa (`dotnet build`)
- [ ] ? Todos los tests pasan (si existen)
- [ ] ? Configurar variable de entorno `BUILD_VERSION` en servidor
- [ ] ? Configurar variable de entorno `BUILD_SOURCEVERSION` en servidor
- [ ] ? Verificar CORS permite el dominio de producción
- [ ] ? Probar endpoint `/config/version` después del deploy
- [ ] ? Verificar que la página principal muestre la versión correcta
- [ ] ?? (Opcional) Deshabilitar Swagger en producción si es necesario

---

## ?? Resumen de Validación

### ? APROBADO PARA DEPLOY

Todos los cambios son **compatibles** con desarrollo y producción:

1. **Nuevas funcionalidades agregadas** - No rompe código existente
2. **Endpoints públicos seguros** - Solo lectura, sin autenticación necesaria
3. **Fallbacks implementados** - Si no hay env vars, usa valores por defecto
4. **CORS configurado correctamente** - Por entorno
5. **Puerto corregido** - No más error ERR_UNSAFE_PORT

### ?? Listo para Deploy

Los cambios están listos para ser pusheados a la rama `Prod` y desplegados.

### ?? Soporte Post-Deploy

Si después del deploy a producción:
- La versión no se muestra correctamente
- Hay errores CORS
- Swagger no funciona

Consulta la sección de Troubleshooting en este documento o en `DEPLOYMENT_VERSION.md`.
