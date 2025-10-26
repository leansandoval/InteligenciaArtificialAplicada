using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;

namespace QuizCraft.Application.ViewModels;

public class GenerateQuizWithAIViewModel
{
    [Required(ErrorMessage = "Debe seleccionar una materia")]
    [Display(Name = "Materia")]
    public int? MateriaId { get; set; }

    [Display(Name = "Contenido de texto")]
    public string? Contenido { get; set; }

    [Display(Name = "Archivo PDF")]
    public IFormFile? ArchivoPDF { get; set; }

    [Range(1, 50, ErrorMessage = "La cantidad de preguntas debe estar entre 1 y 50")]
    [Display(Name = "Cantidad de preguntas")]
    public int? CantidadPreguntas { get; set; } = 5;

    [Display(Name = "Nivel de dificultad")]
    public NivelDificultad NivelDificultad { get; set; } = NivelDificultad.Intermedio;

    [Display(Name = "Título del Quiz")]
    public string? Titulo { get; set; }

    // Para el dropdown de materias
    public List<Materia> Materias { get; set; } = new();
}
