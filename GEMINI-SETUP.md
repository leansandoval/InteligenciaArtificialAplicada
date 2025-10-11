# 🔑 Guía para Obtener API Key de Google Gemini (GRATUITA)

## 📋 Resumen
Google Gemini ofrece una API **completamente gratuita** para uso en desarrollo y proyectos académicos. Solo necesitas una cuenta de Google.

## 🚀 Pasos para Obtener tu API Key

### 1. Acceder a Google AI Studio
1. Ve a [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Inicia sesión con tu cuenta de Google

### 2. Crear API Key
1. Haz clic en **"Create API Key"**
2. Selecciona **"Create API key in new project"** (recomendado)
3. Tu API Key se generará automáticamente

### 3. Copiar y Guardar
1. **Copia** la API Key generada
2. **Guárdala** en un lugar seguro
3. **No la compartas** públicamente

## ⚙️ Configuración en QuizCraft

### 1. Crear archivo de configuración
```bash
# En el directorio del proyecto web
cd src/QuizCraft.Web
cp appsettings.Example.json appsettings.Development.json
```

### 2. Agregar tu API Key
Edita `appsettings.Development.json`:

```json
{
  "Gemini": {
    "ApiKey": "TU_API_KEY_AQUI",
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
}
```

### 3. Verificar configuración
El sistema mostrará en la consola:
- ✅ `Usando Google Gemini - Modelo: gemini-2.0-flash-exp` (configurado correctamente)
- ⚠️ `Usando servicio mock de IA - Gemini no configurado` (necesita configuración)

## 🎯 Modelos Disponibles

| Modelo | Descripción | Recomendado para |
|--------|-------------|------------------|
| `gemini-2.0-flash-exp` | **Más reciente y avanzado** | Generación de flashcards (predeterminado) |
| `gemini-pro` | Estable y confiable | Tareas de producción |
| `gemini-1.5-flash` | Rápido y eficiente | Respuestas rápidas |

## 📊 Límites Gratuitos

### Gemini API (Tier Gratuito)
- **Requests por minuto:** 15
- **Requests por día:** 1,500
- **Tokens por minuto:** 32,000
- **Tokens por día:** 50,000

### ¿Es suficiente para QuizCraft?
✅ **SÍ** - Los límites son muy generosos para:
- Desarrollo y pruebas
- Uso académico
- Proyectos personales
- Generación de flashcards

## 🔒 Seguridad

### ✅ Buenas Prácticas
- Nunca commitees la API Key en Git
- Usa archivos `.gitignore` para excluir configuraciones
- Mantén `appsettings.Development.json` solo localmente

### ❌ Evitar
- Compartir la API Key públicamente
- Incluir la API Key en código fuente
- Subir configuraciones con API Keys a repositorios

## 🆘 Solución de Problemas

### Error: "API Key not found"
1. Verifica que la API Key esté en `appsettings.Development.json`
2. Asegúrate de que el formato JSON sea correcto
3. Reinicia la aplicación

### Error: "403 Forbidden"
1. Verifica que la API Key sea válida
2. Revisa que no hayas excedido los límites
3. Asegúrate de que la API esté habilitada en Google Cloud

### Error: "Quota exceeded"
1. Has alcanzado el límite diario
2. Espera hasta el siguiente día
3. Considera optimizar las requests

## 💡 Consejos de Uso

### Optimización
- Ajusta `MaxTokens` según tus necesidades
- Usa `Temperature` baja (0.3-0.7) para respuestas consistentes
- Configura límites razonables en `MaxRequestsPerDay`

### Desarrollo
- Usa el modo mock durante desarrollo inicial
- Habilita Gemini solo cuando necesites IA real
- Monitorea el uso en Google AI Studio

---

**🎉 ¡Listo!** Con estos pasos tendrás Google Gemini funcionando **gratuitamente** en QuizCraft.