using AI.Workshop.ConsoleAgent;
using Microsoft.Extensions.AI;
using OllamaSharp;

var ollamaUri = new Uri("http://localhost:11434/");
var ollamaModel = "llama3.2";

IChatClient chatClient = new OllamaApiClient(ollamaUri, ollamaModel);

var writers = new GhostWriterAgents();
await writers.RunAsync(chatClient);

//var matrix = new MatrixAgents();
//await matrix.GivePromptAsync(chatClient);
//await matrix.MultiTurnConversationAsync(chatClient);
//await matrix.FunctionCallingAsync(chatClient);
//await matrix.StructuredOutputAsync(chatClient);
//await matrix.UseAgentAsToolAsync(chatClient);

