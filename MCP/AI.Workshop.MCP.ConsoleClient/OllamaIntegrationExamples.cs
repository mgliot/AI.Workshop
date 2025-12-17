using AI.Workshop.Common;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;

namespace AI.Workshop.MCP.ConsoleClient;

internal class OllamaIntegrationExamples : IDisposable
{
    private readonly OllamaApiClient _ollamaClient;
    private readonly IChatClient _chatClient;
    private readonly AppSettings _settings;
    private bool _disposed;

    public OllamaIntegrationExamples(AppSettings settings)
    {
        _settings = settings;
        var ollamaUri = new Uri(settings.Ollama.Uri);
        var ollamaModel = settings.Ollama.ChatModel;

        _ollamaClient = new OllamaApiClient(ollamaUri, ollamaModel);
        _chatClient = _ollamaClient;
    }

    internal async Task BasicRagWithMcpToolsAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = PromptyHelper.GetSystemPrompt("MonkeyAssistant");

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        await using var workshopMcp = new WorkshopMcpService(_settings);
        var mcpTools = await workshopMcp.GetToolsAsync();

        var chatOptions = new ChatOptions
        {
            Tools = [.. mcpTools]
        };

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
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
        _ollamaClient.Dispose();
        _disposed = true;
    }
}
