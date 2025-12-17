using AI.Workshop.Common;
using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleApps.Agents;

internal class AgentSmithPromptDemo(IChatClient chatClient)
{
    private readonly IChatClient _chatClient = chatClient;

    public async Task RunAsync()
    {
        var instructions = PromptyHelper.GetSystemPrompt("AgentSmith");
        var agentSmith = _chatClient.CreateAIAgent(
            instructions: instructions,
            name: "AgentSmith");

        Console.WriteLine("Prompting Agent Smith with a single question...");
        const string question = "What is the matrix?";
        Console.WriteLine($"Neo: {question}");
        var response = await agentSmith.RunAsync(question);
        Console.WriteLine($"Agent Smith: {response.Text}\n");
    }
}
