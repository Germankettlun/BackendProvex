# =========================
# Script: setup-iis.ps1
# Descripción: Crea/actualiza sitio IIS, AppPool y binding HTTP.
# Uso:
#   .\setup-iis.ps1 -SiteName "ProvexApiDev" -AppPoolName "ProvexApiDevPool" -PhysicalPath "C:\inetpub\wwwroot\ProvexApiDev" -BindingPort 5000
# =========================
param(
    [Parameter(Mandatory = $true)]
    [string]$SiteName,

    [Parameter(Mandatory = $true)]
    [string]$AppPoolName,

    [Parameter(Mandatory = $true)]
    [string]$PhysicalPath,

    [Parameter(Mandatory = $true)]
    [int]$BindingPort
)

$ErrorActionPreference = 'Stop'

# Asegurar módulo de IIS
if ($PSVersionTable.PSEdition -eq 'Core') {
    Import-Module WebAdministration -UseWindowsPowerShell
} else {
    Import-Module WebAdministration
}

Write-Host "===== Configurando IIS ====="
Write-Host "  Sitio     : $SiteName"
Write-Host "  AppPool   : $AppPoolName"
Write-Host "  Ruta      : $PhysicalPath"
Write-Host "  Puerto    : $BindingPort"

# Crear carpeta física si no existe
if (-not (Test-Path -LiteralPath $PhysicalPath)) {
    Write-Host "Creando carpeta física $PhysicalPath"
    New-Item -ItemType Directory -Path $PhysicalPath -Force | Out-Null
}

# Crear/actualizar AppPool
if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    Write-Host "Creando AppPool '$AppPoolName'"
    New-WebAppPool -Name $AppPoolName | Out-Null
} else {
    Write-Host "AppPool '$AppPoolName' ya existe"
}

# AppPool self-contained, sin CLR
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value "ApplicationPoolIdentity"

# Crear/actualizar sitio
if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    Write-Host "Creando sitio '$SiteName' en puerto $BindingPort"
    New-Website -Name $SiteName -PhysicalPath $PhysicalPath -Port $BindingPort -IPAddress "*" -Force | Out-Null
} else {
    Write-Host "Sitio '$SiteName' ya existe, actualizando ruta física..."
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $PhysicalPath
}

# Asociar AppPool al sitio
Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName

# Asegurar binding HTTP en el puerto indicado
$bindingInfo = "*:${BindingPort}:"
$siteBindings = (Get-WebBinding -Name $SiteName -ErrorAction SilentlyContinue) | ForEach-Object {
    $_.bindingInformation
}

if ($siteBindings -notcontains $bindingInfo) {
    Write-Host "Agregando binding http en puerto $BindingPort"
    New-WebBinding -Name $SiteName -Protocol "http" -Port $BindingPort -IPAddress "*" | Out-Null
} else {
    Write-Host "Binding http en puerto $BindingPort ya existe"
}

# Resumen
Write-Host "IIS listo:"
Write-Host "  Sitio     : $SiteName"
Write-Host "  AppPool   : $AppPoolName"
Write-Host "  Ruta      : $PhysicalPath"
Write-Host "  Puerto    : $BindingPort"

try {
    $poolState = (Get-WebAppPoolState -Name $AppPoolName).Value
    $siteState = (Get-WebSiteState -Name $SiteName).Value
    Write-Host "  Estado AppPool : $poolState"
    Write-Host "  Estado Sitio   : $siteState"
} catch {
    Write-Host "  No se pudo leer estado de sitio/AppPool (puede ser primera ejecución)"
}
