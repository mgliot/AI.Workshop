using System.Collections.Concurrent;

namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Validates request rate limits to prevent abuse and ensure fair usage.
/// Uses a sliding window algorithm to track requests per client/session.
/// </summary>
public class RateLimitValidator : IContentValidator
{
    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _counters = new();

    /// <summary>
    /// Priority order - runs early to reject before expensive operations
    /// </summary>
    public int Priority => 5;

    /// <summary>
    /// Validates that the request rate is within configured limits
    /// </summary>
    public GuardrailResult Validate(string content, GuardrailsOptions options)
    {
        if (!options.EnableRateLimiting)
        {
            return GuardrailResult.Allowed();
        }

        var clientId = options.RateLimitClientId ?? "default";
        var counter = _counters.GetOrAdd(clientId, _ => new SlidingWindowCounter(options.RateLimitWindowSeconds));

        if (!counter.TryIncrement(options.RateLimitMaxRequests))
        {
            return GuardrailResult.Blocked(
                GuardrailViolationType.RateLimitExceeded,
                $"Rate limit exceeded. Maximum {options.RateLimitMaxRequests} requests per {options.RateLimitWindowSeconds} seconds allowed.",
                matchedPattern: $"Client: {clientId}");
        }

        return GuardrailResult.Allowed();
    }

    /// <summary>
    /// Resets the rate limit counter for a specific client
    /// </summary>
    public void ResetClient(string clientId)
    {
        _counters.TryRemove(clientId, out _);
    }

    /// <summary>
    /// Clears all rate limit counters
    /// </summary>
    public void ResetAll()
    {
        _counters.Clear();
    }

    /// <summary>
    /// Gets the current request count for a client
    /// </summary>
    public int GetCurrentCount(string clientId)
    {
        return _counters.TryGetValue(clientId, out var counter) ? counter.CurrentCount : 0;
    }

    /// <summary>
    /// Sliding window counter for rate limiting
    /// </summary>
    private class SlidingWindowCounter(int windowSeconds)
    {
        private readonly object _lock = new();
        private readonly Queue<DateTime> _timestamps = new();
        private readonly TimeSpan _window = TimeSpan.FromSeconds(windowSeconds);

        public int CurrentCount
        {
            get
            {
                lock (_lock)
                {
                    CleanupExpired();
                    return _timestamps.Count;
                }
            }
        }

        public bool TryIncrement(int maxRequests)
        {
            lock (_lock)
            {
                CleanupExpired();

                if (_timestamps.Count >= maxRequests)
                {
                    return false;
                }

                _timestamps.Enqueue(DateTime.UtcNow);
                return true;
            }
        }

        private void CleanupExpired()
        {
            var cutoff = DateTime.UtcNow - _window;
            while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
            {
                _timestamps.Dequeue();
            }
        }
    }
}
