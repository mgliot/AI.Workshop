using AI.Workshop.VectorStore;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OllamaSharp;

namespace AI.Workshop.ConsoleChat.RAG;

internal class InMemoryVectorStoreSearch
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private InMemoryCollection<int, VectorModel> _cloudServicesStore;

    public InMemoryVectorStoreSearch()
    {
        var ollamaUri = new Uri("http://localhost:11434/");
        var embeddingModel = "all-minilm";

        // OllamaApiClient implements IEmbeddingGenerator
        _generator = new OllamaApiClient(ollamaUri, embeddingModel);
    }

    internal async Task GenerateVectorsAsync()
    {
        var vectorStore = new InMemoryVectorStore();
        _cloudServicesStore = vectorStore.GetCollection<int, VectorModel>("cloudServices");
        await _cloudServicesStore.EnsureCollectionExistsAsync();

        foreach (var service in SampleData.CloudServices)
        {
            service.Vector = await _generator.GenerateVectorAsync(service.Description);
            await _cloudServicesStore.UpsertAsync(service);
        }
    }

    public async Task SearchAsync(string text, int numberOfResults = 1)
    {
        var queryEmbedding = await _generator.GenerateVectorAsync(text);

        var results = _cloudServicesStore.SearchAsync(queryEmbedding, top: numberOfResults);

        await foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Vector match score: {result.Score}");
        }
    }
}
