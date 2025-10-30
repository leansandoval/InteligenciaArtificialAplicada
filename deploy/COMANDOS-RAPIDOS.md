# ⚡ Comandos Rápidos para Producción - QuizCraft

Este archivo contiene los comandos más usados para el despliegue y mantenimiento de QuizCraft en Azure.

---

## 🚀 DESPLIEGUE COMPLETO (Recomendado)

### Opción 1: Script Automatizado
```powershell
cd C:\QuizCraft\deploy
.\deploy-production.ps1
```

### Opción 2: Comandos Manuales
```powershell
cd C:\QuizCraft\src\QuizCraft.Web
Remove-Item -Recurse -Force "publish" -ErrorAction SilentlyContinue
dotnet build -c Release
dotnet publish -c Release -o ./publish
dotnet ef database update --configuration Production
Compress-Archive -Path "publish\*" -DestinationPath "publish.zip" -Force
az webapp deploy --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --src-path "publish.zip" --type zip
Remove-Item "publish.zip" -Force
az webapp restart --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
Start-Sleep -Seconds 30
Invoke-WebRequest -Uri "https://quizcraft-webapp.azurewebsites.net" -UseBasicParsing
```

---

## 🔄 ACTUALIZACIÓN RÁPIDA (Sin Migraciones)

```powershell
cd C:\QuizCraft\src\QuizCraft.Web
dotnet publish -c Release -o ./publish
Compress-Archive -Path "publish\*" -DestinationPath "publish.zip" -Force
az webapp deploy --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --src-path "publish.zip" --type zip
Remove-Item "publish.zip" -Force
az webapp restart --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
```

---

## 🗄️ SOLO MIGRACIONES DE BASE DE DATOS

```powershell
cd C:\QuizCraft\src\QuizCraft.Web
dotnet ef database update --configuration Production
```

---

## 🔍 DIAGNÓSTICO Y MONITOREO

### Ver Logs en Tiempo Real
```powershell
az webapp log tail --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
```

### Ver Estado de la Aplicación
```powershell
az webapp show --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --query "{name:name, state:state, defaultHostName:defaultHostName}" --output table
```

### Ver Estado de la Base de Datos
```powershell
az sql db show --resource-group "IAAplicadaGrupo2" --server "quizcraft-server" --name "quizcraft-database" --query "{name:name, status:status, tier:currentServiceObjectiveName}" --output table
```

### Verificar Connection String
```powershell
az webapp config appsettings list --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --query "[?name=='ConnectionStrings__DefaultConnection']"
```

### Ver Todas las Variables de Entorno
```powershell
az webapp config appsettings list --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --output table
```

### Probar URL de la Aplicación
```powershell
Invoke-WebRequest -Uri "https://quizcraft-webapp.azurewebsites.net" -UseBasicParsing
```

---

## 🔄 GESTIÓN DE LA APLICACIÓN

### Reiniciar Web App
```powershell
az webapp restart --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
```

### Detener Web App (Mantenimiento)
```powershell
az webapp stop --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
```

### Iniciar Web App
```powershell
az webapp start --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"
```

---

## ⚙️ CONFIGURACIÓN DE VARIABLES DE ENTORNO

### Configurar Connection String
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "ConnectionStrings__DefaultConnection=Server=tcp:quizcraft-server.database.windows.net,1433;Initial Catalog=quizcraft-database;Persist Security Info=False;User ID=quizcraft-server-admin;Password=TU_CONTRASEÑA;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Configurar Entorno de Producción
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "ASPNETCORE_ENVIRONMENT=Production" "AllowedHosts=quizcraft-webapp.azurewebsites.net"
```

### Habilitar Logging Detallado (Debugging)
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "ASPNETCORE_DETAILEDERRORS=true" "Logging__LogLevel__Default=Information"
```

### Configurar API Key de Gemini (IA)
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "Gemini__ApiKey=TU_API_KEY_AQUI"
```

**Nota**: Necesario para generar flashcards y quizzes con IA. Sin esto, obtendrás el error "Error en el procesamiento con IA".

---

## 🔥 FIREWALL DE SQL SERVER

### Listar Reglas de Firewall
```powershell
az sql server firewall-rule list --resource-group "IAAplicadaGrupo2" --server "quizcraft-server" --output table
```

### Permitir Servicios de Azure
```powershell
az sql server firewall-rule create `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "AllowAzureServices" `
  --start-ip-address "0.0.0.0" `
  --end-ip-address "0.0.0.0"
```

