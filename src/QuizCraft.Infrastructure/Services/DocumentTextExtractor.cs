using Microsoft.Extensions.Logging;
using QuizCraft.Application.Interfaces;
using System.Text;
using UglyToad.PdfPig;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlWordprocessing = DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPresentation = DocumentFormat.OpenXml.Presentation;

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
                _logger.LogInformation("Extracting text from file: {FileName} with extension: {Extension}", fileName, extension);
                
                switch (extension)
                {
                    case ".pdf":
                        _logger.LogInformation("Using PDF extraction for {FileName}", fileName);
                        return await ExtractFromPdfAsync(documentStream, fileName);
                    case ".txt":
                        _logger.LogInformation("Using TXT extraction for {FileName}", fileName);
                        return await ExtractFromTextAsync(documentStream, fileName);
                    case ".doc":
                    case ".docx":
                        _logger.LogInformation("Using Word extraction for {FileName}", fileName);
                        return await ExtractFromWordAsync(documentStream, fileName);
                    case ".ppt":
                    case ".pptx":
                        _logger.LogInformation("Using PowerPoint extraction for {FileName}", fileName);
                        return await ExtractFromPowerPointAsync(documentStream, fileName);
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
            return extension is ".pdf" or ".txt" or ".doc" or ".docx" or ".ppt" or ".pptx";
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

        private Task<DocumentContent> ExtractFromWordAsync(Stream stream, string fileName)
        {
            _logger.LogInformation("Starting Word document extraction for: {FileName}", fileName);
            
            try
            {
                // Asegurar que el stream esté al inicio
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                    _logger.LogInformation("Stream reset to position 0 for Word document");
                }
                else
                {
                    _logger.LogWarning("Stream is not seekable for Word document: {FileName}", fileName);
                }

                using var wordDocument = WordprocessingDocument.Open(stream, false);
                var body = wordDocument.MainDocumentPart?.Document?.Body;
                
                if (body == null)
                {
                    _logger.LogWarning("No se pudo encontrar el contenido del documento Word");
                    return Task.FromResult(new DocumentContent
                    {
                        RawText = "",
                        Sections = new List<DocumentSection>(),
                        DocumentType = "Word",
                        Metadata = new Dictionary<string, object>
                        {
                            ["FileName"] = fileName,
                            ["Error"] = "No se pudo acceder al contenido del documento",
                            ["ExtractedAt"] = DateTime.UtcNow
                        }
                    });
                }

                var extractedText = new StringBuilder();
                var sections = new List<DocumentSection>();
                var sectionIndex = 1;

                foreach (var element in body.Elements())
                {
                    if (element is OpenXmlWordprocessing.Paragraph paragraph)
                    {
                        var paragraphText = paragraph.InnerText?.Trim();
                        if (!string.IsNullOrEmpty(paragraphText))
                        {
                            extractedText.AppendLine(paragraphText);
                            
                            // Detectar si es un encabezado basándose en el estilo
                            var isHeading = IsHeading(paragraph);
                            var level = GetHeadingLevel(paragraph);
                            
                            sections.Add(new DocumentSection
                            {
                                Title = isHeading ? paragraphText : $"Párrafo {sectionIndex}",
                                Content = paragraphText,
                                Level = level,
                                Type = SectionType.Paragraph,
                                Position = sectionIndex++
                            });
                        }
                    }
                    else if (element is OpenXmlWordprocessing.Table table)
                    {
                        var tableText = ExtractTableText(table);
                        if (!string.IsNullOrEmpty(tableText))
                        {
                            extractedText.AppendLine(tableText);
                            
                            sections.Add(new DocumentSection
                            {
                                Title = $"Tabla {sectionIndex}",
                                Content = tableText,
                                Level = 2,
                                Type = SectionType.Paragraph,
                                Position = sectionIndex++
                            });
                        }
                    }
                }

                var finalText = extractedText.ToString().Trim();
                
                _logger.LogInformation($"Extraído texto de documento Word: {finalText.Length} caracteres, {sections.Count} secciones");

                return Task.FromResult(new DocumentContent
                {
                    RawText = finalText,
                    Sections = sections,
                    DocumentType = "Word",
                    Metadata = new Dictionary<string, object>
                    {
                        ["FileName"] = fileName,
                        ["CharacterCount"] = finalText.Length,
                        ["SectionCount"] = sections.Count,
                        ["ExtractedAt"] = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer texto del documento Word: {FileName}", fileName);
                
                return Task.FromResult(new DocumentContent
                {
                    RawText = "",
                    Sections = new List<DocumentSection>(),
                    DocumentType = "Word",
                    Metadata = new Dictionary<string, object>
                    {
                        ["FileName"] = fileName,
                        ["Error"] = ex.Message,
                        ["ExtractedAt"] = DateTime.UtcNow
                    }
                });
            }
        }

        private bool IsHeading(OpenXmlWordprocessing.Paragraph paragraph)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
            return !string.IsNullOrEmpty(styleId) && 
                   (styleId.StartsWith("Heading") || styleId.StartsWith("Title") || 
                    styleId.Contains("Head") || styleId.Contains("Titulo"));
        }

        private int GetHeadingLevel(OpenXmlWordprocessing.Paragraph paragraph)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
            
            if (string.IsNullOrEmpty(styleId))
                return 3;
                
            // Extraer nivel de encabezado si está presente
            if (styleId.StartsWith("Heading"))
            {
                var levelText = styleId.Replace("Heading", "").Trim();
                if (int.TryParse(levelText, out int level))
                    return Math.Max(1, Math.Min(level, 6));
            }
            
            return IsHeading(paragraph) ? 1 : 3;
        }

        private string ExtractTableText(OpenXmlWordprocessing.Table table)
        {
            var tableText = new StringBuilder();
            
            foreach (var row in table.Elements<OpenXmlWordprocessing.TableRow>())
            {
                var rowData = new List<string>();
                
                foreach (var cell in row.Elements<OpenXmlWordprocessing.TableCell>())
                {
                    var cellText = cell.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(cellText))
                        rowData.Add(cellText);
                }
                
                if (rowData.Any())
                    tableText.AppendLine(string.Join(" | ", rowData));
            }
            
            return tableText.ToString().Trim();
        }

        private Task<DocumentContent> ExtractFromPowerPointAsync(Stream stream, string fileName)
        {
            _logger.LogInformation("Starting PowerPoint document extraction for: {FileName}", fileName);
            
            try
            {
                // Asegurar que el stream esté al inicio
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                    _logger.LogInformation("Stream reset to position 0 for PowerPoint document");
                }
                else
                {
                    _logger.LogWarning("Stream is not seekable for PowerPoint document: {FileName}", fileName);
                }

                using var presentationDocument = PresentationDocument.Open(stream, false);
                var presentationPart = presentationDocument.PresentationPart;
                
                if (presentationPart?.Presentation == null)
                {
                    _logger.LogWarning("No se pudo encontrar el contenido de la presentación PowerPoint");
                    return Task.FromResult(new DocumentContent
                    {
                        RawText = "",
                        Sections = new List<DocumentSection>(),
                        DocumentType = "PowerPoint",
                        Metadata = new Dictionary<string, object>
                        {
                            ["FileName"] = fileName,
                            ["Error"] = "No se pudo acceder al contenido de la presentación",
                            ["ExtractedAt"] = DateTime.UtcNow
                        }
                    });
                }

                var extractedText = new StringBuilder();
                var sections = new List<DocumentSection>();
                var slideIndex = 1;

                var slideIds = presentationPart.Presentation.SlideIdList?.Cast<OpenXmlPresentation.SlideId>();
                if (slideIds != null)
                {
                    foreach (var slideId in slideIds)
                    {
                        var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                        var slideText = ExtractSlideText(slidePart);
                        
                        if (!string.IsNullOrEmpty(slideText))
                        {
                            extractedText.AppendLine($"=== Diapositiva {slideIndex} ===");
                            extractedText.AppendLine(slideText);
                            extractedText.AppendLine();
                            
                            sections.Add(new DocumentSection
                            {
                                Title = $"Diapositiva {slideIndex}",
                                Content = slideText,
                                Level = 1,
                                Type = SectionType.Paragraph,
                                Position = slideIndex
                            });
                        }
                        
                        slideIndex++;
                    }
                }

                var finalText = extractedText.ToString().Trim();
                
                _logger.LogInformation($"Extraído texto de presentación PowerPoint: {finalText.Length} caracteres, {sections.Count} diapositivas");

                return Task.FromResult(new DocumentContent
                {
                    RawText = finalText,
                    Sections = sections,
                    DocumentType = "PowerPoint",
                    Metadata = new Dictionary<string, object>
                    {
                        ["FileName"] = fileName,
                        ["CharacterCount"] = finalText.Length,
                        ["SlideCount"] = sections.Count,
                        ["ExtractedAt"] = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer texto de la presentación PowerPoint: {FileName}", fileName);
                
                return Task.FromResult(new DocumentContent
                {
                    RawText = "",
                    Sections = new List<DocumentSection>(),
                    DocumentType = "PowerPoint",
                    Metadata = new Dictionary<string, object>
                    {
                        ["FileName"] = fileName,
                        ["Error"] = ex.Message,
                        ["ExtractedAt"] = DateTime.UtcNow
                    }
                });
            }
        }

        private string ExtractSlideText(SlidePart slidePart)
        {
            var textBuilder = new StringBuilder();
            
            if (slidePart?.Slide == null)
                return string.Empty;

            // Extraer todo el texto de la diapositiva usando InnerText
            var slideText = slidePart.Slide.InnerText;
            if (!string.IsNullOrEmpty(slideText))
            {
                // Limpiar el texto y dividir en líneas
                var lines = slideText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var cleanLine = line.Trim();
                    if (!string.IsNullOrEmpty(cleanLine))
                    {
                        textBuilder.AppendLine(cleanLine);
                    }
                }
            }

            return textBuilder.ToString().Trim();
        }

        private string ExtractTableTextFromSlide(object table)
        {
            // Para simplificar, por ahora extraemos el texto usando InnerText
            // En una implementación más avanzada se podría parsear la estructura específica
            if (table != null)
            {
                return table.ToString() ?? string.Empty;
            }
            return string.Empty;
        }
    }
}