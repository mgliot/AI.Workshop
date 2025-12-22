namespace AI.Workshop.Common;

/// <summary>
/// Shared constants for AI configuration across the workshop projects
/// </summary>
public static class AIConstants
{
    /// <summary>
    /// Default Ollama endpoint URI
    /// </summary>
    public const string DefaultOllamaUri = "http://localhost:11434/";

    /// <summary>
    /// Default chat model name
    /// </summary>
    public const string DefaultChatModel = "llama3.2";

    /// <summary>
    /// Default embedding model name
    /// </summary>
    public const string DefaultEmbeddingModel = "all-minilm";

    /// <summary>
    /// Vector dimensions for all-minilm embedding model
    /// </summary>
    public const int VectorDimensions = 384;

    /// <summary>
    /// Default Qdrant host
    /// </summary>
    public const string DefaultQdrantHost = "localhost";

    /// <summary>
    /// Default Qdrant gRPC port (not HTTP port 6333)
    /// </summary>
    public const int DefaultQdrantGrpcPort = 6334;

    /// <summary>
    /// Default Qdrant API key (empty = no authentication)
    /// </summary>
    public const string DefaultQdrantApiKey = "";

    /// <summary>
    /// Default GitHub repository owner for markdown ingestion
    /// </summary>
    public const string DefaultGitHubOwner = "";

    /// <summary>
    /// Default GitHub repository name for markdown ingestion
    /// </summary>
    public const string DefaultGitHubRepo = "";

    /// <summary>
    /// Default GitHub repository path for markdown files
    /// </summary>
    public const string DefaultGitHubPath = "";

    /// <summary>
    /// Default GitHub branch for markdown file links
    /// </summary>
    public const string DefaultGitHubBranch = "main";
}
