using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.Workshop.ConsoleAgent;

/// <summary>
/// Documentation:
/// Demo by Evan Gudmestad, with my gratitude
/// https://www.youtube.com/watch?v=GqZo5XvHoH8
/// https://github.com/EvanGudmestad/MAFQuickStart
/// </summary>
internal class MatrixAgents
{
    internal async Task GivePromptAsync(IChatClient chatClient)
    {
        var instructions = PromptyHelper.GetSystemPrompt("AgentSmith");
        AIAgent agentSmith = chatClient.CreateAIAgent(
            instructions: instructions,
            name: "AgentSmith"
            );

        Console.WriteLine(await agentSmith.RunAsync("What is the matrix?"));
    }

    internal async Task MultiTurnConversationAsync(IChatClient chatClient)
    {
        var instructions = PromptyHelper.GetSystemPrompt("AgentSmith");
        AIAgent agentSmith = chatClient.CreateAIAgent(
            instructions: instructions,
            name: "AgentSmith"
            );

        AgentThread thread = agentSmith.GetNewThread();

        string neoInput = "I know what you are, Smith.";
        Console.WriteLine(await agentSmith.RunAsync(neoInput, thread));

        string neoInput2 = "You're just a program, and I'm not afraid of you anymore..";
        Console.WriteLine(await agentSmith.RunAsync(neoInput2, thread));
    }

    internal async Task FunctionCallingAsync(IChatClient chatClient)
    {
        var instructions = PromptyHelper.GetSystemPrompt("WeatherAssistant");
        AIAgent weatherAgent = chatClient.CreateAIAgent(
            instructions: instructions,
            tools: [AIFunctionFactory.Create(GetWeather)]
            );

        Console.WriteLine(await weatherAgent.RunAsync("What's the weather like in Varaždin?"));
    }

    [Description("Get the weather for a given location.")]
    static string GetWeather([Description("The location to get the weather for.")] string location)
   => $"The weather in {location} is cloudy with a high of 15°C.";

    internal async Task StructuredOutputAsync(IChatClient chatClient)
    {
        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(PersonInfo));  

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(schema, nameof(PersonInfo),
                "Information about a person including their name, age, and occupation")
        };

        var instructions = PromptyHelper.GetSystemPrompt("PersonInfo");
        AIAgent structuredAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions()
        {
            Name = "PersonInfoAgent",
            Instructions = instructions,
            ChatOptions = chatOptions
        });

        var response = await structuredAgent.RunAsync("Tell me about Neo from The Matrix.");
        var personInfo = JsonSerializer.Deserialize<PersonInfo>(response.Text);
        Console.WriteLine($"Name: {personInfo?.Name}, Age: {personInfo?.Age}, Occupation: {personInfo?.Occupation}");
    }

    internal async Task UseAgentAsToolAsync(IChatClient chatClient)
    {
        var weatherInstructions = PromptyHelper.GetSystemPrompt("WeatherAssistant");
        AIAgent weatherAgent = chatClient.CreateAIAgent(
            instructions: weatherInstructions,
            tools: [AIFunctionFactory.Create(GetWeather)]
        );

        var croatianInstructions = PromptyHelper.GetSystemPrompt("CroatianTranslator");
        AIAgent croatianAgent = chatClient.CreateAIAgent(
            instructions: croatianInstructions,
            name: "CroatianTranslator",
            tools: [weatherAgent.AsAIFunction()]
            );

        Console.WriteLine(await croatianAgent.RunAsync("What is the weather in Varaždin?"));
    }
}

internal record PersonInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }
}
