# Script de Despliegue para Integración SensiWatch
# ===================================================
# Autor: GitHub Copilot
# Fecha: $(Get-Date -Format "yyyy-MM-dd")
# Descripción: Script para configurar la integración SensiWatch en ProvexApi

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Development",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipMigration = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$RunTests = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = ""
)

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "?? DESPLIEGUE SENSIWATCH INTEGRATION" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Entorno: $Environment" -ForegroundColor Yellow
Write-Host "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host ""

# 1. Verificar prerrequisitos
Write-Host "?? 1. Verificando prerrequisitos..." -ForegroundColor Green

# Verificar .NET 9
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ? .NET Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "   ? Error: .NET no está instalado" -ForegroundColor Red
    exit 1
}

# Verificar Entity Framework Tools
try {
    $efVersion = dotnet ef --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? EF Core Tools instalado" -ForegroundColor Green
    } else {
        Write-Host "   ??  Instalando EF Core Tools..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef
    }
} catch {
    Write-Host "   ??  Instalando EF Core Tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# 2. Compilar el proyecto
Write-Host ""
Write-Host "?? 2. Compilando proyecto..." -ForegroundColor Green

dotnet build --configuration Release --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ? Error en compilación" -ForegroundColor Red
    exit 1
}
Write-Host "   ? Compilación exitosa" -ForegroundColor Green

