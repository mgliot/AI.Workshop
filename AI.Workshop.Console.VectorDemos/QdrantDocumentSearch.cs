using AI.Workshop.Common;
using AI.Workshop.VectorStore.Ingestion.Qdrant;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using OllamaSharp;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.ComponentModel;
using System.Text;

namespace AI.Workshop.ConsoleApps.VectorDemos;

internal class QdrantDocumentSearch : IDisposable
{
    private readonly OllamaApiClient _chatClient;
    private readonly OllamaApiClient _embeddingClient;
    private readonly IChatClient _chatClientInterface;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly string _qdrantHost;
    private readonly int _qdrantGrpcPort;
    private readonly string _qdrantApiKey;
    private SemanticSearch? _semanticSearch;
    private bool _disposed;

    /// <summary>
    /// Creates a new QdrantDocumentSearch instance.
    /// </summary>
    /// <param name="ollamaUri">Ollama server URI</param>
    /// <param name="chatModel">Chat model name</param>
    /// <param name="embeddingModel">Embedding model name</param>
    /// <param name="qdrantHost">Qdrant server host</param>
    /// <param name="qdrantGrpcPort">Qdrant gRPC port (default: 6334). Must be the gRPC port, not HTTP (6333).</param>
    /// <param name="qdrantApiKey">Qdrant API key for authentication. Leave empty for no authentication.</param>
    public QdrantDocumentSearch(string ollamaUri, string chatModel, string embeddingModel, 
        string qdrantHost = "localhost", int qdrantGrpcPort = 6334, string qdrantApiKey = "")
    {
        // for testing the example, run the docker container with:
        // docker run -p 6333:6333 -p 6334:6334 --name qdrant-db qdrant/qdrant

        var uri = new Uri(ollamaUri);

        _chatClient = new OllamaApiClient(uri, chatModel);
        _chatClientInterface = _chatClient;

        // OllamaApiClient implements IEmbeddingGenerator
        _embeddingClient = new OllamaApiClient(uri, embeddingModel);
        _embeddingGenerator = _embeddingClient;

        _qdrantHost = qdrantHost;
        _qdrantGrpcPort = qdrantGrpcPort;
        _qdrantApiKey = qdrantApiKey;
    }

    private QdrantClient CreateQdrantClient()
    {
        if (string.IsNullOrEmpty(_qdrantApiKey))
        {
            return new QdrantClient(_qdrantHost, _qdrantGrpcPort);
        }
        return new QdrantClient(_qdrantHost, _qdrantGrpcPort, apiKey: _qdrantApiKey);
    }

    internal async Task TestQdrantAsync()
    {
        var client = CreateQdrantClient();

        if (!await client.CollectionExistsAsync("test_collection"))
        {
            await client.CreateCollectionAsync(
                collectionName: "test_collection",
                vectorsConfig: new VectorParams
                {
                    Size = 4,
                    Distance = Distance.Dot
                });
        }

        var operationInfo = await client.UpsertAsync(collectionName: "test_collection", points: new List<PointStruct>
        {
            new()
            {
                Id = 1,
                    Vectors = new float[]
                    {
                        0.05f, 0.61f, 0.76f, 0.74f
                    },
                    Payload = {
                        ["city"] = "Berlin"
                    }
            },
            new()
            {
                Id = 2,
                    Vectors = new float[]
                    {
                        0.19f, 0.81f, 0.75f, 0.11f
                    },
                    Payload = {
                        ["city"] = "London"
                    }
            },
            new()
            {
                Id = 3,
                    Vectors = new float[]
                    {
                        0.36f, 0.55f, 0.47f, 0.94f
                    },
                    Payload = {
                        ["city"] = "Moscow"
                    }
            },
            // Truncated
        });

        var searchResult = await client.QueryAsync(
            collectionName: "test_collection",
            query: new float[] { 0.2f, 0.1f, 0.9f, 0.7f },
            limit: 3
        );

        Console.WriteLine(searchResult);
        
        var random = new Random();
        var queryVector = Enumerable.Range(1, 4).Select(_ => (float)random.NextDouble()).ToArray();

        // return the 5 closest points
        var points = await client.SearchAsync(
          "test_collection",
          queryVector,
          limit: 5);

        Console.WriteLine(searchResult);
    }

    internal async Task<DataIngestor> QdrantSetupAsync()
    {
        var client = CreateQdrantClient();

        var store = new QdrantVectorStore(client, true,
            new QdrantVectorStoreOptions() { EmbeddingGenerator = _embeddingGenerator });

        VectorStoreCollection<Guid, IngestedChunk> chunks = store.GetCollection<Guid, IngestedChunk>("chunks");
        VectorStoreCollection<Guid, IngestedDocument> documents = store.GetCollection<Guid, IngestedDocument>("documents");

        var dataIngestor = new DataIngestor(_embeddingGenerator, chunks, documents);
        await dataIngestor.IngestDataAsync(new PDFDirectorySource(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")));

        return dataIngestor;
    }

    internal async Task BasicDocumentSearchAsync()
    {
        var dataIngestor = await QdrantSetupAsync();

        var clientBuilder = new ChatClientBuilder(_chatClientInterface)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = PromptyHelper.GetSystemPrompt("DocumentSearch");
        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        var chatOptions = new ChatOptions
        {
            Tools = [AIFunctionFactory.Create(SearchAsync)]
        };

        _semanticSearch = new SemanticSearch(dataIngestor.Chunks);

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nQ: ");
            var input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exiting chat.");
                Console.ResetColor();
                break;
            }

            history.Add(new(ChatRole.User, input));

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, chatOptions);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("A: ");
            var messageBuilder = new StringBuilder();

            await foreach (var chunk in streamingResponse)
            {
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }

    [Description("Searches for information using a phrase or keyword")]
    private async Task<IEnumerable<string>> SearchAsync(
    [Description("The phrase to search for.")] string searchPhrase,
    [Description("If possible, specify the filename to search that file only. If not provided or empty, the search includes all files.")] string? filenameFilter = null)
    {
        if (_semanticSearch is null)
        {
            return ["Error: Vector store not initialized."];
        }

        var results = await _semanticSearch.SearchAsync(searchPhrase, filenameFilter, maxResults: 5);
        return results.Select(result =>
            $"<result filename=\"{result.DocumentId}\" page_number=\"{result.PageNumber}\">{result.Text}</result>");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _chatClient.Dispose();
        _embeddingClient.Dispose();
        _disposed = true;
    }
}
