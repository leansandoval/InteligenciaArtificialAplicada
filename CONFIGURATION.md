# 🔧 Guía de Configuración - QuizCraft

## 📋 Configuración Inicial

### 1. Copiar Archivos de Configuración

Copia los archivos de ejemplo y personalízalos según tu entorno:

```bash
# Archivo principal
cp src/QuizCraft.Web/appsettings.Example.json src/QuizCraft.Web/appsettings.json

# Archivo de desarrollo
cp src/QuizCraft.Web/appsettings.Development.Example.json src/QuizCraft.Web/appsettings.Development.json
```

### 2. Configurar Base de Datos

Edita tu archivo `appsettings.json` y actualiza la cadena de conexión según tu configuración:

#### 🎯 Para SQL Server Express (Recomendado)
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=QuizCraftDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
```

#### 🎯 Para LocalDB 
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuizCraftDb;Trusted_Connection=true;MultipleActiveResultSets=true"
```

#### 🎯 Para SQL Server con Autenticación
```json
"DefaultConnection": "Server=localhost;Database=QuizCraftDb;User Id=tu_usuario;Password=tu_contraseña;TrustServerCertificate=true"
```

### 3. Configurar Google Gemini (Opcional)

Si planeas usar las funciones de IA, agrega tu API Key GRATUITA:

```json
"Gemini": {
  "ApiKey": "tu-api-key-de-gemini-aqui",
  "Model": "gemini-2.0-flash-exp",
  "MaxTokens": 1500,
  "Temperature": 0.7,
  "TopP": 0.95,
  "TopK": 40,
  "MaxRequestsPerDay": 1000,
  "MaxTokensPerUser": 50000,
  "TimeoutSeconds": 120,
  "MaxRetries": 3,
  "IsEnabled": true,
  "BaseUrl": "https://generativelanguage.googleapis.com"
}
```

**📝 Configuración de Google Gemini:**
- **ApiKey**: Tu clave API GRATUITA de Google Gemini (obtén una en [AI Studio](https://aistudio.google.com/app/apikey))
- **Model**: Modelo a usar (gemini-2.0-flash-exp, gemini-pro, etc.)
- **MaxTokens**: Máximo de tokens por solicitud
- **Temperature**: Creatividad del modelo (0.0 = conservador, 2.0 = creativo)
- **TopP**: Núcleo sampling para diversidad de respuesta
- **TopK**: Selección de tokens para respuesta
- **MaxRequestsPerDay**: Límite diario de solicitudes por usuario
- **MaxTokensPerUser**: Límite de tokens por usuario

**🆓 Ventajas de Gemini:**
- ✅ **Completamente GRATUITO** - No requiere tarjeta de crédito
- ✅ **Límites generosos** - Ideal para desarrollo y uso académico
- ✅ **Fácil configuración** - Solo necesitas cuenta de Google
- ✅ **Modelo avanzado** - Calidad comparable a GPT-4
- ✅ **Sin costos ocultos** - Perfecto para estudiantes y proyectos

### 4. Aplicar Migraciones

```bash
cd src/QuizCraft.Web
dotnet ef database update
```

### 5. Ejecutar la Aplicación

```bash
dotnet run
```

## 🔑 Credenciales por Defecto

Una vez que ejecutes las migraciones, tendrás disponible:

- **Email:** `admin@quizcraft.com`
- **Contraseña:** `Admin123!`

## 🌍 Variables de Entorno (Alternativa Segura)

Para mayor seguridad en producción, puedes usar variables de entorno:

### Windows (PowerShell)
```powershell
$env:QUIZCRAFT_CONNECTION_STRING="Server=.\\SQLEXPRESS;Database=QuizCraftDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
$env:GEMINI_API_KEY="tu-api-key-aqui"
```

### Linux/Mac (Bash)
```bash
export QUIZCRAFT_CONNECTION_STRING="Server=localhost;Database=QuizCraftDb;User Id=usuario;Password=contraseña;TrustServerCertificate=true"
export GEMINI_API_KEY="tu-api-key-aqui"
```

## 🔒 Seguridad

### ⚠️ Archivos que NUNCA debes subir al repositorio:
- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`
- Cualquier archivo con `.secrets.json`
- Archivos `.env`

### ✅ Archivos seguros para subir:
- `appsettings.Example.json`
- `appsettings.Development.Example.json`
- Este archivo de documentación

## 🆘 Solución de Problemas

### Error de Conexión a Base de Datos
1. Verifica que SQL Server esté ejecutándose
2. Confirma la cadena de conexión
3. Verifica permisos de base de datos

### Error de Compilación
1. Restaura paquetes: `dotnet restore`
2. Limpia y recompila: `dotnet clean && dotnet build`

### Usuario Admin No Existe
1. Verifica que las migraciones se aplicaron: `dotnet ef database update`
2. El usuario se crea automáticamente al aplicar migraciones

## 📞 Soporte

Si tienes problemas con la configuración, verifica:

1. **Configuración:** Archivos copiados correctamente
2. **Base de Datos:** Servicio SQL Server ejecutándose
3. **Migraciones:** Aplicadas con `dotnet ef database update`
4. **Compilación:** Sin errores con `dotnet build`

---

💡 **Tip:** Mantén siempre actualizada tu configuración local sin afectar el repositorio compartido.