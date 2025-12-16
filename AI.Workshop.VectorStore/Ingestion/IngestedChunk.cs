using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion;

public class IngestedChunk : IngestedChunk<string>
{
    [VectorStoreVector(VectorDimensions, DistanceFunction = DistanceFunction.CosineDistance)]
    public override string? Vector => base.Vector;
}

public abstract class IngestedChunk<TKey> : IIngestedChunk<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Vector dimensions for the all-minilm embedding model.
    /// Must match AI.Workshop.Common.AIConstants.VectorDimensions
    /// </summary>
    protected const int VectorDimensions = 384;

    [VectorStoreKey]
    public required TKey Key { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string DocumentId { get; set; }

    [VectorStoreData]
    public int PageNumber { get; set; }

    [VectorStoreData]
    public required string Text { get; set; }

    public virtual string? Vector => Text;
}