using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

/// <summary>
/// Qdrant-specific ingested chunk using Guid keys and DotProductSimilarity.
/// </summary>
/// <remarks>
/// This class exists separately from <see cref="Ingestion.IngestedChunk"/> because the
/// <see cref="VectorStoreVectorAttribute"/> requires compile-time constants for the distance function.
/// Qdrant performs better with DotProductSimilarity, while SQLite-Vec uses CosineDistance.
/// </remarks>
public class IngestedChunk : IngestedChunk<Guid>
{
    [VectorStoreVector(VectorDimensions, DistanceFunction = DistanceFunction.DotProductSimilarity)]
    public override string? Vector => base.Vector;
}
