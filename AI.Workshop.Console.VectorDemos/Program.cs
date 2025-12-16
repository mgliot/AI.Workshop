using AI.Workshop.Common;
using AI.Workshop.ConsoleApps.VectorDemos;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Welcome to the AI Workshop Console Chat Ollama examples!\r\n");

// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Read AI settings from configuration
var ollamaUri = configuration["AI:OllamaUri"] ?? AIConstants.DefaultOllamaUri;
var chatModel = configuration["AI:ChatModel"] ?? AIConstants.DefaultChatModel;
var embeddingModel = configuration["AI:EmbeddingModel"] ?? AIConstants.DefaultEmbeddingModel;

// Check Ollama availability before proceeding
if (!await OllamaHealthCheck.EnsureOllamaAvailableAsync(ollamaUri))
{
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
    return;
}

Console.WriteLine();

//var search = new BasicLocalOllamaExamples(ollamaUri, chatModel, embeddingModel);
//await search.BasicPromptWithHistoryAsync();
//await search.BasicLocalStoreSearchAsync();
//await search.BasicRagWithLocalStoreSearchAsync();

var search = new SqlLiteDocumentSearch(ollamaUri, chatModel, embeddingModel);
await search.BasicDocumentSearchAsync();    

//var search = new QdrantDocumentSearch(ollamaUri, chatModel, embeddingModel);
//await search.TestQdrantAsync();
//await search.QdrantSetupAsync();
//await search.BasicDocumentSearchAsync();
