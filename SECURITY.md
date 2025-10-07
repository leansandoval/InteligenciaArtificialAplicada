# 🔒 Guía de Seguridad - QuizCraft

## 📋 Resumen de Seguridad

Este documento describe las medidas de seguridad implementadas en QuizCraft y las mejores prácticas para el manejo de información sensible.

## 🔑 Información Sensible Protegida

### ✅ **Configuraciones Seguras:**

1. **Archivos de configuración excluidos de Git:**
   - `appsettings.json`
   - `appsettings.Development.json`
   - `appsettings.Production.json`
   - Todos los archivos `*.secrets.json`

2. **Variables de entorno recomendadas:**
   ```bash
   # OpenAI Configuration
   OPENAI_API_KEY=tu-api-key-real-aqui
   
   # Admin Password (Production)
   ADMIN_PASSWORD=tu-contraseña-segura-aqui
   
   # Database Connection (Production)
   QUIZCRAFT_CONNECTION_STRING=tu-cadena-conexion-aqui
   ```

## ⚠️ **Configuración Antes del Despliegue:**

### 1. **API Keys de OpenAI:**
- **Desarrollo**: Configura tu API key en `appsettings.Development.json`
- **Producción**: Usa variables de entorno o Azure Key Vault
- **Nunca** subas API keys reales al repositorio

### 2. **Contraseña de Administrador:**
- **Desarrollo**: La contraseña por defecto es `Admin123!`
- **Producción**: Configura `ADMIN_PASSWORD` como variable de entorno
- **Recomendación**: Usa contraseñas de al menos 12 caracteres con mayúsculas, minúsculas, números y símbolos

### 3. **Base de Datos:**
- **Desarrollo**: Usa LocalDB o SQL Server Express
- **Producción**: Configura cadena de conexión segura
- **Nunca** incluyas contraseñas de BD en archivos de configuración

## 🛡️ **Medidas de Seguridad Implementadas:**

### ✅ **Autenticación y Autorización:**
- Sistema de roles (Administrador, Profesor, Estudiante)
- ASP.NET Core Identity con políticas de contraseña
- Validación de email requerida

### ✅ **Protección de Datos:**
- Encriptación de contraseñas con Identity
- Configuración HTTPS recomendada
- Validación de entrada en todos los endpoints

### ✅ **Archivos Seguros:**
- Validación de tipos de archivo permitidos (.pdf, .docx, .txt, .pptx)
- Límites de tamaño de archivo (5MB por defecto)
- Escaneo de contenido antes del procesamiento

## 📁 **Estructura de Archivos de Configuración:**

```
src/QuizCraft.Web/
├── appsettings.json ❌ (Git ignored)
├── appsettings.Development.json ❌ (Git ignored)
├── appsettings.Example.json ✅ (Template seguro)
└── appsettings.Development.Example.json ✅ (Template seguro)
```

## 🔧 **Configuración de Desarrollo:**

1. **Copia los archivos de ejemplo:**
   ```bash
   cp appsettings.Example.json appsettings.json
   cp appsettings.Development.Example.json appsettings.Development.json
   ```

2. **Configura tus valores:**
   ```json
   {
     "OpenAI": {
       "ApiKey": "tu-api-key-aqui",
       "Model": "gpt-4o",
       "MaxTokens": 1500
     }
   }
   ```

## 🚀 **Configuración de Producción:**

### Variables de Entorno Requeridas:
```bash
# API Configuration
OPENAI_API_KEY=sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
ADMIN_PASSWORD=TuContraseñaSegura123!

# Database
QUIZCRAFT_CONNECTION_STRING="Server=servidor;Database=QuizCraftDb;User Id=usuario;Password=contraseña;TrustServerCertificate=true"
```

### Azure App Service:
```bash
# Configurar en el portal de Azure o CLI
az webapp config appsettings set --resource-group myResourceGroup --name myapp --settings OPENAI_API_KEY="sk-xxx" ADMIN_PASSWORD="xxx"
```

## 📝 **Lista de Verificación de Seguridad:**

- [ ] ✅ Archivos de configuración no están en Git
- [ ] ✅ API keys configuradas como variables de entorno
- [ ] ✅ Contraseña de admin cambiada en producción
- [ ] ✅ HTTPS habilitado en producción
- [ ] ✅ Cadena de conexión segura configurada
- [ ] ✅ Límites de archivo configurados apropiadamente
- [ ] ✅ Logs no contienen información sensible

## 🆘 **Reporte de Vulnerabilidades:**

Si encuentras vulnerabilidades de seguridad:
1. **NO** las publiques en issues públicos
2. Contacta al equipo de desarrollo directamente
3. Proporciona detalles técnicos y pasos para reproducir

## 📚 **Recursos Adicionales:**

- [ASP.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [OpenAI API Security](https://platform.openai.com/docs/guides/safety-best-practices)
- [Azure Security Center](https://docs.microsoft.com/en-us/azure/security-center/)

---
**Última actualización:** 7 de octubre de 2025
**Estado:** ✅ Información sensible securizada