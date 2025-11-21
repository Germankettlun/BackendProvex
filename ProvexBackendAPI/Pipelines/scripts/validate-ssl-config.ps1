# =========================
# Script: Validar Configuración SSL/HTTPS en IIS
# =========================
param(
    [Parameter(Mandatory=$false)]
    [string]$SiteName = "ERPApiSite"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VALIDACION DE HTTPS/SSL EN IIS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Importar módulo IIS
try {
    Import-Module WebAdministration -ErrorAction Stop
    Write-Host "[OK] Modulo WebAdministration cargado" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Error al cargar WebAdministration: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# 1. VERIFICAR BINDINGS DEL SITIO
# ========================================
Write-Host "`n1. BINDINGS DEL SITIO '$SiteName'" -ForegroundColor Yellow
Write-Host "========================================"

try {
    $bindings = Get-WebBinding -Name $SiteName -ErrorAction Stop
    
    if ($bindings.Count -eq 0) {
        Write-Host "[WARN] No se encontraron bindings para el sitio" -ForegroundColor Yellow
    } else {
        $bindings | ForEach-Object {
            $protocol = $_.protocol
            $bindingInfo = $_.bindingInformation
            $icon = if ($protocol -eq "https") { "[HTTPS]" } else { "[HTTP]" }
            $color = if ($protocol -eq "https") { "Green" } else { "White" }
            
            Write-Host "$icon Protocol: $protocol | Binding: $bindingInfo" -ForegroundColor $color
        }
        
        # Verificar si existe HTTPS
        $hasHttps = $bindings | Where-Object { $_.protocol -eq "https" }
        if ($hasHttps) {
            Write-Host "`n[OK] HTTPS CONFIGURADO" -ForegroundColor Green
        } else {
            Write-Host "`n[WARN] NO HAY BINDING HTTPS" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
}

# ========================================
# 2. CERTIFICADOS SSL INSTALADOS
# ========================================
Write-Host "`n2. CERTIFICADOS SSL DISPONIBLES" -ForegroundColor Yellow
Write-Host "========================================"

try {
    $certs = Get-ChildItem -Path Cert:\LocalMachine\My | 
             Where-Object { $_.HasPrivateKey -eq $true } |
             Select-Object Subject, Thumbprint, NotBefore, NotAfter
    
    if ($certs.Count -eq 0) {
        Write-Host "[WARN] No se encontraron certificados con clave privada" -ForegroundColor Yellow
    } else {
        foreach ($cert in $certs) {
            $daysUntilExpiry = ($cert.NotAfter - (Get-Date)).Days
            if ($daysUntilExpiry -lt 30 -and $daysUntilExpiry -ge 0) {
                $status = "[WARN] EXPIRA PRONTO"
                $statusColor = "Yellow"
            } elseif ($daysUntilExpiry -lt 0) {
                $status = "[ERROR] EXPIRADO"
                $statusColor = "Red"
            } else {
                $status = "[OK] VALIDO"
                $statusColor = "Green"
            }
            
            Write-Host "`nSubject: $($cert.Subject)" -ForegroundColor Cyan
            Write-Host "Thumbprint: $($cert.Thumbprint)"
            Write-Host "Valido desde: $($cert.NotBefore)"
            Write-Host "Expira: $($cert.NotAfter) ($daysUntilExpiry dias) $status" -ForegroundColor $statusColor
        }
    }
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
}

# ========================================
# 3. SSL BINDINGS (Certificados Asociados)
# ========================================
Write-Host "`n3. CERTIFICADOS ASOCIADOS A BINDINGS" -ForegroundColor Yellow
Write-Host "========================================"

try {
    $sslBindings = Get-ChildItem IIS:\SslBindings -ErrorAction SilentlyContinue
    
    if ($sslBindings.Count -eq 0) {
        Write-Host "[WARN] No hay certificados asociados a bindings SSL" -ForegroundColor Yellow
    } else {
        foreach ($binding in $sslBindings) {
            Write-Host "`nIP: $($binding.IPAddress) | Port: $($binding.Port)" -ForegroundColor Cyan
            Write-Host "Thumbprint: $($binding.Thumbprint)"
            if ($binding.Certificate) {
                Write-Host "Certificado: $($binding.Certificate.Subject)"
                Write-Host "Expira: $($binding.Certificate.NotAfter)"
            }
        }
    }
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
}

# ========================================
# 4. CONFIGURACIÓN DEL APP POOL
# ========================================
Write-Host "`n4. CONFIGURACION DEL APP POOL" -ForegroundColor Yellow
Write-Host "========================================"

try {
    $site = Get-Website -Name $SiteName -ErrorAction Stop
    $appPoolName = $site.applicationPool
    $appPool = Get-Item "IIS:\AppPools\$appPoolName" -ErrorAction Stop
    
    Write-Host "Nombre: $appPoolName" -ForegroundColor Cyan
    Write-Host "Estado: $($appPool.state)"
    Write-Host ".NET CLR Version: $($appPool.managedRuntimeVersion)"
    Write-Host "Pipeline Mode: $($appPool.managedPipelineMode)"
    Write-Host "Identity: $($appPool.processModel.identityType)"
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
}

# ========================================
# 5. TEST DE CONECTIVIDAD
# ========================================
Write-Host "`n5. TEST DE CONECTIVIDAD" -ForegroundColor Yellow
Write-Host "========================================"

# Obtener puertos configurados
$httpBinding = $bindings | Where-Object { $_.protocol -eq "http" } | Select-Object -First 1
$httpsBinding = $bindings | Where-Object { $_.protocol -eq "https" } | Select-Object -First 1

if ($httpBinding) {
    # Extraer puerto de formato "*:8083:" o "10.115.1.253:8083:"
    $httpPort = $httpBinding.bindingInformation -replace '^[^:]*:(\d+):.*$', '$1'
    
    Write-Host "`n[HTTP] Probando puerto $httpPort..." -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$httpPort/api/v1/meta/healthz" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        Write-Host "[OK] HTTP funciona: $($response.StatusCode)" -ForegroundColor Green
    } catch {
        Write-Host "[WARN] HTTP no responde: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

if ($httpsBinding) {
    # Extraer puerto de formato "*:443:" o "10.115.1.253:443:"
    $httpsPort = $httpsBinding.bindingInformation -replace '^[^:]*:(\d+):.*$', '$1'
    
    Write-Host "`n[HTTPS] Probando puerto $httpsPort..." -ForegroundColor Cyan
    try {
        # Ignorar errores de certificado para pruebas locales
        if ($PSVersionTable.PSVersion.Major -ge 6) {
            # PowerShell Core
            $response = Invoke-WebRequest -Uri "https://localhost:$httpsPort/api/v1/meta/healthz" -SkipCertificateCheck -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        } else {
            # Windows PowerShell
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
            $response = Invoke-WebRequest -Uri "https://localhost:$httpsPort/api/v1/meta/healthz" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null
        }
        Write-Host "[OK] HTTPS funciona: $($response.StatusCode)" -ForegroundColor Green
    } catch {
        Write-Host "[WARN] HTTPS no responde: $($_.Exception.Message)" -ForegroundColor Yellow
        if ($PSVersionTable.PSVersion.Major -lt 6) {
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null
        }
    }
}

# ========================================
# 6. RECOMENDACIONES
# ========================================
Write-Host "`n6. RECOMENDACIONES" -ForegroundColor Yellow
Write-Host "========================================"

$hasHttpsBinding = $bindings | Where-Object { $_.protocol -eq "https" }
$hasValidCert = $certs | Where-Object { $_.NotAfter -gt (Get-Date) }

if ($hasHttpsBinding -and $hasValidCert) {
    Write-Host "[OK] Configuracion HTTPS completa detectada" -ForegroundColor Green
    Write-Host "`nPara habilitar redireccion HTTPS en la aplicacion:" -ForegroundColor Cyan
    Write-Host "1. Agrega a appsettings.Production.json:" -ForegroundColor White
    Write-Host '   "UseHttpsRedirection": true' -ForegroundColor Gray
    Write-Host "2. Redeploy la aplicacion" -ForegroundColor White
} elseif (-not $hasHttpsBinding) {
    Write-Host "[WARN] No hay binding HTTPS configurado" -ForegroundColor Yellow
    Write-Host "`nPara agregar HTTPS:" -ForegroundColor Cyan
    Write-Host "New-WebBinding -Name '$SiteName' -Protocol https -Port 443 -SslFlags 1" -ForegroundColor Gray
} elseif (-not $hasValidCert) {
    Write-Host "[WARN] No hay certificado valido" -ForegroundColor Yellow
    Write-Host "`nNecesitas instalar un certificado SSL valido" -ForegroundColor Cyan
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "[OK] VALIDACION COMPLETADA" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan
