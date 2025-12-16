namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Validates input length constraints
/// </summary>
public class InputLengthValidator : IContentValidator
{
    public int Priority => 0;

    public GuardrailResult Validate(string content, GuardrailsOptions options)
    {
        if (content.Length > options.MaxInputLength)
        {
            return GuardrailResult.Blocked(
                GuardrailViolationType.InputTooLong,
                $"Input exceeds maximum allowed length of {options.MaxInputLength} characters (actual: {content.Length})");
        }

        return GuardrailResult.Allowed();
    }
}
