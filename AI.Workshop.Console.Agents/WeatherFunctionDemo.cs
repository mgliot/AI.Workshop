using AI.Workshop.Common;
using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleApps.Agents;

internal class WeatherFunctionDemo(IChatClient chatClient)
{
    private readonly IChatClient _chatClient = chatClient;

    public async Task RunAsync()
    {
        var instructions = PromptyHelper.GetSystemPrompt("WeatherAssistant");
        var weatherAgent = _chatClient.CreateAIAgent(
            instructions: instructions,
            tools: [AIFunctionFactory.Create(WeatherTools.GetWeather)]);

        Console.WriteLine("Weather agent will call the GetWeather function to answer the user.");
        const string locationPrompt = "What's the weather like in Madrid?";
        Console.WriteLine($"User: {locationPrompt}");
        var weatherResponse = await weatherAgent.RunAsync(locationPrompt);
        Console.WriteLine($"Weather Agent: {weatherResponse.Text}\n");
    }
}
