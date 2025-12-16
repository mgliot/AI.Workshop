using AI.Workshop.Common;
using AI.Workshop.VectorStore;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OllamaSharp;

namespace AI.Workshop.ConsoleApps.AgentChat;

internal class InMemoryVectorStoreSearch : IDisposable
{
    private readonly OllamaApiClient _ollamaClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private InMemoryCollection<int, VectorModel>? _cloudServicesStore;
    private bool _disposed;

    public InMemoryVectorStoreSearch()
    {
        var ollamaUri = new Uri(AIConstants.DefaultOllamaUri);
        var embeddingModel = AIConstants.DefaultEmbeddingModel;

        // OllamaApiClient implements IEmbeddingGenerator
        _ollamaClient = new OllamaApiClient(ollamaUri, embeddingModel);
        _generator = _ollamaClient;
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
        if (_cloudServicesStore is null)
        {
            throw new InvalidOperationException("Vector store not initialized. Call GenerateVectorsAsync first.");
        }

        var queryEmbedding = await _generator.GenerateVectorAsync(text);

        var results = _cloudServicesStore.SearchAsync(queryEmbedding, top: numberOfResults);

        await foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Vector match score: {result.Score}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _ollamaClient.Dispose();
        _disposed = true;
    }
}
