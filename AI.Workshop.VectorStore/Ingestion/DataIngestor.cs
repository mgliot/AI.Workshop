using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion;

public class DataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<string, IngestedChunk> chunksCollection,
    VectorStoreCollection<string, IngestedDocument> documentsCollection,
    ILogger<DataIngestor>? logger = null) 
    : DataIngestor<string, IngestedDocument, IngestedChunk>(embeddingGenerator, chunksCollection, documentsCollection, logger)
{
}

public abstract class DataIngestor<TKey, TDocument, TChunk>(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<TKey, TChunk> chunksCollection,
    VectorStoreCollection<TKey, TDocument> documentsCollection,
    ILogger? logger = null) 
    where TKey : notnull
    where TDocument : class, IIngestedDocument<TKey>
    where TChunk : class, IIngestedChunk<TKey>
{
    private readonly ILogger _logger = logger ?? NullLogger.Instance;
    public VectorStoreCollection<TKey, TChunk> Chunks => chunksCollection;

    public async Task IngestDataAsync(IIngestionSource<TDocument, TChunk> source)
    {
        await chunksCollection.EnsureCollectionExistsAsync();
        await documentsCollection.EnsureCollectionExistsAsync();

        var sourceId = source.SourceId;
        var documentsForSource = await documentsCollection.GetAsync(doc => doc.SourceId == sourceId, top: int.MaxValue).ToListAsync();

        var deletedDocuments = await source.GetDeletedDocumentsAsync(documentsForSource);
        foreach (var deletedDocument in deletedDocuments)
        {
            _logger.LogInformation("Removing ingested data for {DocumentId}", deletedDocument.DocumentId);
            await DeleteChunksForDocumentAsync(deletedDocument);
            await documentsCollection.DeleteAsync(deletedDocument.Key);
        }

        var modifiedDocuments = await source.GetNewOrModifiedDocumentsAsync(documentsForSource);
        foreach (var modifiedDocument in modifiedDocuments)
        {
            _logger.LogInformation("Processing {DocumentId}", modifiedDocument.DocumentId);
            await DeleteChunksForDocumentAsync(modifiedDocument);

            await documentsCollection.UpsertAsync(modifiedDocument);

            var newRecords = await source.CreateChunksForDocumentAsync(embeddingGenerator, modifiedDocument);
            await chunksCollection.UpsertAsync(newRecords);
        }

        _logger.LogInformation("Ingestion is up-to-date");

        async Task DeleteChunksForDocumentAsync(TDocument document)
        {
            var documentId = document.DocumentId;
            var chunksToDelete = await chunksCollection.GetAsync(record => record.DocumentId == documentId, int.MaxValue).ToListAsync();
            if (chunksToDelete.Any())
            {
                await chunksCollection.DeleteAsync(chunksToDelete.Select(r => r.Key));
            }
        }
    }
}
