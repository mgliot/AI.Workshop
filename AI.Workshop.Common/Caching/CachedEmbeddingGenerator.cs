using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace AI.Workshop.Common.Caching;

/// <summary>
/// A caching wrapper for IEmbeddingGenerator that stores embeddings in memory.
/// Reduces API calls for repeated text inputs by returning cached results.
/// </summary>
/// <remarks>
/// This implementation uses an in-memory concurrent dictionary for caching.
/// For distributed scenarios, consider using IDistributedCache instead.
/// </remarks>
public class CachedEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _innerGenerator;
    private readonly ConcurrentDictionary<string, CachedEmbedding> _cache;
    private readonly EmbeddingCacheOptions _options;
    private readonly object _cleanupLock = new();
    private DateTime _lastCleanup = DateTime.UtcNow;

    /// <summary>
    /// Creates a new cached embedding generator wrapping the specified inner generator
    /// </summary>
    /// <param name="innerGenerator">The underlying embedding generator to cache</param>
    /// <param name="options">Cache configuration options</param>
    public CachedEmbeddingGenerator(
        IEmbeddingGenerator<string, Embedding<float>> innerGenerator,
        EmbeddingCacheOptions? options = null)
    {
        _innerGenerator = innerGenerator ?? throw new ArgumentNullException(nameof(innerGenerator));
        _options = options ?? new EmbeddingCacheOptions();
        _cache = new ConcurrentDictionary<string, CachedEmbedding>();
    }

    /// <inheritdoc />
    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return _innerGenerator.GetService<TService>(key);
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? key = null)
    {
        return _innerGenerator.GetService(serviceType, key);
    }

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var valuesList = values.ToList();
        var results = new List<Embedding<float>>(valuesList.Count);
        var uncachedValues = new List<(int Index, string Value)>();

        // Check cache for each value
        for (int i = 0; i < valuesList.Count; i++)
        {
            var cacheKey = ComputeCacheKey(valuesList[i]);
            
            if (_cache.TryGetValue(cacheKey, out var cached) && !IsExpired(cached))
            {
                results.Add(cached.Embedding);
                CacheHits++;
            }
            else
            {
                uncachedValues.Add((i, valuesList[i]));
                results.Add(null!); // Placeholder
                CacheMisses++;
            }
        }

        // Generate embeddings for uncached values
        if (uncachedValues.Count > 0)
        {
            var uncachedTexts = uncachedValues.Select(x => x.Value).ToList();
            var generated = await _innerGenerator.GenerateAsync(uncachedTexts, options, cancellationToken);

            // Store in cache and update results
            for (int i = 0; i < uncachedValues.Count; i++)
            {
                var (originalIndex, text) = uncachedValues[i];
                var embedding = generated[i];
                var cacheKey = ComputeCacheKey(text);

                var cachedEntry = new CachedEmbedding(embedding, DateTime.UtcNow);
                _cache[cacheKey] = cachedEntry;
                results[originalIndex] = embedding;
            }
        }

        // Periodic cleanup
        CleanupIfNeeded();

        return new GeneratedEmbeddings<Embedding<float>>(results);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _innerGenerator.Dispose();
        _cache.Clear();
    }

    /// <summary>
    /// Gets the number of cache hits
    /// </summary>
    public long CacheHits { get; private set; }

    /// <summary>
    /// Gets the number of cache misses
    /// </summary>
    public long CacheMisses { get; private set; }

    /// <summary>
    /// Gets the current cache size
    /// </summary>
    public int CacheSize => _cache.Count;

    /// <summary>
    /// Gets the cache hit rate as a percentage
    /// </summary>
    public double HitRate => CacheHits + CacheMisses == 0 
        ? 0 
        : (double)CacheHits / (CacheHits + CacheMisses) * 100;

    /// <summary>
    /// Clears all cached embeddings
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        CacheHits = 0;
        CacheMisses = 0;
    }

    /// <summary>
    /// Removes a specific entry from the cache
    /// </summary>
    public bool RemoveFromCache(string text)
    {
        var cacheKey = ComputeCacheKey(text);
        return _cache.TryRemove(cacheKey, out _);
    }

    /// <summary>
    /// Checks if a text is in the cache
    /// </summary>
    public bool IsInCache(string text)
    {
        var cacheKey = ComputeCacheKey(text);
        return _cache.TryGetValue(cacheKey, out var cached) && !IsExpired(cached);
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public EmbeddingCacheStats GetStats() => new(
        CacheSize: CacheSize,
        CacheHits: CacheHits,
        CacheMisses: CacheMisses,
        HitRate: HitRate,
        MaxCacheSize: _options.MaxCacheSize,
        SlidingExpirationMinutes: _options.SlidingExpirationMinutes);

    private string ComputeCacheKey(string text)
    {
        // Use SHA256 hash for consistent, fixed-length keys
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private bool IsExpired(CachedEmbedding cached)
    {
        if (_options.SlidingExpirationMinutes <= 0)
            return false;

        var expirationTime = cached.CreatedAt.AddMinutes(_options.SlidingExpirationMinutes);
        return DateTime.UtcNow > expirationTime;
    }

    private void CleanupIfNeeded()
    {
        // Only cleanup periodically
        if ((DateTime.UtcNow - _lastCleanup).TotalMinutes < _options.CleanupIntervalMinutes)
            return;

        lock (_cleanupLock)
        {
            if ((DateTime.UtcNow - _lastCleanup).TotalMinutes < _options.CleanupIntervalMinutes)
                return;

            _lastCleanup = DateTime.UtcNow;

            // Remove expired entries
            var expiredKeys = _cache
                .Where(kvp => IsExpired(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            // If still over max size, remove oldest entries
            if (_options.MaxCacheSize > 0 && _cache.Count > _options.MaxCacheSize)
            {
                var toRemove = _cache
                    .OrderBy(kvp => kvp.Value.CreatedAt)
                    .Take(_cache.Count - _options.MaxCacheSize)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in toRemove)
                {
                    _cache.TryRemove(key, out _);
                }
            }
        }
    }

    private record CachedEmbedding(Embedding<float> Embedding, DateTime CreatedAt);
}

/// <summary>
/// Configuration options for the embedding cache
/// </summary>
public class EmbeddingCacheOptions
{
    /// <summary>
    /// Maximum number of embeddings to cache. 0 = unlimited.
    /// Default: 10000
    /// </summary>
    public int MaxCacheSize { get; set; } = 10000;

    /// <summary>
    /// Sliding expiration in minutes. 0 = no expiration.
    /// Default: 60 (1 hour)
    /// </summary>
    public int SlidingExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// How often to run cache cleanup in minutes.
    /// Default: 5
    /// </summary>
    public int CleanupIntervalMinutes { get; set; } = 5;
}

/// <summary>
/// Cache statistics
/// </summary>
public record EmbeddingCacheStats(
    int CacheSize,
    long CacheHits,
    long CacheMisses,
    double HitRate,
    int MaxCacheSize,
    int SlidingExpirationMinutes);
