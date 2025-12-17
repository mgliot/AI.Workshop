using AI.Workshop.Common;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleApps.Agents;

internal class AgentAsToolDemo(IChatClient chatClient)
{
    private readonly IChatClient _chatClient = chatClient;

    public async Task RunAsync()
    {
        var weatherInstructions = PromptyHelper.GetSystemPrompt("WeatherAssistant");
        var weatherAgent = _chatClient.CreateAIAgent(
            instructions: weatherInstructions,
            tools: [AIFunctionFactory.Create(WeatherTools.GetWeather)]);

        var spanishInstructions = PromptyHelper.GetSystemPrompt("SpanishTranslator");
        var spanishAgent = _chatClient.CreateAIAgent(
            instructions: spanishInstructions,
            name: "SpanishTranslator",
            tools: [weatherAgent.AsAIFunction()]);

        Console.WriteLine("Using the translator agent to call the weather agent as a tool...");
        const string translatedPrompt = "What is the weather in Madrid?";
        Console.WriteLine($"User: {translatedPrompt}");
        var croatianResponse = await spanishAgent.RunAsync(translatedPrompt);
        Console.WriteLine($"Croatian Translator: {croatianResponse.Text}\n");
    }
}
