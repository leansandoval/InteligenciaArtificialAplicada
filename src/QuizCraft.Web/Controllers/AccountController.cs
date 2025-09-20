using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCraft.Application.ViewModels;
using QuizCraft.Core.Entities;

namespace QuizCraft.Web.Controllers;

/// <summary>
/// Controlador para gestionar autenticación y cuentas de usuario
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// FUNC_MostrarFormularioLogin - Muestra el formulario de inicio de sesión
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// FUNC_ProcesarLogin - Procesa el intento de inicio de sesión
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario {Email} inició sesión exitosamente", model.Email);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Cuenta de usuario {Email} bloqueada", model.Email);
                ModelState.AddModelError(string.Empty, "Su cuenta está temporalmente bloqueada.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el inicio de sesión para {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Ocurrió un error durante el inicio de sesión.");
            return View(model);
        }
    }

    /// <summary>
    /// FUNC_MostrarFormularioRegistro - Muestra el formulario de registro
    /// </summary>
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    /// <summary>
    /// FUNC_ProcesarRegistro - Procesa el registro de nuevo usuario
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                NombreCompleto = $"{model.Nombre} {model.Apellido}",
                EstaActivo = true,
                NotificacionesEmail = true,
                NotificacionesWeb = true,
                PreferenciaIdioma = "es"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Asignar rol de Estudiante por defecto
                await _userManager.AddToRoleAsync(user, "Estudiante");

                _logger.LogInformation("Usuario {Email} registrado exitosamente", model.Email);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro de {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Ocurrió un error durante el registro.");
        }

        return View(model);
    }

    /// <summary>
    /// FUNC_CerrarSesion - Cierra la sesión del usuario actual
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el cierre de sesión");
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// FUNC_PerfilUsuario - Muestra el perfil del usuario actual
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var model = new ProfileViewModel
            {
                Nombre = user.Nombre ?? string.Empty,
                Apellido = user.Apellido ?? string.Empty,
                Email = user.Email,
                NotificacionesEmail = user.NotificacionesEmail,
                NotificacionesWeb = user.NotificacionesWeb,
                PreferenciaIdioma = user.PreferenciaIdioma,
                TemaPreferido = user.TemaPreferido
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el perfil del usuario");
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// FUNC_ActualizarPerfil - Actualiza el perfil del usuario actual
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            user.Nombre = model.Nombre;
            user.Apellido = model.Apellido;
            user.NombreCompleto = $"{model.Nombre} {model.Apellido}";
            user.NotificacionesEmail = model.NotificacionesEmail;
            user.NotificacionesWeb = model.NotificacionesWeb;
            user.PreferenciaIdioma = model.PreferenciaIdioma;
            user.TemaPreferido = model.TemaPreferido;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Perfil actualizado correctamente";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el perfil del usuario");
            ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el perfil.");
        }

        return View(model);
    }

    /// <summary>
    /// FUNC_RedirigirLocal - Redirige a URL local o por defecto
    /// </summary>
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction("Index", "Home");
    }
}