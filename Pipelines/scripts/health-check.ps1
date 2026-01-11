# =========================
# Script: health-check.ps1
# Descripción: Valida que el endpoint de salud responda correctamente.
# Uso:
#   .\health-check.ps1 -Url "https://api.provexsa.cl/health" -MaxRetries 10 -WaitSeconds 3
# =========================
param(
    [Parameter(Mandatory = $true)]
    [string]$Url,

    [int]$MaxRetries = 10,
    [int]$WaitSeconds = 3,
    [int]$TimeoutSeconds = 10
)

$ErrorActionPreference = 'Continue'
$ProgressPreference    = 'SilentlyContinue'

Write-Host "===== HEALTH CHECK ====="
Write-Host "  Url         : $Url"
Write-Host "  Reintentos  : $MaxRetries"
Write-Host "  Intervalo   : $WaitSeconds s"
Write-Host "  Timeout HTTP: $TimeoutSeconds s"

for ($i = 1; $i -le $MaxRetries; $i++) {
    Write-Host "Intento $i de $MaxRetries..."

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec $TimeoutSeconds -ErrorAction Stop

        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) {
            Write-Host "OK: StatusCode=$($response.StatusCode)"
            exit 0
        } else {
            Write-Warning ("Respuesta inválida (StatusCode={0})" -f $response.StatusCode)
        }
    } catch {
        Write-Warning ("Error en intento {0}: {1}" -f $i, $_.Exception.Message)
    }

    if ($i -lt $MaxRetries) {
        Start-Sleep -Seconds $WaitSeconds
    }
}

Write-Error "Health check FALLÓ después de $MaxRetries intentos contra $Url"
exit 1
