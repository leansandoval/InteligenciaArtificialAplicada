using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizCraft.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insertar Materias
            migrationBuilder.Sql(@"
                DECLARE @AdminUserId NVARCHAR(450)
                SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = 'admin@quizcraft.com'
                
                SET IDENTITY_INSERT Materias ON
                
                INSERT INTO Materias (Id, Nombre, Descripcion, Color, EstaActivo, FechaCreacion, FechaModificacion, Icono, UsuarioId)
                VALUES 
                (1, N'Matemáticas', N'Materia de matemáticas básicas y avanzadas', N'#FF5722', 1, GETDATE(), GETDATE(), N'calculate', @AdminUserId),
                (2, N'Historia', N'Historia universal y local', N'#8BC34A', 1, GETDATE(), GETDATE(), N'history', @AdminUserId),
                (3, N'Ciencias', N'Ciencias naturales y experimentales', N'#2196F3', 1, GETDATE(), GETDATE(), N'science', @AdminUserId),
                (4, N'Programación', N'Fundamentos de programación y desarrollo', N'#9C27B0', 1, GETDATE(), GETDATE(), N'code', @AdminUserId)
                
                SET IDENTITY_INSERT Materias OFF
            ");

            // Insertar Flashcards
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Flashcards ON
                
                INSERT INTO Flashcards (Id, Pregunta, Respuesta, Dificultad, EstaActivo, FechaCreacion, MateriaId, Pista, VecesVista, VecesCorrecta, VecesIncorrecta, IntervaloRepeticion, FactorFacilidad)
                VALUES 
                -- Matemáticas
                (1, N'¿Cuál es la fórmula del área de un círculo?', N'A = π × r²', 0, 1, GETDATE(), 1, N'Donde r es el radio del círculo', 0, 0, 0, 1, 2.5),
                (2, N'¿Qué es el teorema de Pitágoras?', N'En un triángulo rectángulo, el cuadrado de la hipotenusa es igual a la suma de los cuadrados de los catetos: a² + b² = c²', 1, 1, GETDATE(), 1, N'Aplicable solo a triángulos rectángulos', 0, 0, 0, 1, 2.5),
                (3, N'¿Cuál es la derivada de x²?', N'2x', 2, 1, GETDATE(), 1, N'Regla de potencias en derivación', 0, 0, 0, 1, 2.5),
                
                -- Historia
                (4, N'¿En qué año comenzó la Primera Guerra Mundial?', N'1914', 0, 1, GETDATE(), 2, N'Duró hasta 1918', 0, 0, 0, 1, 2.5),
                (5, N'¿Quién fue el primer presidente de Argentina?', N'Bernardino Rivadavia', 1, 1, GETDATE(), 2, N'Presidente desde 1826 hasta 1827', 0, 0, 0, 1, 2.5),
                (6, N'¿Qué significó la Revolución Francesa?', N'Fue un proceso revolucionario que marcó el fin del Antiguo Régimen en Francia y el inicio de la Edad Contemporánea', 1, 1, GETDATE(), 2, N'Ocurrió entre 1789 y 1799', 0, 0, 0, 1, 2.5),
                
                -- Ciencias
                (7, N'¿Cuál es la fórmula química del agua?', N'H₂O', 0, 1, GETDATE(), 3, N'Dos átomos de hidrógeno y uno de oxígeno', 0, 0, 0, 1, 2.5),
                (8, N'¿Qué es la fotosíntesis?', N'Es el proceso por el cual las plantas convierten la luz solar, el dióxido de carbono y el agua en glucosa y oxígeno', 1, 1, GETDATE(), 3, N'Ocurre principalmente en las hojas', 0, 0, 0, 1, 2.5),
                (9, N'¿Cuáles son las leyes de Newton?', N'1) Ley de inercia, 2) F = ma (Fuerza = masa × aceleración), 3) Acción y reacción', 2, 1, GETDATE(), 3, N'Fundamentos de la mecánica clásica', 0, 0, 0, 1, 2.5),
                
                -- Programación
                (10, N'¿Qué es una variable en programación?', N'Es un espacio en memoria que almacena un valor que puede cambiar durante la ejecución del programa', 0, 1, GETDATE(), 4, N'Tiene un nombre, tipo y valor', 0, 0, 0, 1, 2.5),
                (11, N'¿Qué es un algoritmo?', N'Es una secuencia finita de instrucciones bien definidas para resolver un problema o realizar una tarea', 0, 1, GETDATE(), 4, N'Debe ser preciso, finito y eficiente', 0, 0, 0, 1, 2.5),
                (12, N'¿Qué significa POO?', N'Programación Orientada a Objetos - paradigma de programación basado en objetos que contienen datos y métodos', 1, 1, GETDATE(), 4, N'Conceptos clave: encapsulación, herencia, polimorfismo', 0, 0, 0, 1, 2.5),
                (13, N'¿Qué es una función recursiva?', N'Es una función que se llama a sí misma para resolver un problema dividiéndolo en subproblemas más pequeños', 2, 1, GETDATE(), 4, N'Debe tener un caso base para evitar bucles infinitos', 0, 0, 0, 1, 2.5)
                
                SET IDENTITY_INSERT Flashcards OFF
            ");

            // Insertar Quizzes
            migrationBuilder.Sql(@"
                DECLARE @AdminUserId NVARCHAR(450)
                SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = 'admin@quizcraft.com'
                
                SET IDENTITY_INSERT Quizzes ON
                
                INSERT INTO Quizzes (Id, Titulo, Descripcion, EstaActivo, FechaCreacion, MateriaId, CreadorId, TiempoLimite, NivelDificultad, EsPublico, NumeroPreguntas, EsActivo, TiempoPorPregunta, MostrarRespuestasInmediato, PermitirReintento)
                VALUES 
                (1, N'Quiz Básico de Matemáticas', N'Evaluación de conceptos básicos de matemáticas', 1, GETDATE(), 1, @AdminUserId, 15, 1, 1, 4, 1, 30, 1, 1),
                (2, N'Quiz de Historia Universal', N'Preguntas sobre eventos históricos importantes', 1, GETDATE(), 2, @AdminUserId, 20, 2, 1, 4, 1, 30, 1, 1),
                (3, N'Quiz de Ciencias Naturales', N'Conceptos fundamentales de ciencias', 1, GETDATE(), 3, @AdminUserId, 25, 1, 1, 4, 1, 30, 1, 1),
                (4, N'Quiz de Fundamentos de Programación', N'Conceptos básicos de programación', 1, GETDATE(), 4, @AdminUserId, 30, 2, 1, 4, 1, 30, 1, 1)
                
                SET IDENTITY_INSERT Quizzes OFF
            ");

            // Insertar Preguntas de Quiz
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT PreguntasQuiz ON
                
                INSERT INTO PreguntasQuiz (Id, TextoPregunta, TipoPregunta, OpcionA, OpcionB, OpcionC, OpcionD, RespuestaCorrecta, Explicacion, Puntos, Orden, EstaActivo, FechaCreacion, QuizId)
                VALUES 
                -- Quiz Matemáticas
                (1, N'¿Cuál es el resultado de 2 + 2?', 1, N'2', N'3', N'4', N'5', N'C', N'La suma de 2 + 2 es 4', 25, 1, 1, GETDATE(), 1),
                (2, N'¿Cuál es el área de un cuadrado de lado 5?', 1, N'20', N'25', N'30', N'35', N'B', N'El área de un cuadrado es lado², por lo tanto 5² = 25', 25, 2, 1, GETDATE(), 1),
                (3, N'¿Cuál es la raíz cuadrada de 64?', 1, N'6', N'7', N'8', N'9', N'C', N'√64 = 8 porque 8² = 64', 25, 3, 1, GETDATE(), 1),
                (4, N'¿Cuánto es 15% de 200?', 1, N'25', N'30', N'35', N'40', N'B', N'15% de 200 = 0.15 × 200 = 30', 25, 4, 1, GETDATE(), 1),
                
                -- Quiz Historia
                (5, N'¿En qué año se descubrió América?', 1, N'1490', N'1491', N'1492', N'1493', N'C', N'Cristóbal Colón llegó a América el 12 de octubre de 1492', 25, 1, 1, GETDATE(), 2),
                (6, N'¿Quién escribió Don Quijote de la Mancha?', 1, N'Lope de Vega', N'Miguel de Cervantes', N'Garcilaso de la Vega', N'Francisco de Quevedo', N'B', N'Miguel de Cervantes Saavedra escribió esta obra maestra de la literatura española', 25, 2, 1, GETDATE(), 2),
                (7, N'¿Cuál fue la causa principal de la Primera Guerra Mundial?', 1, N'Asesinato archiduque', N'Crisis económica', N'Revolución rusa', N'Invasión Polonia', N'A', N'El asesinato del archiduque Francisco Fernando de Austria en Sarajevo fue el detonante de la Primera Guerra Mundial', 25, 3, 1, GETDATE(), 2),
                (8, N'¿En qué siglo ocurrió la Revolución Industrial?', 1, N'Siglo XVII', N'Siglo XVIII', N'Siglo XIX', N'Siglo XX', N'B', N'La Revolución Industrial comenzó en Inglaterra a mediados del siglo XVIII', 25, 4, 1, GETDATE(), 2),
                
                -- Quiz Ciencias
                (9, N'¿Cuántos planetas tiene nuestro sistema solar?', 1, N'7', N'8', N'9', N'10', N'B', N'Desde 2006, nuestro sistema solar tiene 8 planetas (Plutón fue reclasificado como planeta enano)', 25, 1, 1, GETDATE(), 3),
                (10, N'¿Cuál es el órgano más grande del cuerpo humano?', 1, N'Corazón', N'Cerebro', N'Pulmones', N'Piel', N'D', N'La piel es el órgano más grande del cuerpo humano, representando aproximadamente el 16% del peso corporal', 25, 2, 1, GETDATE(), 3),
                (11, N'¿Cuál es la velocidad de la luz en el vacío?', 1, N'299,792,458 m/s', N'300,000,000 m/s', N'299,000,000 m/s', N'298,000,000 m/s', N'A', N'La velocidad de la luz en el vacío es exactamente 299,792,458 metros por segundo', 25, 3, 1, GETDATE(), 3),
                (12, N'¿Qué gas representa el mayor porcentaje en la atmósfera terrestre?', 1, N'Oxígeno', N'CO2', N'Nitrógeno', N'Hidrógeno', N'C', N'El nitrógeno representa aproximadamente el 78% de la atmósfera terrestre', 25, 4, 1, GETDATE(), 3),
                
                -- Quiz Programación
                (13, N'¿Cuál de estos es un lenguaje de programación?', 1, N'HTML', N'CSS', N'Python', N'XML', N'C', N'Python es un lenguaje de programación, mientras que HTML, CSS y XML son lenguajes de marcado', 25, 1, 1, GETDATE(), 4),
                (14, N'¿Qué significa IDE en programación?', 1, N'Internet Dev Environment', N'Integrated Dev Environment', N'Internal Data Exchange', N'Interactive Design Editor', N'B', N'IDE significa Entorno de Desarrollo Integrado, una aplicación que proporciona herramientas para el desarrollo de software', 25, 2, 1, GETDATE(), 4),
                (15, N'¿Cuál es el resultado de la operación 10 % 3 en la mayoría de lenguajes de programación?', 1, N'0', N'1', N'2', N'3', N'B', N'El operador % es el módulo, que devuelve el resto de una división. 10 ÷ 3 = 3 con resto 1', 25, 3, 1, GETDATE(), 4),
                (16, N'¿Qué tipo de bucle garantiza que se ejecute al menos una vez?', 1, N'for', N'while', N'do-while', N'foreach', N'C', N'El bucle do-while evalúa la condición después de ejecutar el bloque de código, garantizando al menos una ejecución', 25, 4, 1, GETDATE(), 4)
                
                SET IDENTITY_INSERT PreguntasQuiz OFF
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar preguntas de quiz
            migrationBuilder.Sql("DELETE FROM PreguntasQuiz WHERE Id BETWEEN 1 AND 16");
            
            // Eliminar quizzes
            migrationBuilder.Sql("DELETE FROM Quizzes WHERE Id BETWEEN 1 AND 4");
            
            // Eliminar flashcards
            migrationBuilder.Sql("DELETE FROM Flashcards WHERE Id BETWEEN 1 AND 13");
            
            // Eliminar materias
            migrationBuilder.Sql("DELETE FROM Materias WHERE Id BETWEEN 1 AND 4");
        }
    }
}
