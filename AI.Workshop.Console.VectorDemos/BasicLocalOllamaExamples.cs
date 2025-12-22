using AI.Workshop.Common;
using AI.Workshop.VectorStore;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;

namespace AI.Workshop.ConsoleApps.VectorDemos;

internal class BasicLocalOllamaExamples : IDisposable
{
    private readonly OllamaApiClient _chatClient;
    private readonly OllamaApiClient _embeddingClient;
    private readonly IChatClient _chatClientInterface;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private bool _disposed;

    public BasicLocalOllamaExamples(AISettings aiSettings)
    {
        var uri = aiSettings.GetOllamaUri();

        _chatClient = new OllamaApiClient(uri, aiSettings.ChatModel);
        _chatClientInterface = _chatClient;

        // OllamaApiClient implements IEmbeddingGenerator
        _embeddingClient = new OllamaApiClient(uri, aiSettings.EmbeddingModel);
        _embeddingGenerator = _embeddingClient;
    }

    internal async Task BasicPromptWithHistoryAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClientInterface)
            .Build();

        var systemPrompt = PromptyHelper.GetSystemPrompt("BookRecommendation");
        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

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

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history);

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

    internal async Task BasicLocalStoreSearchAsync()
    {
        var inMemoryStore = new InMemoryStore(_embeddingGenerator);
        await inMemoryStore.IngestDataAsync();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Searching for: \"Which service should I use to store my documents?\"\n");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Results:");
        Console.ResetColor();

        var resultCount = 0;
        await foreach (var result in inMemoryStore.SearchAsync("Which service should I use to store my documents?", numberOfResults: 3))
        {
            resultCount++;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  [{resultCount}] {result.Record.Name}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"      {result.Record.Description}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"      Score: {result.Score:F4}");
        }
        Console.ResetColor();
    }

    internal async Task BasicRagWithLocalStoreSearchAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClientInterface)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = PromptyHelper.GetSystemPrompt("ServiceSuggestion");

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        var inMemoryStore = new InMemoryStore(_embeddingGenerator);
        await inMemoryStore.IngestDataAsync();

        var chatOptions = new ChatOptions
        {
            Tools = [AIFunctionFactory.Create(inMemoryStore.SearchToolAsync)],
            ToolMode = ChatToolMode.RequireAny
        };

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

    public void Dispose()
    {
        if (_disposed) return;
        _chatClient.Dispose();
        _embeddingClient.Dispose();
        _disposed = true;
    }
}
