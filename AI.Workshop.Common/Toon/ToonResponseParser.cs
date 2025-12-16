using Microsoft.Extensions.AI;
using System.Text.RegularExpressions;

namespace AI.Workshop.Common.Toon;

/// <summary>
/// Parses TOON-formatted responses from LLMs into structured objects.
/// Handles extraction of TOON from mixed text responses.
/// </summary>
public static partial class ToonResponseParser
{
    // Regex patterns for extracting TOON from responses
    private static readonly Regex ToonCodeBlockPattern = ToonCodeBlockRegex();
    private static readonly Regex GenericCodeBlockPattern = GenericCodeBlockRegex();

    /// <summary>
    /// Extracts and parses TOON data from an LLM response
    /// </summary>
    /// <typeparam name="T">Target type to deserialize to</typeparam>
    /// <param name="response">LLM response text</param>
    /// <returns>Parsed result with extracted data and metadata</returns>
    public static ToonParseResult<T> Parse<T>(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return ToonParseResult<T>.Failed("Response is empty");
        }

        // Try to extract TOON from code block first
        var toonContent = ExtractToonFromCodeBlock(response);
        
        if (toonContent == null)
        {
            // Try to extract from generic code block
            toonContent = ExtractFromGenericCodeBlock(response);
        }

        if (toonContent == null)
        {
            // Try to parse the entire response as TOON
            toonContent = response.Trim();
        }

        // Attempt deserialization
        if (ToonHelper.TryFromToon<T>(toonContent, out var result))
        {
            return ToonParseResult<T>.Success(result!, toonContent, GetNonToonText(response, toonContent));
        }

