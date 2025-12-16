using AI.Workshop.Guardrails.Telemetry;
using AI.Workshop.Guardrails.Validators;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace AI.Workshop.Guardrails;

/// <summary>
/// Service for validating content against configured guardrails
/// </summary>
public class GuardrailsService
{
    private readonly GuardrailsOptions _options;
    private readonly List<IContentValidator> _validators;
    private readonly GuardrailsMetrics? _metrics;
    private readonly bool _enableTelemetry;

    /// <summary>
    /// Creates a guardrails service with basic validators
    /// </summary>
    public GuardrailsService(GuardrailsOptions? options = null, GuardrailsMetrics? metrics = null, bool enableTelemetry = true)
    {
        _options = options ?? new GuardrailsOptions();
        _metrics = metrics;
        _enableTelemetry = enableTelemetry;
        _validators =
        [
            new RateLimitValidator(),
            new InputLengthValidator(),
            new PromptInjectionValidator(),
            new PiiValidator(),
            new ToxicityValidator(),
            new TopicRestrictionValidator(),
            new BlockedKeywordValidator()
        ];
        _validators = [.. _validators.OrderBy(v => v.Priority)];
    }

    /// <summary>
    /// Creates a guardrails service with advanced validators including semantic topic matching and LLM moderation
    /// </summary>
    /// <param name="options">Guardrails configuration</param>
    /// <param name="embeddingGenerator">Embedding generator for semantic topic matching</param>
    /// <param name="moderationClient">Chat client for LLM-based moderation</param>
    /// <param name="metrics">Optional metrics collector</param>
    /// <param name="enableTelemetry">Enable OpenTelemetry instrumentation</param>
    public GuardrailsService(
        GuardrailsOptions options,
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator,
        IChatClient? moderationClient = null,
        GuardrailsMetrics? metrics = null,
        bool enableTelemetry = true)
    {
        _options = options;
        _metrics = metrics;
        _enableTelemetry = enableTelemetry;
        _validators =
        [
            new RateLimitValidator(),
            new InputLengthValidator(),
            new PromptInjectionValidator(),
            new PiiValidator(),
            new ToxicityValidator(),
            embeddingGenerator != null 
                ? new TopicRestrictionValidator(embeddingGenerator) 
                : new TopicRestrictionValidator(),
            new BlockedKeywordValidator(),
            moderationClient != null 
                ? new LlmModerationValidator(moderationClient) 
                : new LlmModerationValidator()
        ];
        _validators = [.. _validators.OrderBy(v => v.Priority)];
    }

    /// <summary>
    /// Validates input content before sending to the LLM
    /// </summary>
    public GuardrailResult ValidateInput(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return GuardrailResult.Allowed();
        }

        using var activity = _enableTelemetry 
            ? GuardrailsTelemetry.StartInputValidation(_options.RateLimitClientId) 
            : null;

        foreach (var validator in _validators)
        {
            var stopwatch = Stopwatch.StartNew();
            
            using var validatorActivity = _enableTelemetry 
                ? GuardrailsTelemetry.StartValidatorActivity(validator.GetType().Name) 
                : null;
            
            var result = validator.Validate(content, _options);
            stopwatch.Stop();

            if (_enableTelemetry)
            {
                GuardrailsTelemetry.RecordValidation(result, validator.GetType().Name, stopwatch.Elapsed, _options.RateLimitClientId);
                GuardrailsTelemetry.RecordActivityResult(validatorActivity, result);
            }

            _metrics?.RecordValidation(result, validator.GetType().Name, stopwatch.Elapsed);

            if (!result.IsAllowed && _options.DefaultAction != GuardrailAction.LogOnly)
            {
                GuardrailsTelemetry.RecordActivityResult(activity, result);
                return result;
            }
        }

        var finalResult = GuardrailResult.Allowed();
        GuardrailsTelemetry.RecordActivityResult(activity, finalResult);
        return finalResult;
    }

    /// <summary>
    /// Validates output content from the LLM
    /// </summary>
    public GuardrailResult ValidateOutput(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return GuardrailResult.Allowed();
        }

        using var activity = _enableTelemetry 
            ? GuardrailsTelemetry.StartOutputValidation(_options.RateLimitClientId) 
            : null;

        // Check output length
        if (content.Length > _options.MaxOutputLength)
        {
            var result = GuardrailResult.Blocked(
                GuardrailViolationType.OutputTooLong,
                $"Output exceeds maximum allowed length of {_options.MaxOutputLength} characters");
            
            if (_enableTelemetry)
            {
                GuardrailsTelemetry.RecordValidation(result, "OutputLengthCheck", TimeSpan.Zero, _options.RateLimitClientId);
                GuardrailsTelemetry.RecordActivityResult(activity, result);
            }

            _metrics?.RecordValidation(result, "OutputLengthCheck", TimeSpan.Zero);
            return result;
        }

        // Run validators that apply to output (PII, Toxicity, Blocked Keywords, LLM Moderation)
        var outputValidators = _validators.Where(v =>
            v is PiiValidator or ToxicityValidator or BlockedKeywordValidator or LlmModerationValidator);

        foreach (var validator in outputValidators)
        {
            var stopwatch = Stopwatch.StartNew();
            
            using var validatorActivity = _enableTelemetry 
                ? GuardrailsTelemetry.StartValidatorActivity(validator.GetType().Name) 
                : null;
            
            var result = validator.Validate(content, _options);
            stopwatch.Stop();

            if (_enableTelemetry)
            {
                GuardrailsTelemetry.RecordValidation(result, validator.GetType().Name, stopwatch.Elapsed, _options.RateLimitClientId);
                GuardrailsTelemetry.RecordActivityResult(validatorActivity, result);
            }

            _metrics?.RecordValidation(result, validator.GetType().Name, stopwatch.Elapsed);

            if (!result.IsAllowed && _options.DefaultAction != GuardrailAction.LogOnly)
            {
                GuardrailsTelemetry.RecordActivityResult(activity, result);
                return result;
            }
        }

        var finalResult = GuardrailResult.Allowed();
        GuardrailsTelemetry.RecordActivityResult(activity, finalResult);
        return finalResult;
    }

    /// <summary>
    /// Gets the current guardrails options
    /// </summary>
    public GuardrailsOptions Options => _options;

    /// <summary>
    /// Gets the metrics collector if configured
    /// </summary>
    public GuardrailsMetrics? Metrics => _metrics;

    /// <summary>
    /// Gets the rate limit validator for manual control
    /// </summary>
    public RateLimitValidator? RateLimiter => 
        _validators.OfType<RateLimitValidator>().FirstOrDefault();

    /// <summary>
    /// Indicates whether OpenTelemetry instrumentation is enabled
    /// </summary>
    public bool TelemetryEnabled => _enableTelemetry;
}
