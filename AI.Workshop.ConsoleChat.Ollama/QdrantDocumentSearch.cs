using AI.Workshop.VectorStore.Ingestion.Qdrant;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using OllamaSharp;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.ComponentModel;
using System.Text;

namespace AI.Workshop.ConsoleChat.Ollama;

internal class QdrantDocumentSearch
{
    private readonly IChatClient _chatClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private SemanticSearch? _semanticSearch;

    public QdrantDocumentSearch()
    {
        // for testing the example, run the docker container with:
        // docker run -p 6333:6333 -p 6334:6334 --name qdrant-db qdrant/qdrant

        var ollamaUri = new Uri("http://localhost:11434/");
        var ollamaModel = "llama3.2";
        var embeddingModel = "all-minilm";

        _chatClient = new OllamaApiClient(ollamaUri, ollamaModel);

        // OllamaApiClient implements IEmbeddingGenerator
        _embeddingGenerator = new OllamaApiClient(ollamaUri, embeddingModel);
    }

    internal async Task TestQdrantAsync()
    {
        var client = new QdrantClient("localhost", 6334);

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
        var client = new QdrantClient("localhost", 6334);

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

        var clientBuilder = new ChatClientBuilder(_chatClient)
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
        var results = await _semanticSearch.SearchAsync(searchPhrase, filenameFilter, maxResults: 5);
        return results.Select(result =>
            $"<result filename=\"{result.DocumentId}\" page_number=\"{result.PageNumber}\">{result.Text}</result>");
    }
}
