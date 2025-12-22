using AI.Workshop.Common;
using AI.Workshop.Common.Toon;
using AI.Workshop.ConsoleApps.AgentChat.Tools;
using AI.Workshop.Guardrails;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OllamaSharp;
using System.Text;

namespace AI.Workshop.ConsoleApps.AgentChat;

/// <summary>
/// Available agent/prompt types that users can select.
/// Organized as a learning progression from simple to advanced.
/// </summary>
public enum AgentType
{
    // Step-by-step RAG learning progression
    Step1_BasicChat,
    Step2_ChatWithTools,
    Step3_ToolsDemo,
    
    // Full RAG implementations
    DocumentSearch,
    DocumentSearchSimple,
    PDFSummarization
}

/// <summary>
/// Provides a navigation system for selecting and running different AI agents/prompts
/// </summary>
internal class AgentNavigator : IDisposable
{
    private readonly OllamaApiClient _chatClient;
    private readonly OllamaApiClient _embeddingClient;
    private readonly IChatClient _client;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly AISettings _aiSettings;
    private readonly ChatSettings _settings = new();
    private readonly GuardrailsService _guardrails;
    private readonly TokenUsageTracker _tokenTracker = new();
    private SemanticSearch? _semanticSearch;
    private bool _disposed;

    private static readonly Dictionary<AgentType, (string Title, string Description, string[] LearningPoints)> AgentInfo = new()
    {
        {
            AgentType.Step1_BasicChat,
            (
                "Step 1: Basic Chat",
                "Simple chat loop with conversation history - no tools.",
                [
                    "• Pure LLM interaction with streaming responses",
                    "• Conversation history for context retention",
                    "• System prompt defines assistant behavior",
                    "• Foundation for understanding chat mechanics"
                ]
            )
        },
        {
            AgentType.Step2_ChatWithTools,
            (
                "Step 2: Chat + Tools",
                "Adds function calling with a simple CurrentTime tool.",
                [
                    "• Introduces AIFunctionFactory for tool creation",
                    "• LLM decides when to call tools",
                    "• UseFunctionInvocation() middleware",
                    "• Tool results integrated into responses"
                ]
            )
        },
        {
            AgentType.Step3_ToolsDemo,
            (
                "Step 3: Multi-Tool Demo",
                "Shopping cart demo with multiple tools (pricing, cart management).",
                [
                    "• Multiple tools working together",
                    "• Stateful tool (Cart class maintains state)",
                    "• LLM orchestrates tool calls",
                    "• Real-world function calling pattern"
                ]
            )
        },
        {
            AgentType.DocumentSearch,
            (
                "Document Search (Full RAG)",
                "Complete RAG with vector store, semantic search, and citations.",
                [
                    "• PDF ingestion with chunking",
                    "• SQLite-Vec vector storage",
                    "• Semantic search as an LLM tool",
                    "• Detailed citations with page numbers"
                ]
            )
        },
        {
            AgentType.DocumentSearchSimple,
            (
                "Document Search (Simple)",
                "Simplified RAG with minimal system prompt.",
                [
                    "• Same RAG pipeline as full version",
                    "• Simpler system prompt for comparison",
                    "• Shows impact of prompt engineering",
                    "• Good for testing prompt variations"
                ]
            )
        },
        {
            AgentType.PDFSummarization,
            (
                "PDF Summarization",
                "Advanced RAG for document summarization with list tools.",
                [
                    "• ListDocuments tool to discover available PDFs",
                    "• Chapter-by-chapter summarization",
                    "• Higher temperature for creative summaries",
                    "• Longer context window (4000 tokens)"
                ]
            )
        }
    };

    public AgentNavigator(AISettings aiSettings)
    {
        _aiSettings = aiSettings;
        
        var uri = aiSettings.GetOllamaUri();

        _chatClient = new OllamaApiClient(uri, aiSettings.ChatModel);
        _client = _chatClient;

        // OllamaApiClient implements IEmbeddingGenerator - create a separate client for embeddings
        _embeddingClient = new OllamaApiClient(uri, aiSettings.EmbeddingModel);
        _embeddingGenerator = _embeddingClient;

        // Initialize guardrails with default options
        _guardrails = new GuardrailsService(new GuardrailsOptions
        {
            BlockedPatterns = [@"\b(password|secret|api[_-]?key)\b"],
            MaxInputLength = 5000,
            MaxOutputLength = 10000,
            EnablePiiDetection = true
        });
    }

    /// <summary>
    /// Creates an AgentNavigator with default configuration from AIConstants
    /// </summary>
    public AgentNavigator() : this(new AISettings())
    {
    }

    /// <summary>
    /// Displays the agent selection menu and returns the user's choice
    /// </summary>
    public (AgentType? Agent, bool OpenSettings) ShowAgentMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              AI Workshop - Agent Chat Demos                            ║");
        Console.WriteLine("║                                                                        ║");
        Console.WriteLine("║  Learn AI agent capabilities from basic chat to full RAG               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
        
        // Show current settings
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("║  ");
        _settings.PrintStatus();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("║  LEARNING PROGRESSION:                                                 ║");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
        Console.ResetColor();

        var agents = Enum.GetValues<AgentType>();
        for (int i = 0; i < agents.Length; i++)
        {
            var agent = agents[i];
            var info = AgentInfo[agent];

            // Add separator before RAG section
            if (agent == AgentType.DocumentSearch)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("║  FULL RAG IMPLEMENTATIONS:                                             ║");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"║  [{i + 1}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{info.Title,-28}");
            Console.ForegroundColor = ConsoleColor.Gray;

            // Truncate description to fit
            var desc = info.Description.Length > 38 ? info.Description[..35] + "..." : info.Description;
            Console.WriteLine($" {desc,-38} ║");
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("║  [S] Settings - Toggle Guardrails, TOON                                ║");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("║  [0] Exit                                                              ║");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.Write("\nSelect an option: ");
        var input = Console.ReadLine()?.Trim().ToUpperInvariant();
        
        if (input == "S")
            return (null, true);
        
        if (int.TryParse(input, out int choice))
        {
            if (choice == 0) return (null, false);
            if (choice >= 1 && choice <= agents.Length)
                return (agents[choice - 1], false);
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid selection. Please try again.");
        Console.ResetColor();
        Thread.Sleep(1500);
        return ShowAgentMenu();
    }

    /// <summary>
    /// Shows the settings menu
    /// </summary>
    private void ShowSettingsMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    Settings Menu                           ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
        Console.ResetColor();

        Console.Write("║  [1] Guardrails: ");
        Console.ForegroundColor = _settings.GuardrailsEnabled ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(_settings.GuardrailsEnabled ? "ENABLED - Content safety validation" : "DISABLED");
        Console.ResetColor();

        Console.Write("║  [2] TOON Format: ");
        Console.ForegroundColor = _settings.ToonEnabled ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(_settings.ToonEnabled ? "ENABLED - Token-efficient formatting" : "DISABLED");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("║");
        Console.WriteLine("║  📊 Token stats are always displayed after each response");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("║  [0] Back to main menu");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.Write("\nToggle setting (0-2): ");
        var input = Console.ReadLine();

        if (int.TryParse(input, out int choice))
        {
            switch (choice)
            {
                case 1:
                    _settings.GuardrailsEnabled = !_settings.GuardrailsEnabled;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n✓ Guardrails {(_settings.GuardrailsEnabled ? "enabled" : "disabled")}");
                    break;
                case 2:
                    _settings.ToonEnabled = !_settings.ToonEnabled;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n✓ TOON format {(_settings.ToonEnabled ? "enabled" : "disabled")}");
                    break;
                case 0:
                    return;
            }
            Console.ResetColor();
            Thread.Sleep(800);
            ShowSettingsMenu();
        }
    }

    /// <summary>
    /// Main entry point - shows menu and runs the selected agent
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            var (selectedAgent, openSettings) = ShowAgentMenu();
            
            if (openSettings)
            {
                ShowSettingsMenu();
                continue;
            }
            
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
        // Reset token tracker for new conversation
        _tokenTracker.Reset();
        
        Console.Clear();
        var info = AgentInfo[agentType];

