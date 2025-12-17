using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Workshop.Common;
using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleApps.Agents;

internal partial class StructuredOutputDemo(IChatClient chatClient)
{
    private readonly IChatClient _chatClient = chatClient;

    // Static JsonSerializerOptions to reuse
    private static readonly JsonSerializerOptions s_serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task RunAsync()
    {
        var instructions = PromptyHelper.GetSystemPrompt("PersonInfo");

        var structuredAgent = _chatClient.CreateAIAgent(
            instructions: instructions,
            name: "PersonInfoAgent");

        var response = await structuredAgent.RunAsync("Tell me about Neo from The Matrix.");

        Console.WriteLine($"Raw response: {response.Text}\n");

        var jsonText = ExtractAndCleanJson(response.Text);

        using var document = JsonDocument.Parse(jsonText);
        var payload = document.RootElement;

        var personInfo = JsonSerializer.Deserialize<PersonInfo>(payload.GetRawText(), s_serializerOptions);

        var name = personInfo?.Name ?? ReadAsString(payload, "name") ?? string.Empty;
        var age = personInfo?.Age?.ToString() ?? ReadAsString(payload, "age") ?? string.Empty;
        var occupation = personInfo?.Occupation ?? ReadAsString(payload, "occupation") ?? string.Empty;

        Console.WriteLine("Parsed structured output:");
        Console.WriteLine($"Name: {name}, Age: {age}, Occupation: {occupation}\n");
    }

    private static string ExtractAndCleanJson(string text)
    {
        var jsonText = text.Trim();

        // Extract JSON if wrapped in markdown code blocks
        if (jsonText.StartsWith("```"))
        {
            // Split by both Windows and Unix line endings
            var lines = jsonText.Split(["\r\n", "\n"], StringSplitOptions.None);

            // Remove first line (```json or ```) and last line (```
            jsonText = string.Join("\n", lines.Skip(1).SkipLast(1));
            jsonText = jsonText.Trim();
        }

        // Clean up inline comments more carefully
        // For numbers: 33 (estimated) -> 33
        jsonText = InlineCommentNumberRegex().Replace(jsonText, "$1");

        // For strings within quotes: "text (comment)" -> "text"
        jsonText = InlineCommentStringRegex().Replace(jsonText, "\"$1\"$2");

        jsonText = BalanceCurlyBraces(jsonText);

        return jsonText.Trim();
    }

    private static string BalanceCurlyBraces(string jsonText)
    {
        var openCount = 0;
        var closeCount = 0;
        foreach (var ch in jsonText)
        {
            if (ch == '{')
            {
                openCount++;
            }
            else if (ch == '}')
            {
                closeCount++;
            }
        }

        if (openCount > closeCount)
        {
            jsonText += new string('}', openCount - closeCount);
        }

        return jsonText;
    }

    private static string? ReadAsString(JsonElement element, string property)
    {
        if (!TryGetPropertyCaseInsensitive(element, property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True or JsonValueKind.False => value.GetBoolean().ToString(),
            JsonValueKind.Null => null,
            _ => value.GetRawText()
        };
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = candidate.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    [GeneratedRegex(@"(\d+)\s+\([^)]+\)")]
    private static partial Regex InlineCommentNumberRegex();

    [GeneratedRegex("\"([^\"]+?)\\s+\\([^)]+\\)\"(\\s*[,}\\]])")]
    private static partial Regex InlineCommentStringRegex();
}
