using AI.Workshop.Common;
using AI.Workshop.ConsoleApps.Agents;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Build host with DI container
var builder = Host.CreateApplicationBuilder(args);

// Register Ollama chat client via DI
builder.Services.AddOllamaChatClient(builder.Configuration);

// Register our agent classes
builder.Services.AddTransient<GhostWriterAgents>();
builder.Services.AddTransient<MatrixAgents>();

var host = builder.Build();

// Run the agent demo
var chatClient = host.Services.GetRequiredService<IChatClient>();

var writers = host.Services.GetRequiredService<GhostWriterAgents>();
await writers.RunAsync(chatClient);

//var matrix = host.Services.GetRequiredService<MatrixAgents>();
//await matrix.GivePromptAsync(chatClient);
//await matrix.MultiTurnConversationAsync(chatClient);
//await matrix.FunctionCallingAsync(chatClient);
//await matrix.StructuredOutputAsync(chatClient);
//await matrix.UseAgentAsToolAsync(chatClient);

