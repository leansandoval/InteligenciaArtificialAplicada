using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;

namespace QuizCraft.Infrastructure.Services.DocumentProcessing
{
    /// <summary>
    /// Procesador tradicional de documentos sin IA
    /// </summary>
    public class TraditionalDocumentProcessor : ITraditionalDocumentProcessor
    {
        private readonly List<IDocumentTextExtractor> _extractors;
        private readonly ILogger<TraditionalDocumentProcessor> _logger;

        public TraditionalDocumentProcessor(
            IEnumerable<IDocumentTextExtractor> extractors,
            ILogger<TraditionalDocumentProcessor> logger)
        {
            _extractors = extractors.ToList();
            _logger = logger;
        }

        public bool CanProcess(string fileName)
        {
            return _extractors.Any(e => e.CanExtract(fileName));
        }

        public async Task<FlashcardGenerationResult> ProcessAsync(
            Stream documentStream, 
            string fileName, 
            TraditionalGenerationSettings settings)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new FlashcardGenerationResult
            {
                ModeUsed = GenerationMode.Traditional
            };

            try
            {
                _logger.LogInformation("Iniciando procesamiento tradicional de {FileName}", fileName);

                // Buscar extractor apropiado
                var extractor = _extractors.FirstOrDefault(e => e.CanExtract(fileName));
                if (extractor == null)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Tipo de archivo no soportado: {Path.GetExtension(fileName)}";
                    return result;
                }

                // Extraer contenido
                var documentContent = await extractor.ExtractTextAsync(documentStream, fileName);
                if (string.IsNullOrWhiteSpace(documentContent.RawText))
                {
                    result.Success = false;
                    result.ErrorMessage = "El documento no contiene texto extraíble";
                    return result;
                }

                // Generar flashcards usando procesamiento tradicional
                var flashcards = await GenerateFlashcardsFromContent(documentContent, settings);

                result.Success = true;
                result.Flashcards = flashcards;
                result.TotalGenerated = flashcards.Count;

