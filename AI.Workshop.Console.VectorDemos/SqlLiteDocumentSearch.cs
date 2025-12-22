using AI.Workshop.Common;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OllamaSharp;
using System.ComponentModel;
using System.Text;

namespace AI.Workshop.ConsoleApps.VectorDemos;

internal class SqlLiteDocumentSearch : IDisposable
{
    private readonly OllamaApiClient _chatClient;
    private readonly OllamaApiClient _embeddingClient;
    private readonly IChatClient _chatClientInterface;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly AISettings _aiSettings;
    private SemanticSearch? _semanticSearch;
    private bool _disposed;

    public SqlLiteDocumentSearch(AISettings aiSettings)
    {
        _aiSettings = aiSettings;
        var uri = aiSettings.GetOllamaUri();

        _chatClient = new OllamaApiClient(uri, aiSettings.ChatModel);
        _chatClientInterface = _chatClient;

        // OllamaApiClient implements IEmbeddingGenerator
        _embeddingClient = new OllamaApiClient(uri, aiSettings.EmbeddingModel);
        _embeddingGenerator = _embeddingClient;
    }

    internal async Task BasicDocumentSearchAsync()
    {
        var store = new SqliteVectorStore("Data Source=vector-store.db", 
            new SqliteVectorStoreOptions() { EmbeddingGenerator = _embeddingGenerator });

        VectorStoreCollection<string, IngestedChunk> chunks = store.GetCollection<string, IngestedChunk>("chunks");
        VectorStoreCollection<string, IngestedDocument> documents = store.GetCollection<string, IngestedDocument>("documents");

        var clientBuilder = new ChatClientBuilder(_chatClientInterface)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = PromptyHelper.GetSystemPrompt("DocumentSearch");
        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        var dataIngestor = new DataIngestor(_embeddingGenerator, chunks, documents);
        await dataIngestor.IngestDataAsync(new PDFDirectorySource(_aiSettings.GetResolvedDataPath(AppDomain.CurrentDomain.BaseDirectory)));

        var chatOptions = new ChatOptions
        {
            Tools = [AIFunctionFactory.Create(SearchAsync)]
        };

        _semanticSearch = new SemanticSearch(chunks);

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
