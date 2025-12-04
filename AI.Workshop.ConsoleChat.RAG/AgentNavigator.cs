using AI.Workshop.ConsoleChat.RAG.Tools;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OllamaSharp;
using System.Text;
using System.Text.Json;

namespace AI.Workshop.ConsoleChat.RAG;

/// <summary>
/// Available agent/prompt types that users can select
/// </summary>
public enum AgentType
{
    GeneralAssistant,
    DocumentSearch,
    DocumentSearchSimple,
    PDFSummarization
}

/// <summary>
/// Provides a navigation system for selecting and running different AI agents/prompts
/// </summary>
internal class AgentNavigator
{
    private readonly IChatClient _client;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private SemanticSearch? _semanticSearch;

    private static readonly Dictionary<AgentType, string> AgentDescriptions = new()
    {
        { AgentType.GeneralAssistant, "General purpose assistant with tool support" },
        { AgentType.DocumentSearch, "Search documents with detailed citations" },
        { AgentType.DocumentSearchSimple, "Simple document search assistant" },
        { AgentType.PDFSummarization, "Summarize PDF documents chapter by chapter" }
    };

    public AgentNavigator()
    {
        var ollamaUri = new Uri("http://localhost:11434/");
        var ollamaModel = "llama3.2";
        var embeddingModel = "all-minilm";

        _client = new OllamaApiClient(ollamaUri, ollamaModel);

        // OllamaApiClient implements IEmbeddingGenerator - create a separate client for embeddings
        _embeddingGenerator = new OllamaApiClient(ollamaUri, embeddingModel);
    }

    /// <summary>
    /// Displays the agent selection menu and returns the user's choice
    /// </summary>
    public static AgentType? ShowAgentMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           AI Workshop - Agent Selection Menu               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
        Console.ResetColor();

        var agents = Enum.GetValues<AgentType>();
        for (int i = 0; i < agents.Length; i++)
        {
            var agent = agents[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"║  [{i + 1}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{agent,-25}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($" - {AgentDescriptions[agent],-20}");
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("║  [0] Exit");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.Write("\nSelect an agent (0-{0}): ", agents.Length);
        
        if (int.TryParse(Console.ReadLine(), out int choice))
        {
            if (choice == 0) return null;
            if (choice >= 1 && choice <= agents.Length)
                return agents[choice - 1];
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid selection. Please try again.");
        Console.ResetColor();
        Thread.Sleep(1500);
        return ShowAgentMenu();
    }

    /// <summary>
    /// Main entry point - shows menu and runs the selected agent
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            var selectedAgent = ShowAgentMenu();
            
            if (selectedAgent == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nGoodbye!");
                Console.ResetColor();
                break;
            }

            await RunAgentAsync(selectedAgent.Value);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n\nPress any key to return to the menu...");
            Console.ReadKey(true);
        }
    }

    /// <summary>
    /// Runs the specified agent type
    /// </summary>
    private async Task RunAgentAsync(AgentType agentType)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══ {agentType} Agent ═══\n");
        Console.ResetColor();

