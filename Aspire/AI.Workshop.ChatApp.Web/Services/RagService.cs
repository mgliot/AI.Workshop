using AI.Workshop.Common;
using AI.Workshop.Common.Toon;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.Options;
using QdrantBased = AI.Workshop.VectorStore.Ingestion.Qdrant;

namespace AI.Workshop.ChatApp.Web.Services;

/// <summary>
/// Service for managing RAG (Retrieval-Augmented Generation) operations
/// </summary>
public class RagService
{
    private readonly SemanticSearch? _sqliteSearch;
    private readonly QdrantBased.SemanticSearch? _qdrantSearch;
    private readonly ChatSettingsService _settings;
    private readonly AISettings _aiSettings;
    private readonly ILogger<RagService> _logger;

    /// <summary>
    /// Last TOON comparison stats (null if TOON not used or no search performed)
    /// </summary>
    public FormatComparison? LastToonComparison { get; private set; }

    // Constructor for SQLite-based SemanticSearch
    public RagService(
        SemanticSearch semanticSearch,
        ChatSettingsService settings,
        IOptions<AISettings> aiSettings,
        ILogger<RagService> logger)
    {
        _sqliteSearch = semanticSearch;
        _settings = settings;
        _aiSettings = aiSettings.Value;
        _logger = logger;
    }

