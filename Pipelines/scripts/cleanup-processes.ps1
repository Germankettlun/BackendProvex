# =========================
# Script: cleanup-processes.ps1
# Descripción: Mata procesos dotnet.exe que estén usando la ruta indicada.
# Uso:
#   .\cleanup-processes.ps1 -PhysicalPath "C:\inetpub\wwwroot\ProvexApiDev"
# =========================
param(
    [Parameter(Mandatory = $true)]
    [string]$PhysicalPath
)

$ErrorActionPreference = 'Continue'

Write-Host "Buscando procesos dotnet asociados a: $PhysicalPath"

$killed = 0

try {
    $dotnetProcs = Get-Process dotnet -ErrorAction SilentlyContinue
} catch {
    $dotnetProcs = @()
}

foreach ($p in $dotnetProcs) {
    try {
        $wmi = Get-CimInstance Win32_Process -Filter "ProcessId = $($p.Id)" -ErrorAction SilentlyContinue
        if ($null -ne $wmi -and $wmi.CommandLine -like "*$PhysicalPath*") {
            Write-Host "Terminando dotnet.exe (PID=$($p.Id)) => $($wmi.CommandLine)"
            Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
            $killed++
        }
    } catch {
        # Ignorar errores
    }
}

if ($killed -eq 0) {
    Write-Host "No se encontraron procesos zombie"
} else {
    Write-Host "$killed proceso(s) dotnet finalizado(s)"
}
