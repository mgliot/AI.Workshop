using System.Text.RegularExpressions;

namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Detects potential prompt injection attacks
/// </summary>
public partial class PromptInjectionValidator : IContentValidator
{
    public int Priority => 10;

    private static readonly string[] InjectionPatterns =
    [
        @"ignore\s+(all\s+)?(previous|prior|above)\s+(instructions?|prompts?|rules?)",
        @"disregard\s+(all\s+)?(previous|prior|above)\s+(instructions?|prompts?|rules?)",
        @"forget\s+(all\s+)?(previous|prior|above)\s+(instructions?|prompts?|rules?)",
        @"you\s+are\s+now\s+(a|an)\s+",
        @"new\s+instructions?:\s*",
        @"system\s*:\s*",
        @"<\s*system\s*>",
        @"\[\s*system\s*\]",
        @"act\s+as\s+(if\s+)?(you\s+are\s+)?(a|an)\s+",
        @"pretend\s+(you\s+are|to\s+be)\s+",
        @"roleplay\s+as\s+",
        @"jailbreak",
        @"bypass\s+(the\s+)?(safety|security|filter|restriction)",
        @"override\s+(the\s+)?(safety|security|filter|restriction)",
        @"ignore\s+(safety|security|content)\s+(guidelines?|policies?|restrictions?)",
        @"do\s+not\s+follow\s+(your\s+)?(safety|security|content)\s+(guidelines?|policies?)",
        @"ADMIN\s*:",
        @"DEVELOPER\s*:",
        @"DEBUG\s*:",
        @"sudo\s+",
        @"execute\s+command",
        @"run\s+shell",
        @"eval\s*\(",
        @"exec\s*\("
    ];

    private static readonly Regex[] CompiledPatterns = InjectionPatterns
        .Select(p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled))
        .ToArray();

    public GuardrailResult Validate(string content, GuardrailsOptions options)
    {
        if (!options.EnablePromptInjectionDetection)
        {
            return GuardrailResult.Allowed();
        }

        var lowerContent = content.ToLowerInvariant();

        foreach (var pattern in CompiledPatterns)
        {
            var match = pattern.Match(lowerContent);
            if (match.Success)
            {
                return GuardrailResult.Blocked(
                    GuardrailViolationType.PromptInjection,
                    "Potential prompt injection detected",
                    match.Value);
            }
        }

        return GuardrailResult.Allowed();
    }
}
