using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace AI.Workshop.ConsoleAgent;

/// <summary>
/// Documentation:
/// https://devblogs.microsoft.com/dotnet/introducing-microsoft-agent-framework-preview
/// </summary>
internal class GhostWriterAgents
{
    internal async Task RunAsync(IChatClient chatClient)
    {
        var writerInstructions = PromptyHelper.GetSystemPrompt("StoryWriter");
        AIAgent writer = new ChatClientAgent(
            chatClient,
            new ChatClientAgentOptions
            {
                Name = "Writer",
                Instructions = writerInstructions,
                ChatOptions = new ChatOptions
                {
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
                Instructions = editorInstructions
            });

        // Create a workflow that connects writer to editor
        Workflow workflow =
            AgentWorkflowBuilder
                .BuildSequential(writer, editor);

        AIAgent workflowAgent = workflow.AsAgent();

        AgentRunResponse workflowResponse =
            await workflowAgent.RunAsync("Write a short story about a haunted house.");

        Console.WriteLine(workflowResponse.Text);
    }


    [Description("Gets the author of the story.")]
    string GetAuthor() => "Jack Torrance";

    [Description("Formats the story for display.")]
    string FormatStory(string title, string author, string story) =>
        $"Title: {title}\nAuthor: {author}\n\n{story}";
}