        var systemPrompt = PromptyHelper.GetSystemPrompt(agentType.ToString());
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"System: {systemPrompt.Substring(0, Math.Min(200, systemPrompt.Length))}...\n");
        Console.ResetColor();

        switch (agentType)
        {
            case AgentType.GeneralAssistant:
                await RunGeneralAssistantAsync(systemPrompt);
                break;
            case AgentType.DocumentSearch:
            case AgentType.DocumentSearchSimple:
                await RunDocumentSearchAsync(systemPrompt);
                break;
            case AgentType.PDFSummarization:
                await RunPDFSummarizationAsync(systemPrompt);
                break;
        }
    }

    private async Task RunGeneralAssistantAsync(string systemPrompt)
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        var chatOptions = new ChatOptions
        {
            Temperature = 0.2f,
            MaxOutputTokens = 1000,
            Tools = [
                AIFunctionFactory.Create(new CurrentTimeTool().InvokeAsync, "CurrentTime", "Returns the current date and time.")
            ]
        };

        await RunChatLoopAsync(clientBuilder, history, chatOptions);
    }

    private async Task RunDocumentSearchAsync(string systemPrompt)
    {
        await InitializeVectorStoreAsync();

        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        var chatOptions = new ChatOptions
        {
            Temperature = 0.2f,
            MaxOutputTokens = 2000,
            Tools = [
                AIFunctionFactory.Create(SearchAsync),
                AIFunctionFactory.Create(new CurrentTimeTool().InvokeAsync, "CurrentTime", "Returns the current date and time.")
            ]
        };

        await RunChatLoopAsync(clientBuilder, history, chatOptions);
    }

    private async Task RunPDFSummarizationAsync(string systemPrompt)
    {
        await InitializeVectorStoreAsync();

        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        var chatOptions = new ChatOptions
        {
            Temperature = 0.5f,
            MaxOutputTokens = 4000,
            Tools = [
                AIFunctionFactory.Create(SearchAsync),
                AIFunctionFactory.Create(ListDocumentsAsync),
                AIFunctionFactory.Create(new CurrentTimeTool().InvokeAsync, "CurrentTime", "Returns the current date and time.")
            ]
        };

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Tip: Ask me to summarize a specific PDF or list available documents.\n");
        Console.ResetColor();

        await RunChatLoopAsync(clientBuilder, history, chatOptions);
    }

    private async Task RunChatLoopAsync(IChatClient client, List<ChatMessage> history, ChatOptions chatOptions)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nYou: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Ending conversation...");
                Console.ResetColor();
                break;
            }

            history.Add(new(ChatRole.User, input));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nAssistant: ");

            var messageBuilder = new StringBuilder();
            var streamingResponse = client.GetStreamingResponseAsync(history, chatOptions);

            await foreach (var chunk in streamingResponse)
            {
                if (chunk.FinishReason == ChatFinishReason.ToolCalls)
                {
                    foreach (var functionCall in chunk.Contents.OfType<FunctionCallContent>())
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"\n[Tool: {functionCall.Name}] ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                }

                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }

    private async Task InitializeVectorStoreAsync()
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Initializing vector store...");

        var store = new SqliteVectorStore("Data Source=vector-store.db",
            new SqliteVectorStoreOptions() { EmbeddingGenerator = _embeddingGenerator });

        VectorStoreCollection<string, IngestedChunk> chunks = store.GetCollection<string, IngestedChunk>("chunks");
        VectorStoreCollection<string, IngestedDocument> documents = store.GetCollection<string, IngestedDocument>("documents");

        var dataIngestor = new DataIngestor(_embeddingGenerator, chunks, documents);
        await dataIngestor.IngestDataAsync(new PDFDirectorySource(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")));

        _semanticSearch = new SemanticSearch(chunks);
        Console.WriteLine("Vector store ready.\n");
        Console.ResetColor();
    }

    [System.ComponentModel.Description("Searches for information in documents using a phrase or keyword")]
    private async Task<IEnumerable<string>> SearchAsync(
        [System.ComponentModel.Description("The phrase to search for.")] string searchPhrase,
        [System.ComponentModel.Description("Optional filename to search in a specific document only.")] string? filenameFilter = null)
    {
        if (_semanticSearch == null)
            return ["Error: Vector store not initialized."];

        var results = await _semanticSearch.SearchAsync(searchPhrase, filenameFilter, maxResults: 5);
        return results.Select(result =>
            $"<result filename=\"{result.DocumentId}\" page_number=\"{result.PageNumber}\">{result.Text}</result>");
    }

    [System.ComponentModel.Description("Lists all available PDF documents that can be searched or summarized")]
    private Task<IEnumerable<string>> ListDocumentsAsync()
    {
        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        if (!Directory.Exists(dataPath))
            return Task.FromResult<IEnumerable<string>>(["No documents found. Data directory does not exist."]);

        var files = Directory.GetFiles(dataPath, "*.pdf")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>();

        return Task.FromResult(files.Any() 
            ? files 
            : (IEnumerable<string>)["No PDF documents found in the Data directory."]);
    }
}
