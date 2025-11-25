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
            // Primero verificar que el usuario existe
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Actualizar último acceso
                user.UltimoAcceso = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                
                // Verificar y agregar claim NombreCompleto si no existe
                var claims = await _userManager.GetClaimsAsync(user);
                var nombreCompletoClaim = claims.FirstOrDefault(c => c.Type == "NombreCompleto");
                
                if (nombreCompletoClaim == null)
                {
                    // Si no existe el claim, agregarlo
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("NombreCompleto", user.NombreCompleto));
                    _logger.LogInformation("Claim NombreCompleto agregado para usuario existente {Email}", model.Email);
                    
                    // Refrescar el sign in para que el claim esté disponible
                    await _signInManager.RefreshSignInAsync(user);
                }
                
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
            ModelState.AddModelError(string.Empty, "Ocurrió un error durante el inicio de sesión. Por favor, inténtelo de nuevo.");
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
            _logger.LogInformation("Iniciando registro para email: {Email}", model.Email);
            
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre ?? string.Empty,
                Apellido = model.Apellido ?? string.Empty,
                NombreCompleto = $"{model.Nombre} {model.Apellido}",
                FechaRegistro = DateTime.UtcNow,
                EstaActivo = true,
                NotificacionesEmail = true,
                NotificacionesWeb = true,
                NotificacionesHabilitadas = true,
                PreferenciaIdioma = "es",
                TemaPreferido = "light"
            };

            _logger.LogInformation("Creando usuario en la base de datos...");
            var result = await _userManager.CreateAsync(user, model.Password);
            _logger.LogInformation("Resultado de CreateAsync: {Succeeded}", result.Succeeded);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario {Email} creado exitosamente, asignando rol...", model.Email);
                
                // Asignar rol de Estudiante por defecto
                var roleResult = await _userManager.AddToRoleAsync(user, "Estudiante");
                
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Error al asignar rol Estudiante al usuario {Email}: {Errors}", 
                        model.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
                else
                {
                    _logger.LogInformation("Rol 'Estudiante' asignado correctamente a {Email}", model.Email);
                }

                // Agregar claim NombreCompleto
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("NombreCompleto", user.NombreCompleto));
                _logger.LogInformation("Claim NombreCompleto agregado para {Email}", model.Email);

                _logger.LogInformation("Usuario {Email} registrado exitosamente, iniciando sesión...", model.Email);

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
            ModelState.AddModelError(string.Empty, "Ocurrió un error durante el registro. Por favor, inténtelo de nuevo.");
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
                Email = user.Email ?? string.Empty,
                NotificacionesHabilitadas = user.NotificacionesHabilitadas,
                NotificacionesEmail = user.NotificacionesEmail,
                NotificacionesWeb = user.NotificacionesWeb,
                PreferenciaIdioma = user.PreferenciaIdioma ?? "es",
                TemaPreferido = user.TemaPreferido ?? "light",
                FechaRegistro = user.FechaRegistro,
                UltimoAcceso = user.UltimoAcceso
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
            user.NotificacionesHabilitadas = model.NotificacionesHabilitadas;
            user.NotificacionesEmail = model.NotificacionesEmail;
            user.NotificacionesWeb = model.NotificacionesWeb;
            user.PreferenciaIdioma = model.PreferenciaIdioma;
            user.TemaPreferido = "light";

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Actualizar el claim del nombre completo
                var existingClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "NombreCompleto");
                if (existingClaim != null)
                {
                    await _userManager.ReplaceClaimAsync(user, existingClaim, new System.Security.Claims.Claim("NombreCompleto", user.NombreCompleto));
                }
                else
                {
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("NombreCompleto", user.NombreCompleto));
                }
                
                // Refrescar el signin para incluir los claims actualizados
                await _signInManager.RefreshSignInAsync(user);
                
                // Recargar el usuario actualizado desde la base de datos para obtener UltimoAcceso actualizado
                user = await _userManager.GetUserAsync(User);
                
                // Actualizar el modelo con los datos más recientes
                model.FechaRegistro = user!.FechaRegistro;
                model.UltimoAcceso = user.UltimoAcceso;
                model.Email = user.Email ?? string.Empty;
                
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