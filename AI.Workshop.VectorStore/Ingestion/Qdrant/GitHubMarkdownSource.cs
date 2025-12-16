using Octokit;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

/// <summary>
/// Qdrant-specific GitHub markdown source using Guid keys.
/// </summary>
/// <remarks>
/// This is a convenience class that binds the generic <see cref="GitHubMarkdownSource{TKey, TDocument, TChunk}"/>
/// to Qdrant-specific types (Guid keys).
/// </remarks>
public class GitHubMarkdownSource(GitHubClient gitHubClient, string owner, string repo, string path) 
    : GitHubMarkdownSource<Guid, IngestedDocument, IngestedChunk>(gitHubClient, owner, repo, path)
{
    public override IngestedDocument CreateDocument(string documentPath, string sourceId, string documentVersion)
    {
        return new IngestedDocument
        {
            Key = Guid.NewGuid(),
            DocumentId = documentPath,
            SourceId = SourceId,
            DocumentVersion = documentVersion
        };
    }

    public override IngestedChunk CreateChunk(string documentId, string text)
    {
        return new IngestedChunk
        {
            Key = Guid.NewGuid(),
            DocumentId = documentId,
            Text = text
        };
    }
}
