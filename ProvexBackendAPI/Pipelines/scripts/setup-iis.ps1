# =========================
# Script: Setup IIS Infrastructure
# Descripción: Crea sitio, app pool y directorios si no existen
# =========================
param(
    [Parameter(Mandatory=$true)]
    [string]$SiteName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppPoolName,
    
    [Parameter(Mandatory=$true)]
    [string]$PhysicalPath,
    
    [Parameter(Mandatory=$true)]
    [string]$BindingPort
)

# Importar módulo WebAdministration
if ($PSVersionTable.PSEdition -eq "Core") { 
    Import-Module WebAdministration -UseWindowsPowerShell 
} else { 
    Import-Module WebAdministration 
}

Write-Host "?? Verificando infraestructura IIS..."
Write-Host "   Site Name: $SiteName"
Write-Host "   App Pool: $AppPoolName"
Write-Host "   Physical Path: $PhysicalPath"
Write-Host "   Binding Port: $BindingPort"

# ========================================
# 1. Crear directorio físico
# ========================================
if (-not (Test-Path $PhysicalPath)) {
    New-Item -ItemType Directory -Path $PhysicalPath -Force | Out-Null
    Write-Host "? Directorio creado: $PhysicalPath"
} else {
    Write-Host "? Directorio ya existe: $PhysicalPath"
}

# Crear subdirectorio de logs
$logsPath = Join-Path $PhysicalPath 'logs'
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
    Write-Host "? Subdirectorio logs creado"
}

# ========================================
# 2. Crear App Pool si no existe
# ========================================
if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    New-WebAppPool -Name $AppPoolName
    
    # Configurar para .NET Core (sin managed runtime)
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ''
    
    # Configurar identidad (ApplicationPoolIdentity es seguro)
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value 'ApplicationPoolIdentity'
    
    # Aumentar timeouts para .NET Core
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.idleTimeout -Value '00:20:00'
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name startupTimeLimit -Value '00:05:00'
    
    Write-Host "? App Pool creado: $AppPoolName"
    Write-Host "   - Runtime: Sin managed runtime (.NET Core)"
    Write-Host "   - Identidad: ApplicationPoolIdentity"
} else {
    Write-Host "? App Pool ya existe: $AppPoolName"
    
    # Asegurar configuración correcta incluso si ya existe
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ''
    Write-Host "   - Configuración actualizada (No Managed Code)"
}

# ========================================
# 3. Crear sitio si no existe
# ========================================
if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    # Verificar que el puerto esté disponible
    $existingSite = Get-Website | Where-Object { 
        $_.Bindings.Collection.bindingInformation -like "*:${BindingPort}:*" 
    }
    
    if ($existingSite) {
        Write-Warning "?? Puerto $BindingPort ya está en uso por el sitio: $($existingSite.Name)"
        $alternativePort = [int]$BindingPort + 1
        Write-Warning "   Usando puerto alternativo: $alternativePort"
        $BindingPort = $alternativePort
    }
    
    New-Website -Name $SiteName `
                -PhysicalPath $PhysicalPath `
                -ApplicationPool $AppPoolName `
                -Port $BindingPort `
                -Force
    
    Write-Host "? Sitio creado: $SiteName"
    Write-Host "   - URL: http://localhost:$BindingPort"
    Write-Host "   - App Pool: $AppPoolName"
} else {
    Write-Host "? Sitio ya existe: $SiteName"
    
    # Asegurar que use el App Pool correcto
    $currentPool = (Get-Website -Name $SiteName).ApplicationPool
    if ($currentPool -ne $AppPoolName) {
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
        Write-Host "? App Pool actualizado de '$currentPool' a '$AppPoolName'"
    }
    
    # Verificar/actualizar binding
    $currentBindings = (Get-Website -Name $SiteName).Bindings.Collection
    $hasCorrectBinding = $currentBindings | Where-Object { 
        $_.bindingInformation -like "*:${BindingPort}:*" 
    }
    
    if (-not $hasCorrectBinding) {
        Write-Host "?? Binding incorrecto. Actualizando a puerto $BindingPort..."
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name bindings -Value @{
            protocol='http';
            bindingInformation="*:${BindingPort}:"
        }
    }
}

# ========================================
# 4. Resumen final
# ========================================
Write-Host ""
Write-Host "?? Infraestructura IIS lista:"
Write-Host "   ? Sitio: $SiteName"
Write-Host "   ? App Pool: $AppPoolName"
Write-Host "   ? Ruta física: $PhysicalPath"
Write-Host "   ? Puerto: $BindingPort"

try {
    $poolState = (Get-WebAppPoolState -Name $AppPoolName).Value
    $siteState = (Get-WebSiteState -Name $SiteName).Value
    Write-Host "   ? Estado App Pool: $poolState"
    Write-Host "   ? Estado Sitio: $siteState"
} catch {
    Write-Host "   ?? No se pudo verificar estado (esperado si es primera vez)"
}
