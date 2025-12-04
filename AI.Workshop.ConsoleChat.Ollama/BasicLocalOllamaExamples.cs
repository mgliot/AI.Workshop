using AI.Workshop.VectorStore;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;

namespace AI.Workshop.ConsoleChat.Ollama;

internal class BasicLocalOllamaExamples
{
    private readonly IChatClient _chatClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public BasicLocalOllamaExamples()
    {
        var ollamaUri = new Uri("http://localhost:11434/");
        var ollamaModel = "llama3.2";
        var embeddingModel = "all-minilm";

        _chatClient = new OllamaApiClient(ollamaUri, ollamaModel);

        // OllamaApiClient implements IEmbeddingGenerator
        _embeddingGenerator = new OllamaApiClient(ollamaUri, embeddingModel);
    }

    internal async Task BasicPromptWithHistoryAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
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

        await foreach (var result in inMemoryStore.SearchAsync("Which service should I use to store my documents?"));
    }

    internal async Task BasicRagWithLocalStoreSearchAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
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
            Tools = [AIFunctionFactory.Create(inMemoryStore.SearchToolAsync)]
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
}
