using AI.Workshop.MCP.ConsoleClient;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .Build();

var appSettings = new AppSettings();
configuration.Bind(appSettings);

await RunDemoMenuAsync(appSettings);

static async Task RunDemoMenuAsync(AppSettings settings)
{
    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           MCP Console Client - Demo Menu               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  1. List Server Info (capabilities, tools, prompts)    ║");
        Console.WriteLine("║  2. Call MCP Server Tools (echo, reverse_echo)         ║");
        Console.WriteLine("║  3. Call Monkey Tools (get_monkeys, get_monkey)        ║");
        Console.WriteLine("║  4. RAG with MCP Tools (requires Ollama)               ║");
        Console.WriteLine("║  0. Exit                                               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Ollama URI: {settings.Ollama.Uri}");
        Console.WriteLine($"Chat Model: {settings.Ollama.ChatModel}");
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("Select an option: ");

        var key = Console.ReadKey();
        Console.WriteLine();

        try
        {
            switch (key.KeyChar)
            {
                case '1':
                    await RunDemoAsync("List Server Info", async () =>
                    {
                        await using var service = new McpServerStdioExamples(settings);
                        await service.EnlistServerInfoAsync();
                    });
                    break;

                case '2':
                    await RunDemoAsync("Call MCP Server Tools", async () =>
                    {
                        await using var service = new McpServerStdioExamples(settings);
                        await service.CallMcpServerToolsAsync();
                    });
                    break;

                case '3':
                    await RunDemoAsync("Call Monkey Tools", async () =>
                    {
                        await using var service = new McpServerStdioExamples(settings);
                        await service.CallMonkeyToolsAsync();
                    });
                    break;

                case '4':
                    await RunDemoAsync("RAG with MCP Tools", async () =>
                    {
                        using var ollama = new OllamaIntegrationExamples(settings);
                        await ollama.BasicRagWithMcpToolsAsync();
                    });
                    break;

                case '0':
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Goodbye!");
                    Console.ResetColor();
                    return;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ResetColor();
                    Console.ReadKey();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}

static async Task RunDemoAsync(string demoName, Func<Task> demo)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"=== {demoName} ===\n");
    Console.ResetColor();

    await demo();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("Press any key to return to menu...");
    Console.ResetColor();
    Console.ReadKey();
}
