using Microsoft.Extensions.AI;

namespace AI.Workshop.Common.Toon;

/// <summary>
/// Fluent builder for constructing prompts with TOON-formatted data
/// </summary>
public class ToonPromptBuilder
{
    private string? _systemPrompt;
    private readonly List<DataSection> _dataSections = [];
    private string? _userPrompt;
    private bool _useCodeBlocks;

    /// <summary>
    /// Creates a new TOON prompt builder
    /// </summary>
    public static ToonPromptBuilder Create() => new();

    /// <summary>
    /// Sets the system prompt
    /// </summary>
    /// <param name="systemPrompt">System prompt text</param>
    /// <returns>Builder for chaining</returns>
    public ToonPromptBuilder WithSystemPrompt(string systemPrompt)
    {
        _systemPrompt = systemPrompt;
        return this;
    }

    /// <summary>
    /// Adds data to the prompt in TOON format
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="data">Data to include</param>
    /// <param name="label">Optional label for the data section</param>
    /// <returns>Builder for chaining</returns>
    public ToonPromptBuilder WithData<T>(T data, string? label = null)
    {
        var toon = ToonHelper.ToToon(data);
        _dataSections.Add(new DataSection(label, toon));
        return this;
    }

    /// <summary>
    /// Adds multiple data items with a shared label
    /// </summary>
    /// <typeparam name="T">Type of data items</typeparam>
    /// <param name="items">Collection of items</param>
    /// <param name="label">Label for the collection</param>
    /// <returns>Builder for chaining</returns>
    public ToonPromptBuilder WithDataCollection<T>(IEnumerable<T> items, string? label = null)
    {
        var wrapper = new { items = items.ToArray() };
        var toon = ToonHelper.ToToon(wrapper);
        _dataSections.Add(new DataSection(label, toon));
        return this;
    }

    /// <summary>
    /// Sets the user prompt/question
    /// </summary>
    /// <param name="userPrompt">User prompt text</param>
    /// <returns>Builder for chaining</returns>
    public ToonPromptBuilder WithUserPrompt(string userPrompt)
    {
        _userPrompt = userPrompt;
        return this;
    }

    /// <summary>
    /// Wraps TOON data in code blocks for better formatting
    /// </summary>
    /// <param name="useCodeBlocks">Whether to use code blocks</param>
    /// <returns>Builder for chaining</returns>
    public ToonPromptBuilder UseCodeBlocks(bool useCodeBlocks = true)
    {
        _useCodeBlocks = useCodeBlocks;
        return this;
    }

    /// <summary>
    /// Builds the list of chat messages
    /// </summary>
    /// <returns>List of ChatMessage objects</returns>
    public List<ChatMessage> Build()
    {
        var messages = new List<ChatMessage>();

        // Add system prompt if set
        if (!string.IsNullOrEmpty(_systemPrompt))
        {
            messages.Add(new ChatMessage(ChatRole.System, _systemPrompt));
        }

        // Build user message with data sections
        var userContent = BuildUserContent();
        if (!string.IsNullOrEmpty(userContent))
        {
            messages.Add(new ChatMessage(ChatRole.User, userContent));
        }

        return messages;
    }

    /// <summary>
    /// Builds just the user message content
    /// </summary>
    /// <returns>Formatted user message content</returns>
    public string BuildUserContent()
    {
        var parts = new List<string>();

        // Add data sections
        foreach (var section in _dataSections)
        {
            if (_useCodeBlocks)
            {
                var prefix = string.IsNullOrEmpty(section.Label) ? "" : $"{section.Label}\n";
                parts.Add($"{prefix}```toon\n{section.Toon}\n```");
            }
            else
            {
                if (!string.IsNullOrEmpty(section.Label))
                {
                    parts.Add($"{section.Label}:\n{section.Toon}");
                }
                else
                {
                    parts.Add(section.Toon);
                }
            }
        }

        // Add user prompt
        if (!string.IsNullOrEmpty(_userPrompt))
        {
            parts.Add(_userPrompt);
        }

        return string.Join("\n\n", parts);
    }

    /// <summary>
    /// Gets estimated token savings from using TOON
    /// </summary>
    /// <returns>Combined token savings info</returns>
    public ToonPromptSavings GetEstimatedSavings()
    {
        var jsonLength = 0;
        var toonLength = 0;

        foreach (var section in _dataSections)
        {
            // Convert TOON back to JSON for comparison
            var json = ToonHelper.ToonToJson(section.Toon);
            jsonLength += json.Length;
            toonLength += section.Toon.Length;
        }

        // Add labels and formatting overhead
        var formattedToon = BuildUserContent();
        toonLength = formattedToon.Length;

        var savings = jsonLength > 0 
            ? (1 - (double)toonLength / jsonLength) * 100 
            : 0;

        return new ToonPromptSavings(
            JsonEquivalentLength: jsonLength,
            ToonLength: toonLength,
            EstimatedSavingsPercent: savings);
    }

    private record DataSection(string? Label, string Toon);
}

/// <summary>
/// Token savings information for a TOON prompt
/// </summary>
public record ToonPromptSavings(
    int JsonEquivalentLength,
    int ToonLength,
    double EstimatedSavingsPercent)
{
    public override string ToString() =>
        $"JSON equivalent: {JsonEquivalentLength} chars, TOON: {ToonLength} chars, Savings: ~{EstimatedSavingsPercent:F1}%";
}
