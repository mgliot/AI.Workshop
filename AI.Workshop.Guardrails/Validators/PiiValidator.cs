using System.Text.RegularExpressions;

namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Detects personally identifiable information (PII)
/// </summary>
public partial class PiiValidator : IContentValidator
{
    public int Priority => 20;

    // Common PII patterns
    private static readonly Dictionary<string, Regex> PiiPatterns = new()
    {
        ["SSN"] = SsnRegex(),
        ["CreditCard"] = CreditCardRegex(),
        ["Email"] = EmailRegex(),
        ["Phone"] = PhoneRegex(),
        ["IPAddress"] = IpAddressRegex(),
        ["Passport"] = PassportRegex(),
        ["DriversLicense"] = DriversLicenseRegex()
    };

    public GuardrailResult Validate(string content, GuardrailsOptions options)
    {
        if (!options.EnablePiiDetection)
        {
            return GuardrailResult.Allowed();
        }

        foreach (var (piiType, pattern) in PiiPatterns)
        {
            var match = pattern.Match(content);
            if (match.Success)
            {
                if (options.DefaultAction == GuardrailAction.Redact)
                {
                    var redacted = RedactPii(content);
                    return GuardrailResult.Redacted(
                        GuardrailViolationType.PiiDetected,
                        content,
                        redacted,
                        piiType);
                }

                return GuardrailResult.Blocked(
                    GuardrailViolationType.PiiDetected,
                    $"Personally identifiable information detected: {piiType}",
                    piiType);
            }
        }

        return GuardrailResult.Allowed();
    }

    private static string RedactPii(string content)
    {
        var result = content;
        foreach (var (_, pattern) in PiiPatterns)
        {
            result = pattern.Replace(result, "[REDACTED]");
        }
        return result;
    }

    // SSN: XXX-XX-XXXX or XXXXXXXXX
    [GeneratedRegex(@"\b\d{3}-\d{2}-\d{4}\b|\b\d{9}\b", RegexOptions.Compiled)]
    private static partial Regex SsnRegex();

    // Credit card: 13-19 digits with optional separators
    [GeneratedRegex(@"\b(?:\d[ -]*?){13,19}\b", RegexOptions.Compiled)]
    private static partial Regex CreditCardRegex();

    // Email address
    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    // Phone number (various formats)
    [GeneratedRegex(@"\b(?:\+?1[-.\s]?)?(?:\(?[0-9]{3}\)?[-.\s]?)?[0-9]{3}[-.\s]?[0-9]{4}\b", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    // IP Address
    [GeneratedRegex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", RegexOptions.Compiled)]
    private static partial Regex IpAddressRegex();

    // Passport number (simplified - varies by country)
    [GeneratedRegex(@"\b[A-Z]{1,2}[0-9]{6,9}\b", RegexOptions.Compiled)]
    private static partial Regex PassportRegex();

    // US Driver's License (simplified)
    [GeneratedRegex(@"\b[A-Z]{1,2}\d{5,8}\b", RegexOptions.Compiled)]
    private static partial Regex DriversLicenseRegex();
}
