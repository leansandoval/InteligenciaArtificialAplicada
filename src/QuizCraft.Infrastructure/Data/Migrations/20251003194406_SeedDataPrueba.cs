using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizCraft.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear Materias usando SQL directo para obtener el UsuarioId
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Materias ON;
                
                INSERT INTO Materias (Id, Nombre, Descripcion, Color, Icono, UsuarioId, EstaActivo, FechaCreacion, FechaModificacion)
                SELECT 100, 'Matemáticas', 'Conceptos fundamentales de matemáticas', '#4CAF50', 'fa-calculator', Id, 1, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM'
                UNION ALL
                SELECT 101, 'Historia', 'Historia mundial y acontecimientos importantes', '#FF9800', 'fa-book', Id, 1, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM'
                UNION ALL
                SELECT 102, 'Ciencias', 'Conceptos de física, química y biología', '#2196F3', 'fa-flask', Id, 1, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM'
                UNION ALL
                SELECT 103, 'Programación', 'Fundamentos de desarrollo de software', '#9C27B0', 'fa-code', Id, 1, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM';
                
                SET IDENTITY_INSERT Materias OFF;
            ");

            // Crear Flashcards usando SQL directo
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Flashcards ON;
                
                INSERT INTO Flashcards (Id, Pregunta, Respuesta, Pista, MateriaId, Dificultad, EstaActivo, FechaCreacion, FechaModificacion, VecesVista, VecesCorrecta, VecesIncorrecta, FactorFacilidad, IntervaloRepeticion, ProximaRevision, UltimaRevision)
                VALUES 
                -- Flashcards para Matemáticas (MateriaId = 100)
                (200, '¿Cuál es la fórmula del área de un círculo?', 'π × r²', 'Piensa en pi y el radio al cuadrado', 100, 1, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (201, '¿Cuánto es 7 × 8?', '56', '7 veces 8', 100, 0, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (202, '¿Qué es un número primo?', 'Un número que solo es divisible por 1 y por sí mismo', 'Solo tiene dos divisores', 100, 2, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (203, '¿Cuál es la derivada de x²?', '2x', 'Aplica la regla de potencias', 100, 2, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                
                -- Flashcards para Historia (MateriaId = 101)
                (204, '¿En qué año comenzó la Segunda Guerra Mundial?', '1939', 'Finales de los años 30', 101, 1, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (205, '¿Quién fue el primer presidente de Estados Unidos?', 'George Washington', 'Padre fundador', 101, 1, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (206, '¿En qué año cayó el Muro de Berlín?', '1989', 'Final de los 80', 101, 2, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                
                -- Flashcards para Ciencias (MateriaId = 102)
                (207, '¿Cuál es el símbolo químico del oro?', 'Au', 'Viene del latín aurum', 102, 1, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (208, '¿A qué velocidad viaja la luz?', '299,792,458 m/s', 'Aproximadamente 300,000 km/s', 102, 2, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (209, '¿Cuántos cromosomas tiene el ser humano?', '46', '23 pares', 102, 1, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                
                -- Flashcards para Programación (MateriaId = 103)
                (210, '¿Qué significa HTML?', 'HyperText Markup Language', 'Lenguaje de marcado de hipertexto', 103, 0, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (211, '¿Qué es una variable en programación?', 'Un espacio en memoria para almacenar datos', 'Contenedor de información', 103, 1, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL),
                (212, '¿Qué es la recursión?', 'Una función que se llama a sí misma', 'Auto-referencia', 103, 2, 1, GETDATE(), GETDATE(), 0, 0, 0, 2.5, 1, DATEADD(day, 1, GETDATE()), NULL);
                
                SET IDENTITY_INSERT Flashcards OFF;
            ");

            // Crear Quizzes usando SQL directo
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Quizzes ON;
                
                INSERT INTO Quizzes (Id, Titulo, Descripcion, MateriaId, CreadorId, NivelDificultad, TiempoLimite, TiempoPorPregunta, NumeroPreguntas, EsPublico, EsActivo, EstaActivo, PermitirReintento, MostrarRespuestasInmediato, FechaCreacion, FechaModificacion)
                SELECT 300, 'Quiz de Matemáticas Básicas', 'Evaluación de conceptos fundamentales de matemáticas', 100, Id, 1, 600, 60, 5, 1, 1, 1, 1, 1, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM'
                UNION ALL
                SELECT 301, 'Historia Mundial', 'Quiz sobre eventos históricos importantes', 101, Id, 1, 900, 90, 4, 1, 1, 1, 1, 0, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM'
                UNION ALL
                SELECT 302, 'Ciencias Naturales', 'Conocimientos básicos de física, química y biología', 102, Id, 2, 1200, 120, 6, 0, 1, 1, 1, 1, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM'
                UNION ALL
                SELECT 303, 'Fundamentos de Programación', 'Quiz para evaluar conocimientos básicos de programación', 103, Id, 2, 1800, 180, 8, 1, 1, 1, 0, 0, GETDATE(), GETDATE()
                FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM';
                
                SET IDENTITY_INSERT Quizzes OFF;
            ");

            // Crear Preguntas para los Quizzes
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT PreguntasQuiz ON;
                
                INSERT INTO PreguntasQuiz (Id, QuizId, TextoPregunta, TipoPregunta, PuntajeMaximo, OrdenPregunta, EstaActivo, FechaCreacion, FechaModificacion)
                VALUES 
                -- Preguntas para Quiz de Matemáticas (QuizId = 300)
                (400, 300, '¿Cuánto es 15 + 27?', 0, 10, 1, 1, GETDATE(), GETDATE()),
                (401, 300, '¿Cuál es el área de un rectángulo de 5x8?', 0, 10, 2, 1, GETDATE(), GETDATE()),
                (402, 300, '¿Cuánto es 12 × 9?', 0, 10, 3, 1, GETDATE(), GETDATE()),
                (403, 300, '¿Cuál es la raíz cuadrada de 64?', 0, 10, 4, 1, GETDATE(), GETDATE()),
                (404, 300, '¿Cuánto es 100 ÷ 4?', 0, 10, 5, 1, GETDATE(), GETDATE()),
                
                -- Preguntas para Quiz de Historia (QuizId = 301)
                (405, 301, '¿En qué año se descubrió América?', 0, 15, 1, 1, GETDATE(), GETDATE()),
                (406, 301, '¿Quién fue Napoleón Bonaparte?', 0, 15, 2, 1, GETDATE(), GETDATE()),
                (407, 301, '¿Cuándo terminó la Primera Guerra Mundial?', 0, 15, 3, 1, GETDATE(), GETDATE()),
                (408, 301, '¿Qué fue la Revolución Industrial?', 0, 15, 4, 1, GETDATE(), GETDATE());
                
                SET IDENTITY_INSERT PreguntasQuiz OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar datos en orden inverso para respetar las foreign keys
            migrationBuilder.Sql("DELETE FROM PreguntasQuiz WHERE Id BETWEEN 400 AND 408");
            migrationBuilder.Sql("DELETE FROM Quizzes WHERE Id BETWEEN 300 AND 303");
            migrationBuilder.Sql("DELETE FROM Flashcards WHERE Id BETWEEN 200 AND 212");
            migrationBuilder.Sql("DELETE FROM Materias WHERE Id BETWEEN 100 AND 103");
        }
    }
}
