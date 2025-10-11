using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using System.Text;
using UglyToad.PdfPig;
using System.IO;

namespace QuizCraft.Infrastructure.Services
{
    public class DocumentTextExtractor : IDocumentTextExtractor
    {
        private readonly ILogger<DocumentTextExtractor> _logger;

        public DocumentTextExtractor(ILogger<DocumentTextExtractor> logger)
        {
            _logger = logger;
        }

        public async Task<DocumentContent> ExtractTextAsync(Stream documentStream, string fileName)
        {
            try
            {
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                
                switch (extension)
                {
                    case ".pdf":
                        return await ExtractFromPdfAsync(documentStream, fileName);
                    case ".txt":
                        return await ExtractFromTextAsync(documentStream, fileName);
                    case ".doc":
                    case ".docx":
                        return await ExtractFromWordAsync(documentStream, fileName);
                    default:
                        throw new NotSupportedException($"File format {extension} is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from document: {FileName}", fileName);
                throw;
            }
        }

        public bool CanExtract(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension is ".pdf" or ".txt" or ".doc" or ".docx";
        }

        private Task<DocumentContent> ExtractFromPdfAsync(Stream stream, string fileName)
        {
            try
            {
                using var document = PdfDocument.Open(stream);
                var text = new StringBuilder();
                var sections = new List<DocumentSection>();

                foreach (var page in document.GetPages())
                {
                    var pageText = page.Text;
                    text.AppendLine(pageText);

                    // Crear una sección por página
                    sections.Add(new DocumentSection
                    {
                        Title = $"Página {page.Number}",
                        Content = pageText,
                        Level = 1,
                        Type = SectionType.Paragraph,
                        Position = page.Number
                    });
                }

                return Task.FromResult(new DocumentContent
                {
                    RawText = text.ToString(),
                    Sections = sections,
                    DocumentType = "PDF",
                    Metadata = new Dictionary<string, object>
                    {
                        ["FileName"] = fileName,
                        ["PageCount"] = document.NumberOfPages,
                        ["ExtractedAt"] = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF");
                throw;
            }
        }

        private async Task<DocumentContent> ExtractFromTextAsync(Stream stream, string fileName)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = await reader.ReadToEndAsync();

            var sections = new List<DocumentSection>
            {
                new DocumentSection
                {
                    Title = "Contenido completo",
                    Content = content,
                    Level = 1,
                    Type = SectionType.Paragraph,
                    Position = 1
                }
            };

            return new DocumentContent
            {
                RawText = content,
                Sections = sections,
                DocumentType = "Text",
                Metadata = new Dictionary<string, object>
                {
                    ["FileName"] = fileName,
                    ["Length"] = content.Length,
                    ["ExtractedAt"] = DateTime.UtcNow
                }
            };
        }

        private async Task<DocumentContent> ExtractFromWordAsync(Stream stream, string fileName)
        {
            // Para una implementación básica, retornamos un placeholder
            // En producción, usarías una librería como DocumentFormat.OpenXml
            _logger.LogWarning("Word document processing not fully implemented yet");
            
            var placeholderContent = "Contenido extraído de documento Word (implementación pendiente)";
            
            return await Task.FromResult(new DocumentContent
            {
                RawText = placeholderContent,
                Sections = new List<DocumentSection>
                {
                    new DocumentSection
                    {
                        Title = "Documento Word",
                        Content = placeholderContent,
                        Level = 1,
                        Type = SectionType.Paragraph,
                        Position = 1
                    }
                },
                DocumentType = "Word",
                Metadata = new Dictionary<string, object>
                {
                    ["FileName"] = fileName,
                    ["Status"] = "Pendiente implementación completa",
                    ["ExtractedAt"] = DateTime.UtcNow
                }
            });
        }
    }
}