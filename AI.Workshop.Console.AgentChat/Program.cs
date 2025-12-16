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

// Use the new Agent Navigator for interactive prompt/agent selection
var navigator = new AgentNavigator(ollamaUri, chatModel, embeddingModel);
await navigator.RunAsync();

// --- Legacy examples (uncomment to run directly) ---

//var search = new InMemoryVectorStoreSearch();
//await search.GenerateVectorsAsync();
//await search.SearchAsync("Which service should I use to store my documents?");

//var userPrompt = "Search for information about cloud storage and tell me the current time.";

//var tools = new BasicToolsExamples();
//await tools.ItemPriceMethod();
//await tools.ShoppingCartMethods();

//var workflow = new RagWorkflowExamples();
//await workflow.InitialMessageLoopAsync();
//await workflow.RagWithBasicToolAsync();
//await workflow.RagWithDocumentSearchAsync(userPrompt);
//await workflow.RagWithDocumentSearchLoopAsync();
