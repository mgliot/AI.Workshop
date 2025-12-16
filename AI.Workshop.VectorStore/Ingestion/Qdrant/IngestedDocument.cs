using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

/// <summary>
/// Qdrant-specific ingested document using Guid keys and DotProductSimilarity.
/// </summary>
/// <remarks>
/// This class exists separately from <see cref="Ingestion.IngestedDocument"/> because the
/// <see cref="VectorStoreVectorAttribute"/> requires compile-time constants for the distance function.
/// </remarks>
public class IngestedDocument : IngestedDocument<Guid>
{
    [VectorStoreVector(VectorDimensions, DistanceFunction = DistanceFunction.DotProductSimilarity)]
    public override ReadOnlyMemory<float> Vector { get; set; } = new ReadOnlyMemory<float>([0, 0]);
}