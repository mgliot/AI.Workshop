namespace AI.Workshop.VectorStore.Ingestion;

/// <summary>
/// Factory for creating ingestion components based on the vector store type.
/// </summary>
/// <remarks>
/// The VectorStore library provides two sets of ingestion classes:
/// <list type="bullet">
/// <item>
/// <term>Base namespace (Ingestion)</term>
/// <description>Uses string keys and CosineDistance. Best for SQLite-Vec and similar stores.</description>
/// </item>
/// <item>
/// <term>Qdrant namespace (Ingestion.Qdrant)</term>
/// <description>Uses Guid keys and DotProductSimilarity. Optimized for Qdrant vector database.</description>
/// </item>
/// </list>
/// 
/// The separation exists because the <see cref="Microsoft.Extensions.VectorData.VectorStoreVectorAttribute"/>
/// requires compile-time constants for the distance function, so we cannot make this configurable at runtime.
/// </remarks>
public static class IngestionFactory
{
    /// <summary>
    /// Supported vector store types
    /// </summary>
    public enum VectorStoreType
    {
        /// <summary>SQLite-Vec or similar string-key stores (CosineDistance)</summary>
        SqliteVec,
        /// <summary>Qdrant vector database (Guid keys, DotProductSimilarity)</summary>
        Qdrant
    }

    /// <summary>
    /// Gets information about the ingestion classes for a specific vector store type.
    /// </summary>
    public static VectorStoreInfo GetInfo(VectorStoreType storeType) => storeType switch
    {
        VectorStoreType.SqliteVec => new VectorStoreInfo(
            KeyType: typeof(string),
            ChunkType: typeof(IngestedChunk),
            DocumentType: typeof(IngestedDocument),
            DataIngestorType: typeof(DataIngestor),
            SemanticSearchType: typeof(SemanticSearch),
            PdfSourceType: typeof(PDFDirectorySource),
            GitHubSourceType: typeof(GitHubMarkdownSource),
            DistanceFunction: "CosineDistance"),
            
        VectorStoreType.Qdrant => new VectorStoreInfo(
            KeyType: typeof(Guid),
            ChunkType: typeof(Qdrant.IngestedChunk),
            DocumentType: typeof(Qdrant.IngestedDocument),
            DataIngestorType: typeof(Qdrant.DataIngestor),
            SemanticSearchType: typeof(Qdrant.SemanticSearch),
            PdfSourceType: typeof(Qdrant.PDFDirectorySource),
            GitHubSourceType: typeof(Qdrant.GitHubMarkdownSource),
            DistanceFunction: "DotProductSimilarity"),
            
        _ => throw new ArgumentOutOfRangeException(nameof(storeType))
    };
}

/// <summary>
/// Information about the types used for a specific vector store implementation.
/// </summary>
public record VectorStoreInfo(
    Type KeyType,
    Type ChunkType,
    Type DocumentType,
    Type DataIngestorType,
    Type SemanticSearchType,
    Type PdfSourceType,
    Type GitHubSourceType,
    string DistanceFunction);
