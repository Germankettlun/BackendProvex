# =========================
# Script: Cleanup Zombie Processes
# Descripción: Mata procesos dotnet.exe zombie que bloquean archivos
# =========================
param(
    [Parameter(Mandatory=$true)]
    [string]$PhysicalPath
)

$ErrorActionPreference = 'Continue'

Write-Host "?? Buscando procesos dotnet.exe corriendo desde: $PhysicalPath"

$killedCount = 0

Get-Process -Name 'dotnet' -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        $processId = $_.Id
        $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $processId" -ErrorAction SilentlyContinue).CommandLine
        
        if ($cmdLine -and $cmdLine -match [regex]::Escape($PhysicalPath)) {
            Write-Host "?? Matando proceso zombie: PID $processId"
            Write-Host "   Command: $($cmdLine.Substring(0, [Math]::Min(100, $cmdLine.Length)))"
            
            Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
            $killedCount++
        }
    } catch {
        # Ignorar errores al inspeccionar procesos
    }
}

if ($killedCount -eq 0) {
    Write-Host "? No se encontraron procesos zombie"
} else {
    Write-Host "? $killedCount proceso(s) zombie eliminado(s)"
    Start-Sleep -Seconds 3
}

Write-Host "? Limpieza de procesos completada"
