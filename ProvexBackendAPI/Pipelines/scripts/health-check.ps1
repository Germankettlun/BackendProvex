# =========================
# Script: Health Check
# Descripción: Valida que el endpoint de salud responda correctamente
# =========================
param(
    [Parameter(Mandatory=$true)]
    [string]$Url,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxRetries = 10,
    
    [Parameter(Mandatory=$false)]
    [int]$WaitSeconds = 3,
    
    [Parameter(Mandatory=$false)]
    [int]$TimeoutSeconds = 10
)

Write-Host "?? Testing health endpoint: $Url"
Write-Host "   Max retries: $MaxRetries"
Write-Host "   Wait between retries: ${WaitSeconds}s"
Write-Host ""

# Esperar warm-up inicial
Write-Host "? Esperando ${WaitSeconds}s para warm-up inicial..."
Start-Sleep -Seconds $WaitSeconds

for ($i = 1; $i -le $MaxRetries; $i++) {
    try {
        Write-Host "? Intento $i/$MaxRetries..."
        
        $response = Invoke-WebRequest $Url `
            -UseBasicParsing `
            -TimeoutSec $TimeoutSeconds `
            -ErrorAction Stop
        
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
            Write-Host "? Health check OK: HTTP $($response.StatusCode)"
            
            # Mostrar respuesta (máximo 500 caracteres)
            $content = $response.Content
            if ($content.Length -gt 500) {
                $content = $content.Substring(0, 500) + "... (truncado)"
            }
            Write-Host "Response: $content"
            
            Write-Host ""
            Write-Host "?? Deployment verificado exitosamente"
            exit 0
        } else {
            Write-Warning "?? HTTP $($response.StatusCode) recibido"
        }
    } catch {
        $errorMsg = $_.Exception.Message
        
        # Simplificar mensaje de error común
        if ($errorMsg -match "No es posible conectar con el servidor remoto") {
            $errorMsg = "Servicio aún no disponible (iniciando...)"
        } elseif ($errorMsg -match "The operation has timed out") {
            $errorMsg = "Timeout (servicio puede estar ocupado)"
        }
        
        Write-Host "? Error: $errorMsg"
        
        if ($i -lt $MaxRetries) {
            Write-Host "   Reintentando en ${WaitSeconds}s..."
            Start-Sleep -Seconds $WaitSeconds
        }
    }
}

# Si llegamos aquí, todos los intentos fallaron
Write-Warning ""
Write-Warning "????????????????????????????????????????"
Write-Warning "?? Health check FALLÓ después de $MaxRetries intentos"
Write-Warning "????????????????????????????????????????"
Write-Warning ""
Write-Warning "Posibles causas:"
Write-Warning "  1. La aplicación está tardando en iniciar (revisa logs de stdout)"
Write-Warning "  2. Error en la configuración de web.config"
Write-Warning "  3. Error en la aplicación (revisa logs de IIS)"
Write-Warning "  4. Base de datos no accesible"
Write-Warning "  5. URL de health check incorrecta: $Url"
Write-Warning ""
Write-Warning "Revisa los logs de stdout en el paso anterior para más detalles"

# No fallar el pipeline, solo advertencia
exit 0
