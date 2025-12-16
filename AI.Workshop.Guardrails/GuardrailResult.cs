namespace AI.Workshop.Guardrails;

/// <summary>
/// Result of a guardrail validation check
/// </summary>
public class GuardrailResult
{
    /// <summary>
    /// Whether the content passed all guardrail checks
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Type of violation detected, if any
    /// </summary>
    public GuardrailViolationType? ViolationType { get; init; }

    /// <summary>
    /// Detailed message about the violation
    /// </summary>
    public string? ViolationMessage { get; init; }

    /// <summary>
    /// The original content that was validated
    /// </summary>
    public string? OriginalContent { get; init; }

    /// <summary>
    /// Redacted content (if action is Redact)
    /// </summary>
    public string? RedactedContent { get; init; }

    /// <summary>
    /// Matched pattern or keyword that triggered the guardrail
    /// </summary>
    public string? MatchedPattern { get; init; }

    /// <summary>
    /// Creates a successful (allowed) result
    /// </summary>
    public static GuardrailResult Allowed() => new() { IsAllowed = true };

    /// <summary>
    /// Creates a blocked result with violation details
    /// </summary>
    public static GuardrailResult Blocked(
        GuardrailViolationType violationType,
        string message,
        string? matchedPattern = null) => new()
    {
        IsAllowed = false,
        ViolationType = violationType,
        ViolationMessage = message,
        MatchedPattern = matchedPattern
    };

    /// <summary>
    /// Creates a redacted result
    /// </summary>
    public static GuardrailResult Redacted(
        GuardrailViolationType violationType,
        string originalContent,
        string redactedContent,
        string? matchedPattern = null) => new()
    {
        IsAllowed = true,
        ViolationType = violationType,
        OriginalContent = originalContent,
        RedactedContent = redactedContent,
        MatchedPattern = matchedPattern
    };
}

/// <summary>
/// Types of guardrail violations
/// </summary>
public enum GuardrailViolationType
{
    /// <summary>
    /// Input exceeds maximum length
    /// </summary>
    InputTooLong,

    /// <summary>
    /// Output exceeds maximum length
    /// </summary>
    OutputTooLong,

    /// <summary>
    /// Potential prompt injection detected
    /// </summary>
    PromptInjection,

    /// <summary>
    /// Personally identifiable information detected
    /// </summary>
    PiiDetected,

    /// <summary>
    /// Toxic or harmful content detected
    /// </summary>
    ToxicContent,

    /// <summary>
    /// Off-topic content detected
    /// </summary>
    OffTopic,

    /// <summary>
    /// Blocked keyword detected
    /// </summary>
    BlockedKeyword,

    /// <summary>
    /// Blocked pattern matched
    /// </summary>
    BlockedPattern,

    /// <summary>
    /// Rate limit exceeded
    /// </summary>
    RateLimitExceeded,

    /// <summary>
    /// Content flagged by LLM moderation
    /// </summary>
    LlmModerationFlagged,

    /// <summary>
    /// Error during content moderation
    /// </summary>
    ModerationError
}