### Permitir tu IP Local
```powershell
$MI_IP = (Invoke-WebRequest -Uri "https://ifconfig.me/ip").Content.Trim()
az sql server firewall-rule create `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "AllowMyIP" `
  --start-ip-address $MI_IP `
  --end-ip-address $MI_IP
```

---

## 💰 OPTIMIZACIÓN DE COSTOS

### Ver Tier Actual de la Base de Datos
```powershell
az sql db show --resource-group "IAAplicadaGrupo2" --server "quizcraft-server" --name "quizcraft-database" --query "currentServiceObjectiveName"
```

### Cambiar a Basic Tier ($5/mes) - RECOMENDADO
```powershell
az sql db update `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database" `
  --service-objective Basic `
  --max-size 2GB
```

### Ver Uso de Créditos (Requiere extensión cost-management)
```powershell
az extension add --name cost-management
az costmanagement query `
  --type ActualCost `
  --dataset-aggregation "{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"}}" `
  --dataset-grouping name=ResourceGroup type=Dimension `
  --timeframe MonthToDate
```

---

## 🗂️ MIGRACIONES DE BASE DE DATOS

### Crear Nueva Migración
```powershell
cd C:\QuizCraft\src\QuizCraft.Infrastructure
dotnet ef migrations add NombreDeLaMigracion --startup-project ../QuizCraft.Web
```

### Aplicar Migraciones a Producción
```powershell
cd C:\QuizCraft\src\QuizCraft.Web
dotnet ef database update --configuration Production
```

### Ver Migraciones Aplicadas
```powershell
cd C:\QuizCraft\src\QuizCraft.Web
dotnet ef migrations list --configuration Production
```

### Revertir Última Migración
```powershell
cd C:\QuizCraft\src\QuizCraft.Web
dotnet ef database update NombreDeMigracionAnterior --configuration Production
```

---

## 📦 BACKUP Y RESTAURACIÓN

### Crear Backup de Base de Datos
```powershell
az sql db copy `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database" `
  --dest-name "quizcraft-database-backup-$(Get-Date -Format 'yyyyMMdd')" `
  --dest-server "quizcraft-server"
```

### Listar Backups Disponibles
```powershell
az sql db list --resource-group "IAAplicadaGrupo2" --server "quizcraft-server" --query "[?contains(name, 'backup')]" --output table
```

---

## 🧹 LIMPIEZA LOCAL

### Limpiar Carpetas de Publicación
```powershell
cd C:\QuizCraft\src\QuizCraft.Web
Remove-Item -Recurse -Force "publish" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "bin\Release" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "obj\Release" -ErrorAction SilentlyContinue
```

### Limpiar Todas las Carpetas bin/obj del Proyecto
```powershell
cd C:\QuizCraft\src
Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Recurse -Force
```

---

## 🔐 SEGURIDAD

### Resetear Contraseña de SQL Server
```powershell
az sql server update `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-server" `
  --admin-password "NUEVA_CONTRASEÑA_AQUI"
```

**⚠️ IMPORTANTE**: Después de cambiar la contraseña, actualizar la Connection String en Azure App Settings.

---

## 📊 INFORMACIÓN DEL PROYECTO

### Recursos de Azure
- **Resource Group**: IAAplicadaGrupo2
- **Región**: canadacentral
- **SQL Server**: quizcraft-server.database.windows.net
- **SQL Database**: quizcraft-database
- **Web App**: quizcraft-webapp.azurewebsites.net
- **Usuario SQL**: quizcraft-server-admin

### URLs Importantes
- **Aplicación**: https://quizcraft-webapp.azurewebsites.net
- **Portal Azure**: https://portal.azure.com
- **Azure for Students**: https://azure.microsoft.com/en-us/free/students/

---

## 🆘 COMANDOS DE EMERGENCIA

### Si la Aplicación No Responde
```powershell
# 1. Ver logs
az webapp log tail --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"

# 2. Verificar estado
az webapp show --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --query "state"

# 3. Reiniciar
az webapp restart --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"

# 4. Si persiste, redesplegar
cd C:\QuizCraft\deploy
.\deploy-production.ps1 -SkipMigrations
```

### Si la Base de Datos No Se Conecta
```powershell
# 1. Verificar connection string
az webapp config appsettings list --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp" --query "[?name=='ConnectionStrings__DefaultConnection']"

# 2. Verificar firewall
az sql server firewall-rule list --resource-group "IAAplicadaGrupo2" --server "quizcraft-server" --output table

# 3. Probar conexión local
cd C:\QuizCraft\src\QuizCraft.Web
dotnet ef database update --configuration Production --verbose
```

---

**Última actualización**: 30 de octubre de 2025  
**Versión**: 1.0  
**Proyecto**: QuizCraft
