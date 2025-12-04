using AI.Workshop.ConsoleChat.RAG;

Console.WriteLine("Welcome to the AI Workshop Console Chat RAG examples!\r\n");

// Use the new Agent Navigator for interactive prompt/agent selection
var navigator = new AgentNavigator();
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
