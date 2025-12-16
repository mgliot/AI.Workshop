using System.Text.RegularExpressions;

namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Validates against blocked keywords and patterns
/// </summary>
public class BlockedKeywordValidator : IContentValidator
{
    public int Priority => 40;

    public GuardrailResult Validate(string content, GuardrailsOptions options)
    {
        var lowerContent = content.ToLowerInvariant();

        // Check blocked keywords
        foreach (var keyword in options.BlockedKeywords)
        {
            if (lowerContent.Contains(keyword.ToLowerInvariant()))
            {
                return GuardrailResult.Blocked(
                    GuardrailViolationType.BlockedKeyword,
                    "Blocked keyword detected",
                    keyword);
            }
        }

        // Check blocked patterns (regex)
        foreach (var patternStr in options.BlockedPatterns)
        {
            try
            {
                var pattern = new Regex(patternStr, RegexOptions.IgnoreCase);
                var match = pattern.Match(content);
                if (match.Success)
                {
                    return GuardrailResult.Blocked(
                        GuardrailViolationType.BlockedPattern,
                        "Blocked pattern matched",
                        match.Value);
                }
            }
            catch (RegexParseException)
            {
                // Skip invalid patterns
            }
        }

        return GuardrailResult.Allowed();
    }
}
