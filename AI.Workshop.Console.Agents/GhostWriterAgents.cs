using AI.Workshop.Common;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace AI.Workshop.ConsoleApps.Agents;

/// <summary>
/// Documentation:
/// https://devblogs.microsoft.com/dotnet/introducing-microsoft-agent-framework-preview
/// </summary>
internal class GhostWriterAgents
{
    internal async Task RunAsync(IChatClient chatClient)
    {
        Console.WriteLine("Configuring Ghost Writer workflow (Writer -> Editor)...");
        var writerInstructions = PromptyHelper.GetSystemPrompt("StoryWriter");
        AIAgent writer = new ChatClientAgent(
            chatClient,
            new ChatClientAgentOptions
            {
                Name = "Writer",
                ChatOptions = new ChatOptions
                {
                    Instructions = writerInstructions,
                    Tools = [
                        AIFunctionFactory.Create(GetAuthor),
                        AIFunctionFactory.Create(FormatStory)
                    ],
                }
            });

        var editorInstructions = PromptyHelper.GetSystemPrompt("StoryEditor");
        AIAgent editor = new ChatClientAgent(
            chatClient,
            new ChatClientAgentOptions
            {
                Name = "Editor",
                ChatOptions = new ChatOptions
                {
                    Instructions = editorInstructions
                }
            });

        // Create a workflow that connects writer to editor
        Workflow workflow =
            AgentWorkflowBuilder
                .BuildSequential(writer, editor);

        AIAgent workflowAgent = workflow.AsAgent();

        const string storyPrompt = "Write a short story about a haunted house.";
        Console.WriteLine($"Running workflow prompt: {storyPrompt}");
        AgentRunResponse workflowResponse =
            await workflowAgent.RunAsync(storyPrompt);

        Console.WriteLine("Ghost Writer result:\n");
        Console.WriteLine(workflowResponse.Text);
    }


    [Description("Gets the author of the story.")]
    string GetAuthor() => "Jack Torrance";

    [Description("Formats the story for display.")]
    string FormatStory(string title, string author, string story) =>
        $"Title: {title}\nAuthor: {author}\n\n{story}";
}
