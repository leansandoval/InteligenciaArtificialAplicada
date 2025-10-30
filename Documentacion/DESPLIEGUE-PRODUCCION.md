# 📦 Guía de Despliegue en Producción - QuizCraft

Esta guía documenta el proceso completo para desplegar la aplicación QuizCraft en Azure, basado en el proceso real utilizado con Azure for Students.

---

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#requisitos-previos)
2. [Recursos de Azure](#recursos-de-azure)
3. [Configuración Inicial](#configuración-inicial)
4. [Proceso de Despliegue](#proceso-de-despliegue)
5. [Configuración Post-Despliegue](#configuración-post-despliegue)
6. [Verificación](#verificación)
7. [Solución de Problemas](#solución-de-problemas)
8. [Comandos de Mantenimiento](#comandos-de-mantenimiento)

---

## 🔧 Requisitos Previos

### Software Necesario

- **.NET 8.0 SDK** o superior
- **Azure CLI** (`az`) instalado y configurado
- **PowerShell** 7+ (recomendado)
- **Git** (para control de versiones)

### Verificación de Instalación

```powershell
# Verificar .NET
dotnet --version

# Verificar Azure CLI
az --version

# Iniciar sesión en Azure
az login
```

### Suscripción de Azure

- **Azure for Students** con $100 en créditos
- Regiones permitidas: `canadacentral`, `southcentralus`, `northcentralus`, `westus3`, `southafricanorth`

---

## ☁️ Recursos de Azure

### Recursos Creados Manualmente (Portal de Azure)

Debido a limitaciones de cuota en Azure for Students, los recursos se crean **manualmente** a través del Portal de Azure:

#### 1. **Resource Group**
- Nombre: `IAAplicadaGrupo2`
- Región: `canadacentral`

#### 2. **SQL Server**
- Nombre: `quizcraft-server`
- URL: `quizcraft-server.database.windows.net`
- Usuario Admin: `quizcraft-server-admin`
- Región: `canadacentral`

#### 3. **SQL Database**
- Nombre: `quizcraft-database`
- Servidor: `quizcraft-server`
- **⚠️ IMPORTANTE**: Usar tier **Basic** ($5/mes) en lugar de GP_Gen5 ($400/mes)
- Tamaño recomendado: 2GB

#### 4. **App Service Plan**
- Nombre: `ASP-IAAplicadaGrupo2-b1d5`
- Tier: **B1** (Basic)
- Sistema Operativo: **Linux**
- Región: `canadacentral`

#### 5. **Web App**
- Nombre: `quizcraft-webapp`
- URL: `https://quizcraft-webapp.azurewebsites.net`
- Runtime: **.NET 8.0** (DOTNETCORE:8.0)
- App Service Plan: `ASP-IAAplicadaGrupo2-b1d5`

### Pasos en el Portal de Azure

1. Ir a [Azure Portal](https://portal.azure.com)
2. Crear **Resource Group** → Elegir región permitida
3. Crear **SQL Server** → Configurar admin y contraseña
4. Crear **SQL Database** → Seleccionar tier Basic, vincular a SQL Server
5. Crear **App Service Plan** → Seleccionar B1, Linux
6. Crear **Web App** → Seleccionar .NET 8.0, vincular a App Service Plan

---

## ⚙️ Configuración Inicial

### 1. Configurar Firewall de SQL Server

Permitir acceso desde Azure y tu IP local:

```powershell
# Permitir servicios de Azure (0.0.0.0)
az sql server firewall-rule create `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "AllowAzureServices" `
  --start-ip-address "0.0.0.0" `
  --end-ip-address "0.0.0.0"

# Permitir tu IP local (reemplazar con tu IP)
az sql server firewall-rule create `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "AllowMyIP" `
  --start-ip-address "TU_IP_AQUI" `
  --end-ip-address "TU_IP_AQUI"
```

**Nota**: Puedes obtener tu IP con `(Invoke-WebRequest -Uri "https://ifconfig.me/ip").Content.Trim()`

### 2. Configurar Connection String en Azure

**⚠️ CRÍTICO**: La connection string debe configurarse en **Azure App Settings**, no solo en `appsettings.json`:

```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "ConnectionStrings__DefaultConnection=Server=tcp:quizcraft-server.database.windows.net,1433;Initial Catalog=quizcraft-database;Persist Security Info=False;User ID=quizcraft-server-admin;Password=TU_CONTRASEÑA_AQUI;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Formato importante**: `ConnectionStrings__DefaultConnection` (doble guión bajo `__`)

### 3. Configurar Otras Variables de Entorno

```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings `
    "ASPNETCORE_ENVIRONMENT=Production" `
    "AllowedHosts=quizcraft-webapp.azurewebsites.net" `
    "ASPNETCORE_DETAILEDERRORS=true" `
    "Logging__LogLevel__Default=Information" `
    "Logging__LogLevel__Microsoft.AspNetCore=Warning"
```

### 4. Configurar API Key de Gemini (IA)

**⚠️ CRÍTICO para funcionalidad de IA**: La generación de flashcards y quizzes con IA requiere la API Key de Gemini configurada:

```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "Gemini__ApiKey=TU_API_KEY_AQUI"
```

**Nota**: Obtén tu API Key en [Google AI Studio](https://makersuite.google.com/app/apikey). Sin esta configuración, obtendrás el error "Error en el procesamiento con IA".

---

## 🚀 Proceso de Despliegue

### Paso 1: Preparar la Aplicación

```powershell
# Navegar al directorio del proyecto
cd C:\QuizCraft\src\QuizCraft.Web

# Limpiar compilaciones previas (opcional)
Remove-Item -Recurse -Force "bin\Release" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "obj\Release" -ErrorAction SilentlyContinue
```

### Paso 2: Compilar en Release

```powershell
dotnet build -c Release
```

**Verificar que no hay errores de compilación antes de continuar.**

### Paso 3: Publicar la Aplicación

```powershell
dotnet publish -c Release -o ./publish
```

Esto genera los archivos listos para producción en `./publish`

### Paso 4: Aplicar Migraciones de Base de Datos

**⚠️ IMPORTANTE**: Ejecutar antes del primer despliegue o cuando hay nuevas migraciones.

```powershell
# Asegurarse de que la connection string en appsettings.Production.json es correcta
dotnet ef database update --configuration Production
```

**Verificar en Azure Portal** que las tablas se crearon correctamente en la base de datos.

### Paso 5: Desplegar a Azure

```powershell
# Comprimir la carpeta publish en un archivo ZIP
Compress-Archive -Path "./publish\*" -DestinationPath "publish.zip" -Force

# Desplegar el archivo ZIP a Azure
az webapp deploy `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp" `
  --src-path "publish.zip" `
  --type zip

# Limpiar archivo temporal
Remove-Item "publish.zip" -Force
```

Este comando:
- Comprime la carpeta `./publish` en un archivo ZIP
- Lo sube a Azure Web App
- Descomprime automáticamente en el servidor
- Reinicia la aplicación

### Paso 6: Reiniciar la Web App

```powershell
az webapp restart `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"

# Esperar 30 segundos para que la app inicie completamente
Start-Sleep -Seconds 30
```

---

## 🔐 Configuración Post-Despliegue

### 1. Verificar Roles de Identity

La aplicación inicializa automáticamente los roles en `Program.cs`:
- **Administrador**
- **Profesor**
- **Estudiante**

Los roles se crean al iniciar la aplicación por primera vez.

### 2. Verificar Usuario Administrador

El usuario admin se crea automáticamente:
- **Email**: `admin@quizcraft.com`
- **Contraseña**: Configurada en variable de entorno `ADMIN_PASSWORD` o por defecto `Admin123!`

### 3. Verificar Logs de Inicialización

```powershell
# Ver logs en tiempo real
az webapp log tail `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"
```

Buscar mensajes como:
```
INICIO DE INICIALIZACIÓN DE BASE DE DATOS
==> INICIALIZANDO ROLES...
✅✅✅ Rol 'Administrador' ya existe
==> VERIFICANDO USUARIO ADMINISTRADOR...
✅✅✅ Usuario administrador creado: admin@quizcraft.com
INICIALIZACIÓN COMPLETADA CON ÉXITO
```

---

## ✅ Verificación

### 1. Verificar que la App Responde

```powershell
# Probar la URL de la aplicación
Invoke-WebRequest -Uri "https://quizcraft-webapp.azurewebsites.net" -UseBasicParsing
```

Debe devolver **StatusCode: 200 OK**

### 2. Verificar Base de Datos

```powershell
# Listar tablas en la base de datos
az sql db show `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database"
```

### 3. Probar Funcionalidades

1. **Abrir la aplicación**: `https://quizcraft-webapp.azurewebsites.net`
2. **Probar Login**: Iniciar sesión con `admin@quizcraft.com`
3. **Probar Registro**: Crear un nuevo usuario estudiante
4. **Verificar Dashboard**: Comprobar que se muestra correctamente

---

## 🛠️ Solución de Problemas

### Error: "Bad Request - Invalid Hostname"

**Causa**: `AllowedHosts` no configurado correctamente.

**Solución**:
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "AllowedHosts=quizcraft-webapp.azurewebsites.net"
```

### Error: "Login failed for user"

**Causa**: Connection string no configurada en Azure App Settings.

**Solución**: Verificar que la connection string esté en Azure (no solo en appsettings.json):

```powershell
# Verificar configuración
az webapp config appsettings list `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --query "[?name=='ConnectionStrings__DefaultConnection']"

# Si está vacío, configurar nuevamente (ver sección Configuración Inicial)
```

**Después de cualquier cambio en App Settings, reiniciar la aplicación.**

### Error: "Cannot connect to SQL Server"

**Causa**: Firewall bloqueando la conexión.

**Solución**: Verificar reglas de firewall:

```powershell
az sql server firewall-rule list `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --output table
```

Asegurarse de que existe la regla `AllowAzureServices` (0.0.0.0 - 0.0.0.0).

### Error: "Ocurrió un error durante el registro"

**Causas posibles**:
1. Connection string no configurada
2. Base de datos no migrada
3. Roles no inicializados

**Solución**:
1. Verificar connection string en Azure App Settings
2. Ejecutar migraciones: `dotnet ef database update --configuration Production`
3. Revisar logs: `az webapp log tail --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"`
4. Reiniciar la aplicación después de cualquier cambio

### Logs de Aplicación

```powershell
# Ver logs en tiempo real
az webapp log tail `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"

# Configurar logging (si no está habilitado)
az webapp log config `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --application-logging filesystem `
  --level information
```

---

## 🔄 Comandos de Mantenimiento

### Actualizar la Aplicación

```powershell
# 1. Navegar al proyecto
cd C:\QuizCraft\src\QuizCraft.Web

# 2. Compilar
dotnet build -c Release

# 3. Publicar
dotnet publish -c Release -o ./publish

# 4. Desplegar
az webapp deploy `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp" `
  --src-path "./publish" `
  --type zip

# 5. Reiniciar
az webapp restart `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"
```

### Aplicar Nueva Migración

```powershell
# 1. Crear migración localmente
cd C:\QuizCraft\src\QuizCraft.Infrastructure
dotnet ef migrations add NombreDeLaMigracion --startup-project ../QuizCraft.Web

# 2. Aplicar a producción
cd ../QuizCraft.Web
dotnet ef database update --configuration Production
```

### Ver Estado de la Aplicación

```powershell
# Estado del Web App
az webapp show `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp" `
  --query "{name:name, state:state, defaultHostName:defaultHostName}" `
  --output table

# Estado de la base de datos
az sql db show `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database" `
  --query "{name:name, status:status, serviceLevelObjective:currentServiceObjectiveName}" `
  --output table
```

### Reiniciar Servicios

```powershell
# Reiniciar Web App
az webapp restart `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"

# Detener Web App (para mantenimiento)
az webapp stop `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"

# Iniciar Web App
az webapp start `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"
```

### Monitoreo de Costos

```powershell
# Ver uso de la base de datos
az sql db show-usage `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database"

# Cambiar tier de base de datos (para optimizar costos)
az sql db update `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database" `
  --service-objective Basic `
  --max-size 2GB
```

**⚠️ IMPORTANTE**: Con Azure for Students ($100 créditos):
- **Basic tier** ($5/mes) = 20 meses de uso ✅
- **GP_Gen5** ($400/mes) = Solo 7 días de uso ❌

### Backup de Base de Datos

```powershell
# Crear backup manual
az sql db copy `
  --resource-group "IAAplicadaGrupo2" `
  --server "quizcraft-server" `
  --name "quizcraft-database" `
  --dest-name "quizcraft-database-backup-$(Get-Date -Format 'yyyyMMdd')" `
  --dest-server "quizcraft-server"
```

### Limpiar Recursos Locales

```powershell
# Limpiar carpetas publish
cd C:\QuizCraft\src\QuizCraft.Web
Remove-Item -Recurse -Force "publish" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "bin\Release" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "obj\Release" -ErrorAction SilentlyContinue
```

---

## 📊 Checklist de Despliegue

Antes de cada despliegue, verificar:

- [ ] Código compilado sin errores (`dotnet build -c Release`)
- [ ] Migraciones aplicadas (`dotnet ef database update --configuration Production`)
- [ ] Connection string configurada en Azure App Settings
- [ ] Variables de entorno configuradas (ASPNETCORE_ENVIRONMENT, AllowedHosts)
- [ ] Firewall de SQL Server permite Azure services (0.0.0.0)
- [ ] Tier de base de datos es **Basic** (no GP_Gen5) para optimizar costos
- [ ] Logs habilitados para debugging
- [ ] Aplicación publicada (`dotnet publish -c Release -o ./publish`)
- [ ] Despliegue completado (`az webapp deploy`)
- [ ] Aplicación reiniciada (`az webapp restart`)
- [ ] URL responde 200 OK (`Invoke-WebRequest`)
- [ ] Login funciona con usuario admin
- [ ] Registro de nuevos usuarios funciona

---

## 📚 Referencias

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://learn.microsoft.com/azure/azure-sql/)
- [ASP.NET Core Deployment](https://learn.microsoft.com/aspnet/core/host-and-deploy/azure-apps/)
- [Entity Framework Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)

---

## 🆘 Soporte

Si encuentras problemas:

1. **Revisar logs**: `az webapp log tail --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"`
2. **Verificar estado**: `az webapp show --resource-group "IAAplicadaGrupo2" --name "quizcraft-webapp"`
3. **Consultar la sección**: [Solución de Problemas](#solución-de-problemas)
4. **Reintentar despliegue**: A veces un simple reinicio resuelve el problema

---

**Última actualización**: 30 de octubre de 2025  
**Versión**: 1.0  
**Proyecto**: QuizCraft - Aplicación de Gestión de Estudio con IA
