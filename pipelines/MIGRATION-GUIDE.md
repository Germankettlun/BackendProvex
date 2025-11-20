# ?? RESUMEN EJECUTIVO - Migración a Pipeline Modular

## ? ARCHIVOS CREADOS

```
pipelines/
??? azure-pipelines-backend.yml       ? Pipeline principal (sin hardcoded values)
??? templates/
?   ??? deploy-backend-template.yml   ? Template reutilizable Dev/Prod
??? scripts/
?   ??? setup-iis.ps1                 ? Crear infraestructura IIS automáticamente
?   ??? cleanup-processes.ps1         ? Limpiar procesos zombie
?   ??? health-check.ps1              ? Validación post-deployment
??? README.md                         ? Documentación completa
```

---

## ?? ACCIONES REQUERIDAS (ANTES DE USAR)

### 1?? **AGREGAR VARIABLE EN AZURE DEVOPS** (?? CRÍTICO)

**Variable Group**: `ERP-Backend-Development`

| Variable | Valor |
|----------|-------|
| `agentNameDev` | `IISDev` |

**Pasos**:
1. Azure DevOps ? Pipelines ? Library
2. Abrir grupo `ERP-Backend-Development`
3. Click "+ Add"
4. Name: `agentNameDev`
5. Value: `IISDev`
6. Save

---

### 2?? **RENOMBRAR VARIABLE EXISTENTE** (?? IMPORTANTE)

**En ambos grupos** (`Development` y `Production`):

| Actual | Nuevo Nombre |
|--------|--------------|
| `ConnectionBD` | `ConnectionStrings__DatabaseConnection` |

**Pasos**:
1. Azure DevOps ? Pipelines ? Library
2. Abrir grupo `ERP-Backend-Development`
3. Click en `ConnectionBD` ? Edit
4. Cambiar nombre a: `ConnectionStrings__DatabaseConnection`
5. Save
6. **Repetir para grupo `ERP-Backend-Production`**

---

### 3?? **MOVER ARCHIVOS Y ACTUALIZAR PIPELINE**

```bash
# Desde raíz del repositorio (BackEnd-ERP/)
git checkout feature/cors
git pull origin feature/cors

# Eliminar pipeline viejo (ya está respaldado en pipelines/)
git rm azure-pipelines.yml

# Agregar nuevos archivos
git add pipelines/

# Commit
git commit -m "refactor(pipeline): Migrar a estructura modular sin hardcoded values

- Separar stages en templates reutilizables
- Extraer scripts PowerShell a /pipelines/scripts
- Remover todos los valores hardcoded
- Agregar documentación completa en README.md
- Alinear estructura con pipeline de frontend"

# Push
git push origin feature/cors
```

**Luego en Azure DevOps**:
1. Pipelines ? Seleccionar pipeline existente
2. Edit
3. Cambiar "YAML file path" de:
   - `azure-pipelines.yml` 
   - **A**: `pipelines/azure-pipelines-backend.yml`
4. Save & Run

---

## ?? COMPARACIÓN: ANTES vs DESPUÉS

### ? ANTES (Problemas)
```yaml
# ? Valores hardcoded
- agent.name -equals IISAgent01          # ? Hardcoded
value: 'C:\inetpub\wwwroot\ERPApiProd'  # ? Hardcoded
value: 'ERPApiProdSite'                 # ? Hardcoded

# ? Código duplicado Dev/Prod (400+ líneas repetidas)
# ? Scripts inline (difícil de testear)
# ? Sin documentación
```

### ? DESPUÉS (Solución)
```yaml
# ? Todo desde variables
agentName: $(agentName)                # ? Variable de DevOps
physicalPath: $(physicalPath)          # ? Variable de DevOps
siteName: $(siteName)                  # ? Variable de DevOps

# ? Template reutilizable (1 solo archivo para Dev/Prod)
# ? Scripts separados (testeables individualmente)
# ? README.md con documentación completa
```

---

## ?? BENEFICIOS ALCANZADOS

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Mantenibilidad** | ? Código duplicado | ? Template único reutilizable |
| **Seguridad** | ?? Valores expuestos | ? Todo desde variable groups |
| **Testability** | ? Scripts inline | ? Scripts separados testeables |
| **Consistencia** | ? Diferente a frontend | ? Mismo patrón que frontend |
| **Documentación** | ? Sin docs | ? README completo |
| **Infraestructura** | ?? Manual | ? Setup automático con `setup-iis.ps1` |

---

## ?? TESTING RECOMENDADO

### Test 1: Validar sintaxis del pipeline
```bash
# Desde Azure DevOps
Pipelines ? Edit ? Validate
```

### Test 2: Test local de scripts
```powershell
# Setup IIS (crear infraestructura)
.\pipelines\scripts\setup-iis.ps1 `
  -SiteName "ERPApiSite" `
  -AppPoolName "ERPApiPool" `
  -PhysicalPath "C:\Temp\TestERPApi" `
  -BindingPort 9999

# Cleanup (limpiar procesos)
.\pipelines\scripts\cleanup-processes.ps1 `
  -PhysicalPath "C:\inetpub\wwwroot\ERPApi"

# Health check
.\pipelines\scripts\health-check.ps1 `
  -Url "http://localhost:8082/api/v1/health" `
  -MaxRetries 3
```

### Test 3: Deploy a Dev primero
```bash
# Hacer push a dev para validar
git checkout dev
git merge feature/cors
git push origin dev

# Monitorear pipeline en Azure DevOps
# Si funciona ? hacer merge a prod
```

---

## ?? CHECKLIST FINAL

Antes de hacer merge a `prod`, verificar:

- [ ] Variable `agentNameDev` agregada a `ERP-Backend-Development`
- [ ] Variable `ConnectionBD` renombrada a `ConnectionStrings__DatabaseConnection` (Dev)
- [ ] Variable `ConnectionBD` renombrada a `ConnectionStrings__DatabaseConnection` (Prod)
- [ ] Pipeline path actualizado en Azure DevOps: `pipelines/azure-pipelines-backend.yml`
- [ ] Deploy a `dev` exitoso
- [ ] Health check pasando en `dev`
- [ ] Logs de stdout sin errores
- [ ] README.md revisado y entendido

---

## ?? ROLLBACK (si algo falla)

Si necesitas volver al pipeline anterior:

```bash
# Restaurar pipeline viejo
git checkout origin/prod -- azure-pipelines.yml

# Volver a configurar en Azure DevOps
Pipelines ? Edit ? YAML path: azure-pipelines.yml
```

---

## ?? SOPORTE

Si encuentras problemas:

1. **Revisar logs** en Azure DevOps (cada paso tiene logs detallados)
2. **Verificar variables** en Library ? Variable Groups
3. **Testear scripts** localmente (ver sección Testing)
4. **Consultar README.md** en `pipelines/README.md`

---

**Creado**: 2025-11-20  
**Autor**: GitHub Copilot  
**Versión**: 1.0
