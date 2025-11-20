# ?? RESUMEN DE CAMBIOS - Pipeline Azure DevOps

## ?? Problema Identificado

### 1. Ubicación incorrecta de `azure-pipelines.yml`
- **Problema**: El archivo estaba dentro de `ProvexBackendAPI/` 
- **Corrección**: Movido a la raíz del repositorio `BackEnd-ERP/`
- **Razón**: Azure DevOps **SIEMPRE** busca el pipeline en la raíz del repositorio

### 2. Scripts no encontrados en deployment jobs
- **Error**: `Invalid file path 'C:\agent\_work\2\s\ProvexBackendAPI\Pipelines\scripts\setup-iis.ps1'`
- **Causa**: Los deployment jobs **NO descargan el código fuente** por defecto, solo los artifacts
- **Corrección**: Agregado `checkout: self` al inicio del deployment

---

## ? Cambios Aplicados

### 1. Estructura de Archivos CORRECTA

```
BackEnd-ERP/                                    ? Raíz del repositorio
??? azure-pipelines.yml                         ? ? ARCHIVO PRINCIPAL (debe estar aquí)
??? ProvexBackendAPI/
    ??? ProvexBackendAPI.csproj
    ??? Pipelines/
    ?   ??? azure-pipelines-backend.yml         ? Pipeline modular
    ?   ??? scripts/
    ?   ?   ??? cleanup-processes.ps1
    ?   ?   ??? health-check.ps1
    ?   ?   ??? setup-iis.ps1
    ?   ??? templates/
    ?       ??? deploy-backend-template.yml     ? Template de deployment
    ??? [resto del proyecto]
```

### 2. Archivo `azure-pipelines.yml` (raíz del repo)

**Ubicación**: `BackEnd-ERP/azure-pipelines.yml`

```yaml
# Orquestador principal
trigger:
  branches:
    include:
      - dev
      - prod

extends:
  template: ProvexBackendAPI/Pipelines/azure-pipelines-backend.yml
```

**¿Por qué en la raíz?**
- Azure DevOps **siempre** busca el pipeline en la raíz del repositorio
- Es el punto de entrada estándar para todos los pipelines
- Permite que Azure DevOps detecte automáticamente el pipeline

### 3. Template de Deployment Actualizado

**Ubicación**: `ProvexBackendAPI/Pipelines/templates/deploy-backend-template.yml`

**Cambio clave**: Agregado `checkout: self` al inicio:

```yaml
jobs:
- deployment: DeployToIIS_${{ parameters.environment }}
  # ...
  strategy:
    runOnce:
      deploy:
        steps:
        # ? NUEVO: Descargar código fuente para acceder a los scripts
        - checkout: self
          displayName: 'Checkout source code'
          clean: true

        # Resto de los pasos...
        - task: DownloadPipelineArtifact@2
          # ...
```

**¿Por qué es necesario?**
- Los **deployment jobs** solo descargan artifacts por defecto
- NO descargan automáticamente el código fuente
- Los scripts PowerShell están en el repo, no en los artifacts
- `checkout: self` descarga el código fuente en `$(Build.SourcesDirectory)`

---

## ?? Impacto de los Cambios

### ? Beneficios

1. **Pipeline funcional**
   - Los scripts ahora se encuentran correctamente
   - El deployment puede ejecutarse sin errores

2. **Estructura estándar**
   - Sigue las mejores prácticas de Azure DevOps
   - Facilita el mantenimiento y la colaboración

3. **Separación de responsabilidades**
   - **Artifacts**: Contienen el código compilado (.dll, etc.)
   - **Source code**: Contiene los scripts de infraestructura

### ?? Consideraciones

1. **Tiempo de ejecución**
   - El `checkout: self` agrega ~5-10 segundos al deployment
   - Es mínimo comparado con el beneficio de tener scripts disponibles

2. **Limpieza del workspace**
   - El `clean: true` asegura que siempre haya código fresco
   - Evita problemas de archivos antiguos

3. **Variables de rutas**
   - `$(Build.SourcesDirectory)` = Código fuente descargado
   - `$(Pipeline.Workspace)` = Artifacts descargados
   - Las rutas de scripts usan `$(Build.SourcesDirectory)`

---

## ?? Próximos Pasos

### 1. Verificar en Azure DevOps

Después de hacer push, verifica:

```bash
# Hacer commit y push
git add .
git commit -m "fix: Move azure-pipelines.yml to repo root and add checkout step"
git push origin feature/cors
```

### 2. Configurar Pipeline en Azure DevOps

Si es la primera vez:

1. Ve a **Pipelines** ? **New Pipeline**
2. Selecciona tu repositorio
3. Azure DevOps detectará automáticamente `azure-pipelines.yml`
4. Confirma y ejecuta

### 3. Monitorear la Ejecución

Durante el deployment, deberías ver:

```
? Checkout source code
? Download Artifact
? Setup IIS Infrastructure      ? Ahora funcionará
? Resolve Artifact Path
? Cleanup Zombie Processes       ? Ahora funcionará
? Stop IIS
? Configure Permissions
? Copy Files
? Create web.config
? Start IIS
? View Startup Logs
? Health Check                   ? Ahora funcionará
```

---

## ?? Referencias

- [Azure DevOps Pipeline YAML Schema](https://docs.microsoft.com/azure/devops/pipelines/yaml-schema)
- [Deployment Jobs](https://docs.microsoft.com/azure/devops/pipelines/process/deployment-jobs)
- [Checkout Step](https://docs.microsoft.com/azure/devops/pipelines/yaml-schema/steps-checkout)

---

## ? Preguntas Frecuentes

### ¿Por qué no puedo ver `azure-pipelines.yml` en Visual Studio?

**Respuesta**: Porque está fuera de la carpeta del proyecto. Para verlo:
1. En el Explorador de soluciones, haz clic en **Mostrar todos los archivos**
2. O agrégalo como **Elemento de Solución**

### ¿Puedo mover los scripts a los artifacts?

**Respuesta**: Sí, pero no es recomendable porque:
- Los scripts son infraestructura, no código compilado
- Cambiarían con cada build, aumentando el tamaño de artifacts
- Es mejor práctica mantenerlos en source control

### ¿Qué pasa si elimino `checkout: self`?

**Respuesta**: Los scripts no estarán disponibles y el deployment fallará con:
```
##[error]Invalid file path '...\scripts\setup-iis.ps1'. 
A path to a .ps1 file is required.
```

---

**Última actualización**: 2024-11-20
**Autor**: GitHub Copilot
**Versión**: 1.0
