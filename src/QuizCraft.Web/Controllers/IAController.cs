using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuizCraft.Core.Entities;
using QuizCraft.Application.Interfaces;
using AIService = QuizCraft.Application.Interfaces.IAIService;

namespace QuizCraft.Web.Controllers
{
    [Authorize]
    public class IAController : Controller
    {
        private readonly AIService _aiService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IAController> _logger;

        public IAController(
            AIService aiService,
            UserManager<ApplicationUser> userManager,
            ILogger<IAController> logger)
        {
            _aiService = aiService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: IA/GenerateResumen
        [HttpGet]
        public IActionResult GenerateResumen()
        {
            return View();
        }

        // POST: IA/GenerateResumen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateResumen(string Contenido)
        {
            if (string.IsNullOrWhiteSpace(Contenido))
            {
                ViewBag.Error = "Debe proporcionar contenido para generar el resumen.";
                return View();
            }

            try
            {
                var response = await _aiService.GenerateTextAsync(
                    $"Resume el siguiente texto de manera concisa y clara, manteniendo los puntos clave:\n\n{Contenido}");

                if (response.Success && !string.IsNullOrWhiteSpace(response.Content))
                {
                    ViewBag.Resumen = response.Content;
                    ViewBag.Success = "Resumen generado exitosamente.";
                }
                else
                {
                    ViewBag.Error = "No se pudo generar el resumen. " + response.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary with AI");
                ViewBag.Error = "Ocurrió un error al generar el resumen.";
            }

            ViewBag.ContenidoOriginal = Contenido;
            return View();
        }

        // GET: IA/GenerateExplicacion
        [HttpGet]
        public IActionResult GenerateExplicacion()
        {
            return View();
        }

        // POST: IA/GenerateExplicacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateExplicacion(string Concepto, int NivelDetalle = 1)
        {
            if (string.IsNullOrWhiteSpace(Concepto))
            {
                ViewBag.Error = "Debe proporcionar un concepto para explicar.";
                return View();
            }

            try
            {
                string promptDetalle = NivelDetalle switch
                {
                    0 => "de manera muy simple y breve, como para un niño",
                    1 => "de manera clara y comprensible, nivel intermedio",
                    2 => "de manera detallada y técnica, nivel avanzado",
                    _ => "de manera clara"
                };

                var response = await _aiService.GenerateTextAsync(
                    $"Explica el concepto '{Concepto}' {promptDetalle}. Incluye ejemplos si es necesario.");

                if (response.Success && !string.IsNullOrWhiteSpace(response.Content))
                {
                    ViewBag.Explicacion = response.Content;
                    ViewBag.Success = "Explicación generada exitosamente.";
                }
                else
                {
                    ViewBag.Error = "No se pudo generar la explicación. " + response.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating explanation with AI");
                ViewBag.Error = "Ocurrió un error al generar la explicación.";
            }

            ViewBag.ConceptoOriginal = Concepto;
            ViewBag.NivelDetalle = NivelDetalle;
            return View();
        }
    }
}
