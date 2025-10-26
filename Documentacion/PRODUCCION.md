# Configuración de Producción - QuizCraft

## Variables de Entorno Requeridas

### Base de Datos
- `SQL_SERVER`: Servidor SQL Server (ej: `quizcraft-sql.database.windows.net`)
- `SQL_DATABASE`: Nombre de la base de datos (ej: `QuizCraftDb`)
- `SQL_USER`: Usuario de SQL Server
- `SQL_PASSWORD`: Contraseña de SQL Server

### Servicios Externos
- `GEMINI_API_KEY`: API Key de Google Gemini para generación de contenido con IA
- `APPINSIGHTS_INSTRUMENTATIONKEY`: Clave de Application Insights (opcional)
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Cadena de conexión de Application Insights (opcional)

## Despliegue en Azure App Service

### 1. Crear Recursos en Azure
```bash
# Crear Resource Group
az group create --name QuizCraft-RG --location eastus

# Crear App Service Plan
az appservice plan create --name QuizCraft-Plan --resource-group QuizCraft-RG --sku B1

# Crear Web App
az webapp create --name quizcraft-app --resource-group QuizCraft-RG --plan QuizCraft-Plan --runtime "DOTNET|8.0"

# Crear SQL Server
az sql server create --name quizcraft-sql --resource-group QuizCraft-RG --location eastus --admin-user sqladmin --admin-password [PASSWORD]

# Crear Base de Datos
az sql db create --resource-group QuizCraft-RG --server quizcraft-sql --name QuizCraftDb --service-objective S0
```

### 2. Configurar Variables de Entorno en Azure
```bash
az webapp config appsettings set --name quizcraft-app --resource-group QuizCraft-RG --settings \
  SQL_SERVER="quizcraft-sql.database.windows.net" \
  SQL_DATABASE="QuizCraftDb" \
  SQL_USER="sqladmin" \
  SQL_PASSWORD="[PASSWORD]" \
  GEMINI_API_KEY="[YOUR_GEMINI_API_KEY]"
```

### 3. Configurar Firewall de SQL Server
```bash
# Permitir servicios de Azure
az sql server firewall-rule create --resource-group QuizCraft-RG --server quizcraft-sql --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Permitir tu IP (para administración)
az sql server firewall-rule create --resource-group QuizCraft-RG --server quizcraft-sql --name AllowMyIP --start-ip-address [YOUR_IP] --end-ip-address [YOUR_IP]
```

### 4. Compilar y Publicar
```powershell
# Navegar al proyecto web
cd src/QuizCraft.Web

# Publicar la aplicación
dotnet publish -c Release -o ./publish

# Comprimir para despliegue
Compress-Archive -Path ./publish/* -DestinationPath ../deploy.zip -Force

# Desplegar a Azure
az webapp deployment source config-zip --resource-group QuizCraft-RG --name quizcraft-app --src ../deploy.zip
```

### 5. Aplicar Migraciones de Base de Datos
```powershell
# Desde el proyecto web
dotnet ef database update --connection "[CONNECTION_STRING]"
```

## Configuración de Seguridad

### HTTPS
- La aplicación requiere HTTPS en producción
- Azure App Service proporciona certificado SSL automáticamente

### Rate Limiting
- Configurado en `appsettings.Production.json`
- 100 solicitudes por minuto por IP
- 5000 solicitudes por hora por IP

### CORS
- Deshabilitado por defecto en producción
- Habilitar solo si es necesario y configurar orígenes específicos

## Monitoreo

### Application Insights (Opcional)
1. Crear recurso de Application Insights en Azure
2. Configurar las variables `APPINSIGHTS_INSTRUMENTATIONKEY` y `APPLICATIONINSIGHTS_CONNECTION_STRING`
3. La aplicación comenzará a enviar telemetría automáticamente

### Logs
- Los logs se envían a Azure App Service Logs
- Nivel de log: Warning y Error solamente
- Ver logs: `az webapp log tail --name quizcraft-app --resource-group QuizCraft-RG`

## Optimizaciones de Producción

### Caché
- Caché habilitado con expiración de 30 minutos
- Caché deslizante de 10 minutos

### Base de Datos
- Connection pooling habilitado
- Multiple Active Result Sets habilitado
- Queries optimizadas con índices

### Archivos Estáticos
- Compresión de respuestas habilitada
- Caché de archivos estáticos configurado

## Backup y Recuperación

### Base de Datos
```bash
# Crear backup
az sql db export --resource-group QuizCraft-RG --server quizcraft-sql --name QuizCraftDb --admin-user sqladmin --admin-password [PASSWORD] --storage-key [STORAGE_KEY] --storage-key-type StorageAccessKey --storage-uri https://[STORAGE_ACCOUNT].blob.core.windows.net/backups/quizcraft-backup.bacpac
```

### Aplicación
- Azure App Service mantiene snapshots automáticos
- Configurar backup programado en el portal de Azure

## Escalabilidad

### Escalado Vertical
```bash
# Cambiar a plan más grande
az appservice plan update --name QuizCraft-Plan --resource-group QuizCraft-RG --sku P1V2
```

### Escalado Horizontal
```bash
# Configurar auto-scaling
az monitor autoscale create --resource-group QuizCraft-RG --resource quizcraft-app --resource-type Microsoft.Web/sites --min-count 1 --max-count 5 --count 1
```

## Troubleshooting

### Ver logs en tiempo real
```bash
az webapp log tail --name quizcraft-app --resource-group QuizCraft-RG
```

### Reiniciar aplicación
```bash
az webapp restart --name quizcraft-app --resource-group QuizCraft-RG
```

### Verificar configuración
```bash
az webapp config appsettings list --name quizcraft-app --resource-group QuizCraft-RG
```

## Costos Estimados (USD/mes)

- App Service Plan B1: ~$13
- SQL Database S0: ~$15
- Application Insights (opcional): ~$0-5 (según uso)
- **Total estimado: $28-33/mes**

## Contacto y Soporte

Para problemas de producción:
- Revisar logs en Azure Portal
- Verificar Application Insights (si está configurado)
- Contactar al equipo de desarrollo
