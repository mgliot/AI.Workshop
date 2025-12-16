using AI.Workshop.Common;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Register Ollama chat client via DI (uses settings from appsettings.json)
builder.Services.AddOllamaChatClient(builder.Configuration);

var writerInstructions = PromptyHelper.GetSystemPrompt("StoryWriter");
builder.AddAIAgent("Writer", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    return new ChatClientAgent(
        chatClient,
        name: key,
        instructions: writerInstructions,
        tools: [
            AIFunctionFactory.Create(GetAuthor),
            AIFunctionFactory.Create(FormatStory)
        ]
    );
});

var editorInstructions = PromptyHelper.GetSystemPrompt("StoryEditor");
builder.AddAIAgent(
    name: "Editor",
    instructions: editorInstructions);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseHttpsRedirection();

app.MapGet("/agent/chat", async (
    [FromKeyedServices("Writer")] AIAgent writer,
    [FromKeyedServices("Editor")] AIAgent editor,
    HttpContext context,
    string prompt) =>
{
    // Build a sequential workflow: Writer -> Editor
    Workflow workflow = AgentWorkflowBuilder.BuildSequential(writer, editor);

    AIAgent workflowAgent = workflow.AsAgent();

    AgentRunResponse response = await workflowAgent.RunAsync(prompt);
    return Results.Ok(response);
});

app.Run();

[Description("Gets the author of the story.")]
string GetAuthor() => "Jack Torrance";

[Description("Formats the story for display.")]
string FormatStory(string title, string author, string story) =>
    $"Title: {title}\nAuthor: {author}\n\n{story}";