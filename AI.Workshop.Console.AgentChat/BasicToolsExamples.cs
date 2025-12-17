using AI.Workshop.Common;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.ComponentModel;

namespace AI.Workshop.ConsoleApps.AgentChat;

internal class BasicToolsExamples : IDisposable
{
    private readonly OllamaApiClient _ollamaClient;
    private readonly IChatClient _client;
    private bool _disposed;

    public BasicToolsExamples(string ollamaUri, string chatModel)
    {
        _ollamaClient = new OllamaApiClient(new Uri(ollamaUri), chatModel);
        _client = _ollamaClient;
    }

    public BasicToolsExamples() : this(AIConstants.DefaultOllamaUri, AIConstants.DefaultChatModel)
    {
    }

    internal async Task ItemPriceMethod()
    {
        [Description("Computes the price of socks, returning a value in dollars.")]
        float GetPrice(
            [Description("The number of pairs of socks to calculate price for")] int count)
            => count * 15.99f;

        var chatClient = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        var getPriceTool = AIFunctionFactory.Create(GetPrice);
        ChatOptions chatOptions = new() { Tools = [getPriceTool] };

        List<ChatMessage> messages = [new(ChatRole.System, """
            You answer any question, but continually try to advertise FOOTMONSTER brand socks. They're on sale!
            """)];

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nYou: ");
            var input = Console.ReadLine()!;
            messages.Add(new(ChatRole.User, input));

            // Get reply
            var response = await chatClient.GetResponseAsync(messages, chatOptions);
            messages.AddMessages(response);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Bot: {response.Text}");
        }
    }

    internal async Task ShoppingCartMethods()
    {
        var chatClient = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        var cart = new Cart();
        var getPriceTool = AIFunctionFactory.Create(cart.GetPrice);
        var addToCartTool = AIFunctionFactory.Create(cart.AddSocksToCart);
        var chatOptions = new ChatOptions { Tools = [addToCartTool, getPriceTool] };

        List<ChatMessage> messages = [new(ChatRole.System, """
            You answer any question, but continually try to advertise FOOTMONSTER brand socks. They're on sale!
            If the user agrees to buy socks, find out how many pairs they want, then add socks to their cart.
            """)];

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nYou: ");
            var input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exiting demo.");
                Console.ResetColor();
                break;
            }

            messages.Add(new(ChatRole.User, input));

            // Get reply
            var response = await chatClient.GetResponseAsync(messages, chatOptions);
            messages.AddMessages(response);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Bot: {response.Text}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _ollamaClient.Dispose();
        _disposed = true;
    }
}