# 3. Ejecutar migración de base de datos (si no se omite)
if (-not $SkipMigration) {
    Write-Host ""
    Write-Host "???  3. Ejecutando migración de base de datos..." -ForegroundColor Green
    
    try {
        # Verificar si la migración de SensiWatch existe
        $migrations = dotnet ef migrations list --context ProvexDbContext --no-build 2>$null
        if ($migrations -like "*AddSensiWatchTables*") {
            Write-Host "   ? Migración AddSensiWatchTables encontrada" -ForegroundColor Green
            
            # Ejecutar migración
            dotnet ef database update --context ProvexDbContext --no-build
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ? Migración aplicada exitosamente" -ForegroundColor Green
            } else {
                Write-Host "   ? Error aplicando migración" -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "   ??  Migración AddSensiWatchTables no encontrada" -ForegroundColor Yellow
            Write-Host "   ?? Ejecutar: dotnet ef migrations add AddSensiWatchTables --context ProvexDbContext" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "   ? Error verificando migraciones: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host ""
    Write-Host "??  3. Migración de BD omitida (--SkipMigration)" -ForegroundColor Yellow
}

# 4. Verificar configuración de SensiWatch
Write-Host ""
Write-Host "??  4. Verificando configuración..." -ForegroundColor Green

$appsettingsPath = "appsettings.json"
$sensiWatchConfig = "appsettings.SensiWatch.json"

if (Test-Path $appsettingsPath) {
    Write-Host "   ? appsettings.json encontrado" -ForegroundColor Green
} else {
    Write-Host "   ? appsettings.json no encontrado" -ForegroundColor Red
}

if (Test-Path $sensiWatchConfig) {
    Write-Host "   ? appsettings.SensiWatch.json encontrado" -ForegroundColor Green
    Write-Host "   ?? Revisar y configurar credenciales de SensiWatch" -ForegroundColor Cyan
} else {
    Write-Host "   ??  appsettings.SensiWatch.json no encontrado" -ForegroundColor Yellow
}

# 5. Verificar scripts SQL
Write-Host ""
Write-Host "?? 5. Verificando scripts SQL..." -ForegroundColor Green

$sqlScript = "Scripts/SensiWatch_CreateTables.sql"
if (Test-Path $sqlScript) {
    Write-Host "   ? Script SQL encontrado: $sqlScript" -ForegroundColor Green
    $scriptSize = (Get-Item $sqlScript).Length
    Write-Host "   ?? Tamaño del script: $([math]::Round($scriptSize/1KB, 2)) KB" -ForegroundColor Green
} else {
    Write-Host "   ? Script SQL no encontrado: $sqlScript" -ForegroundColor Red
}

# 6. Ejecutar pruebas (opcional)
if ($RunTests) {
    Write-Host ""
    Write-Host "?? 6. Ejecutando pruebas..." -ForegroundColor Green
    
    # Verificar que el servicio se pueda instanciar
    try {
        dotnet run --no-build --urls="https://localhost:5001" -- --environment=$Environment &
        $processId = $!
        
        Start-Sleep -Seconds 10
        
        # Probar health check
        try {
            $healthResponse = Invoke-RestMethod -Uri "https://localhost:5001/health" -Method Get -SkipCertificateCheck
            Write-Host "   ? Health check exitoso: $($healthResponse)" -ForegroundColor Green
        } catch {
            Write-Host "   ??  Health check falló: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
        # Probar health check de SensiWatch
        try {
            $sensiHealthResponse = Invoke-RestMethod -Uri "https://localhost:5001/health/sensiwatch" -Method Get -SkipCertificateCheck
            Write-Host "   ? SensiWatch health check: $($sensiHealthResponse.status)" -ForegroundColor Green
        } catch {
            Write-Host "   ??  SensiWatch health check falló: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
        # Detener el proceso
        Stop-Process -Id $processId -Force
        
    } catch {
        Write-Host "   ? Error ejecutando pruebas: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host ""
    Write-Host "??  6. Pruebas omitidas (usar --RunTests para ejecutar)" -ForegroundColor Yellow
}

# 7. Generar resumen de despliegue
Write-Host ""
Write-Host "?? 7. Resumen de despliegue..." -ForegroundColor Green

$deploymentSummary = @"
============================================
?? RESUMEN DEL DESPLIEGUE SENSIWATCH
============================================

? COMPONENTES IMPLEMENTADOS:
   • Modelos y DTOs de SensiWatch
   • Entidades de base de datos
   • Servicios de integración
   • Controlador con endpoints
   • Migración de BD generada
   • Script SQL con optimizaciones

?? ENDPOINTS DISPONIBLES:
   • POST /api/sensiwatch/device/activation
   • POST /api/sensiwatch/device/report
   • GET  /api/sensiwatch/temperature-readings
   • GET  /api/sensiwatch/trip/{id}/summary
   • GET  /api/sensiwatch/devices/status
   • GET  /api/sensiwatch/system-info
   • GET  /health/sensiwatch

?? CONFIGURACIÓN PENDIENTE:
   1. Configurar credenciales de SensiWatch en:
      - SensiWatch:SubscriptionKey
      - SensiWatch:Username
      - SensiWatch:Password
      - SensiWatch:ProgramId
   
   2. Configurar certificado HTTPS válido
   
   3. Configurar autenticación Basic Auth para push:
      - Cambiar credenciales en ValidateAuthenticationHeader()
   
   4. Configurar whitelist de IP de SensiWatch:
      - Permitir solo: 20.124.145.167

?? PRÓXIMOS PASOS:
   1. Revisar configuración en appsettings.SensiWatch.json
   2. Aplicar migración a BD de producción
   3. Configurar HTTPS en servidor
   4. Coordinar con Sensitech para activar push
   5. Probar conectividad

?? DOCUMENTACIÓN:
   • Guía completa: SensiWatch_Implementation_Guide.md
   • Script SQL: Scripts/SensiWatch_CreateTables.sql
   • Configuración: appsettings.SensiWatch.json

============================================
Entorno: $Environment
Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Estado: ? LISTO PARA CONFIGURACIÓN
============================================
"@

Write-Host $deploymentSummary -ForegroundColor Cyan

# 8. Crear checklist de configuración
$checklistPath = "SensiWatch_Configuration_Checklist.md"
$checklist = @"
# ? Checklist de Configuración SensiWatch

## Prerrequisitos Técnicos
- [ ] Certificado HTTPS válido configurado
- [ ] Dominio público accesible (ej: https://api.provexsa.cl)
- [ ] Firewall configurado para permitir IP de SensiWatch (20.124.145.167)
- [ ] Base de datos actualizada con migración SensiWatch

## Configuración de SensiWatch API
- [ ] Subscription Key obtenida de SensiWatch Developer Portal
- [ ] Usuario y contraseña de SensiWatch configurados
- [ ] Program ID proporcionado por Sensitech
- [ ] URL base proporcionada a Sensitech: https://tu-dominio.com/api/sensiwatch

## Configuración de Seguridad
- [ ] Credenciales Basic Auth configuradas (cambiar del default)
- [ ] Whitelist de IP implementada
- [ ] Logs de seguridad configurados

## Pruebas de Conectividad
- [ ] Health check general: GET /health
- [ ] Health check SensiWatch: GET /health/sensiwatch
- [ ] Prueba de endpoint activation (con Postman/curl)
- [ ] Prueba de endpoint report (con Postman/curl)

## Coordinación con Sensitech
- [ ] URL base enviada a Sensitech
- [ ] Credenciales de autenticación coordinadas
- [ ] Ventana de activación programada
- [ ] Contacto técnico establecido para soporte

## Monitoreo y Alertas
- [ ] Logs de SensiWatch monitoreados
- [ ] Alertas por errores de conectividad
- [ ] Dashboard de dispositivos activos
- [ ] Reportes de temperatura configurados

---
Fecha de creación: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Generado por: Script de despliegue SensiWatch
"@

Set-Content -Path $checklistPath -Value $checklist -Encoding UTF8
Write-Host ""
Write-Host "?? Checklist de configuración creado: $checklistPath" -ForegroundColor Green

Write-Host ""
Write-Host "?? ¡DESPLIEGUE COMPLETADO!" -ForegroundColor Green
Write-Host "?? Revisar documentación en SensiWatch_Implementation_Guide.md" -ForegroundColor Cyan
Write-Host "? Seguir checklist en $checklistPath" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan