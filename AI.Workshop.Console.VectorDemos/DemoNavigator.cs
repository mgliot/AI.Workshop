using AI.Workshop.Common;

namespace AI.Workshop.ConsoleApps.VectorDemos;

/// <summary>
/// Available demo types for vector store exploration
/// </summary>
public enum VectorDemoType
{
    InMemoryVectorSearch,
    SQLiteDocumentSearch,
    QdrantDocumentSearch
}

/// <summary>
/// Interactive navigator for vector store demos.
/// Focuses on demonstrating different vector store backends and their capabilities.
/// </summary>
internal class DemoNavigator : IDisposable
{
    private readonly string _ollamaUri;
    private readonly string _chatModel;
    private readonly string _embeddingModel;
    private readonly string _qdrantHost;
    private readonly int _qdrantGrpcPort;
    private readonly string _qdrantApiKey;
    private bool _disposed;

    public DemoNavigator(string ollamaUri, string chatModel, string embeddingModel,
        string qdrantHost = AIConstants.DefaultQdrantHost, 
        int qdrantGrpcPort = AIConstants.DefaultQdrantGrpcPort,
        string qdrantApiKey = "")
    {
        _ollamaUri = ollamaUri;
        _chatModel = chatModel;
        _embeddingModel = embeddingModel;
        _qdrantHost = qdrantHost;
        _qdrantGrpcPort = qdrantGrpcPort;
        _qdrantApiKey = qdrantApiKey;
    }

    private static readonly Dictionary<VectorDemoType, (string Title, string Description, string[] LearningPoints)> DemoInfo = new()
    {
        {
            VectorDemoType.InMemoryVectorSearch,
            (
                "In-Memory Vector Store",
                "Demonstrates vector embeddings and similarity search with in-memory storage.",
                [
                    "• Creates embeddings from sample cloud service descriptions",
                    "• Uses InMemoryVectorStore from Semantic Kernel",
                    "• Performs semantic search using cosine similarity",
                    "• Shows raw search results with similarity scores",
                    "• No persistence - data lost on restart"
                ]
            )
        },
        {
            VectorDemoType.SQLiteDocumentSearch,
            (
                "SQLite-Vec Vector Store",
                "Full document ingestion with SQLite-Vec for persistent embedded vector storage.",
                [
                    "• PDF document ingestion with chunking",
                    "• SQLite-Vec for embedded vector storage",
                    "• Persistent storage (survives restarts)",
                    "• Semantic search with page citations",
                    "• No external dependencies - single file DB"
                ]
            )
        },
        {
            VectorDemoType.QdrantDocumentSearch,
            (
                "Qdrant Vector Store",
                "External vector database with Qdrant for scalable production scenarios.",
                [
                    "• Standalone: docker run -p 6333:6333 -p 6334:6334 qdrant/qdrant",
                    "• If using Aspire's Qdrant, set QdrantApiKey in appsettings.json",
                    "• Production-ready external vector database",
                    "• Supports filtering, metadata, and horizontal scaling"
                ]
            )
        }
    };

    /// <summary>
    /// Main entry point - shows menu and runs selected demos
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            var selectedDemo = ShowDemoMenu();

            if (selectedDemo == null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nGoodbye!");
                Console.ResetColor();
                break;
            }

            await RunDemoAsync(selectedDemo.Value);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n\nPress any key to return to the menu...");
            Console.ReadKey(true);
        }
    }

    /// <summary>
    /// Displays the demo selection menu
    /// </summary>
    private static VectorDemoType? ShowDemoMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              AI Workshop - Vector Store Demos                          ║");
        Console.WriteLine("║                                                                        ║");
        Console.WriteLine("║  Explore different vector store backends and their capabilities        ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
        Console.ResetColor();

        var demos = Enum.GetValues<VectorDemoType>();
        for (int i = 0; i < demos.Length; i++)
        {
            var demo = demos[i];
            var info = DemoInfo[demo];

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"║  [{i + 1}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{info.Title,-30}");
            Console.ForegroundColor = ConsoleColor.Gray;

            // Truncate description to fit
            var desc = info.Description.Length > 35 ? info.Description[..32] + "..." : info.Description;
            Console.WriteLine($" {desc,-36} ║");
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("║  [0] Exit                                                              ║");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.Write("\nSelect a demo: ");
        var input = Console.ReadLine()?.Trim();

        if (int.TryParse(input, out int choice))
        {
            if (choice == 0) return null;
            if (choice >= 1 && choice <= demos.Length)
                return demos[choice - 1];
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid selection. Please try again.");
        Console.ResetColor();
        Thread.Sleep(1500);
        return ShowDemoMenu();
    }

    /// <summary>
    /// Runs the selected demo with explanatory header
    /// </summary>
    private async Task RunDemoAsync(VectorDemoType demoType)
    {
        Console.Clear();
        var info = DemoInfo[demoType];

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

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n  Using: {_chatModel} (chat) | {_embeddingModel} (embeddings)");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══════════════════════════════════════════════════════════════════════════\n");
        Console.ResetColor();

        switch (demoType)
        {
            case VectorDemoType.InMemoryVectorSearch:
                await RunInMemoryVectorSearchAsync();
                break;
            case VectorDemoType.SQLiteDocumentSearch:
                await RunSQLiteDocumentSearchAsync();
                break;
            case VectorDemoType.QdrantDocumentSearch:
                await RunQdrantDocumentSearchAsync();
                break;
        }
    }

    private async Task RunInMemoryVectorSearchAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Initializing in-memory vector store with sample data...\n");
        Console.ResetColor();

        using var examples = new BasicLocalOllamaExamples(_ollamaUri, _chatModel, _embeddingModel);
        await examples.BasicLocalStoreSearchAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Vector search complete! Results shown above with similarity scores.");
        Console.ResetColor();
    }

    private async Task RunSQLiteDocumentSearchAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Initializing SQLite-Vec document search...");
        Console.WriteLine("Documents will be loaded from the Data folder.\n");
        Console.ResetColor();

        using var search = new SqlLiteDocumentSearch(_ollamaUri, _chatModel, _embeddingModel);
        await search.BasicDocumentSearchAsync();
    }

    private async Task RunQdrantDocumentSearchAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Checking Qdrant connection at {_qdrantHost}:{_qdrantGrpcPort} (gRPC)...\n");
        Console.ResetColor();

        try
        {
            using var search = new QdrantDocumentSearch(_ollamaUri, _chatModel, _embeddingModel, _qdrantHost, _qdrantGrpcPort, _qdrantApiKey);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Testing Qdrant connection...");
            await search.TestQdrantAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ Qdrant connection successful!\n");
            Console.ResetColor();

            Console.WriteLine("Starting document search... (type empty line to exit)\n");
            await search.BasicDocumentSearchAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n⚠ Qdrant connection failed: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            
            if (ex.Message.Contains("Unauthenticated") || ex.Message.Contains("API key"))
            {
                Console.WriteLine("\nAuthentication required. Options:");
                Console.WriteLine("  1. Set QdrantApiKey in appsettings.json (for Aspire's Qdrant)");
                Console.WriteLine("  2. Start standalone Qdrant without auth:");
                Console.WriteLine("     docker run -p 6333:6333 -p 6334:6334 --name qdrant-standalone qdrant/qdrant");
            }
            else
            {
                Console.WriteLine("\nTo start Qdrant, run:");
                Console.WriteLine("  docker run -p 6333:6333 -p 6334:6334 --name qdrant-db qdrant/qdrant");
            }
            Console.ResetColor();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
