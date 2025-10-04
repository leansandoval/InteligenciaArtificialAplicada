using System.Text;
using System.Text.RegularExpressions;
using QuizCraft.Application.Interfaces;

namespace QuizCraft.Infrastructure.Services.DocumentProcessing
{
    /// <summary>
    /// Extractor de texto para archivos de texto plano (.txt)
    /// </summary>
    public class TextFileExtractor : IDocumentTextExtractor
    {
        public bool CanExtract(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension == ".txt";
        }

        public async Task<DocumentContent> ExtractTextAsync(Stream documentStream, string fileName)
        {
            try
            {
                documentStream.Position = 0;
                using var reader = new StreamReader(documentStream, Encoding.UTF8);
                var content = await reader.ReadToEndAsync();

                var documentContent = new DocumentContent
                {
                    RawText = content,
                    DocumentType = "Text",
                    Metadata = new Dictionary<string, object>
                    {
                        ["FileName"] = fileName,
                        ["Size"] = content.Length,
                        ["Lines"] = content.Split('\n').Length
                    }
                };

                // Dividir en secciones por párrafos
                documentContent.Sections = ParseTextIntoSections(content);

                return documentContent;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al extraer texto de {fileName}: {ex.Message}", ex);
            }
        }

        private List<DocumentSection> ParseTextIntoSections(string content)
        {
            var sections = new List<DocumentSection>();
            var paragraphs = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < paragraphs.Length; i++)
            {
                var paragraph = paragraphs[i].Trim();
                if (string.IsNullOrWhiteSpace(paragraph)) continue;

                var section = new DocumentSection
                {
                    Content = paragraph,
                    Position = i,
                    Type = DetermineContentType(paragraph)
                };

                // Detectar si es un título (líneas cortas, mayúsculas, etc.)
                if (IsLikelyTitle(paragraph))
                {
                    section.Type = SectionType.Title;
                    section.Title = paragraph;
                }
                else if (paragraph.StartsWith("- ") || paragraph.StartsWith("• "))
                {
                    section.Type = SectionType.BulletPoint;
                }

                sections.Add(section);
            }

            return sections;
        }

        private SectionType DetermineContentType(string text)
        {
            // Detectar patrones comunes
            if (text.Length < 100 && char.IsUpper(text[0]) && !text.EndsWith('.'))
                return SectionType.Title;

            if (text.StartsWith("- ") || text.StartsWith("• ") || text.StartsWith("* "))
                return SectionType.BulletPoint;

            if (text.StartsWith(">") || text.StartsWith("\""))
                return SectionType.Quote;

            return SectionType.Paragraph;
        }

        private bool IsLikelyTitle(string text)
        {
            // Heurísticas para detectar títulos
            return text.Length < 100 && 
                   !text.EndsWith('.') && 
                   !text.EndsWith(',') &&
                   !text.Contains('\n') &&
                   (char.IsUpper(text[0]) || text.All(c => char.IsUpper(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)));
        }
    }
}