                _logger.LogInformation("Procesamiento completado: {Count} flashcards generadas en {Time}ms", 
                    flashcards.Count, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el procesamiento tradicional de {FileName}", fileName);
                result.Success = false;
                result.ErrorMessage = $"Error durante el procesamiento: {ex.Message}";
                return result;
            }
            finally
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
            }
        }

        private async Task<List<GeneratedFlashcard>> GenerateFlashcardsFromContent(
            DocumentContent content, 
            TraditionalGenerationSettings settings)
        {
            var flashcards = new List<GeneratedFlashcard>();

            // Estrategia 1: Procesar por secciones estructuradas
            if (settings.UseStructuralElements && content.Sections.Any())
            {
                flashcards.AddRange(await ProcessStructuredSections(content.Sections, settings));
            }

            // Estrategia 2: Detectar patrones de preguntas existentes
            if (settings.DetectQuestionPatterns)
            {
                flashcards.AddRange(await DetectExistingQuestions(content.RawText, settings));
            }

            // Estrategia 3: División por párrafos
            if (settings.SplitByParagraph)
            {
                flashcards.AddRange(await ProcessByParagraphs(content.RawText, settings));
            }

            // Estrategia 4: Separador personalizado
            if (!string.IsNullOrEmpty(settings.CustomSeparator))
            {
                flashcards.AddRange(await ProcessByCustomSeparator(content.RawText, settings));
            }

            // Filtrar y limpiar resultados
            flashcards = FilterAndCleanFlashcards(flashcards, settings);

            // Limitar cantidad según configuración
            if (flashcards.Count > settings.MaxCardsPerDocument)
            {
                flashcards = flashcards.Take(settings.MaxCardsPerDocument).ToList();
            }

            return flashcards;
        }

        private async Task<List<GeneratedFlashcard>> ProcessStructuredSections(
            List<DocumentSection> sections, 
            TraditionalGenerationSettings settings)
        {
            var flashcards = new List<GeneratedFlashcard>();

            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];

                // Si es un título, usar como pregunta y siguiente sección como respuesta
                if (section.Type == SectionType.Title && i + 1 < sections.Count)
                {
                    var nextSection = sections[i + 1];
                    if (nextSection.Type == SectionType.Paragraph)
                    {
                        flashcards.Add(new GeneratedFlashcard
                        {
                            Pregunta = $"¿Qué puedes decir sobre: {section.Title}?",
                            Respuesta = nextSection.Content,
                            Source = $"Sección: {section.Title}",
                            Confidence = 85
                        });
                    }
                }

                // Si es un párrafo con definiciones evidentes
                if (section.Type == SectionType.Paragraph && ContainsDefinition(section.Content))
                {
                    var (question, answer) = ExtractDefinition(section.Content);
                    if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                    {
                        flashcards.Add(new GeneratedFlashcard
                        {
                            Pregunta = question,
                            Respuesta = answer,
                            Source = $"Párrafo {section.Position + 1}",
                            Confidence = 90
                        });
                    }
                }
            }

            return flashcards;
        }

        private async Task<List<GeneratedFlashcard>> DetectExistingQuestions(
            string text, 
            TraditionalGenerationSettings settings)
        {
            var flashcards = new List<GeneratedFlashcard>();
            var lines = text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Detectar preguntas explícitas
                if (IsQuestion(line, settings.QuestionKeywords))
                {
                    var question = line;
                    var answer = "";

                    // Buscar respuesta en las siguientes líneas
                    for (int j = i + 1; j < Math.Min(i + 5, lines.Length); j++)
                    {
                        var nextLine = lines[j].Trim();
                        if (!string.IsNullOrEmpty(nextLine) && !IsQuestion(nextLine, settings.QuestionKeywords))
                        {
                            answer += nextLine + " ";
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(answer.Trim()))
                    {
                        flashcards.Add(new GeneratedFlashcard
                        {
                            Pregunta = question,
                            Respuesta = answer.Trim(),
                            Source = $"Pregunta existente (línea {i + 1})",
                            Confidence = 95
                        });
                    }
                }
            }

            return flashcards;
        }

        private async Task<List<GeneratedFlashcard>> ProcessByParagraphs(
            string text, 
            TraditionalGenerationSettings settings)
        {
            var flashcards = new List<GeneratedFlashcard>();
            var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < paragraphs.Length; i++)
            {
                var paragraph = paragraphs[i].Trim();
                
                if (paragraph.Length >= settings.MinTextLength && paragraph.Length <= settings.MaxTextLength)
                {
                    // Crear pregunta genérica basada en el contenido
                    var question = GenerateQuestionFromParagraph(paragraph);
                    
                    if (!string.IsNullOrEmpty(question))
                    {
                        flashcards.Add(new GeneratedFlashcard
                        {
                            Pregunta = question,
                            Respuesta = paragraph,
                            Source = $"Párrafo {i + 1}",
                            Confidence = 70
                        });
                    }
                }
            }

            return flashcards;
        }

        private async Task<List<GeneratedFlashcard>> ProcessByCustomSeparator(
            string text, 
            TraditionalGenerationSettings settings)
        {
            var flashcards = new List<GeneratedFlashcard>();
            if (string.IsNullOrEmpty(settings.CustomSeparator)) return flashcards;
            
            var segments = text.Split(new[] { settings.CustomSeparator }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < segments.Length; i += 2)
            {
                if (i + 1 < segments.Length)
                {
                    var question = segments[i].Trim();
                    var answer = segments[i + 1].Trim();

                    if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                    {
                        flashcards.Add(new GeneratedFlashcard
                        {
                            Pregunta = question,
                            Respuesta = answer,
                            Source = $"Separador personalizado (segmento {i / 2 + 1})",
                            Confidence = 88
                        });
                    }
                }
            }

            return flashcards;
        }

        private List<GeneratedFlashcard> FilterAndCleanFlashcards(
            List<GeneratedFlashcard> flashcards, 
            TraditionalGenerationSettings settings)
        {
            return flashcards
                .Where(f => !string.IsNullOrWhiteSpace(f.Pregunta) && !string.IsNullOrWhiteSpace(f.Respuesta))
                .Where(f => settings.FilterShortContent ? 
                    f.Pregunta.Length >= settings.MinTextLength && f.Respuesta.Length >= settings.MinTextLength : true)
                .Where(f => f.Pregunta.Length <= settings.MaxTextLength && f.Respuesta.Length <= settings.MaxTextLength)
                .GroupBy(f => f.Pregunta.ToLower())
                .Select(g => g.OrderByDescending(f => f.Confidence).First()) // Eliminar duplicados
                .OrderByDescending(f => f.Confidence)
                .ToList();
        }

        private bool ContainsDefinition(string text)
        {
            var definitionPatterns = new[]
            {
                @"\b\w+\s+es\s+",
                @"\b\w+\s+se\s+define\s+como\s+",
                @"\bDefinición:\s*",
                @"\bConcepto:\s*",
                @"\w+\s*:\s*\w+"
            };

            return definitionPatterns.Any(pattern => Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase));
        }

        private (string question, string answer) ExtractDefinition(string text)
        {
            // Patrón: "X es Y" -> ¿Qué es X? / Y
            var match = Regex.Match(text, @"(.+?)\s+es\s+(.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return ($"¿Qué es {match.Groups[1].Value.Trim()}?", match.Groups[2].Value.Trim());
            }

            // Patrón: "Concepto: Definición"
            match = Regex.Match(text, @"(.+?):\s*(.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return ($"¿Qué es {match.Groups[1].Value.Trim()}?", match.Groups[2].Value.Trim());
            }

            return (string.Empty, string.Empty);
        }

        private bool IsQuestion(string line, List<string> questionKeywords)
        {
            return questionKeywords.Any(keyword => 
                line.Contains(keyword, StringComparison.OrdinalIgnoreCase)) || 
                line.EndsWith('?');
        }

        private string GenerateQuestionFromParagraph(string paragraph)
        {
            // Extraer palabras clave del párrafo
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstSentence = paragraph.Split('.').FirstOrDefault()?.Trim();

            if (!string.IsNullOrEmpty(firstSentence) && firstSentence.Length < 100)
            {
                return $"¿Qué información importante contiene el siguiente texto sobre {ExtractKeyTopic(firstSentence)}?";
            }

            return "¿Cuál es la información principal de este contenido?";
        }

        private string ExtractKeyTopic(string sentence)
        {
            // Extraer sustantivos importantes (simplificado)
            var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var importantWords = words
                .Where(w => w.Length > 4)
                .Where(w => char.IsUpper(w[0]))
                .Take(2);

            return string.Join(" y ", importantWords);
        }
    }
}