        // Show demo header with learning points
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══════════════════════════════════════════════════════════════════════════");
        Console.WriteLine($"  {info.Title}");
        Console.WriteLine($"═══════════════════════════════════════════════════════════════════════════");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\n  {info.Description}\n");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  What this demo shows:");
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var point in info.LearningPoints)
        {
            Console.WriteLine($"  {point}");
        }
        Console.WriteLine();
        
        // Show active settings (token tracking always on)
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Active: ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("📊 Stats ");
        if (_settings.GuardrailsEnabled)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("🛡️ Guardrails ");
        }
        if (_settings.ToonEnabled)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("📝 TOON ");
        }
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══════════════════════════════════════════════════════════════════════════\n");
        Console.ResetColor();

        // Get system prompt - use GeneralAssistant for step demos that don't have their own
        var promptName = agentType switch
        {
            AgentType.Step1_BasicChat => "GeneralAssistant",
            AgentType.Step2_ChatWithTools => "GeneralAssistant",
            AgentType.Step3_ToolsDemo => "GeneralAssistant",
            _ => agentType.ToString()
        };
        
        var systemPrompt = PromptyHelper.GetSystemPrompt(promptName);
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"System prompt: {systemPrompt.Substring(0, Math.Min(150, systemPrompt.Length))}...\n");
        Console.ResetColor();

        switch (agentType)
        {
            case AgentType.Step1_BasicChat:
                await RunBasicChatAsync(systemPrompt);
                break;
            case AgentType.Step2_ChatWithTools:
                await RunChatWithToolsAsync(systemPrompt);
                break;
            case AgentType.Step3_ToolsDemo:
                await RunToolsDemoAsync();
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

    private async Task RunBasicChatAsync(string systemPrompt)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Starting basic chat... (type empty line to exit)\n");
        Console.ResetColor();

        // No tools - pure chat
        var clientBuilder = new ChatClientBuilder(_client)
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        var chatOptions = new ChatOptions
        {
            Temperature = 0.2f,
            MaxOutputTokens = 1000
        };

        await RunChatLoopAsync(clientBuilder, history, chatOptions);
    }

    private async Task RunChatWithToolsAsync(string systemPrompt)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Starting chat with CurrentTime tool... (type empty line to exit)");
        Console.WriteLine("Try asking: \"What time is it?\" or \"What's the current date?\"\n");
        Console.ResetColor();

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

    private async Task RunToolsDemoAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Starting FOOTMONSTER socks shopping demo... (type empty line to exit)");
        Console.WriteLine("The assistant will try to sell you socks. Try buying some!\n");
        Console.ResetColor();

        // Use the BasicToolsExamples shopping cart demo
        using var tools = new BasicToolsExamples(_aiSettings.OllamaUri, _aiSettings.ChatModel);
        await tools.ShoppingCartMethods();
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
                
                // Always show session token summary
                if (_tokenTracker.RequestCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n📈 Session Summary: {_tokenTracker.GetSummary()}");
                }
                
                Console.ResetColor();
                break;
            }

            // Guardrails: Validate input
            if (_settings.GuardrailsEnabled)
            {
                var inputResult = _guardrails.ValidateInput(input);
                if (!inputResult.IsAllowed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n⛔ Input blocked: {inputResult.ViolationMessage}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"   Violation: {inputResult.ViolationType}");
                    Console.ResetColor();
                    continue;
                }
                if (inputResult.RedactedContent != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"\n⚠️ Input redacted: {inputResult.ViolationType}");
                    Console.ResetColor();
                    input = inputResult.RedactedContent;
                }
            }

            history.Add(new(ChatRole.User, input));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nAssistant: ");

            var messageBuilder = new StringBuilder();
            var streamingResponse = client.GetStreamingResponseAsync(history, chatOptions);
            UsageDetails? lastUsage = null;

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

                // Capture usage from UsageContent in the stream
                foreach (var usageContent in chunk.Contents.OfType<UsageContent>())
                {
                    lastUsage ??= new UsageDetails();
                    lastUsage.Add(usageContent.Details);
                }

                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            var response = messageBuilder.ToString();

            // Always track and display token usage
            if (lastUsage != null)
            {
                _tokenTracker.RecordUsage(lastUsage);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\n[📊 Tokens: {TokenUsageTracker.FormatUsage(lastUsage)}]");
                Console.ResetColor();
            }

            // Guardrails: Validate output
            if (_settings.GuardrailsEnabled)
            {
                var outputResult = _guardrails.ValidateOutput(response);
                if (!outputResult.IsAllowed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\n⛔ Response blocked: {outputResult.ViolationMessage}");
                    Console.ResetColor();
                    response = "[Response blocked by guardrails]";
                }
                else if (outputResult.RedactedContent != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"\n\n⚠️ Response redacted: {outputResult.ViolationType}");
                    Console.ResetColor();
                    response = outputResult.RedactedContent;
                }
            }

            history.Add(new(ChatRole.Assistant, response));
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
        await dataIngestor.IngestDataAsync(new PDFDirectorySource(_aiSettings.GetResolvedDataPath(AppDomain.CurrentDomain.BaseDirectory)));

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
        
        // Use TOON format when enabled
        if (_settings.ToonEnabled)
        {
            var comparison = ToonSearchFormatter.Compare(results, r => 
                new SearchResultData(r.DocumentId, r.PageNumber, r.Text));
            
            // Always show TOON stats when TOON is enabled
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"\n[TOON Stats: {comparison}]");
            Console.ResetColor();
            
            return [comparison.ToonFormat];
        }
        
        // Traditional XML format
        return results.Select(result =>
            $"<result filename=\"{result.DocumentId}\" page_number=\"{result.PageNumber}\">{result.Text}</result>");
    }

    [System.ComponentModel.Description("Lists all available PDF documents that can be searched or summarized")]
    private Task<IEnumerable<string>> ListDocumentsAsync()
    {
        var dataPath = _aiSettings.GetResolvedDataPath(AppDomain.CurrentDomain.BaseDirectory);
        if (!Directory.Exists(dataPath))
            return Task.FromResult<IEnumerable<string>>([]);

        var files = Directory.GetFiles(dataPath, "*.pdf")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>();

        return Task.FromResult<IEnumerable<string>>(files);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _chatClient.Dispose();
        _embeddingClient.Dispose();
        _disposed = true;
    }
}