        return ToonParseResult<T>.Failed($"Failed to parse TOON content", toonContent);
    }

    /// <summary>
    /// Extracts and parses TOON data from a ChatResponse
    /// </summary>
    /// <typeparam name="T">Target type to deserialize to</typeparam>
    /// <param name="response">Chat response</param>
    /// <returns>Parsed result with extracted data and metadata</returns>
    public static ToonParseResult<T> Parse<T>(ChatResponse response)
    {
        var text = response.Text ?? string.Empty;
        return Parse<T>(text);
    }

    /// <summary>
    /// Tries to extract and parse TOON data from a response
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="response">LLM response text</param>
    /// <param name="result">Parsed object if successful</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool TryParse<T>(string response, out T? result)
    {
        var parseResult = Parse<T>(response);
        result = parseResult.Data;
        return parseResult.IsSuccess;
    }

    /// <summary>
    /// Tries to extract and parse TOON data from a ChatResponse
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="response">Chat response</param>
    /// <param name="result">Parsed object if successful</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool TryParse<T>(ChatResponse response, out T? result)
    {
        return TryParse(response.Text ?? string.Empty, out result);
    }

    /// <summary>
    /// Extracts all TOON blocks from a response
    /// </summary>
    /// <param name="response">LLM response text</param>
    /// <returns>List of TOON content blocks</returns>
    public static IReadOnlyList<string> ExtractAllToonBlocks(string response)
    {
        var blocks = new List<string>();

        // Extract from ```toon blocks
        var toonMatches = ToonCodeBlockPattern.Matches(response);
        foreach (Match match in toonMatches)
        {
            if (match.Groups[1].Success)
            {
                blocks.Add(match.Groups[1].Value.Trim());
            }
        }

        return blocks;
    }

    /// <summary>
    /// Parses multiple TOON blocks from a response into a list of objects
    /// </summary>
    /// <typeparam name="T">Target type for each block</typeparam>
    /// <param name="response">LLM response text</param>
    /// <returns>List of parsed objects</returns>
    public static IReadOnlyList<ToonParseResult<T>> ParseAll<T>(string response)
    {
        var blocks = ExtractAllToonBlocks(response);
        var results = new List<ToonParseResult<T>>();

        foreach (var block in blocks)
        {
            if (ToonHelper.TryFromToon<T>(block, out var data))
            {
                results.Add(ToonParseResult<T>.Success(data!, block));
            }
            else
            {
                results.Add(ToonParseResult<T>.Failed("Failed to parse block", block));
            }
        }

        // If no blocks found, try parsing entire response
        if (results.Count == 0)
        {
            results.Add(Parse<T>(response));
        }

        return results;
    }

    /// <summary>
    /// Creates a system prompt that instructs the LLM to respond in TOON format
    /// </summary>
    /// <param name="schema">Description of expected response structure</param>
    /// <param name="additionalInstructions">Additional instructions to include</param>
    /// <returns>System prompt for TOON responses</returns>
    public static string CreateToonResponsePrompt(string schema, string? additionalInstructions = null)
    {
        var prompt = $"""
            Respond using TOON (Token-Oriented Object Notation) format.
            TOON is a compact format similar to YAML but more token-efficient.

            Expected response structure:
            {schema}

            TOON format rules:
            - Use key: value pairs on separate lines
            - Use indentation (2 spaces) for nested objects
            - Arrays use [count]: value1,value2,value3 format
            - No quotes needed for simple strings
            - Wrap response in ```toon code block
            """;

        if (!string.IsNullOrEmpty(additionalInstructions))
        {
            prompt += $"\n\n{additionalInstructions}";
        }

        return prompt;
    }

    /// <summary>
    /// Creates a system prompt with an example TOON response
    /// </summary>
    /// <typeparam name="T">Type to use for example</typeparam>
    /// <param name="example">Example object to serialize as TOON</param>
    /// <param name="description">Description of what the response should contain</param>
    /// <returns>System prompt with TOON example</returns>
    public static string CreateToonResponsePromptWithExample<T>(T example, string description)
    {
        var exampleToon = ToonHelper.ToToon(example);

        return $"""
            Respond using TOON (Token-Oriented Object Notation) format.
            {description}

            Example response format:
            ```toon
            {exampleToon}
            ```

            Always wrap your TOON response in ```toon code blocks.
            """;
    }

    private static string? ExtractToonFromCodeBlock(string response)
    {
        var match = ToonCodeBlockPattern.Match(response);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractFromGenericCodeBlock(string response)
    {
        var match = GenericCodeBlockPattern.Match(response);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? GetNonToonText(string fullResponse, string toonContent)
    {
        // Try to find and remove the TOON block to get surrounding text
        var toonBlockMatch = ToonCodeBlockPattern.Match(fullResponse);
        if (toonBlockMatch.Success)
        {
            var before = fullResponse[..toonBlockMatch.Index].Trim();
            var after = fullResponse[(toonBlockMatch.Index + toonBlockMatch.Length)..].Trim();
            var combined = $"{before} {after}".Trim();
            return string.IsNullOrEmpty(combined) ? null : combined;
        }

        return null;
    }

    [GeneratedRegex(@"```toon\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase)]
    private static partial Regex ToonCodeBlockRegex();

    [GeneratedRegex(@"```\s*([\s\S]*?)\s*```")]
    private static partial Regex GenericCodeBlockRegex();
}

/// <summary>
/// Result of parsing TOON from an LLM response
/// </summary>
/// <typeparam name="T">Type of parsed data</typeparam>
public class ToonParseResult<T>
{
    /// <summary>
    /// Whether parsing was successful
    /// </summary>
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// The parsed data (null if parsing failed)
    /// </summary>
    public T? Data { get; private init; }

    /// <summary>
    /// The raw TOON content that was parsed
    /// </summary>
    public string? RawToon { get; private init; }

    /// <summary>
    /// Any non-TOON text from the response (explanations, etc.)
    /// </summary>
    public string? AdditionalText { get; private init; }

    /// <summary>
    /// Error message if parsing failed
    /// </summary>
    public string? Error { get; private init; }

    private ToonParseResult() { }

    /// <summary>
    /// Creates a successful parse result
    /// </summary>
    public static ToonParseResult<T> Success(T data, string rawToon, string? additionalText = null)
    {
        return new ToonParseResult<T>
        {
            IsSuccess = true,
            Data = data,
            RawToon = rawToon,
            AdditionalText = additionalText
        };
    }

    /// <summary>
    /// Creates a failed parse result
    /// </summary>
    public static ToonParseResult<T> Failed(string error, string? rawToon = null)
    {
        return new ToonParseResult<T>
        {
            IsSuccess = false,
            Error = error,
            RawToon = rawToon
        };
    }

    /// <summary>
    /// Gets the data or throws if parsing failed
    /// </summary>
    public T GetDataOrThrow()
    {
        if (!IsSuccess || Data == null)
        {
            throw new InvalidOperationException(Error ?? "Parsing failed");
        }
        return Data;
    }

    /// <summary>
    /// Gets the data or a default value if parsing failed
    /// </summary>
    public T? GetDataOrDefault(T? defaultValue = default)
    {
        return IsSuccess ? Data : defaultValue;
    }
}
