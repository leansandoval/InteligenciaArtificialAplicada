using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.Interfaces;
using QuizCraft.Web.ViewModels;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador para diagnosticar el estado de la configuración de IA
/// </summary>
[Authorize]
public class DiagnosticoIAController : Controller
{
    private readonly IAIService _aiService;
    private readonly IAIConfigurationService _aiConfigurationService;
    private readonly ILogger<DiagnosticoIAController> _logger;

    public DiagnosticoIAController(
        IAIService aiService,
        IAIConfigurationService aiConfigurationService,
        ILogger<DiagnosticoIAController> logger)
    {
        _aiService = aiService;
        _aiConfigurationService = aiConfigurationService;
        _logger = logger;
    }

    /// <summary>
    /// Página de diagnóstico de la configuración de IA
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new DiagnosticoIAViewModel
        {
            FechaVerificacion = DateTime.Now
        };

        try
        {
            // Verificar configuración
            var settings = await _aiConfigurationService.GetSettingsAsync();
            var isConfigured = await _aiConfigurationService.IsConfiguredAsync();
            
            model.ConfiguracionValida = isConfigured;
            model.ModeloConfigurudo = settings.Model;
            model.ApiKeyConfigurada = isConfigured;
            
            if (model.ApiKeyConfigurada)
            {
                var apiKey = await _aiConfigurationService.GetApiKeyAsync();
                if (!string.IsNullOrEmpty(apiKey) && apiKey.Length > 12)
                {
                    model.ApiKeyParcial = $"{apiKey.Substring(0, 8)}...{apiKey.Substring(apiKey.Length - 4)}";
                }
            }

            // Hacer una prueba simple
            if (model.ConfiguracionValida)
            {
                _logger.LogInformation("Iniciando prueba de conexión con Gemini...");
                
                var resultado = await _aiService.GenerateTextAsync(
                    "Di solo 'OK' si recibes este mensaje correctamente."
                );

                model.ConexionExitosa = resultado.Success;
                model.MensajeRespuesta = resultado.Content;
                
                if (resultado.Success)
                {
                    model.TokensUsados = resultado.TokenUsage?.TotalTokens ?? 0;
                    _logger.LogInformation("✅ Prueba exitosa. Tokens usados: {Tokens}", model.TokensUsados);
                }
                else
                {
                    model.MensajeError = resultado.Content;
                    _logger.LogWarning("❌ Prueba fallida: {Error}", resultado.Content);
                }
            }
            else
            {
                model.MensajeError = "API Key no configurada o inválida";
            }
        }
        catch (Exception ex)
        {
            model.ConexionExitosa = false;
            model.MensajeError = $"Error: {ex.Message}";
            _logger.LogError(ex, "Error en diagnóstico de IA");
        }

        return View(model);
    }

    /// <summary>
    /// Verificar estado de la API mediante AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> VerificarEstado()
    {
        try
        {
            var resultado = await _aiService.GenerateTextAsync(
                "Responde solo con: CONEXION_OK"
            );

            return Json(new
            {
                success = resultado.Success,
                mensaje = resultado.Content,
                tokens = resultado.TokenUsage?.TotalTokens ?? 0,
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando estado de API");
            return Json(new
            {
                success = false,
                mensaje = ex.Message,
                tokens = 0,
                timestamp = DateTime.Now
            });
        }
    }
}
