using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore;

public class VectorModel
{
    /// <summary>
    /// Vector dimensions for the all-minilm embedding model.
    /// Must match AI.Workshop.Common.AIConstants.VectorDimensions
    /// </summary>
    private const int VectorDimensions = 384;

    [VectorStoreKey]
    public int Key { get; set; }

    [VectorStoreData]
    public string Name { get; set; } = string.Empty;

    [VectorStoreData]
    public string Description { get; set; } = string.Empty;

    [VectorStoreVector(Dimensions: VectorDimensions, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
