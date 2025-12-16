using System.Collections.Concurrent;

namespace AI.Workshop.Guardrails;

/// <summary>
/// Provides telemetry and metrics for guardrail triggers.
/// Thread-safe for use in concurrent environments.
/// </summary>
public class GuardrailsMetrics
{
    private readonly ConcurrentDictionary<GuardrailViolationType, long> _violationCounts = new();
    private readonly ConcurrentDictionary<string, long> _validatorTriggerCounts = new();
    private readonly ConcurrentQueue<GuardrailEvent> _recentEvents = new();
    private readonly int _maxRecentEvents;

    private long _totalValidations;
    private long _totalBlocked;
    private long _totalAllowed;
    private long _totalRedacted;

    /// <summary>
    /// Creates a new metrics instance
    /// </summary>
    /// <param name="maxRecentEvents">Maximum number of recent events to keep in memory</param>
    public GuardrailsMetrics(int maxRecentEvents = 100)
    {
        _maxRecentEvents = maxRecentEvents;
    }

    /// <summary>
    /// Records a validation event
    /// </summary>
    public void RecordValidation(GuardrailResult result, string validatorName, TimeSpan duration)
    {
        Interlocked.Increment(ref _totalValidations);

        if (result.IsAllowed)
        {
            if (result.RedactedContent != null)
            {
                Interlocked.Increment(ref _totalRedacted);
            }
            else
            {
                Interlocked.Increment(ref _totalAllowed);
            }
        }
        else
        {
            Interlocked.Increment(ref _totalBlocked);

            if (result.ViolationType.HasValue)
            {
                _violationCounts.AddOrUpdate(result.ViolationType.Value, 1, (_, count) => count + 1);
            }
        }

        _validatorTriggerCounts.AddOrUpdate(validatorName, 1, (_, count) => count + 1);

        // Add to recent events
        var evt = new GuardrailEvent(
            Timestamp: DateTime.UtcNow,
            ValidatorName: validatorName,
            Result: result.IsAllowed ? "Allowed" : "Blocked",
            ViolationType: result.ViolationType?.ToString(),
            DurationMs: duration.TotalMilliseconds);

        _recentEvents.Enqueue(evt);

        // Trim old events
        while (_recentEvents.Count > _maxRecentEvents)
        {
            _recentEvents.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Gets the total number of validations performed
    /// </summary>
    public long TotalValidations => Interlocked.Read(ref _totalValidations);

    /// <summary>
    /// Gets the total number of blocked requests
    /// </summary>
    public long TotalBlocked => Interlocked.Read(ref _totalBlocked);

    /// <summary>
    /// Gets the total number of allowed requests
    /// </summary>
    public long TotalAllowed => Interlocked.Read(ref _totalAllowed);

    /// <summary>
    /// Gets the total number of redacted requests
    /// </summary>
    public long TotalRedacted => Interlocked.Read(ref _totalRedacted);

    /// <summary>
    /// Gets the block rate as a percentage
    /// </summary>
    public double BlockRate => TotalValidations == 0 ? 0 : (double)TotalBlocked / TotalValidations * 100;

    /// <summary>
    /// Gets violation counts by type
    /// </summary>
    public IReadOnlyDictionary<GuardrailViolationType, long> ViolationCounts =>
        new Dictionary<GuardrailViolationType, long>(_violationCounts);

    /// <summary>
    /// Gets trigger counts by validator name
    /// </summary>
    public IReadOnlyDictionary<string, long> ValidatorTriggerCounts =>
        new Dictionary<string, long>(_validatorTriggerCounts);

    /// <summary>
    /// Gets recent validation events
    /// </summary>
    public IReadOnlyList<GuardrailEvent> RecentEvents => _recentEvents.ToArray();

    /// <summary>
    /// Resets all metrics
    /// </summary>
    public void Reset()
    {
        _violationCounts.Clear();
        _validatorTriggerCounts.Clear();
        _recentEvents.Clear();
        Interlocked.Exchange(ref _totalValidations, 0);
        Interlocked.Exchange(ref _totalBlocked, 0);
        Interlocked.Exchange(ref _totalAllowed, 0);
        Interlocked.Exchange(ref _totalRedacted, 0);
    }

    /// <summary>
    /// Gets a summary of all metrics
    /// </summary>
    public MetricsSummary GetSummary() => new(
        TotalValidations: TotalValidations,
        TotalBlocked: TotalBlocked,
        TotalAllowed: TotalAllowed,
        TotalRedacted: TotalRedacted,
        BlockRate: BlockRate,
        ViolationCounts: ViolationCounts,
        ValidatorTriggerCounts: ValidatorTriggerCounts);
}

/// <summary>
/// Represents a guardrail validation event
/// </summary>
public record GuardrailEvent(
    DateTime Timestamp,
    string ValidatorName,
    string Result,
    string? ViolationType,
    double DurationMs);

/// <summary>
/// Summary of guardrail metrics
/// </summary>
public record MetricsSummary(
    long TotalValidations,
    long TotalBlocked,
    long TotalAllowed,
    long TotalRedacted,
    double BlockRate,
    IReadOnlyDictionary<GuardrailViolationType, long> ViolationCounts,
    IReadOnlyDictionary<string, long> ValidatorTriggerCounts);
