using System.ComponentModel.DataAnnotations;

namespace QuizCraft.Core.Entities;

/// <summary>
/// Entidad que representa una pregunta dentro de un quiz
/// </summary>
public class PreguntaQuiz : BaseEntity
{
    [Required]
    [StringLength(1000)]
    public string TextoPregunta { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string RespuestaCorrecta { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? OpcionA { get; set; }
    
    [StringLength(500)]
    public string? OpcionB { get; set; }
    
    [StringLength(500)]
    public string? OpcionC { get; set; }
    
    [StringLength(500)]
    public string? OpcionD { get; set; }
    
    [StringLength(1000)]
    public string? Explicacion { get; set; }
    
    public int Orden { get; set; }
    public int Puntos { get; set; } = 1;
    
    // Tipo de pregunta: 1=Múltiple opción, 2=Verdadero/Falso, 3=Texto libre
    public int TipoPregunta { get; set; } = 1;
    
    // Claves foráneas
    public int QuizId { get; set; }
    public int? FlashcardId { get; set; }
    
    // Propiedades de navegación
    public virtual Quiz Quiz { get; set; } = null!;
    public virtual Flashcard? Flashcard { get; set; }
    public virtual ICollection<RespuestaUsuario> RespuestasUsuario { get; set; } = new List<RespuestaUsuario>();
}