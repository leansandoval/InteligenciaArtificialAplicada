using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Infrastructure.Services;

namespace QuizCraft.Web.Controllers
{
    /// <summary>
    /// Controlador para monitorear el uso de la API de Gemini
    /// </summary>
    [Authorize]
    public class GeminiMonitorController : Controller
    {
        private readonly GeminiService _geminiService;
        private readonly ILogger<GeminiMonitorController> _logger;

        public GeminiMonitorController(
            GeminiService geminiService,
            ILogger<GeminiMonitorController> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        /// <summary>
        /// Página de monitoreo del uso de la API
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var stats = _geminiService.GetRateLimitStats();
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de rate limiting");
                TempData["Error"] = "Error al cargar las estadísticas";
                return View();
            }
        }

        /// <summary>
        /// API endpoint para obtener estadísticas en formato JSON
        /// </summary>
        [HttpGet]
        public IActionResult GetStats()
        {
            try
            {
                var stats = _geminiService.GetRateLimitStats();
                return Json(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de rate limiting");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}
