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
var aiSettings = configuration.GetAISettings();

// Check Ollama availability before proceeding
if (!await OllamaHealthCheck.EnsureOllamaAvailableAsync(aiSettings.OllamaUri))
{
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
    return;
}

Console.WriteLine();

// Use the interactive Demo Navigator for demo selection
var navigator = new DemoNavigator(aiSettings);
await navigator.RunAsync();
