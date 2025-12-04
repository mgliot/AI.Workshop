using AI.Workshop.ConsoleChat.RAG.Tools;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OllamaSharp;
using System.Text;
using System.Text.Json;

namespace AI.Workshop.ConsoleChat.RAG;

internal class RagWorkflowExamples
{
    protected readonly IChatClient _client;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    private readonly string _systemPrompt;
    private SemanticSearch? _semanticSearch;

    private readonly ChatOptions _chatOptions = new()
    {
        Temperature = 0.2f,
        MaxOutputTokens = 1000,
        FrequencyPenalty = 0.1f,
        PresencePenalty = 0.0f,
        TopP = 0.3f,
        ToolMode = ChatToolMode.Auto,
        Tools = []
    };

    public RagWorkflowExamples()
    {
        var ollamaUri = new Uri("http://localhost:11434/");
        var ollamaModel = "llama3.2";
        var embeddingModel = "all-minilm";

        _client = new OllamaApiClient(ollamaUri, ollamaModel);

        // OllamaApiClient implements IEmbeddingGenerator - create a separate client for embeddings
        _embeddingGenerator = new OllamaApiClient(ollamaUri, embeddingModel);

        _systemPrompt = PromptyHelper.GetSystemPrompt("GeneralAssistant");
    }

    internal async Task InitialMessageLoopAsync()
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
        Console.ResetColor();

        while (true)
        {
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
           
            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

            var messageBuilder = new StringBuilder();
            await foreach (var chunk in streamingResponse)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));            
            Console.ResetColor();
        }
    }

    internal async Task RagWithBasicToolAsync()
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
        Console.ResetColor();

        var currentTime = new CurrentTimeTool();
        var currentTimeTool = AIFunctionFactory.Create(
            method: currentTime.InvokeAsync,
            name: "CurrentTime",
            description: "Returns the current date and time for Central European Time Zone. This tool needs no parameters.");

        _chatOptions.Tools!.Add(currentTimeTool);

        while (true)
        {
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

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

            var messageBuilder = new StringBuilder();
            await foreach (var chunk in streamingResponse)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }

    internal async Task RagWithDocumentSearchAsync(string userPrompt)
    {
        var store = new SqliteVectorStore("Data Source=vector-store.db",
            new SqliteVectorStoreOptions() { EmbeddingGenerator = _embeddingGenerator });

        VectorStoreCollection<string, IngestedChunk> chunks = store.GetCollection<string, IngestedChunk>("chunks");
        VectorStoreCollection<string, IngestedDocument> documents = store.GetCollection<string, IngestedDocument>("documents");

        var dataIngestor = new DataIngestor(_embeddingGenerator, chunks, documents);
        await dataIngestor.IngestDataAsync(new PDFDirectorySource(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")));

        _semanticSearch = new SemanticSearch(chunks);

        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = PromptyHelper.GetSystemPrompt("DocumentSearch");

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        var chatOptions = new ChatOptions
        {
            Tools = [
                AIFunctionFactory.Create(SearchAsync),
                AIFunctionFactory.Create(new CurrentTimeTool().InvokeAsync, "CurrentTime", "Returns the current date and time.")
            ]
        };

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\nQ: {userPrompt}");
        history.Add(new(ChatRole.User, userPrompt));

        var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, chatOptions);

        var messageBuilder = new StringBuilder();
        await foreach (var update in streamingResponse)
        {
            if (update.FinishReason == ChatFinishReason.ToolCalls)
            {
                foreach (var functionCall in update.Contents.OfType<FunctionCallContent>())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nTool Call: {functionCall.Name}");

                    var parameters = functionCall.Arguments;
                    var json = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(json);
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(update.Text);
            messageBuilder.Append(update.Text);
        }

        history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
        Console.ResetColor();
    }

    internal async Task RagWithDocumentSearchLoopAsync()
    {
        var store = new SqliteVectorStore("Data Source=vector-store.db",
            new SqliteVectorStoreOptions() { EmbeddingGenerator = _embeddingGenerator });

        VectorStoreCollection<string, IngestedChunk> chunks = store.GetCollection<string, IngestedChunk>("chunks");
        VectorStoreCollection<string, IngestedDocument> documents = store.GetCollection<string, IngestedDocument>("documents");

        var dataIngestor = new DataIngestor(_embeddingGenerator, chunks, documents);
        await dataIngestor.IngestDataAsync(new PDFDirectorySource(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")));

        _semanticSearch = new SemanticSearch(chunks);

        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = PromptyHelper.GetSystemPrompt("DocumentSearchSimple");

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        var chatOptions = new ChatOptions
        {
            Tools = [
                AIFunctionFactory.Create(SearchAsync),
                AIFunctionFactory.Create(new CurrentTimeTool().InvokeAsync, "CurrentTime", "Returns the current date and time.")
            ]
        };

        while (true)
        {
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

    [System.ComponentModel.Description("Searches for information using a phrase or keyword")]
    private async Task<IEnumerable<string>> SearchAsync(
        [System.ComponentModel.Description("The phrase to search for.")] string searchPhrase,
        [System.ComponentModel.Description("If possible, specify the filename to search that file only. If not provided or empty, the search includes all files.")] string? filenameFilter = null)
    {
        var results = await _semanticSearch.SearchAsync(searchPhrase, filenameFilter, maxResults: 5);
        return results.Select(result =>
            $"<result filename=\"{result.DocumentId}\" page_number=\"{result.PageNumber}\">{result.Text}</result>");
    }
}
