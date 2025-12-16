using AI.Workshop.Common;
using AI.Workshop.Common.Toon;
using AI.Workshop.VectorStore.Ingestion;
using QdrantBased = AI.Workshop.VectorStore.Ingestion.Qdrant;

namespace AI.Workshop.ChatApp.Web.Services;

/// <summary>
/// Service for managing RAG (Retrieval-Augmented Generation) operations
/// </summary>
public class RagService
{
    private readonly SemanticSearch? _sqliteSearch;
    private readonly QdrantBased.SemanticSearch? _qdrantSearch;
    private readonly IWebHostEnvironment _environment;
    private readonly ChatSettingsService _settings;
    private readonly ILogger<RagService> _logger;

    /// <summary>
    /// Last TOON comparison stats (null if TOON not used or no search performed)
    /// </summary>
    public FormatComparison? LastToonComparison { get; private set; }

    // Constructor for SQLite-based SemanticSearch
    public RagService(
        SemanticSearch semanticSearch,
        IWebHostEnvironment environment,
        ChatSettingsService settings,
        ILogger<RagService> logger)
    {
        _sqliteSearch = semanticSearch;
        _environment = environment;
        _settings = settings;
        _logger = logger;
    }

    // Constructor for Qdrant-based SemanticSearch
    public RagService(
        QdrantBased.SemanticSearch semanticSearch,
        IWebHostEnvironment environment,
        ChatSettingsService settings,
        ILogger<RagService> logger)
    {
        _qdrantSearch = semanticSearch;
        _environment = environment;
        _settings = settings;
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
        var dataPath = Path.Combine(_environment.WebRootPath, "Data");
        if (!Directory.Exists(dataPath))
            return Task.FromResult<IEnumerable<string>>(["No documents found. Data directory does not exist."]);

        var files = Directory.GetFiles(dataPath, "*.pdf")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>();

        return Task.FromResult(files.Any()
            ? files
            : (IEnumerable<string>)["No PDF documents found in the Data directory."]);
    }
}
