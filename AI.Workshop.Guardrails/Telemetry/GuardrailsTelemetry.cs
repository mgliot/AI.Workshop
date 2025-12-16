using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AI.Workshop.Guardrails.Telemetry;

/// <summary>
/// Provides OpenTelemetry instrumentation for guardrails.
/// Includes ActivitySource for distributed tracing and Meter for metrics.
/// </summary>
public static class GuardrailsTelemetry
{
    /// <summary>
    /// The name used for the ActivitySource and Meter
    /// </summary>
    public const string SourceName = "AI.Workshop.Guardrails";

    /// <summary>
    /// Version of the instrumentation
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// ActivitySource for distributed tracing of guardrail operations
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, Version);

    /// <summary>
    /// Meter for guardrail metrics
    /// </summary>
    public static readonly Meter Meter = new(SourceName, Version);

    // Counters
    private static readonly Counter<long> _validationsTotal = Meter.CreateCounter<long>(
        "guardrails.validations.total",
        unit: "{validation}",
        description: "Total number of guardrail validations performed");

    private static readonly Counter<long> _validationsBlocked = Meter.CreateCounter<long>(
        "guardrails.validations.blocked",
        unit: "{validation}",
        description: "Total number of blocked validations");

    private static readonly Counter<long> _validationsAllowed = Meter.CreateCounter<long>(
        "guardrails.validations.allowed",
        unit: "{validation}",
        description: "Total number of allowed validations");

    private static readonly Counter<long> _validationsRedacted = Meter.CreateCounter<long>(
        "guardrails.validations.redacted",
        unit: "{validation}",
        description: "Total number of redacted validations");

    private static readonly Counter<long> _violationsByType = Meter.CreateCounter<long>(
        "guardrails.violations",
        unit: "{violation}",
        description: "Violations by type");

    private static readonly Counter<long> _rateLimitHits = Meter.CreateCounter<long>(
        "guardrails.rate_limit.hits",
        unit: "{hit}",
        description: "Number of rate limit hits");

    // Histograms
    private static readonly Histogram<double> _validationDuration = Meter.CreateHistogram<double>(
        "guardrails.validation.duration",
        unit: "ms",
        description: "Duration of guardrail validations in milliseconds");

    /// <summary>
    /// Records a validation event with OpenTelemetry metrics
    /// </summary>
    public static void RecordValidation(
        GuardrailResult result,
        string validatorName,
        TimeSpan duration,
        string? clientId = null)
    {
        var tags = new TagList
        {
            { "validator", validatorName },
            { "result", result.IsAllowed ? (result.RedactedContent != null ? "redacted" : "allowed") : "blocked" }
        };

        if (clientId != null)
        {
            tags.Add("client_id", clientId);
        }

        _validationsTotal.Add(1, tags);
        _validationDuration.Record(duration.TotalMilliseconds, tags);

        if (result.IsAllowed)
        {
            if (result.RedactedContent != null)
            {
                _validationsRedacted.Add(1, tags);
            }
            else
            {
                _validationsAllowed.Add(1, tags);
            }
        }
        else
        {
            _validationsBlocked.Add(1, tags);

            if (result.ViolationType.HasValue)
            {
                var violationTags = new TagList
                {
                    { "violation_type", result.ViolationType.Value.ToString() },
                    { "validator", validatorName }
                };
                _violationsByType.Add(1, violationTags);

                if (result.ViolationType == GuardrailViolationType.RateLimitExceeded)
                {
                    var rateLimitTags = new TagList();
                    if (clientId != null)
                    {
                        rateLimitTags.Add("client_id", clientId);
                    }
                    _rateLimitHits.Add(1, rateLimitTags);
                }
            }
        }
    }

    /// <summary>
    /// Starts a new activity for input validation
    /// </summary>
    public static Activity? StartInputValidation(string? clientId = null)
    {
        var activity = ActivitySource.StartActivity("guardrails.validate_input", ActivityKind.Internal);
        
        if (activity != null)
        {
            activity.SetTag("guardrails.direction", "input");
            if (clientId != null)
            {
                activity.SetTag("guardrails.client_id", clientId);
            }
        }

        return activity;
    }

    /// <summary>
    /// Starts a new activity for output validation
    /// </summary>
    public static Activity? StartOutputValidation(string? clientId = null)
    {
        var activity = ActivitySource.StartActivity("guardrails.validate_output", ActivityKind.Internal);
        
        if (activity != null)
        {
            activity.SetTag("guardrails.direction", "output");
            if (clientId != null)
            {
                activity.SetTag("guardrails.client_id", clientId);
            }
        }

        return activity;
    }

    /// <summary>
    /// Starts a new activity for a specific validator
    /// </summary>
    public static Activity? StartValidatorActivity(string validatorName)
    {
        var activity = ActivitySource.StartActivity($"guardrails.validator.{validatorName}", ActivityKind.Internal);
        
        activity?.SetTag("guardrails.validator", validatorName);

        return activity;
    }

    /// <summary>
    /// Records validation result on an activity
    /// </summary>
    public static void RecordActivityResult(Activity? activity, GuardrailResult result)
    {
        if (activity == null) return;

        activity.SetTag("guardrails.allowed", result.IsAllowed);
        
        if (result.ViolationType.HasValue)
        {
            activity.SetTag("guardrails.violation_type", result.ViolationType.Value.ToString());
        }

        if (!result.IsAllowed)
        {
            activity.SetStatus(ActivityStatusCode.Error, result.ViolationMessage);
        }
    }
}
