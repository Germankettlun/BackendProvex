# =========================
# Script: Agregar Binding HTTPS al Sitio IIS
# =========================
param(
    [Parameter(Mandatory=$false)]
    [string]$SiteName = "ERPApiSite",
    
    [Parameter(Mandatory=$false)]
    [int]$HttpsPort = 443,
    
    [Parameter(Mandatory=$false)]
    [string]$CertThumbprint = $null
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AGREGAR BINDING HTTPS A IIS" -ForegroundColor Cyan
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
# 1. VERIFICAR SITIO EXISTE
# ========================================
Write-Host "`n1. VERIFICANDO SITIO IIS" -ForegroundColor Yellow
Write-Host "========================================"

try {
    $site = Get-Website -Name $SiteName -ErrorAction Stop
    Write-Host "[OK] Sitio encontrado: $SiteName" -ForegroundColor Green
    Write-Host "Estado: $($site.state)" -ForegroundColor Cyan
} catch {
    Write-Host "[ERROR] El sitio '$SiteName' no existe" -ForegroundColor Red
    exit 1
}

# ========================================
# 2. SELECCIONAR CERTIFICADO
# ========================================
Write-Host "`n2. SELECCIONANDO CERTIFICADO SSL" -ForegroundColor Yellow
Write-Host "========================================"

if ([string]::IsNullOrEmpty($CertThumbprint)) {
    Write-Host "Buscando certificados SSL validos..." -ForegroundColor Cyan
    
    $certs = Get-ChildItem -Path Cert:\LocalMachine\My | 
             Where-Object { 
                 $_.HasPrivateKey -eq $true -and 
                 $_.NotAfter -gt (Get-Date)
             } |
             Sort-Object NotAfter -Descending
    
    if ($certs.Count -eq 0) {
        Write-Host "[ERROR] No se encontraron certificados SSL validos" -ForegroundColor Red
        Write-Host "Necesitas instalar un certificado SSL primero" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "`nCertificados disponibles:" -ForegroundColor Cyan
    for ($i = 0; $i -lt $certs.Count; $i++) {
        $cert = $certs[$i]
        $daysValid = ($cert.NotAfter - (Get-Date)).Days
        Write-Host "[$i] Subject: $($cert.Subject)" -ForegroundColor White
        Write-Host "    Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
        Write-Host "    Expira en $daysValid dias ($($cert.NotAfter))" -ForegroundColor Gray
        Write-Host ""
    }
    
    # Auto-seleccionar el primer certificado (con mayor vigencia)
    $selectedCert = $certs[0]
    Write-Host "[OK] Certificado seleccionado automaticamente:" -ForegroundColor Green
    Write-Host "Subject: $($selectedCert.Subject)" -ForegroundColor Cyan
    Write-Host "Thumbprint: $($selectedCert.Thumbprint)" -ForegroundColor Cyan
    
    $CertThumbprint = $selectedCert.Thumbprint
} else {
    $selectedCert = Get-ChildItem -Path Cert:\LocalMachine\My\$CertThumbprint -ErrorAction SilentlyContinue
    if (-not $selectedCert) {
        Write-Host "[ERROR] Certificado con thumbprint '$CertThumbprint' no encontrado" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Usando certificado especificado:" -ForegroundColor Green
    Write-Host "Subject: $($selectedCert.Subject)" -ForegroundColor Cyan
}

# ========================================
# 3. VERIFICAR SI YA EXISTE BINDING HTTPS
# ========================================
Write-Host "`n3. VERIFICANDO BINDINGS EXISTENTES" -ForegroundColor Yellow
Write-Host "========================================"

$existingHttpsBinding = Get-WebBinding -Name $SiteName -Protocol https -ErrorAction SilentlyContinue

if ($existingHttpsBinding) {
    Write-Host "[WARN] Ya existe un binding HTTPS en este sitio" -ForegroundColor Yellow
    Write-Host "Binding actual: $($existingHttpsBinding.bindingInformation)" -ForegroundColor Cyan
    
    $response = Read-Host "`n¿Deseas reemplazarlo? (S/N)"
    if ($response -ne 'S' -and $response -ne 's') {
        Write-Host "[INFO] Operacion cancelada por el usuario" -ForegroundColor Yellow
        exit 0
    }
    
    Write-Host "Eliminando binding existente..." -ForegroundColor Cyan
    Remove-WebBinding -Name $SiteName -Protocol https -BindingInformation $existingHttpsBinding.bindingInformation
    Write-Host "[OK] Binding anterior eliminado" -ForegroundColor Green
}

# ========================================
# 4. CREAR BINDING HTTPS
# ========================================
Write-Host "`n4. CREANDO BINDING HTTPS" -ForegroundColor Yellow
Write-Host "========================================"

try {
    # Crear el binding
    New-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -SslFlags 1 -ErrorAction Stop
    Write-Host "[OK] Binding HTTPS creado en puerto $HttpsPort" -ForegroundColor Green
    
    # Asociar el certificado
    $binding = Get-WebBinding -Name $SiteName -Protocol https
    $binding.AddSslCertificate($CertThumbprint, "My")
    Write-Host "[OK] Certificado asociado al binding" -ForegroundColor Green
    
} catch {
    Write-Host "[ERROR] No se pudo crear el binding: $_" -ForegroundColor Red
    exit 1
}

# ========================================
# 5. VERIFICAR CONFIGURACION
# ========================================
Write-Host "`n5. VERIFICACION FINAL" -ForegroundColor Yellow
Write-Host "========================================"

$allBindings = Get-WebBinding -Name $SiteName

Write-Host "`nBindings configurados para '$SiteName':" -ForegroundColor Cyan
foreach ($b in $allBindings) {
    $icon = if ($b.protocol -eq "https") { "[HTTPS]" } else { "[HTTP]" }
    $color = if ($b.protocol -eq "https") { "Green" } else { "White" }
    Write-Host "$icon $($b.protocol) - $($b.bindingInformation)" -ForegroundColor $color
}

# Verificar certificado SSL asociado
Write-Host "`nCertificado SSL asociado:" -ForegroundColor Cyan
$sslBinding = Get-ChildItem IIS:\SslBindings | Where-Object { $_.Port -eq $HttpsPort } | Select-Object -First 1
if ($sslBinding -and $sslBinding.Certificate) {
    Write-Host "Subject: $($sslBinding.Certificate.Subject)" -ForegroundColor Green
    Write-Host "Thumbprint: $($sslBinding.Thumbprint)" -ForegroundColor Green
    Write-Host "Expira: $($sslBinding.Certificate.NotAfter)" -ForegroundColor Green
}

# ========================================
# 6. TEST DE CONECTIVIDAD
# ========================================
Write-Host "`n6. TEST DE CONECTIVIDAD" -ForegroundColor Yellow
Write-Host "========================================"

Write-Host "`n[HTTPS] Probando puerto $HttpsPort..." -ForegroundColor Cyan
Start-Sleep -Seconds 2

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response = Invoke-WebRequest -Uri "https://localhost:$HttpsPort/api/v1/meta/healthz" -SkipCertificateCheck -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    } else {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
        $response = Invoke-WebRequest -Uri "https://localhost:$HttpsPort/api/v1/meta/healthz" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null
    }
    Write-Host "[OK] HTTPS funciona correctamente! Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "[WARN] HTTPS no responde todavia: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "Esto puede ser normal si la aplicacion no esta iniciada" -ForegroundColor Gray
    if ($PSVersionTable.PSVersion.Major -lt 6) {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null
    }
}

# ========================================
# 7. SIGUIENTES PASOS
# ========================================
Write-Host "`n7. SIGUIENTES PASOS" -ForegroundColor Yellow
Write-Host "========================================"

Write-Host "`n[OK] Binding HTTPS configurado exitosamente!" -ForegroundColor Green
Write-Host "`nPara habilitar redireccion HTTP->HTTPS en tu aplicacion:" -ForegroundColor Cyan
Write-Host "1. Edita: appsettings.Production.json" -ForegroundColor White
Write-Host '   Agrega: "UseHttpsRedirection": true' -ForegroundColor Gray
Write-Host "2. Redeploy la aplicacion" -ForegroundColor White
Write-Host "`n3. Verifica acceso HTTPS:" -ForegroundColor White
Write-Host "   https://localhost:$HttpsPort/api/v1/meta/healthz" -ForegroundColor Gray
Write-Host "   https://<tu-dominio>/api/v1/meta/healthz" -ForegroundColor Gray

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "[OK] CONFIGURACION COMPLETADA" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan
