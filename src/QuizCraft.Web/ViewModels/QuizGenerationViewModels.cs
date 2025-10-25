using System.ComponentModel.DataAnnotations;
using QuizCraft.Application.Models;
using QuizCraft.Core.Enums;
using static QuizCraft.Application.Models.QuestionType;

namespace QuizCraft.Web.ViewModels
{
    /// <summary>
    /// ViewModel para la configuración de generación de quiz con IA
    /// </summary>
    public class QuizGenerationConfigViewModel
    {
        /// <summary>
        /// ID de la materia (opcional, para contexto)
        /// </summary>
        public int? MateriaId { get; set; }

        /// <summary>
        /// Nombre de la materia
        /// </summary>
        public string? MateriaNombre { get; set; }

        /// <summary>
        /// Número de preguntas a generar
        /// </summary>
        [Required(ErrorMessage = "El número de preguntas es requerido")]
        [Range(1, 50, ErrorMessage = "El número de preguntas debe estar entre 1 y 50")]
        [Display(Name = "Número de preguntas")]
        public int NumberOfQuestions { get; set; } = 5;

        /// <summary>
        /// Nivel de dificultad
        /// </summary>
        [Required(ErrorMessage = "El nivel de dificultad es requerido")]
        [Display(Name = "Nivel de dificultad")]
        public NivelDificultad DifficultyLevel { get; set; } = NivelDificultad.Intermedio;

        /// <summary>
        /// Tipos de preguntas seleccionadas
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar al menos un tipo de pregunta")]
        [Display(Name = "Tipos de preguntas")]
        public List<QuestionType> SelectedQuestionTypes { get; set; } = new() { QuestionType.MultipleChoice };

        /// <summary>
        /// Tema específico (opcional)
        /// </summary>
        [Display(Name = "Tema específico")]
        [MaxLength(200, ErrorMessage = "El tema no puede exceder 200 caracteres")]
        public string? Subject { get; set; }

        /// <summary>
        /// Instrucciones personalizadas
        /// </summary>
        [Display(Name = "Instrucciones adicionales")]
        [MaxLength(1000, ErrorMessage = "Las instrucciones no pueden exceder 1000 caracteres")]
        public string? CustomInstructions { get; set; }

        /// <summary>
        /// Incluir explicaciones
        /// </summary>
        [Display(Name = "Incluir explicaciones en las respuestas")]
        public bool IncludeExplanations { get; set; } = true;

        /// <summary>
        /// Variar complejidad
        /// </summary>
        [Display(Name = "Variar complejidad de preguntas")]
        public bool VariedComplexity { get; set; } = true;

        /// <summary>
        /// Creatividad de la IA (temperatura)
        /// </summary>
        [Range(0.0, 1.0, ErrorMessage = "La creatividad debe estar entre 0.0 y 1.0")]
        [Display(Name = "Nivel de creatividad")]
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// Contenido de texto directo
        /// </summary>
        [Display(Name = "Contenido de texto")]
        public string? TextContent { get; set; }

        /// <summary>
        /// Archivo subido
        /// </summary>
        [Display(Name = "Documento")]
        public IFormFile? DocumentFile { get; set; }

        /// <summary>
        /// Información de estimación de tokens
        /// </summary>
        public TokenUsageInfo? EstimatedTokenUsage { get; set; }

        /// <summary>
        /// Tipos de archivo soportados para mostrar en la interfaz
        /// </summary>
        public List<string> SupportedFileTypes { get; set; } = new()
        {
            ".txt", ".md", ".pdf", ".docx", ".doc", ".rtf", ".odt", ".html", ".htm"
        };

        /// <summary>
        /// Convierte este ViewModel a QuizGenerationSettings
        /// </summary>
        public QuizGenerationSettings ToQuizGenerationSettings()
        {
            return new QuizGenerationSettings
            {
                NumberOfQuestions = NumberOfQuestions,
                DifficultyLevel = DifficultyLevel,
                QuestionTypes = SelectedQuestionTypes,
                Subject = Subject,
                CustomInstructions = CustomInstructions,
                IncludeExplanations = IncludeExplanations,
                Temperature = Temperature,
                Language = "es",
                VariedComplexity = VariedComplexity
            };
        }
    }

