using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

/// <summary>
/// Qdrant-specific semantic search using Guid keys.
/// </summary>
/// <remarks>
/// This is a convenience class that binds the vector collection to Qdrant-specific types.
/// </remarks>
public class SemanticSearch(
    VectorStoreCollection<Guid, IngestedChunk> vectorCollection)
{
    public async Task<IReadOnlyList<IngestedChunk>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        var nearest = vectorCollection.SearchAsync(text, maxResults, new VectorSearchOptions<IngestedChunk>
        {
            Filter = documentIdFilter is { Length: > 0 } ? record => record.DocumentId == documentIdFilter : null,
        });

        return await nearest.Select(result => result.Record).ToListAsync();
    }

    /// <summary>
    /// Retrieves all chunks for a specific document, ordered by page number.
    /// Useful for generating comprehensive summaries of entire documents.
    /// </summary>
    public async Task<IReadOnlyList<IngestedChunk>> GetDocumentChunksAsync(string documentId)
    {
        var chunks = await vectorCollection
            .GetAsync(record => record.DocumentId == documentId, top: int.MaxValue)
            .ToListAsync();

        return chunks.OrderBy(c => c.PageNumber).ToList();
    }
}