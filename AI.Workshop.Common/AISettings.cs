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
    /// Gets the Ollama URI as a Uri object
    /// </summary>
    public Uri GetOllamaUri() => new(OllamaUri);
}
