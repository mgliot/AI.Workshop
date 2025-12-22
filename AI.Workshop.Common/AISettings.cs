namespace AI.Workshop.Common;

/// <summary>
/// Configuration settings for AI services.
/// Can be bound from appsettings.json under the "AI" section.
/// </summary>
public class AISettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "AI";

    /// <summary>
    /// Ollama endpoint URI
    /// </summary>
    public string OllamaUri { get; set; } = AIConstants.DefaultOllamaUri;

    /// <summary>
    /// Chat model name (e.g., "llama3.2", "mistral", "phi3")
    /// </summary>
    public string ChatModel { get; set; } = AIConstants.DefaultChatModel;

    /// <summary>
    /// Embedding model name (e.g., "all-minilm", "nomic-embed-text")
    /// </summary>
    public string EmbeddingModel { get; set; } = AIConstants.DefaultEmbeddingModel;

    /// <summary>
    /// Vector dimensions for the embedding model
    /// </summary>
    public int VectorDimensions { get; set; } = AIConstants.VectorDimensions;

    /// <summary>
    /// Qdrant vector database host
    /// </summary>
    public string QdrantHost { get; set; } = AIConstants.DefaultQdrantHost;

    /// <summary>
    /// Qdrant gRPC port (default: 6334). Note: This must be the gRPC port, not the HTTP port (6333).
    /// </summary>
    public int QdrantGrpcPort { get; set; } = AIConstants.DefaultQdrantGrpcPort;

    /// <summary>
    /// Qdrant API key for authentication. Leave empty for no authentication.
    /// </summary>
    public string QdrantApiKey { get; set; } = AIConstants.DefaultQdrantApiKey;

    /// <summary>
    /// GitHub repository owner for markdown ingestion (e.g., "microsoft")
    /// </summary>
    public string GitHubOwner { get; set; } = AIConstants.DefaultGitHubOwner;

    /// <summary>
    /// GitHub repository name for markdown ingestion (e.g., "semantic-kernel")
    /// </summary>
    public string GitHubRepo { get; set; } = AIConstants.DefaultGitHubRepo;

    /// <summary>
    /// GitHub repository path for markdown files (e.g., "docs")
    /// </summary>
    public string GitHubPath { get; set; } = AIConstants.DefaultGitHubPath;

    /// <summary>
    /// GitHub branch for markdown file links (e.g., "main", "master")
    /// </summary>
    public string GitHubBranch { get; set; } = AIConstants.DefaultGitHubBranch;

    /// <summary>
    /// Path to data files (PDFs, documents). Can be absolute or relative to application root.
    /// </summary>
    public string DataPath { get; set; } = AIConstants.DefaultDataPath;

    /// <summary>
    /// Gets the Ollama URI as a Uri object
    /// </summary>
    public Uri GetOllamaUri() => new(OllamaUri);

    /// <summary>
    /// Checks if GitHub markdown ingestion is configured
    /// </summary>
    public bool IsGitHubConfigured => !string.IsNullOrWhiteSpace(GitHubOwner) && !string.IsNullOrWhiteSpace(GitHubRepo);

    /// <summary>
    /// Gets the GitHub URL for viewing a markdown file
    /// </summary>
    public string GetGitHubViewerUrl(string filePath)
    {
        if (!IsGitHubConfigured)
            return string.Empty;

        return $"https://github.com/{GitHubOwner}/{GitHubRepo}/blob/{GitHubBranch}/{Uri.EscapeDataString(filePath)}";
    }

    /// <summary>
    /// Gets the resolved data path. If DataPath is relative, resolves it against the provided base path.
    /// </summary>
    /// <param name="basePath">Base path to resolve relative paths against (typically ContentRootPath)</param>
    public string GetResolvedDataPath(string basePath)
    {
        if (Path.IsPathRooted(DataPath))
            return DataPath;

        return Path.Combine(basePath, DataPath);
    }
}
