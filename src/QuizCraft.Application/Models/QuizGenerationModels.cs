using System.ComponentModel.DataAnnotations;
using QuizCraft.Core.Enums;

namespace QuizCraft.Application.Models
{
    /// <summary>
    /// Configuración para la generación automática de quizzes
    /// </summary>
    public class QuizGenerationSettings
    {
        /// <summary>
        /// Número de preguntas a generar
        /// </summary>
        [Range(1, 50, ErrorMessage = "El número de preguntas debe estar entre 1 y 50")]
        public int NumberOfQuestions { get; set; } = 10;

        /// <summary>
        /// Nivel de dificultad deseado
        /// </summary>
        public NivelDificultad DifficultyLevel { get; set; } = NivelDificultad.Intermedio;

        /// <summary>
        /// Tipos de preguntas a incluir
        /// </summary>
        public List<QuestionType> QuestionTypes { get; set; } = new() { QuestionType.MultipleChoice };

        /// <summary>
        /// Tema o materia específica
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Instrucciones personalizadas para la IA
        /// </summary>
        public string? CustomInstructions { get; set; }

        /// <summary>
        /// Incluir explicaciones en las respuestas
        /// </summary>
        public bool IncludeExplanations { get; set; } = true;

        /// <summary>
        /// Configuración de temperatura para la IA (creatividad)
        /// </summary>
        [Range(0.0, 1.0, ErrorMessage = "La temperatura debe estar entre 0.0 y 1.0")]
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// Idioma de generación
        /// </summary>
        public string Language { get; set; } = "es";

        /// <summary>
        /// Incluir preguntas de diferentes niveles de complejidad
        /// </summary>
        public bool VariedComplexity { get; set; } = true;
    }

    /// <summary>
    /// Tipos de preguntas soportadas
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice,      // Opción múltiple
        TrueFalse,          // Verdadero/Falso
        FillInTheBlank,     // Completar espacios
        ShortAnswer,        // Respuesta corta
        Matching            // Emparejar conceptos
    }

    /// <summary>
    /// Resultado de la generación de quiz
    /// </summary>
    public class QuizGenerationResult
    {
        /// <summary>
        /// Indica si la generación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Lista de preguntas generadas
        /// </summary>
        public List<GeneratedQuizQuestion> Questions { get; set; } = new();

        /// <summary>
        /// Mensaje de error si hubo algún problema
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Método utilizado para el procesamiento
        /// </summary>
        public string ProcessingMethod { get; set; } = string.Empty;

        /// <summary>
        /// Tokens utilizados en la generación
        /// </summary>
        public int TokensUsed { get; set; }

        /// <summary>
        /// Costo estimado de la generación
        /// </summary>
        public decimal EstimatedCost { get; set; }

        /// <summary>
        /// Tiempo de procesamiento
        /// </summary>
        public DateTime ProcessingTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Información adicional sobre el uso de tokens
        /// </summary>
        public TokenUsageInfo TokenUsage { get; set; } = new();

        /// <summary>
        /// Contenido original procesado (para referencia)
        /// </summary>
        public string SourceContent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pregunta generada por IA
    /// </summary>
    public class GeneratedQuizQuestion
    {
        /// <summary>
        /// Texto de la pregunta
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de pregunta
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Nivel de dificultad
        /// </summary>
        public NivelDificultad DifficultyLevel { get; set; }

        /// <summary>
        /// Opciones de respuesta (para preguntas de opción múltiple)
        /// </summary>
        public List<QuizAnswerOption> AnswerOptions { get; set; } = new();

        /// <summary>
        /// Respuesta correcta (para preguntas de respuesta corta o completar)
        /// </summary>
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Explicación de la respuesta correcta
        /// </summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// Puntuación asignada a la pregunta
        /// </summary>
        public int Points { get; set; } = 1;

        /// <summary>
        /// Etiquetas o categorías asociadas
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Referencia al contenido original que generó la pregunta
        /// </summary>
        public string SourceReference { get; set; } = string.Empty;

        /// <summary>
        /// Puntuación de confianza de la IA en la calidad de la pregunta
        /// </summary>
        [Range(0.0, 1.0)]
        public double ConfidenceScore { get; set; } = 0.5;

        /// <summary>
        /// Indica si la pregunta fue aprobada manualmente
        /// </summary>
        public bool IsApproved { get; set; } = false;

        /// <summary>
        /// Comentarios o notas sobre la pregunta
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Opción de respuesta para preguntas de opción múltiple
    /// </summary>
    public class QuizAnswerOption
    {
        /// <summary>
        /// Texto de la opción
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Indica si es la respuesta correcta
        /// </summary>
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Explicación de por qué esta opción es correcta o incorrecta
        /// </summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// Orden de presentación de la opción
        /// </summary>
        public int Order { get; set; }
    }

    /// <summary>
    /// Configuración específica para prompts de IA
    /// </summary>
    public class QuizGenerationPrompt
    {
        /// <summary>
        /// Contenido base para generar preguntas
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Configuración de generación
        /// </summary>
        public QuizGenerationSettings Settings { get; set; } = new();

        /// <summary>
        /// Instrucciones personalizadas adicionales
        /// </summary>
        public string CustomInstructions { get; set; } = string.Empty;

        /// <summary>
        /// Contexto adicional sobre la materia
        /// </summary>
        public string SubjectContext { get; set; } = string.Empty;

        /// <summary>
        /// Ejemplos de preguntas deseadas (optional)
        /// </summary>
        public List<string> ExampleQuestions { get; set; } = new();
    }
}