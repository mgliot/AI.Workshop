using AI.Workshop.Common;
using Microsoft.Extensions.AI;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Markdown;

namespace AI.Workshop.ChatApp.Web.Services;

/// <summary>
/// Orchestrates the creation of comprehensive study guides by processing documents section by section.
/// This service manages the iterative process of calling the LLM for each section and aggregating results.
/// </summary>
public class StudyGuideOrchestrator
{
    private readonly IChatClient _chatClient;
    private readonly RagService _ragService;
    private readonly ILogger<StudyGuideOrchestrator> _logger;
    private readonly IWebHostEnvironment _environment;
    
    private const int PagesPerSection = 3;

    static StudyGuideOrchestrator()
    {
        // Configure QuestPDF license (Community license for open source/small business)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public StudyGuideOrchestrator(
        IChatClient chatClient,
        RagService ragService,
        IWebHostEnvironment environment,
        ILogger<StudyGuideOrchestrator> logger)
    {
        _chatClient = chatClient;
        _ragService = ragService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Result of study guide generation including the content and file path
    /// </summary>
    public record StudyGuideResult(string MarkdownContent, string PdfFileName, string Message);

    /// <summary>
    /// Generates a complete study guide for a document by processing it section by section.
    /// </summary>
    /// <param name="filename">The PDF filename to process</param>
    /// <param name="progressCallback">Optional callback to report progress</param>
    /// <returns>The study guide result with content and PDF file path</returns>
    public async Task<StudyGuideResult> GenerateCompleteStudyGuideAsync(
        string filename,
        Action<string>? progressCallback = null)
    {
        var documents = await _ragService.ListDocumentsAsync();
        if (!documents.Contains(filename))
        {
            return new StudyGuideResult(
                "", 
                "", 
                $"Error: Documento '{filename}' no encontrado. Documentos disponibles: {string.Join(", ", documents)}");
        }

        // Get all chunks to determine page range
        var allContent = await _ragService.GetDocumentSectionAsync(filename, 1, 1000);
        var pageMatches = System.Text.RegularExpressions.Regex.Matches(allContent, @"--- Page (\d+) ---");
        var maxPage = pageMatches.Count > 0 
            ? pageMatches.Cast<System.Text.RegularExpressions.Match>().Max(m => int.Parse(m.Groups[1].Value))
            : 1;

        _logger.LogInformation("Processing document {Filename} with {MaxPage} pages", filename, maxPage);
        progressCallback?.Invoke($"📄 Procesando documento: {filename} ({maxPage} páginas)");

        // Load the section prompt from prompty file
        var sectionSystemPrompt = PromptyHelper.GetSystemPrompt("StudyGuideSection");

        var fullGuide = new StringBuilder();
        fullGuide.AppendLine($"# 📚 GUÍA DE ESTUDIO: {Path.GetFileNameWithoutExtension(filename)}");
        fullGuide.AppendLine();
        fullGuide.AppendLine($"*Generado automáticamente - {DateTime.Now:dd/MM/yyyy HH:mm}*");
        fullGuide.AppendLine();
        fullGuide.AppendLine("---");
        fullGuide.AppendLine();

        // Process document in sections
        var currentPage = 1;
        var sectionNumber = 1;
        
        while (currentPage <= maxPage)
        {
            var endPage = Math.Min(currentPage + PagesPerSection - 1, maxPage);
            
            progressCallback?.Invoke($"📖 Procesando páginas {currentPage}-{endPage} de {maxPage}...");
            _logger.LogInformation("Processing pages {Start}-{End}", currentPage, endPage);

            // Get section content
            var sectionContent = await _ragService.GetDocumentSectionAsync(filename, currentPage, endPage);
            
            if (string.IsNullOrWhiteSpace(sectionContent) || sectionContent.Contains("No content found"))
            {
                currentPage = endPage + 1;
                continue;
            }

            // Build messages for LLM call
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, sectionSystemPrompt),
                new(ChatRole.User, $"Analiza el siguiente contenido y crea un resumen detallado para estudio:\n\n{sectionContent}")
            };

            try
            {
                var response = await _chatClient.GetResponseAsync(messages);
                var summary = response.Text ?? "";

                if (!string.IsNullOrWhiteSpace(summary))
                {
                    fullGuide.AppendLine($"## 📖 Sección {sectionNumber}: Páginas {currentPage}-{endPage}");
                    fullGuide.AppendLine();
                    fullGuide.AppendLine(summary);
                    fullGuide.AppendLine();
                    fullGuide.AppendLine("---");
                    fullGuide.AppendLine();
                    
                    sectionNumber++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pages {Start}-{End}", currentPage, endPage);
                fullGuide.AppendLine($"*Error procesando páginas {currentPage}-{endPage}: {ex.Message}*");
                fullGuide.AppendLine();
            }

            currentPage = endPage + 1;
        }

        // Add final sections
        fullGuide.AppendLine("## 🔑 Información del Documento");
        fullGuide.AppendLine();
        fullGuide.AppendLine($"- **Documento original:** {filename}");
        fullGuide.AppendLine($"- **Total de páginas:** {maxPage}");
        fullGuide.AppendLine($"- **Secciones procesadas:** {sectionNumber - 1}");
        fullGuide.AppendLine($"- **Fecha de generación:** {DateTime.Now:dd/MM/yyyy HH:mm}");
        fullGuide.AppendLine();

        progressCallback?.Invoke("📝 Generando PDF...");

        // Generate PDF
        var markdownContent = fullGuide.ToString();
        var pdfFileName = $"guia_estudio_{Path.GetFileNameWithoutExtension(filename)}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var pdfPath = Path.Combine(_environment.WebRootPath, "downloads", pdfFileName);

        // Ensure downloads directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(pdfPath)!);

        try
        {
            // Generate PDF using QuestPDF with Markdown support
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    
                    page.Content().Markdown(markdownContent);
                });
            }).GeneratePdf(pdfPath);
            
            _logger.LogInformation("PDF generated at {Path}", pdfPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF");
            // Fall back to just returning markdown
            return new StudyGuideResult(
                markdownContent,
                "",
                $"⚠️ Guía generada pero error al crear PDF: {ex.Message}. Contenido disponible en formato Markdown.");
        }

        progressCallback?.Invoke("✅ Guía de estudio completada!");
        
        return new StudyGuideResult(
            markdownContent,
            pdfFileName,
            $"✅ **Guía de estudio generada exitosamente**\n\n" +
            $"📄 **Documento:** {filename}\n" +
            $"📊 **Páginas procesadas:** {maxPage}\n" +
            $"📖 **Secciones creadas:** {sectionNumber - 1}\n\n" +
            $"[📥 **Descargar PDF**](/downloads/{pdfFileName})");
    }
}
