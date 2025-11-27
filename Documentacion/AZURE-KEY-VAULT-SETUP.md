# Azure Key Vault - Configuración

## Estado Actual

### ✅ Completado (27/11/2024)

1. **Key Vault creado**: `quizcraft-keyvault` en resource group `IAAplicadaGrupo2`
2. **Secretos almacenados**:
   - `Gemini--ApiKey`: AIzaSyBnTcX7Plr_lD8aIKdQrLJr0KL6kMdfrXM
   - `Gemini--Model`: gemini-2.0-flash
3. **Identidad administrada**: Habilitada en `quizcraft-webapp`
4. **Permisos**: App Service tiene acceso de lectura (get, list) a secretos
5. **API Key actualizada**: Variable de entorno `Gemini__ApiKey` actualizada en App Service

## Configuración Actual (Variables de Entorno)

Por el momento, la aplicación utiliza **variables de entorno** en App Service:

```bash
Gemini__ApiKey = AIzaSyBnTcX7Plr_lD8aIKdQrLJr0KL6kMdfrXM
```

El modelo se toma del archivo `appsettings.Production.json`:

```json
"Gemini": {
  "Model": "gemini-2.0-flash",
  ...
}
```

## Migración a Key Vault (Futuro)

### Opción 1: Usar referencias de Key Vault en App Settings

Cambiar las variables de entorno para que apunten al Key Vault:

```bash
# En vez de:
Gemini__ApiKey = AIzaSyBnTcX7Plr_lD8aIKdQrLJr0KL6kMdfrXM

# Usar:
Gemini__ApiKey = @Microsoft.KeyVault(SecretUri=https://quizcraft-keyvault.vault.azure.net/secrets/Gemini--ApiKey/)
Gemini__Model = @Microsoft.KeyVault(SecretUri=https://quizcraft-keyvault.vault.azure.net/secrets/Gemini--Model/)
```

**Comando para actualizar**:

```powershell
az webapp config appsettings set `
  --name quizcraft-webapp `
  --resource-group IAAplicadaGrupo2 `
  --settings `
    "Gemini__ApiKey=@Microsoft.KeyVault(SecretUri=https://quizcraft-keyvault.vault.azure.net/secrets/Gemini--ApiKey/)" `
    "Gemini__Model=@Microsoft.KeyVault(SecretUri=https://quizcraft-keyvault.vault.azure.net/secrets/Gemini--Model/)"
```

### Opción 2: Usar Azure.Extensions.AspNetCore.Configuration.Secrets

Modificar `Program.cs` para cargar secretos directamente desde Key Vault:

1. **Instalar paquete NuGet**:

```powershell
cd src/QuizCraft.Web
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

2. **Modificar `Program.cs`**:

```csharp
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// En producción, cargar secretos desde Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri("https://quizcraft-keyvault.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new DefaultAzureCredential());
}

// ... resto del código
```

3. **Actualizar `appsettings.Production.json`**:

```json
{
  "Gemini": {
    "ApiKey": "${Gemini--ApiKey}",
    "Model": "${Gemini--Model}",
    ...
  }
}
```

## Gestión de Secretos

### Listar secretos

```powershell
az keyvault secret list --vault-name quizcraft-keyvault -o table
```

### Ver un secreto específico

```powershell
az keyvault secret show --vault-name quizcraft-keyvault --name "Gemini--ApiKey"
```

### Actualizar un secreto

```powershell
az keyvault secret set --vault-name quizcraft-keyvault --name "Gemini--ApiKey" --value "nueva-api-key"
```

### Agregar nuevo secreto

```powershell
az keyvault secret set --vault-name quizcraft-keyvault --name "NuevoSecreto" --value "valor"
```

## Verificación

### Verificar identidad administrada del App Service

```powershell
az webapp identity show --name quizcraft-webapp --resource-group IAAplicadaGrupo2
```

### Verificar permisos en Key Vault

```powershell
az keyvault show --name quizcraft-keyvault --query properties.accessPolicies
```

### Verificar variables de entorno actuales

```powershell
az webapp config appsettings list --name quizcraft-webapp --resource-group IAAplicadaGrupo2 --query "[?contains(name, 'Gemini')]" -o table
```

## Comparación Development vs Production

| Configuración | Development | Production (Actual) |
|--------------|-------------|---------------------|
| **API Key** | AIzaSyBnTcX7Plr_lD8aIKdQrLJr0KL6kMdfrXM | AIzaSyBnTcX7Plr_lD8aIKdQrLJr0KL6kMdfrXM ✅ |
| **Model** | gemini-2.0-flash | gemini-2.0-flash ✅ |
| **Almacenamiento** | appsettings.Development.json | App Service Variables + Key Vault (backup) |

## Notas de Seguridad

- ✅ **Soft Delete habilitado**: Los secretos eliminados se pueden recuperar durante 90 días
- ✅ **Identidad administrada**: No se necesitan credenciales en el código
- ✅ **Permisos mínimos**: App Service solo tiene `get` y `list`, no puede modificar secretos
- ⚠️ **Acceso público habilitado**: Considerar restricciones de red si es necesario

## Recursos

- **Key Vault URI**: https://quizcraft-keyvault.vault.azure.net/
- **Resource Group**: IAAplicadaGrupo2
- **App Service**: quizcraft-webapp
- **Location**: Canada Central
- **Principal ID (App Service)**: 075dca74-1f16-48a8-8786-f8f53b075c03
