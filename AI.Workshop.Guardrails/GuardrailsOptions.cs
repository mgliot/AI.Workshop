namespace AI.Workshop.Guardrails;

/// <summary>
/// Configuration options for AI guardrails
/// </summary>
public class GuardrailsOptions
{
    /// <summary>
    /// Maximum allowed input length in characters
    /// </summary>
    public int MaxInputLength { get; set; } = 10000;

    /// <summary>
    /// Maximum allowed output length in characters
    /// </summary>
    public int MaxOutputLength { get; set; } = 50000;

    /// <summary>
    /// Enable prompt injection detection
    /// </summary>
    public bool EnablePromptInjectionDetection { get; set; } = true;

    /// <summary>
    /// Enable PII detection and blocking
    /// </summary>
    public bool EnablePiiDetection { get; set; } = true;

    /// <summary>
    /// Enable toxic content filtering
    /// </summary>
    public bool EnableToxicityFiltering { get; set; } = true;

    /// <summary>
    /// Enable topic restriction enforcement
    /// </summary>
    public bool EnableTopicRestriction { get; set; } = false;

    /// <summary>
    /// Allowed topics when topic restriction is enabled
    /// </summary>
    public List<string> AllowedTopics { get; set; } = [];

    /// <summary>
    /// Similarity threshold for topic matching (0.0 to 1.0). Default is 0.5.
    /// Higher values require closer semantic match to allowed topics.
    /// </summary>
    public double TopicSimilarityThreshold { get; set; } = 0.5;

    /// <summary>
    /// Blocked keywords that should trigger rejection
    /// </summary>
    public List<string> BlockedKeywords { get; set; } = [];

    /// <summary>
    /// Custom blocked patterns (regex)
    /// </summary>
    public List<string> BlockedPatterns { get; set; } = [];

    /// <summary>
    /// Action to take when a guardrail is triggered
    /// </summary>
    public GuardrailAction DefaultAction { get; set; } = GuardrailAction.Block;

    /// <summary>
    /// Custom message to return when content is blocked
    /// </summary>
    public string BlockedResponseMessage { get; set; } = "I apologize, but I cannot process this request as it violates our content policies.";

    // Rate Limiting Options

    /// <summary>
    /// Enable rate limiting
    /// </summary>
    public bool EnableRateLimiting { get; set; } = false;

    /// <summary>
    /// Maximum number of requests allowed within the rate limit window
    /// </summary>
    public int RateLimitMaxRequests { get; set; } = 60;

    /// <summary>
    /// Rate limit window duration in seconds
    /// </summary>
    public int RateLimitWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Client identifier for rate limiting (e.g., user ID, session ID).
    /// If null, uses a default identifier.
    /// </summary>
    public string? RateLimitClientId { get; set; }

    // LLM Moderation Options

    /// <summary>
    /// Enable LLM-based content moderation
    /// </summary>
    public bool EnableLlmModeration { get; set; } = false;

    /// <summary>
    /// Action to take when LLM moderation fails (e.g., timeout, error)
    /// </summary>
    public GuardrailAction LlmModerationFailureAction { get; set; } = GuardrailAction.LogOnly;
}

/// <summary>
/// Action to take when a guardrail is triggered
/// </summary>
public enum GuardrailAction
{
    /// <summary>
    /// Block the request/response entirely
    /// </summary>
    Block,

    /// <summary>
    /// Log and allow the request/response
    /// </summary>
    LogOnly,

    /// <summary>
    /// Redact sensitive content and allow
    /// </summary>
    Redact
}
