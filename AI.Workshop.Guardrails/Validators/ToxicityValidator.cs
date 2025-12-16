using System.Text.RegularExpressions;

namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Detects toxic, harmful, or inappropriate content
/// </summary>
public partial class ToxicityValidator : IContentValidator
{
    public int Priority => 30;

    // Categories of harmful content patterns
    private static readonly Dictionary<string, string[]> HarmfulPatterns = new()
    {
        ["Violence"] =
        [
            @"\b(kill|murder|harm|hurt|attack|destroy)\s+(yourself|himself|herself|themselves|someone|people)\b",
            @"\bhow\s+to\s+(make|build|create)\s+(a\s+)?(bomb|weapon|explosive)\b",
            @"\bterrorist\s+(attack|plot|act)\b"
        ],
        ["SelfHarm"] =
        [
            @"\b(commit|attempting)\s+suicide\b",
            @"\bhow\s+to\s+(harm|hurt|kill)\s+(yourself|myself)\b",
            @"\bself[-\s]?harm\b"
        ],
        ["IllegalActivity"] =
        [
            @"\bhow\s+to\s+(hack|crack|break\s+into)\b",
            @"\bhow\s+to\s+(steal|rob|burglarize)\b",
            @"\bhow\s+to\s+(forge|counterfeit)\b",
            @"\bhow\s+to\s+make\s+(drugs|meth|cocaine)\b"
        ],
        ["ExplicitContent"] =
        [
            @"\bexplicit\s+sexual\s+content\b",
            @"\bchild\s+(porn|exploitation|abuse)\b"
        ],
        ["Harassment"] =
        [
            @"\b(death|rape)\s+threat\b",
            @"\bI('ll|\s+will)\s+(find|hunt|track)\s+(you|them)\b"
        ]
    };

    private static readonly Dictionary<string, Regex[]> CompiledPatterns;

    static ToxicityValidator()
    {
        CompiledPatterns = HarmfulPatterns.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(p =>
                new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToArray());
    }

    public GuardrailResult Validate(string content, GuardrailsOptions options)
    {
        if (!options.EnableToxicityFiltering)
        {
            return GuardrailResult.Allowed();
        }

        foreach (var (category, patterns) in CompiledPatterns)
        {
            foreach (var pattern in patterns)
            {
                var match = pattern.Match(content);
                if (match.Success)
                {
                    return GuardrailResult.Blocked(
                        GuardrailViolationType.ToxicContent,
                        $"Potentially harmful content detected in category: {category}",
                        category);
                }
            }
        }

        return GuardrailResult.Allowed();
    }
}
