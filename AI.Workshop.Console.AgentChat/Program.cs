using AI.Workshop.Common;
using AI.Workshop.ConsoleApps.AgentChat;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Welcome to the AI Workshop Console Chat RAG examples!\r\n");

// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Read AI settings from configuration
var ollamaUri = configuration["AI:OllamaUri"] ?? AIConstants.DefaultOllamaUri;
var chatModel = configuration["AI:ChatModel"] ?? AIConstants.DefaultChatModel;
var embeddingModel = configuration["AI:EmbeddingModel"] ?? AIConstants.DefaultEmbeddingModel;

// Check Ollama availability before starting
if (!await OllamaHealthCheck.EnsureOllamaAvailableAsync(ollamaUri))
{
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey(true);
    return;
}

Console.WriteLine();

// Use the Agent Navigator for interactive demo selection
// Demos progress from basic chat to full RAG implementation
var navigator = new AgentNavigator(ollamaUri, chatModel, embeddingModel);
await navigator.RunAsync();