    /// <summary>
    /// ViewModel para revisar el quiz generado
    /// </summary>
    public class ReviewGeneratedQuizViewModel
    {
        /// <summary>
        /// Resultado de la generación
        /// </summary>
        public QuizGenerationResult GenerationResult { get; set; } = new();

        /// <summary>
        /// Preguntas con estado de aprobación
        /// </summary>
        public List<ReviewQuestionViewModel> Questions { get; set; } = new();

        /// <summary>
        /// Configuración utilizada en la generación
        /// </summary>
        public QuizGenerationSettings OriginalSettings { get; set; } = new();

        /// <summary>
        /// Título del quiz
        /// </summary>
        [Required(ErrorMessage = "El título del quiz es requerido")]
        [Display(Name = "Título del quiz")]
        [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string QuizTitle { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del quiz
        /// </summary>
        [Display(Name = "Descripción")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? QuizDescription { get; set; }

        /// <summary>
        /// ID de la materia a la que pertenecerá el quiz
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar una materia")]
        [Display(Name = "Materia")]
        public int MateriaId { get; set; }

        /// <summary>
        /// Tiempo límite en minutos (opcional)
        /// </summary>
        [Range(1, 300, ErrorMessage = "El tiempo límite debe estar entre 1 y 300 minutos")]
        [Display(Name = "Tiempo límite (minutos)")]
        public int? TimeLimit { get; set; }

        /// <summary>
        /// Número de intentos permitidos
        /// </summary>
        [Range(1, 10, ErrorMessage = "Los intentos permitidos deben estar entre 1 y 10")]
        [Display(Name = "Intentos permitidos")]
        public int MaxAttempts { get; set; } = 1;

        /// <summary>
        /// Mostrar resultados inmediatamente
        /// </summary>
        [Display(Name = "Mostrar resultados inmediatamente")]
        public bool ShowResultsImmediately { get; set; } = true;

        /// <summary>
        /// Mezclar preguntas
        /// </summary>
        [Display(Name = "Mezclar orden de preguntas")]
        public bool ShuffleQuestions { get; set; } = false;

        /// <summary>
        /// Obtiene las preguntas aprobadas
        /// </summary>
        public List<ReviewQuestionViewModel> ApprovedQuestions => 
            Questions.Where(q => q.IsApproved).ToList();

        /// <summary>
        /// Obtiene el total de preguntas aprobadas
        /// </summary>
        public int ApprovedQuestionsCount => ApprovedQuestions.Count;

        /// <summary>
        /// Obtiene el total de puntos del quiz
        /// </summary>
        public int TotalPoints => ApprovedQuestions.Sum(q => q.Points);
    }

    /// <summary>
    /// ViewModel para revisar una pregunta individual
    /// </summary>
    public class ReviewQuestionViewModel
    {
        /// <summary>
        /// ID temporal de la pregunta
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Texto de la pregunta
        /// </summary>
        [Required(ErrorMessage = "El texto de la pregunta es requerido")]
        [Display(Name = "Pregunta")]
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de pregunta
        /// </summary>
        [Display(Name = "Tipo")]
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Nivel de dificultad
        /// </summary>
        [Display(Name = "Dificultad")]
        public NivelDificultad DifficultyLevel { get; set; }

        /// <summary>
        /// Opciones de respuesta
        /// </summary>
        public List<ReviewAnswerOptionViewModel> AnswerOptions { get; set; } = new();

        /// <summary>
        /// Respuesta correcta (para preguntas que no son de opción múltiple)
        /// </summary>
        [Display(Name = "Respuesta correcta")]
        public string? CorrectAnswer { get; set; }

        /// <summary>
        /// Explicación
        /// </summary>
        [Display(Name = "Explicación")]
        public string? Explanation { get; set; }

        /// <summary>
        /// Puntos asignados
        /// </summary>
        [Range(1, 10, ErrorMessage = "Los puntos deben estar entre 1 y 10")]
        [Display(Name = "Puntos")]
        public int Points { get; set; } = 1;

        /// <summary>
        /// Etiquetas
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Puntuación de confianza de la IA
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// Está aprobada para incluir en el quiz
        /// </summary>
        [Display(Name = "Incluir en el quiz")]
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// Comentarios del revisor
        /// </summary>
        [Display(Name = "Comentarios")]
        public string? ReviewerNotes { get; set; }

        /// <summary>
        /// Indica si la pregunta fue editada manualmente
        /// </summary>
        public bool WasEdited { get; set; } = false;

        /// <summary>
        /// Convierte a GeneratedQuizQuestion
        /// </summary>
        public GeneratedQuizQuestion ToGeneratedQuizQuestion()
        {
            return new GeneratedQuizQuestion
            {
                QuestionText = QuestionText,
                QuestionType = QuestionType,
                DifficultyLevel = DifficultyLevel,
                AnswerOptions = AnswerOptions.Select(a => a.ToQuizAnswerOption()).ToList(),
                CorrectAnswer = CorrectAnswer ?? string.Empty,
                Explanation = Explanation ?? string.Empty,
                Points = Points,
                Tags = Tags,
                ConfidenceScore = ConfidenceScore,
                IsApproved = IsApproved,
                Notes = ReviewerNotes ?? string.Empty
            };
        }
    }

    /// <summary>
    /// ViewModel para revisar una opción de respuesta
    /// </summary>
    public class ReviewAnswerOptionViewModel
    {
        /// <summary>
        /// Texto de la opción
        /// </summary>
        [Required(ErrorMessage = "El texto de la opción es requerido")]
        [Display(Name = "Opción")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Es la respuesta correcta
        /// </summary>
        [Display(Name = "Correcta")]
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Explicación de la opción
        /// </summary>
        [Display(Name = "Explicación")]
        public string? Explanation { get; set; }

        /// <summary>
        /// Orden de presentación
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Convierte a QuizAnswerOption
        /// </summary>
        public QuizAnswerOption ToQuizAnswerOption()
        {
            return new QuizAnswerOption
            {
                Text = Text,
                IsCorrect = IsCorrect,
                Explanation = Explanation ?? string.Empty,
                Order = Order
            };
        }
    }

    /// <summary>
    /// ViewModel para mostrar el estado de la generación
    /// </summary>
    public class QuizGenerationStatusViewModel
    {
        /// <summary>
        /// Estado de la generación
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje de estado
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Número de preguntas generadas
        /// </summary>
        public int QuestionsGenerated { get; set; }

        /// <summary>
        /// Tokens utilizados
        /// </summary>
        public int TokensUsed { get; set; }

        /// <summary>
        /// Costo estimado
        /// </summary>
        public decimal EstimatedCost { get; set; }

        /// <summary>
        /// Tiempo de procesamiento
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }

        /// <summary>
        /// Método de procesamiento utilizado
        /// </summary>
        public string ProcessingMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel para revisar y nombrar el quiz generado antes de guardarlo
    /// </summary>
    public class ReviewAndNameQuizViewModel
    {
        /// <summary>
        /// Título del quiz
        /// </summary>
        [Required(ErrorMessage = "El título del quiz es requerido")]
        [Display(Name = "Título del quiz")]
        [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [MinLength(3, ErrorMessage = "El título debe tener al menos 3 caracteres")]
        public string QuizTitle { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del quiz
        /// </summary>
        [Display(Name = "Descripción")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? QuizDescription { get; set; }

        /// <summary>
        /// ID de la materia
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar una materia")]
        public int MateriaId { get; set; }

        /// <summary>
        /// Si el quiz debe ser público
        /// </summary>
        [Display(Name = "Quiz público")]
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Preguntas generadas (serializado para hidden field)
        /// </summary>
        public string GeneratedQuestions { get; set; } = string.Empty;

        /// <summary>
        /// Preguntas para vista previa
        /// </summary>
        public List<GeneratedQuizQuestion> PreviewQuestions { get; set; } = new();

        /// <summary>
        /// Número total de preguntas
        /// </summary>
        public int QuestionCount { get; set; }

        /// <summary>
        /// Configuración utilizada
        /// </summary>
        public QuizGenerationSettings? OriginalSettings { get; set; }
    }
}