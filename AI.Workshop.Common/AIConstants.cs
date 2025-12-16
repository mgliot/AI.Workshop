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
}
