using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Application.ViewModels
{
    public class MateriaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Color")]
        public string Color { get; set; } = "#007bff";

        [Display(Name = "Icono")]
        public string Icono { get; set; } = "fas fa-book";

        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Fecha de Modificación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaModificacion { get; set; }

        [Display(Name = "Activo")]
        public bool EstaActivo { get; set; }

        [Display(Name = "Total de Flashcards")]
        public int TotalFlashcards { get; set; }

        [Display(Name = "Total de Quizzes")]
        public int TotalQuizzes { get; set; }
    }

    public class CreateMateriaViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El color es requerido")]
        [Display(Name = "Color")]
        public string Color { get; set; } = "#007bff";

        [Required(ErrorMessage = "El icono es requerido")]
        [Display(Name = "Icono")]
        public string Icono { get; set; } = "fas fa-book";
    }

    public class EditMateriaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El color es requerido")]
        [Display(Name = "Color")]
        public string Color { get; set; } = "#007bff";

        [Required(ErrorMessage = "El icono es requerido")]
        [Display(Name = "Icono")]
        public string Icono { get; set; } = "fas fa-book";

        [Display(Name = "Activo")]
        public bool EstaActivo { get; set; } = true;
    }
}