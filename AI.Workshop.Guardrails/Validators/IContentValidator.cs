namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Interface for content validation
/// </summary>
public interface IContentValidator
{
    /// <summary>
    /// Validates the given content
    /// </summary>
    /// <param name="content">Content to validate</param>
    /// <param name="options">Guardrails configuration options</param>
    /// <returns>Validation result</returns>
    GuardrailResult Validate(string content, GuardrailsOptions options);

    /// <summary>
    /// Priority order for this validator (lower = runs first)
    /// </summary>
    int Priority { get; }
}
