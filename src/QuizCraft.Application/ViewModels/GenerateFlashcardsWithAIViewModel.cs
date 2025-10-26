using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuizCraft.Core.Enums;

namespace QuizCraft.Application.ViewModels;

/// <summary>
/// ViewModel para generar flashcards con IA
/// </summary>
public class GenerateFlashcardsWithAIViewModel
{
    [Display(Name = "Materia")]
    [Required(ErrorMessage = "Debe seleccionar una materia")]
    public int? MateriaId { get; set; }

    [Display(Name = "Contenido de Texto")]
    public string? Contenido { get; set; }

    [Display(Name = "Archivo PDF (opcional)")]
    public IFormFile? ArchivoPDF { get; set; }

    [Display(Name = "Cantidad de Flashcards")]
    [Range(1, 50, ErrorMessage = "Debe generar entre 1 y 50 flashcards")]
    public int? CantidadFlashcards { get; set; } = 5;

    [Display(Name = "Nivel de Dificultad")]
    public NivelDificultad NivelDificultad { get; set; } = NivelDificultad.Intermedio;

    // Para el formulario
    public List<QuizCraft.Core.Entities.Materia> Materias { get; set; } = new();
}
