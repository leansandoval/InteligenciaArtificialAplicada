using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Web.ViewModels.Account;

/// <summary>
/// ViewModel para el perfil de usuario
/// </summary>
public class ProfileViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Notificaciones por email")]
    public bool NotificacionesEmail { get; set; } = true;

    [Display(Name = "Notificaciones web")]
    public bool NotificacionesWeb { get; set; } = true;

    [Display(Name = "Idioma preferido")]
    public string PreferenciaIdioma { get; set; } = "es";

    [Display(Name = "Tema preferido")]
    public string? TemaPreferido { get; set; } = "light";

    [Display(Name = "Notificaciones habilitadas")]
    public bool NotificacionesHabilitadas { get; set; } = true;

    [Display(Name = "Fecha de registro")]
    public DateTime FechaRegistro { get; set; }

    [Display(Name = "Último acceso")]
    public DateTime? UltimoAcceso { get; set; }
}