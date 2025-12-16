using System.Text.Json;
using System.Text.Json.Nodes;
using ToonSharp;

namespace AI.Workshop.Common.Toon;

/// <summary>
/// Helper class for working with TOON (Token-Oriented Object Notation).
/// TOON reduces LLM token usage by 30-60% compared to JSON.
/// </summary>
public static class ToonHelper
{
    private static readonly ToonSerializerOptions DefaultOptions = new()
    {
        Strict = false,
        IndentSize = 2
    };

    private static readonly ToonSerializerOptions StrictOptions = new()
    {
        Strict = true,
        IndentSize = 2
    };

    /// <summary>
    /// Serializes an object to TOON format
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <param name="strict">Whether to use strict mode</param>
    /// <returns>TOON-formatted string</returns>
    public static string ToToon<T>(T obj, bool strict = false)
    {
        var options = strict ? StrictOptions : DefaultOptions;
        return ToonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// Deserializes a TOON string to an object
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="toon">TOON string</param>
    /// <param name="strict">Whether to use strict mode</param>
    /// <returns>Deserialized object</returns>
    public static T? FromToon<T>(string toon, bool strict = false)
    {
        var options = strict ? StrictOptions : DefaultOptions;
        return ToonSerializer.Deserialize<T>(toon, options);
    }

    /// <summary>
    /// Tries to deserialize a TOON string to an object
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="toon">TOON string</param>
    /// <param name="result">Deserialized object if successful</param>
    /// <param name="strict">Whether to use strict mode</param>
    /// <returns>True if successful, false otherwise</returns>
    public static bool TryFromToon<T>(string toon, out T? result, bool strict = false)
    {
        var options = strict ? StrictOptions : DefaultOptions;
        return ToonSerializer.TryDeserialize(toon, out result, options);
    }

    /// <summary>
    /// Deserializes a TOON string to a JsonNode for dynamic access
    /// </summary>
    /// <param name="toon">TOON string</param>
    /// <returns>JsonNode representing the data</returns>
    public static JsonNode? ToJsonNode(string toon)
    {
        return ToonSerializer.Deserialize(toon);
    }

    /// <summary>
    /// Converts a JSON string to TOON format
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <returns>TOON-formatted string</returns>
    public static string JsonToToon(string json)
    {
        var jsonNode = JsonNode.Parse(json);
        if (jsonNode == null)
        {
            return string.Empty;
        }

        // Deserialize JSON to object and re-serialize as TOON
        return ToonSerializer.Serialize(jsonNode);
    }

    /// <summary>
    /// Converts a TOON string to JSON format
    /// </summary>
    /// <param name="toon">TOON string</param>
    /// <param name="indented">Whether to indent the JSON output</param>
    /// <returns>JSON-formatted string</returns>
    public static string ToonToJson(string toon, bool indented = true)
    {
        var jsonNode = ToonSerializer.Deserialize(toon);
        if (jsonNode == null)
        {
            return "{}";
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };

        return jsonNode.ToJsonString(options);
    }

    /// <summary>
    /// Estimates the token savings of using TOON vs JSON for the given data
    /// </summary>
    /// <param name="data">Data to analyze</param>
    /// <returns>Token savings information</returns>
    public static TokenSavingsInfo EstimateTokenSavings<T>(T data)
    {
        var json = JsonSerializer.Serialize(data);
        var toon = ToToon(data);

        // Rough token estimation (average ~4 chars per token for English text)
        // This is a simplified estimate; actual tokenization varies by model
        var jsonTokens = EstimateTokenCount(json);
        var toonTokens = EstimateTokenCount(toon);

        var savedTokens = jsonTokens - toonTokens;
        var savingsPercent = jsonTokens > 0 
            ? (double)savedTokens / jsonTokens * 100 
            : 0;

        return new TokenSavingsInfo(
            JsonLength: json.Length,
            ToonLength: toon.Length,
            EstimatedJsonTokens: jsonTokens,
            EstimatedToonTokens: toonTokens,
            EstimatedSavedTokens: savedTokens,
            SavingsPercent: savingsPercent);
    }

    /// <summary>
    /// Formats data as TOON for inclusion in an LLM prompt
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="data">Data to format</param>
    /// <param name="label">Optional label for the data section</param>
    /// <returns>Formatted TOON string for prompt inclusion</returns>
    public static string FormatForPrompt<T>(T data, string? label = null)
    {
        var toon = ToToon(data);
        
        if (string.IsNullOrEmpty(label))
        {
            return toon;
        }

        return $"{label}:\n{toon}";
    }

    /// <summary>
    /// Wraps TOON data in a code block for prompt clarity
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="data">Data to format</param>
    /// <param name="label">Optional label before the code block</param>
    /// <returns>TOON data wrapped in a code block</returns>
    public static string FormatAsCodeBlock<T>(T data, string? label = null)
    {
        var toon = ToToon(data);
        var prefix = string.IsNullOrEmpty(label) ? "" : $"{label}\n";
        
        return $"{prefix}```toon\n{toon}\n```";
    }

    private static int EstimateTokenCount(string text)
    {
        // Simplified token estimation
        // GPT models average ~4 characters per token for English
        // Punctuation and special characters often count as separate tokens
        // This is a rough estimate for comparison purposes
        
        if (string.IsNullOrEmpty(text))
            return 0;

        // Count words (split by whitespace)
        var words = text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
        
        // Count punctuation/special chars that are often separate tokens
        var punctuation = text.Count(c => char.IsPunctuation(c) || c == '{' || c == '}' || c == '[' || c == ']');
        
        // Rough estimate: words + punctuation
        return words.Length + punctuation;
    }
}

/// <summary>
/// Information about token savings when using TOON vs JSON
/// </summary>
public record TokenSavingsInfo(
    int JsonLength,
    int ToonLength,
    int EstimatedJsonTokens,
    int EstimatedToonTokens,
    int EstimatedSavedTokens,
    double SavingsPercent)
{
    public override string ToString() =>
        $"JSON: {JsonLength} chars (~{EstimatedJsonTokens} tokens), " +
        $"TOON: {ToonLength} chars (~{EstimatedToonTokens} tokens), " +
        $"Savings: {EstimatedSavedTokens} tokens ({SavingsPercent:F1}%)";
}
