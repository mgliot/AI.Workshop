using Microsoft.Extensions.AI;

namespace AI.Workshop.Common.Toon;

/// <summary>
/// Extension methods for using TOON with IChatClient
/// </summary>
public static class ToonChatExtensions
{
    /// <summary>
    /// Sends a chat request with data formatted as TOON to reduce token usage
    /// </summary>
    /// <typeparam name="T">Type of data to include</typeparam>
    /// <param name="client">Chat client</param>
    /// <param name="data">Data to include in the prompt as TOON</param>
    /// <param name="userPrompt">User's question or instruction</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="dataLabel">Label for the data section</param>
    /// <param name="options">Chat options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chat response</returns>
    public static async Task<ChatResponse> GetResponseWithToonDataAsync<T>(
        this IChatClient client,
        T data,
        string userPrompt,
        string? systemPrompt = null,
        string dataLabel = "Data",
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>();

        // Add system prompt if provided
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        }

        // Build user message with TOON-formatted data
        var toonData = ToonHelper.FormatForPrompt(data, dataLabel);
        var fullUserMessage = $"{toonData}\n\n{userPrompt}";
        
        messages.Add(new ChatMessage(ChatRole.User, fullUserMessage));

        return await client.GetResponseAsync(messages, options, cancellationToken);
    }

    /// <summary>
    /// Sends a chat request with data as a TOON code block for clarity
    /// </summary>
    /// <typeparam name="T">Type of data to include</typeparam>
    /// <param name="client">Chat client</param>
    /// <param name="data">Data to include in the prompt as TOON</param>
    /// <param name="userPrompt">User's question or instruction</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="dataLabel">Label for the data section</param>
    /// <param name="options">Chat options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chat response</returns>
    public static async Task<ChatResponse> GetResponseWithToonCodeBlockAsync<T>(
        this IChatClient client,
        T data,
        string userPrompt,
        string? systemPrompt = null,
        string dataLabel = "Here is the data in TOON format:",
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>();

        // Add system prompt if provided
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        }

        // Build user message with TOON code block
        var toonBlock = ToonHelper.FormatAsCodeBlock(data, dataLabel);
        var fullUserMessage = $"{toonBlock}\n\n{userPrompt}";
        
        messages.Add(new ChatMessage(ChatRole.User, fullUserMessage));

        return await client.GetResponseAsync(messages, options, cancellationToken);
    }

    /// <summary>
    /// Creates a ChatMessage with data formatted as TOON
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="role">Message role</param>
    /// <param name="data">Data to include</param>
    /// <param name="additionalText">Additional text to include after the TOON data</param>
    /// <param name="dataLabel">Label for the data section</param>
    /// <returns>ChatMessage with TOON-formatted content</returns>
    public static ChatMessage CreateToonMessage<T>(
        ChatRole role,
        T data,
        string? additionalText = null,
        string? dataLabel = null)
    {
        var toonData = ToonHelper.FormatForPrompt(data, dataLabel);
        var content = string.IsNullOrEmpty(additionalText)
            ? toonData
            : $"{toonData}\n\n{additionalText}";

        return new ChatMessage(role, content);
    }

    /// <summary>
    /// Adds TOON-formatted data to an existing message list
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="messages">Message list to add to</param>
    /// <param name="role">Message role</param>
    /// <param name="data">Data to include</param>
    /// <param name="additionalText">Additional text to include</param>
    /// <param name="dataLabel">Label for the data section</param>
    /// <returns>The message list for chaining</returns>
    public static IList<ChatMessage> AddToonMessage<T>(
        this IList<ChatMessage> messages,
        ChatRole role,
        T data,
        string? additionalText = null,
        string? dataLabel = null)
    {
        messages.Add(CreateToonMessage(role, data, additionalText, dataLabel));
        return messages;
    }

    /// <summary>
    /// Parses TOON data from a ChatResponse
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="response">Chat response to parse</param>
    /// <returns>Parse result with data and metadata</returns>
    public static ToonParseResult<T> ParseToon<T>(this ChatResponse response)
    {
        return ToonResponseParser.Parse<T>(response);
    }

    /// <summary>
    /// Tries to parse TOON data from a ChatResponse
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="response">Chat response to parse</param>
    /// <param name="result">Parsed data if successful</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool TryParseToon<T>(this ChatResponse response, out T? result)
    {
        return ToonResponseParser.TryParse(response, out result);
    }

    /// <summary>
    /// Gets structured TOON data from a ChatResponse, throwing if parsing fails
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="response">Chat response to parse</param>
    /// <returns>Parsed data</returns>
    /// <exception cref="InvalidOperationException">Thrown if parsing fails</exception>
    public static T GetToonData<T>(this ChatResponse response)
    {
        return ToonResponseParser.Parse<T>(response).GetDataOrThrow();
    }

    /// <summary>
    /// Gets structured TOON data from a ChatResponse, or default if parsing fails
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="response">Chat response to parse</param>
    /// <param name="defaultValue">Default value if parsing fails</param>
    /// <returns>Parsed data or default</returns>
    public static T? GetToonDataOrDefault<T>(this ChatResponse response, T? defaultValue = default)
    {
        return ToonResponseParser.Parse<T>(response).GetDataOrDefault(defaultValue);
    }

    /// <summary>
    /// Sends a request expecting a TOON response and parses it
    /// </summary>
    /// <typeparam name="TResponse">Expected response type</typeparam>
    /// <param name="client">Chat client</param>
    /// <param name="prompt">User prompt</param>
    /// <param name="responseSchema">Description of expected response structure</param>
    /// <param name="options">Chat options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parse result with structured data</returns>
    public static async Task<ToonParseResult<TResponse>> GetStructuredResponseAsync<TResponse>(
        this IChatClient client,
        string prompt,
        string responseSchema,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, ToonResponseParser.CreateToonResponsePrompt(responseSchema)),
            new(ChatRole.User, prompt)
        };

        var response = await client.GetResponseAsync(messages, options, cancellationToken);
        return response.ParseToon<TResponse>();
    }

    /// <summary>
    /// Sends a request with an example response format and parses the result
    /// </summary>
    /// <typeparam name="TResponse">Expected response type</typeparam>
    /// <param name="client">Chat client</param>
    /// <param name="prompt">User prompt</param>
    /// <param name="example">Example of expected response</param>
    /// <param name="description">Description of what the response should contain</param>
    /// <param name="options">Chat options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parse result with structured data</returns>
    public static async Task<ToonParseResult<TResponse>> GetStructuredResponseWithExampleAsync<TResponse>(
        this IChatClient client,
        string prompt,
        TResponse example,
        string description,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, ToonResponseParser.CreateToonResponsePromptWithExample(example, description)),
            new(ChatRole.User, prompt)
        };

        var response = await client.GetResponseAsync(messages, options, cancellationToken);
        return response.ParseToon<TResponse>();
    }
}
