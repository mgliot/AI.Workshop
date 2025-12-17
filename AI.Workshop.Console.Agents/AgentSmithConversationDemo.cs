using AI.Workshop.Common;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleApps.Agents;

internal class AgentSmithConversationDemo(IChatClient chatClient)
{
    private readonly IChatClient _chatClient = chatClient;

    public async Task RunAsync()
    {
        var instructions = PromptyHelper.GetSystemPrompt("AgentSmith");
        var agentSmith = _chatClient.CreateAIAgent(
            instructions: instructions,
            name: "AgentSmith");

        AgentThread thread = agentSmith.GetNewThread();

        Console.WriteLine("Continuing the conversation thread with Agent Smith...");
        const string neoInput = "I know what you are, Smith.";
        Console.WriteLine($"Neo: {neoInput}");
        var smithResponse1 = await agentSmith.RunAsync(neoInput, thread);
        Console.WriteLine($"Agent Smith: {smithResponse1.Text}\n");

        const string neoInput2 = "You're just a program, and I'm not afraid of you anymore..";
        Console.WriteLine($"Neo: {neoInput2}");
        var smithResponse2 = await agentSmith.RunAsync(neoInput2, thread);
        Console.WriteLine($"Agent Smith: {smithResponse2.Text}\n");
    }
}