    // Constructor for Qdrant-based SemanticSearch
    public RagService(
        QdrantBased.SemanticSearch semanticSearch,
        ChatSettingsService settings,
        IOptions<AISettings> aiSettings,
        ILogger<RagService> logger)
    {
        _qdrantSearch = semanticSearch;
        _settings = settings;
        _aiSettings = aiSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Clears the last TOON comparison
    /// </summary>
    public void ClearLastComparison() => LastToonComparison = null;

    /// <summary>
    /// Searches for information in documents
    /// </summary>
    [System.ComponentModel.Description("Searches for information in documents using a phrase or keyword")]
    public async Task<string> SearchAsync(
        [System.ComponentModel.Description("The phrase to search for")] string searchPhrase,
        [System.ComponentModel.Description("Optional filename to filter results")] string? filenameFilter = null)
    {
        LastToonComparison = null;
        IEnumerable<(string DocumentId, int PageNumber, string Text)> results;

        if (_sqliteSearch != null)
        {
            var chunks = await _sqliteSearch.SearchAsync(searchPhrase, filenameFilter, 5);
            results = chunks.Select(c => (c.DocumentId, c.PageNumber, c.Text));
        }
        else if (_qdrantSearch != null)
        {
            var chunks = await _qdrantSearch.SearchAsync(searchPhrase, filenameFilter, 5);
            results = chunks.Select(c => (c.DocumentId, c.PageNumber, c.Text));
        }
        else
        {
            return "Error: Search service not configured.";
        }

        if (!results.Any())
            return "No results found for the search query.";

        // Always compute comparison for stats
        var comparison = ToonSearchFormatter.Compare(results, r =>
            new SearchResultData(r.DocumentId, r.PageNumber, r.Text));
        
        LastToonComparison = comparison;

        // Use TOON format when enabled, otherwise XML
        if (_settings.ToonEnabled)
        {
            return comparison.ToonFormat;
        }

        return comparison.XmlFormat;
    }

    /// <summary>
    /// Lists all available PDF documents
    /// </summary>
    [System.ComponentModel.Description("Lists all available PDF documents that can be searched or summarized")]
    public Task<IEnumerable<string>> ListDocumentsAsync()
    {
        var dataPath = _aiSettings.GetResolvedDataPath(AppContext.BaseDirectory);
        if (!Directory.Exists(dataPath))
            return Task.FromResult<IEnumerable<string>>([]);

        var files = Directory.GetFiles(dataPath, "*.pdf")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>();

        return Task.FromResult<IEnumerable<string>>(files);
    }

    /// <summary>
    /// Retrieves the full content of a specific PDF document for comprehensive analysis.
    /// Returns all text chunks ordered by page number.
    /// </summary>
    [System.ComponentModel.Description("Retrieves the complete content of a specific PDF document for comprehensive summarization. Use this when you need to create a detailed study guide or exhaustive summary of an entire document.")]
    public async Task<string> GetDocumentContentAsync(
        [System.ComponentModel.Description("The exact filename of the PDF document to retrieve (e.g., '01_Constitution.pdf')")] string filename)
    {
        var chunks = await GetChunksForDocumentAsync(filename);
        if (chunks == null)
            return "Error: Search service not configured.";

        if (!chunks.Any())
            return $"No content found for document '{filename}'. Please verify the filename using ListDocuments.";

        // Group by page and format for comprehensive reading
        var content = new System.Text.StringBuilder();
        content.AppendLine($"=== COMPLETE CONTENT OF: {filename} ===");
        content.AppendLine($"Total chunks: {chunks.Count}");
        content.AppendLine();

        var currentPage = -1;
        foreach (var chunk in chunks)
        {
            if (chunk.PageNumber != currentPage)
            {
                currentPage = chunk.PageNumber;
                content.AppendLine($"\n--- Page {currentPage} ---\n");
            }
            content.AppendLine(chunk.Text);
        }

        return content.ToString();
    }

    /// <summary>
    /// Gets an overview of a document including page count and structure.
    /// Use this first to understand the document before requesting specific sections.
    /// </summary>
    [System.ComponentModel.Description("Gets an overview of a PDF document including total pages, chunk count, and a sample of the first page. Use this FIRST to understand the document structure before processing sections.")]
    public async Task<string> GetDocumentOverviewAsync(
        [System.ComponentModel.Description("The exact filename of the PDF document")] string filename)
    {
        var chunks = await GetChunksForDocumentAsync(filename);
        if (chunks == null)
            return "Error: Search service not configured.";

        if (!chunks.Any())
            return $"No content found for document '{filename}'. Please verify the filename using ListDocuments.";

        var pageNumbers = chunks.Select(c => c.PageNumber).Distinct().OrderBy(p => p).ToList();
        var totalPages = pageNumbers.Count;
        var firstPageContent = string.Join("\n", chunks.Where(c => c.PageNumber == pageNumbers.First()).Select(c => c.Text));

        var overview = new System.Text.StringBuilder();
        overview.AppendLine($"=== DOCUMENT OVERVIEW: {filename} ===");
        overview.AppendLine($"Total Pages: {totalPages}");
        overview.AppendLine($"Total Chunks: {chunks.Count}");
        overview.AppendLine($"Page Range: {pageNumbers.First()} - {pageNumbers.Last()}");
        overview.AppendLine();
        overview.AppendLine("=== FIRST PAGE CONTENT (for structure analysis) ===");
        overview.AppendLine(firstPageContent.Length > 2000 ? firstPageContent[..2000] + "..." : firstPageContent);
        overview.AppendLine();
        overview.AppendLine("INSTRUCTION: Use GetDocumentSection to retrieve content page by page or in small ranges (e.g., pages 1-5, then 6-10, etc.)");

        return overview.ToString();
    }

    /// <summary>
    /// Retrieves content from a specific page range of a document.
    /// Use this to process large documents section by section.
    /// </summary>
    [System.ComponentModel.Description("Retrieves content from a specific page range of a PDF document. Use this to process large documents incrementally, section by section.")]
    public async Task<string> GetDocumentSectionAsync(
        [System.ComponentModel.Description("The exact filename of the PDF document")] string filename,
        [System.ComponentModel.Description("Starting page number (inclusive)")] int startPage,
        [System.ComponentModel.Description("Ending page number (inclusive)")] int endPage)
    {
        var chunks = await GetChunksForDocumentAsync(filename);
        if (chunks == null)
            return "Error: Search service not configured.";

        var sectionChunks = chunks
            .Where(c => c.PageNumber >= startPage && c.PageNumber <= endPage)
            .ToList();

        if (!sectionChunks.Any())
            return $"No content found for pages {startPage}-{endPage} in '{filename}'.";

        var content = new System.Text.StringBuilder();
        content.AppendLine($"=== {filename} - Pages {startPage} to {endPage} ===");
        content.AppendLine($"Chunks in this section: {sectionChunks.Count}");
        content.AppendLine();

        var currentPage = -1;
        foreach (var chunk in sectionChunks)
        {
            if (chunk.PageNumber != currentPage)
            {
                currentPage = chunk.PageNumber;
                content.AppendLine($"\n--- Page {currentPage} ---\n");
            }
            content.AppendLine(chunk.Text);
        }

        return content.ToString();
    }

    /// <summary>
    /// Generates a complete study guide structure for a document.
    /// Returns all content organized by pages with clear section markers.
    /// The LLM should use this content to create the final formatted study guide.
    /// </summary>
    [System.ComponentModel.Description("Generates complete study material for a PDF document. Returns ALL content organized by pages. Use this to create a comprehensive study guide - you will receive the full document content to summarize.")]
    public async Task<string> GetFullStudyMaterialAsync(
        [System.ComponentModel.Description("The exact filename of the PDF document")] string filename)
    {
        var chunks = await GetChunksForDocumentAsync(filename);
        if (chunks == null)
            return "Error: Search service not configured.";

        if (!chunks.Any())
            return $"No content found for document '{filename}'. Please verify the filename using ListDocuments.";

        var pageGroups = chunks.GroupBy(c => c.PageNumber).OrderBy(g => g.Key).ToList();
        var totalPages = pageGroups.Count;

        var content = new System.Text.StringBuilder();
        content.AppendLine($"# STUDY MATERIAL: {filename}");
        content.AppendLine($"## Document Statistics");
        content.AppendLine($"- Total Pages: {totalPages}");
        content.AppendLine($"- Total Text Chunks: {chunks.Count}");
        content.AppendLine();
        content.AppendLine("## INSTRUCTIONS FOR THE AI");
        content.AppendLine("Create a COMPLETE study guide from the content below. Include:");
        content.AppendLine("1. Overview of the document");
        content.AppendLine("2. Summary of EACH section/title found");
        content.AppendLine("3. Key concepts and definitions");
        content.AppendLine("4. Important articles with their content");
        content.AppendLine("5. Exam focus points marked with ⭐");
        content.AppendLine("6. Potential exam questions");
        content.AppendLine();
        content.AppendLine("---");
        content.AppendLine("## DOCUMENT CONTENT BY PAGE");
        content.AppendLine();

        foreach (var pageGroup in pageGroups)
        {
            content.AppendLine($"### PAGE {pageGroup.Key}");
            foreach (var chunk in pageGroup)
            {
                content.AppendLine(chunk.Text);
            }
            content.AppendLine();
        }

        // Limit output to avoid token overflow - take first ~30 pages worth
        var result = content.ToString();
        if (result.Length > 50000)
        {
            result = result.Substring(0, 50000);
            result += $"\n\n[DOCUMENT TRUNCATED - Showing first portion. Total pages: {totalPages}. Request specific page ranges for more content.]";
        }

        return result;
    }

    private async Task<IReadOnlyList<(string DocumentId, int PageNumber, string Text)>?> GetChunksForDocumentAsync(string filename)
    {
        if (_sqliteSearch != null)
        {
            var results = await _sqliteSearch.GetDocumentChunksAsync(filename);
            return results.Select(c => (c.DocumentId, c.PageNumber, c.Text)).ToList();
        }
        else if (_qdrantSearch != null)
        {
            var results = await _qdrantSearch.GetDocumentChunksAsync(filename);
            return results.Select(c => (c.DocumentId, c.PageNumber, c.Text)).ToList();
        }
        return null;
    }
}
