using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Enums;

namespace QuizCraft.Application.ViewModels
{
    public class QuizIndexViewModel
    {
        public List<QuizItemViewModel> MisQuizzes { get; set; } = new();
        public List<QuizItemViewModel> QuizzesPublicos { get; set; } = new();
        public List<QuizListItemViewModel> QuizzesCreados { get; set; } = new();
        public int TotalQuizzes { get; set; }
        public int? MateriaSeleccionada { get; set; }
        public NivelDificultad? DificultadSeleccionada { get; set; }
        public string? FiltroMateria { get; set; }
        public List<MateriaSelectViewModel> MateriasDisponibles { get; set; } = new();
    }

    public class QuizListItemViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int NumeroPreguntas { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string CreadorNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool EsPublico { get; set; }
        public int TotalResultados { get; set; }
        public NivelDificultad Dificultad { get; set; }
        public int TiempoLimite { get; set; } // en minutos
        public bool YaRealizado { get; set; }
        public int? UltimoResultado { get; set; } // porcentaje
    }

    public class QuizItemViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int NumeroPreguntas { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string CreadorNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool EsPublico { get; set; }
        public int TotalResultados { get; set; }
        public NivelDificultad Dificultad { get; set; }
        public int TiempoLimite { get; set; }
        public bool YaRealizado { get; set; }
        public double? UltimoResultado { get; set; }
    }

    public class CreateQuizViewModel
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una materia")]
        public int MateriaId { get; set; }

        [Range(1, 50, ErrorMessage = "El número de preguntas debe estar entre 1 y 50")]
        public int NumeroPreguntas { get; set; } = 10;

        [Range(0, 180, ErrorMessage = "El tiempo límite debe estar entre 0 y 180 minutos")]
        public int TiempoLimite { get; set; } = 0;

        [Required(ErrorMessage = "Debe seleccionar un nivel de dificultad")]
        public NivelDificultad Dificultad { get; set; } = NivelDificultad.Intermedio;

        public bool EsPublico { get; set; } = false;
        public bool MostrarRespuestasInmediato { get; set; } = true;
        public bool MezclarPreguntas { get; set; } = true;
        public bool MezclarOpciones { get; set; } = true;
        public bool PermitirReintento { get; set; } = true;
        public int? TiempoPorPreguntaSegundos { get; set; } = 30;

