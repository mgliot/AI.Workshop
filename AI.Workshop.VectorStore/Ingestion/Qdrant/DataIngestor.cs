using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

/// <summary>
/// Qdrant-specific data ingestor using Guid keys.
/// </summary>
/// <remarks>
/// This is a convenience class that binds the generic <see cref="DataIngestor{TKey, TDocument, TChunk}"/>
/// to Qdrant-specific types (Guid keys, DotProductSimilarity).
/// </remarks>
public class DataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<Guid, IngestedChunk> chunksCollection,
    VectorStoreCollection<Guid, IngestedDocument> documentsCollection) 
    : DataIngestor<Guid, IngestedDocument, IngestedChunk>(embeddingGenerator, chunksCollection, documentsCollection)
{
}

