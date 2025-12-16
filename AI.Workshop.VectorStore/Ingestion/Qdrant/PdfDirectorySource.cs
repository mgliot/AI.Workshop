namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

/// <summary>
/// Qdrant-specific PDF directory source using Guid keys.
/// </summary>
/// <remarks>
/// This is a convenience class that binds the generic <see cref="PDFDirectorySource{TKey, TDocument, TChunk}"/>
/// to Qdrant-specific types (Guid keys).
/// </remarks>
public class PDFDirectorySource(string sourceDirectory) : PDFDirectorySource<Guid, IngestedDocument, IngestedChunk>(sourceDirectory)
{
    public override IngestedDocument CreateDocument(string sourceFileId, string sourceFileVersion)
    {
        return new IngestedDocument
        {
            Key = Guid.NewGuid(),
            SourceId = SourceId,
            DocumentId = sourceFileId,
            DocumentVersion = sourceFileVersion
        };
    }

    public override IngestedChunk CreateChunk(string documentId, int pageNumber, string text)
    {
        return new IngestedChunk
        {
            Key = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = pageNumber,
            Text = text
        };
    }
}
