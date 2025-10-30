# 🔐 Configuración de Variables de Entorno en Azure

Este documento describe las variables de entorno requeridas que deben configurarse en **Azure App Settings** para el correcto funcionamiento de la aplicación en producción.

---

## ⚠️ IMPORTANTE

**NO colocar credenciales directamente en archivos de configuración** como `appsettings.Production.json`. 

Todas las credenciales y secretos deben configurarse en **Azure App Settings**, que es la forma segura de manejar información sensible en producción.

---

## 📋 Variables Requeridas

### 1. Connection String de SQL Server

**Variable**: `ConnectionStrings__DefaultConnection`

**Valor**:
```
Server=tcp:quizcraft-server.database.windows.net,1433;Initial Catalog=quizcraft-database;Persist Security Info=False;User ID=quizcraft-server-admin;Password=TU_CONTRASEÑA_AQUI;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**Comando**:
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "ConnectionStrings__DefaultConnection=Server=tcp:quizcraft-server.database.windows.net,1433;Initial Catalog=quizcraft-database;Persist Security Info=False;User ID=quizcraft-server-admin;Password=TU_CONTRASEÑA_AQUI;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Nota**: Reemplazar `TU_CONTRASEÑA_AQUI` con la contraseña real del servidor SQL.

---

### 2. API Key de Gemini (Google AI)

**Variable**: `Gemini__ApiKey`

**Valor**: Tu API Key de Google AI (formato: `AIzaSy...`)

**Comando**:
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "Gemini__ApiKey=TU_API_KEY_AQUI"
```

**Obtener API Key**:
1. Ve a [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Crea un nuevo proyecto o selecciona uno existente
3. Genera una nueva API Key
4. Copia la clave y úsala en el comando anterior

**⚠️ Sin esta configuración**: La generación de flashcards y quizzes con IA no funcionará (error: "Error en el procesamiento con IA").

---

### 3. Entorno de Ejecución

**Variable**: `ASPNETCORE_ENVIRONMENT`

**Valor**: `Production`

**Comando**:
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "ASPNETCORE_ENVIRONMENT=Production"
```

---

### 4. Hosts Permitidos

**Variable**: `AllowedHosts`

**Valor**: `quizcraft-webapp.azurewebsites.net`

**Comando**:
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings "AllowedHosts=quizcraft-webapp.azurewebsites.net"
```

---

### 5. Logging (Opcional para Debugging)

**Variables**:
- `ASPNETCORE_DETAILEDERRORS=true`
- `Logging__LogLevel__Default=Information`
- `Logging__LogLevel__Microsoft.AspNetCore=Warning`

**Comando**:
```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings `
    "ASPNETCORE_DETAILEDERRORS=true" `
    "Logging__LogLevel__Default=Information" `
    "Logging__LogLevel__Microsoft.AspNetCore=Warning"
```

**Nota**: En producción real, `ASPNETCORE_DETAILEDERRORS` debería estar en `false` por seguridad.

---

## 🚀 Configuración Completa en un Solo Comando

```powershell
az webapp config appsettings set `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --settings `
    "ConnectionStrings__DefaultConnection=Server=tcp:quizcraft-server.database.windows.net,1433;Initial Catalog=quizcraft-database;Persist Security Info=False;User ID=quizcraft-server-admin;Password=TU_CONTRASEÑA_SQL;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" `
    "Gemini__ApiKey=TU_API_KEY_GEMINI" `
    "ASPNETCORE_ENVIRONMENT=Production" `
    "AllowedHosts=quizcraft-webapp.azurewebsites.net" `
    "ASPNETCORE_DETAILEDERRORS=true" `
    "Logging__LogLevel__Default=Information" `
    "Logging__LogLevel__Microsoft.AspNetCore=Warning"
```

**⚠️ Recuerda**: Reemplazar `TU_CONTRASEÑA_SQL` y `TU_API_KEY_GEMINI` con los valores reales.

---

## ✅ Verificar Configuración

### Listar todas las configuraciones
```powershell
az webapp config appsettings list `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --output table
```

### Verificar una configuración específica
```powershell
# Connection String
az webapp config appsettings list `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --query "[?name=='ConnectionStrings__DefaultConnection']"

# Gemini API Key
az webapp config appsettings list `
  --name "quizcraft-webapp" `
  --resource-group "IAAplicadaGrupo2" `
  --query "[?name=='Gemini__ApiKey']"
```

---

## 🔄 Aplicar Cambios

**Después de modificar cualquier configuración**, reinicia la aplicación para que tome los nuevos valores:

```powershell
az webapp restart `
  --resource-group "IAAplicadaGrupo2" `
  --name "quizcraft-webapp"

# Esperar que la aplicación inicie
Start-Sleep -Seconds 30

# Verificar que responde
Invoke-WebRequest -Uri "https://quizcraft-webapp.azurewebsites.net" -UseBasicParsing
```

---

## 📝 Placeholders en appsettings.Production.json

Los siguientes placeholders están configurados en el archivo `appsettings.Production.json`:

| Placeholder | Variable Azure | Descripción |
|-------------|----------------|-------------|
| `${SQL_PASSWORD}` | `ConnectionStrings__DefaultConnection` | Contraseña SQL Server |
| `${GEMINI_API_KEY}` | `Gemini__ApiKey` | API Key de Google Gemini |

**Estos placeholders se reemplazan automáticamente** por los valores configurados en Azure App Settings cuando la aplicación se ejecuta en Azure.

---

## 🔐 Seguridad

### ✅ Buenas Prácticas Implementadas:
- ✅ Credenciales NO están en archivos de código fuente
- ✅ Uso de Azure App Settings para secretos
- ✅ Connection strings con placeholder
- ✅ API Keys con placeholder

### ⚠️ Nunca hacer:
- ❌ Commitear credenciales en Git
- ❌ Compartir API Keys en público
- ❌ Hardcodear contraseñas en código
- ❌ Dejar credenciales en archivos .json

---

## 📚 Referencias

- [Azure App Service Configuration](https://learn.microsoft.com/azure/app-service/configure-common)
- [ASP.NET Core Configuration](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Google AI Studio](https://makersuite.google.com/app/apikey)

---

**Última actualización**: 30 de octubre de 2025  
**Versión**: 1.0
