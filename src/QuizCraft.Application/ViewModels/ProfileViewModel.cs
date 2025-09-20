using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Application.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Idioma preferido")]
        public string PreferenciaIdioma { get; set; } = "es";

        [Display(Name = "Tema preferido")]
        public string TemaPreferido { get; set; } = "light";

        [Display(Name = "Notificaciones habilitadas")]
        public bool NotificacionesHabilitadas { get; set; } = true;

        [Display(Name = "Notificaciones por email")]
        public bool NotificacionesEmail { get; set; } = true;

        [Display(Name = "Notificaciones web")]
        public bool NotificacionesWeb { get; set; } = true;

        public DateTime FechaRegistro { get; set; }

        public DateTime? UltimoAcceso { get; set; }
    }
}