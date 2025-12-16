namespace AI.Workshop.Common;

/// <summary>
/// Tracks token usage across chat interactions
/// </summary>
public class TokenUsageTracker
{
    private long _totalInputTokens;
    private long _totalOutputTokens;
    private int _requestCount;
    private readonly object _lock = new();

    /// <summary>
    /// Gets the total input tokens used across all tracked requests
    /// </summary>
    public long TotalInputTokens
    {
        get { lock (_lock) return _totalInputTokens; }
    }

    /// <summary>
    /// Gets the total output tokens used across all tracked requests
    /// </summary>
    public long TotalOutputTokens
    {
        get { lock (_lock) return _totalOutputTokens; }
    }

    /// <summary>
    /// Gets the total tokens used (input + output) across all tracked requests
    /// </summary>
    public long TotalTokens => TotalInputTokens + TotalOutputTokens;

    /// <summary>
    /// Gets the number of requests tracked
    /// </summary>
    public int RequestCount
    {
        get { lock (_lock) return _requestCount; }
    }

    /// <summary>
    /// Gets the average tokens per request
    /// </summary>
    public double AverageTokensPerRequest
    {
        get
        {
            lock (_lock)
            {
                return _requestCount > 0 ? (double)(_totalInputTokens + _totalOutputTokens) / _requestCount : 0;
            }
        }
    }

    /// <summary>
    /// Records token usage from a single request
    /// </summary>
    public void RecordUsage(long? inputTokens, long? outputTokens)
    {
        lock (_lock)
        {
            _totalInputTokens += inputTokens ?? 0;
            _totalOutputTokens += outputTokens ?? 0;
            _requestCount++;
        }
    }

    /// <summary>
    /// Records token usage from Microsoft.Extensions.AI UsageDetails
    /// </summary>
    public void RecordUsage(Microsoft.Extensions.AI.UsageDetails? usage)
    {
        if (usage != null)
        {
            RecordUsage(usage.InputTokenCount, usage.OutputTokenCount);
        }
    }

    /// <summary>
    /// Resets all tracked usage statistics
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _totalInputTokens = 0;
            _totalOutputTokens = 0;
            _requestCount = 0;
        }
    }

    /// <summary>
    /// Gets a formatted summary of token usage
    /// </summary>
    public string GetSummary()
    {
        lock (_lock)
        {
            return $"Tokens: {_totalInputTokens:N0} in / {_totalOutputTokens:N0} out = {_totalInputTokens + _totalOutputTokens:N0} total ({_requestCount} requests)";
        }
    }

    /// <summary>
    /// Gets a compact display string for the last request
    /// </summary>
    public static string FormatUsage(long? inputTokens, long? outputTokens)
    {
        if (inputTokens == null && outputTokens == null)
            return "N/A";

        var input = inputTokens?.ToString("N0") ?? "?";
        var output = outputTokens?.ToString("N0") ?? "?";
        var total = (inputTokens ?? 0) + (outputTokens ?? 0);
        return $"{input} in / {output} out = {total:N0} total";
    }

    /// <summary>
    /// Gets usage from Microsoft.Extensions.AI UsageDetails
    /// </summary>
    public static string FormatUsage(Microsoft.Extensions.AI.UsageDetails? usage)
    {
        if (usage == null)
            return "N/A";

        return FormatUsage(usage.InputTokenCount, usage.OutputTokenCount);
    }
}
