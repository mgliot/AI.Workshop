using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System.ComponentModel;

namespace AI.Workshop.VectorStore;

public class InMemoryStore(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    private static InMemoryCollection<int, VectorModel> _inMemoryVectorStore;

    public async Task IngestDataAsync()
    {
        var vectorStore = new InMemoryVectorStore();
        _inMemoryVectorStore = vectorStore.GetCollection<int, VectorModel>("cloudServices");
        await _inMemoryVectorStore.EnsureCollectionExistsAsync();

        foreach (var service in SampleData.CloudServices)
        {
            service.Vector = await embeddingGenerator.GenerateVectorAsync(service.Description);
            await _inMemoryVectorStore.UpsertAsync(service);
        }
    }

    public async IAsyncEnumerable<VectorSearchResult<VectorModel>> SearchAsync(string text, int numberOfResults = 1)
    {
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(text);

        var results = _inMemoryVectorStore.SearchAsync(queryEmbedding, top: numberOfResults);

        Console.ForegroundColor = ConsoleColor.Green;

        await foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Vector match score: {result.Score}");
            yield return result;
        }

        Console.ResetColor();
    }

    [Description("Searches for information about services using a phrase or keyword")]
    public async Task<IEnumerable<VectorModel>> SearchToolAsync(
    [Description("The phrase to search for.")] string searchPhrase,
    [Description("If possible, specify number of results. If not provided or empty, the search returns the first result only.")] int numberOfResults = 1)
    {
        var nearest = SearchAsync(searchPhrase, numberOfResults);
        return await nearest.Select(result => result.Record).ToListAsync();
    }
}
