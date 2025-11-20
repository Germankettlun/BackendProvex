# ERP Backend API - Pipeline Modular

## ?? Descripción General
Pipeline automatizado para build, deploy y validación de la API .NET 9 en ambientes Development y Production.

## ?? Estructura de Archivos

```
pipelines/
??? azure-pipelines-backend.yml       # Pipeline principal
??? templates/
?   ??? deploy-backend-template.yml   # Template reutilizable de deployment
??? scripts/
?   ??? setup-iis.ps1                 # Crear infraestructura IIS (sitio, app pool, directorios)
?   ??? cleanup-processes.ps1         # Limpiar procesos dotnet.exe zombie
?   ??? health-check.ps1              # Validación post-deployment
??? README.md                         # Este archivo
```

## ?? Flujo de Ejecución

### 1?? Trigger
Pipeline se ejecuta automáticamente en:
- **Push a rama `dev`** ? Deploy a Development
- **Push a rama `prod`** ? Deploy a Production

### 2?? Stages

#### **Stage 1: Build & Publish**
```
1. Install .NET SDK 9.0.203
2. Clean obj/bin directories
3. Restore NuGet packages
4. Build solution (Release)
5. Publish (framework-dependent deployment)
6. Verify artifacts (ProvexBackendAPI.dll)
7. Publish pipeline artifact
```

#### **Stage 2: Deploy to Development**
```
1. Download artifact
2. Setup IIS infrastructure (scripts/setup-iis.ps1)
3. Resolve artifact path
4. Cleanup zombie processes (scripts/cleanup-processes.ps1)
5. Stop IIS (app_offline.htm)
6. Configure permissions
7. Copy files with robocopy
8. Create web.config with environment variables
9. Start IIS
10. View startup logs
11. Health check (scripts/health-check.ps1)
```

#### **Stage 3: Deploy to Production**
Idéntico a Development, pero usando variables de `ERP-Backend-Production`

---

## ?? Variables Requeridas en Azure DevOps

### ?? Variable Group: `ERP-Backend-Development`

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `agentNameDev` | **?? FALTANTE** - Nombre del agente IIS Dev | `IISDev` |
| `siteName` | Nombre del sitio IIS | `ERPApiSite` |
| `appPoolName` | Nombre del App Pool | `ERPApiPool` |
| `physicalPath` | Ruta física de deployment | `C:\inetpub\wwwroot\ERPApi` |
| `bindingPort` | Puerto del sitio IIS | `8082` |
| `VerifyUrl` | URL de health check | `http://10.115.1.252:8082/api/v1/health` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Development` |
| `ConnectionStrings__DatabaseConnection` | Connection string (renombrar `ConnectionBD`) | `Server=...` |
| `Jwt__Key` | ? Ya existe | `Pr0v3xAuth0.2025@&Ap1-Fwt$09834ExtraChars` |
| `Jwt__Issuer` | ? Ya existe | `Provex.Auth` |
| `Jwt__Audience` | ? Ya existe | `ProvexBackend.Client` |
| `SecretKey` | ? Ya existe | `Esta es una clave secreta...` |
| `AzureAd__Instance` | ? Ya existe | `https://login.microsoftonline.com/` |
| `AzureAd__Domain` | ? Ya existe | `tu-tenant.onmicrosoft.com` |
| `AzureAd__TenantId` | ? Ya existe | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |
| `AzureAd__ClientId` | ? Ya existe | `yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy` |

### ?? Variable Group: `ERP-Backend-Production`

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `agentName` | ? Ya existe - Nombre del agente IIS Prod | `IISAgent01` |
| `siteName` | ? Ya existe | `ERPApiSite` |
| `appPoolName` | ? Ya existe | `ERPApiPool` |
| `physicalPath` | ? Ya existe | `C:\inetpub\wwwroot\ERPApi` |
| `bindingPort` | ? Ya existe | `8083` |
| `VerifyUrl` | ? Ya existe | `https://api2.provexsa.cl/health` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Production` |
| `ConnectionStrings__DatabaseConnection` | ?? Renombrar de `ConnectionBD` | `Server=...` |
| **(Resto de variables JWT/AzureAd)** | ? Ya existen | - |

---

## ?? Acciones Requeridas

### 1. **Agregar variable `agentNameDev` a Development**
```
Variable Group: ERP-Backend-Development
Nombre: agentNameDev
Valor: IISDev
```

### 2. **Renombrar variable en ambos grupos**
```
De: ConnectionBD
A:  ConnectionStrings__DatabaseConnection
```
*(Mantener el mismo valor, solo cambiar el nombre)*

### 3. **Mover pipeline a nueva ubicación**
```bash
# Desde raíz del repositorio
git mv azure-pipelines.yml pipelines/azure-pipelines-backend.yml
git add pipelines/
git commit -m "refactor: Reorganizar pipeline en estructura modular"
```

### 4. **Actualizar pipeline en Azure DevOps**
```
Azure DevOps ? Pipelines ? Edit ? 
  YAML file path: pipelines/azure-pipelines-backend.yml
```

---

## ?? Ventajas de la Nueva Estructura

? **Modularidad**: Template reutilizable para Dev/Prod  
? **Mantenibilidad**: Scripts separados, fácil de testear  
? **Seguridad**: Sin valores hardcoded, todo desde variable groups  
? **Consistencia**: Mismo patrón que pipeline de frontend  
? **Trazabilidad**: Build info en variables de entorno  

---

## ?? Testing Local de Scripts

```powershell
# Desde raíz del repositorio

# Test setup-iis.ps1
.\pipelines\scripts\setup-iis.ps1 `
  -SiteName "ERPApiSite" `
  -AppPoolName "ERPApiPool" `
  -PhysicalPath "C:\inetpub\wwwroot\ERPApi" `
  -BindingPort 8082

# Test cleanup-processes.ps1
.\pipelines\scripts\cleanup-processes.ps1 `
  -PhysicalPath "C:\inetpub\wwwroot\ERPApi"

# Test health-check.ps1
.\pipelines\scripts\health-check.ps1 `
  -Url "http://localhost:8082/api/v1/health" `
  -MaxRetries 5
```

---

## ?? Troubleshooting

### ? Error: "App Pool no existe"
**Solución**: El script `setup-iis.ps1` debería crear el App Pool automáticamente. Verifica que el paso "Setup IIS Infrastructure" se ejecutó correctamente.

### ? Error: "DirectoryNotFoundException"
**Solución**: El script `setup-iis.ps1` crea directorios automáticamente. Si falla, verifica permisos del agente en `C:\inetpub\wwwroot\`.

### ? Health check falla
**Solución**: 
1. Revisa logs de stdout en el paso "View Startup Logs"
2. Verifica que `VerifyUrl` apunte al endpoint correcto
3. Valida que la aplicación esté escuchando en el puerto correcto

---

## ?? Referencias

- [ASP.NET Core deployment to IIS](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
- [Azure Pipelines YAML reference](https://learn.microsoft.com/en-us/azure/devops/pipelines/yaml-schema/)
- [Template expressions](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/templates)

---

**Última actualización**: 2025-11-20  
**Versión**: 2.0 (Estructura modular)
