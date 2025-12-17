using AI.Workshop.Common;
using AI.Workshop.ConsoleApps.VectorDemos;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Welcome to the AI Workshop - Vector Store Demos!\r\n");

// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Read AI settings from configuration
var ollamaUri = configuration["AI:OllamaUri"] ?? AIConstants.DefaultOllamaUri;
var chatModel = configuration["AI:ChatModel"] ?? AIConstants.DefaultChatModel;
var embeddingModel = configuration["AI:EmbeddingModel"] ?? AIConstants.DefaultEmbeddingModel;
var qdrantHost = configuration["AI:QdrantHost"] ?? AIConstants.DefaultQdrantHost;
var qdrantGrpcPort = int.TryParse(configuration["AI:QdrantGrpcPort"], out var port) ? port : AIConstants.DefaultQdrantGrpcPort;
var qdrantApiKey = configuration["AI:QdrantApiKey"] ?? AIConstants.DefaultQdrantApiKey;

// Check Ollama availability before proceeding
if (!await OllamaHealthCheck.EnsureOllamaAvailableAsync(ollamaUri))
{
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
    return;
}

Console.WriteLine();

// Use the interactive Demo Navigator for demo selection
var navigator = new DemoNavigator(ollamaUri, chatModel, embeddingModel, qdrantHost, qdrantGrpcPort, qdrantApiKey);
await navigator.RunAsync();
