namespace QuizCraft.Core.Enums;

/// <summary>
/// Enumeración que define los tipos de entidad a los que se puede adjuntar un archivo
/// </summary>
public enum TipoEntidad
{
    /// <summary>
    /// Archivo adjunto a una flashcard
    /// </summary>
    Flashcard = 1,

    /// <summary>
    /// Archivo adjunto a una pregunta de quiz
    /// </summary>
    PreguntaQuiz = 2,

    /// <summary>
    /// Archivo adjunto a un quiz
    /// </summary>
    Quiz = 3,

    /// <summary>
    /// Archivo adjunto a una materia
    /// </summary>
    Materia = 4
}