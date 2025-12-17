using AI.Workshop.Common;
using AI.Workshop.ConsoleApps.Agents;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Build host with DI container
        var builder = Host.CreateApplicationBuilder(args);

        // Read OllamaUri setting from configuration
        var ollamaUri = builder.Configuration["AI:OllamaUri"] ?? AIConstants.DefaultOllamaUri;

        // Check Ollama availability before starting
        if (!await OllamaHealthCheck.EnsureOllamaAvailableAsync(ollamaUri))
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
            return;
        }

        // Register Ollama chat client via DI
        builder.Services.AddOllamaChatClient(builder.Configuration);

        // Register our agent classes
        builder.Services.AddTransient<GhostWriterAgents>();
        builder.Services.AddTransient<AgentSmithPromptDemo>();
        builder.Services.AddTransient<AgentSmithConversationDemo>();
        builder.Services.AddTransient<StructuredOutputDemo>();
        builder.Services.AddTransient<WeatherFunctionDemo>();
        builder.Services.AddTransient<AgentAsToolDemo>();

        var host = builder.Build();

        // Run the ghostwriter agent
        var chatClient = host.Services.GetRequiredService<IChatClient>();

        WriteSection("Ghost Writer Workflow");
        var writers = host.Services.GetRequiredService<GhostWriterAgents>();
        await writers.RunAsync(chatClient);
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);

        // Run the matrix demos
        WriteSection("Matrix Agents - Single Prompt");
        await host.Services.GetRequiredService<AgentSmithPromptDemo>().RunAsync();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);

        WriteSection("Matrix Agents - Multi-turn Conversation");
        await host.Services.GetRequiredService<AgentSmithConversationDemo>().RunAsync();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);

        // Run the weather function calling demo
        WriteSection("Weather Agent - Function Calling");
        await host.Services.GetRequiredService<WeatherFunctionDemo>().RunAsync();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);

        // Run the structured output demo
        WriteSection("PersonInfo Agent - Structured Output");
        await host.Services.GetRequiredService<StructuredOutputDemo>().RunAsync();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);

        // Run the agent-as-tool demo
        WriteSection("CroatianTranslator Agent - Agent-as-Tool Workflow");
        await host.Services.GetRequiredService<AgentAsToolDemo>().RunAsync();

        static void WriteSection(string title)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine();
        }
    }
}