        // Para los dropdowns
        public List<MateriaSelectViewModel> MateriasDisponibles { get; set; } = new();
        public int FlashcardsDisponibles { get; set; }
    }

    public class TakeQuizViewModel
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int NumeroPreguntas { get; set; }
        public int TiempoPorPregunta { get; set; }
        public int TiempoLimite { get; set; } // en minutos
        public bool MostrarRespuestasInmediato { get; set; }
        public List<PreguntaQuizViewModel> Preguntas { get; set; } = new();
        public int PreguntaActual { get; set; } = 0;
        public int TotalPreguntas { get; set; }
        public DateTime FechaInicio { get; set; }
        public bool MezclarOpciones { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
    }

    public class PreguntaQuizViewModel
    {
        public int Id { get; set; }
        public string TextoPregunta { get; set; } = string.Empty;
        public string OpcionA { get; set; } = string.Empty;
        public string OpcionB { get; set; } = string.Empty;
        public string OpcionC { get; set; } = string.Empty;
        public string OpcionD { get; set; } = string.Empty;
        public List<OpcionRespuestaViewModel> Opciones { get; set; } = new();
        public List<OpcionRespuestaViewModel> TodasLasOpciones { get; set; } = new();
        public int Orden { get; set; }
        public int Puntos { get; set; }
        public string? RespuestaSeleccionada { get; set; }
        public string? RespuestaUsuario { get; set; }
        public string? RespuestaCorrecta { get; set; }
        public bool EsCorrecta { get; set; }
        public string? Explicacion { get; set; }
        public int TiempoRespuesta { get; set; } // en segundos
    }

    public class OpcionRespuestaViewModel
    {
        public string Texto { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public bool EsCorrecta { get; set; }
    }

    public class SubmitAnswerViewModel
    {
        public int QuizId { get; set; }
        public int PreguntaId { get; set; }
        public string RespuestaSeleccionada { get; set; } = string.Empty;
        public int TiempoRespuesta { get; set; } // en segundos
        public bool EsUltimaPregnta { get; set; }
    }

    public class QuizResultsViewModel
    {
        public int QuizId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public int TotalPreguntas { get; set; }
        public int RespuestasCorrectas { get; set; }
        public int PuntuacionTotal { get; set; }
        public double PorcentajeAcierto { get; set; }
        public TimeSpan TiempoTotal { get; set; }
        public DateTime FechaRealizacion { get; set; }
        public List<RespuestaDetalleViewModel> DetalleRespuestas { get; set; } = new();
        public string MensajeResultado { get; set; } = string.Empty;
        public string ColorResultado { get; set; } = string.Empty;
        public bool MostrarRespuestasDetalle { get; set; } = true;
    }

    public class RespuestaDetalleViewModel
    {
        public int Orden { get; set; }
        public string Pregunta { get; set; } = string.Empty;
        public string RespuestaUsuario { get; set; } = string.Empty;
        public string RespuestaCorrecta { get; set; } = string.Empty;
        public bool EsCorrecta { get; set; }
        public string? Explicacion { get; set; }
        public int TiempoRespuesta { get; set; } // en segundos
        public List<OpcionRespuestaViewModel> TodasLasOpciones { get; set; } = new();
    }

    public class QuizDetailsViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string CreadorNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        
        // Propiedades para quizzes importados
        public bool EsImportado { get; set; }
        public string? CreadorOriginalNombre { get; set; }
        public DateTime? FechaImportacion { get; set; }
        
        // Propiedades del quiz
        public int CantidadPreguntas { get; set; }
        public int NumeroPreguntas { get; set; }
        public int TiempoLimite { get; set; }
        public int? TiempoPorPregunta { get; set; }
        public NivelDificultad NivelDificultad { get; set; }
        public bool EsPublico { get; set; }
        public bool MostrarRespuestasInmediato { get; set; }
        
        // Estadísticas
        public int CantidadIntentos { get; set; }
        public int TotalIntentos { get; set; }
        public int TotalRealizaciones { get; set; }
        public double PromedioAciertos { get; set; }
        public double? PromedioCalificacion { get; set; }
        
        // Permisos y estados
        public bool PuedeRealizar { get; set; }
        public bool PuedeRealizarQuiz { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool YaRealizado { get; set; }
        public string MensajeNoDisponible { get; set; } = string.Empty;
        
        // Resultados
        public int? UltimoResultado { get; set; }
        public List<ResultadoQuizResumenViewModel> MisResultados { get; set; } = new();
        
        // Preguntas (opcional para mostrar)
        public bool MostrarPreguntas { get; set; } = false;
        public List<PreguntaQuizViewModel> Preguntas { get; set; } = new();
    }

    public class MateriaSelectViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadFlashcards { get; set; }
        public int TotalFlashcards { get; set; }
        public int TotalQuizzes { get; set; }
    }

    public class ResultadoQuizResumenViewModel
    {
        public int Id { get; set; }
        public double PorcentajeAcierto { get; set; }
        public DateTime FechaRealizacion { get; set; }
        public TimeSpan TiempoTotal { get; set; }
        public int PuntajeObtenido { get; set; }
        public int PuntajeMaximo { get; set; }
    }

    public class QuizEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una materia")]
        public int MateriaId { get; set; }

        [Range(0, 300, ErrorMessage = "El tiempo límite debe estar entre 0 y 300 minutos")]
        public int TiempoLimite { get; set; } = 30;

        public NivelDificultad NivelDificultad { get; set; } = NivelDificultad.Intermedio;

        public bool EsPublico { get; set; }

        public bool MostrarRespuestasInmediato { get; set; } = true;

        public bool PermitirReintento { get; set; } = true;

        // Nota: Esta propiedad se manejará en el controlador para evitar dependencias
        public object? Materias { get; set; } = new();
    }
}