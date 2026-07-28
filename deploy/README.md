# 📦 Scripts de Despliegue - QuizCraft

Esta carpeta contiene scripts y documentación para el despliegue de QuizCraft en Azure.

## 🚀 Despliegue Rápido

**Para desplegar a producción**, usa el script automatizado:

```powershell
.\deploy-production.ps1
```

Este script ejecuta automáticamente:
1. ✅ Limpieza de publicaciones anteriores
2. 🔨 Compilación en modo Release
3. 📦 Publicación de la aplicación
4. 🗄️ Aplicación de migraciones de base de datos
5. ☁️ Despliegue a Azure Web App
6. 🔄 Reinicio de la aplicación
7. 🔍 Verificación del despliegue

### Opciones del Script

```powershell
# Omitir compilación (si ya compilaste)
.\deploy-production.ps1 -SkipBuild

# Omitir migraciones (si no hay cambios en DB)
.\deploy-production.ps1 -SkipMigrations

# Omitir reinicio automático
.\deploy-production.ps1 -SkipRestart

# Modo verbose para debugging
.\deploy-production.ps1 -Verbose

# Combinar opciones
.\deploy-production.ps1 -SkipMigrations -SkipRestart
```

## 📚 Recursos de despliegue

Consulta también los [comandos rápidos](COMANDOS-RAPIDOS.md). Este README incluye:
- ✅ Requisitos previos
- ☁️ Configuración de recursos de Azure
- 🔧 Configuración inicial (firewall, connection strings)
- 🚀 Proceso completo de despliegue
- 🛠️ Solución de problemas
- 🔄 Comandos de mantenimiento

## 🔧 Requisitos Previos

Antes de usar los scripts, asegúrate de tener:

1. **Azure CLI** instalado (`az --version`)
2. **Sesión activa** en Azure (`az login`)
3. **.NET 8.0 SDK** instalado (`dotnet --version`)
4. **Recursos de Azure** creados manualmente:
   - Resource Group: `IAAplicadaGrupo2`
   - SQL Server: `quizcraft-server`
   - SQL Database: `quizcraft-database`
   - Web App: `quizcraft-webapp`

## 📊 Proceso Manual (Si el Script Falla)

Si prefieres ejecutar los comandos manualmente:

```powershell
# 1. Navegar al proyecto
cd C:\QuizCraft\src\QuizCraft.Web

# 2. Limpiar
Remove-Item -Recurse -Force "publish" -ErrorAction SilentlyContinue

# 3. Compilar
dotnet build -c Release

# 4. Publicar
dotnet publish -c Release -o ./publish

# 5. Aplicar migraciones
dotnet ef database update --configuration Production

# 6. Desplegar
az webapp deploy `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp" `
  --src-path "./publish" `
  --type zip

# 7. Reiniciar
az webapp restart `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"

# 8. Esperar y verificar
Start-Sleep -Seconds 30
Invoke-WebRequest -Uri "https://quizcraft-webapp.azurewebsites.net" -UseBasicParsing
```

## 🆘 Solución de Problemas

Si encuentras errores durante el despliegue:

1. **Ver logs en tiempo real**:
   ```powershell
   az webapp log tail --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
   ```

2. **Verificar estado del Web App**:
   ```powershell
   az webapp show --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
   ```

3. **Verificar connection string**:
   ```powershell
   az webapp config appsettings list `
     --resource-group "IAAplicadaGrupo2" `
     --name "quizcraft-webapp" `
     --query "[?name=='ConnectionStrings__DefaultConnection']"
   ```

4. **Reintentar despliegue**:
   ```powershell
   .\deploy-production.ps1 -SkipBuild -SkipMigrations
   ```

Para más detalles, revisa las secciones anteriores de este README y los
[comandos rápidos](COMANDOS-RAPIDOS.md).

## 📞 Comandos Útiles

```powershell
# Ver logs en tiempo real
az webapp log tail --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"

# Ver estado de la aplicación
az webapp show --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --query "{name:name, state:state}"

# Reiniciar la aplicación
az webapp restart --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"

# Ver configuración
az webapp config appsettings list --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"

# Detener la aplicación (mantenimiento)
az webapp stop --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"

# Iniciar la aplicación
az webapp start --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
```

---

## ⚠️ Nota Importante sobre Costos

**Azure for Students** proporciona $100 en créditos. Asegúrate de:

- ✅ Usar tier **Basic** para la base de datos ($5/mes)
- ❌ Evitar tier **GP_Gen5** ($400/mes)
- 🔍 Monitorear el uso de créditos regularmente

Para cambiar el tier de la base de datos:

```powershell
az sql db update `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database" `
  --service-objective Basic `
  --max-size 2GB
```

---

**Última actualización**: 30 de octubre de 2025  
**Aplicación**: QuizCraft v1.0
