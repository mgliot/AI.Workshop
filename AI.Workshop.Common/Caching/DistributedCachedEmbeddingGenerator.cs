using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AI.Workshop.Common.Caching;

/// <summary>
/// A caching wrapper for IEmbeddingGenerator that uses IDistributedCache.
/// Suitable for distributed scenarios with Redis, SQL Server, or other distributed cache providers.
/// </summary>
public class DistributedCachedEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _innerGenerator;
    private readonly IDistributedCache _cache;
    private readonly DistributedEmbeddingCacheOptions _options;
    private long _cacheHits;
    private long _cacheMisses;

    /// <summary>
    /// Creates a new distributed cached embedding generator
    /// </summary>
    /// <param name="innerGenerator">The underlying embedding generator</param>
    /// <param name="cache">The distributed cache to use</param>
    /// <param name="options">Cache configuration options</param>
    public DistributedCachedEmbeddingGenerator(
        IEmbeddingGenerator<string, Embedding<float>> innerGenerator,
        IDistributedCache cache,
        DistributedEmbeddingCacheOptions? options = null)
    {
        _innerGenerator = innerGenerator ?? throw new ArgumentNullException(nameof(innerGenerator));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options ?? new DistributedEmbeddingCacheOptions();
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
        var results = new Embedding<float>?[valuesList.Count];
        var uncachedValues = new List<(int Index, string Value)>();

        // Check cache for each value
        for (int i = 0; i < valuesList.Count; i++)
        {
            var cacheKey = ComputeCacheKey(valuesList[i]);
            
            try
            {
                var cachedData = await _cache.GetAsync(cacheKey, cancellationToken);
                
                if (cachedData != null)
                {
                    var embedding = DeserializeEmbedding(cachedData);
                    results[i] = embedding;
                    Interlocked.Increment(ref _cacheHits);
                }
                else
                {
                    uncachedValues.Add((i, valuesList[i]));
                    Interlocked.Increment(ref _cacheMisses);
                }
            }
            catch
            {
                // If cache fails, treat as miss
                uncachedValues.Add((i, valuesList[i]));
                Interlocked.Increment(ref _cacheMisses);
            }
        }

        // Generate embeddings for uncached values
        if (uncachedValues.Count > 0)
        {
            var uncachedTexts = uncachedValues.Select(x => x.Value).ToList();
            var generated = await _innerGenerator.GenerateAsync(uncachedTexts, options, cancellationToken);

            // Store in cache and update results
            var cacheEntryOptions = new DistributedCacheEntryOptions();
            
            if (_options.SlidingExpiration.HasValue)
            {
                cacheEntryOptions.SlidingExpiration = _options.SlidingExpiration;
            }
            
            if (_options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = _options.AbsoluteExpirationRelativeToNow;
            }

            for (int i = 0; i < uncachedValues.Count; i++)
            {
                var (originalIndex, text) = uncachedValues[i];
                var embedding = generated[i];
                var cacheKey = ComputeCacheKey(text);

                results[originalIndex] = embedding;

                // Store in cache (fire and forget for performance)
                try
                {
                    var serialized = SerializeEmbedding(embedding);
                    _ = _cache.SetAsync(cacheKey, serialized, cacheEntryOptions, cancellationToken);
                }
                catch
                {
                    // Ignore cache write failures
                }
            }
        }

        return new GeneratedEmbeddings<Embedding<float>>(results.Select(e => e!).ToList());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _innerGenerator.Dispose();
    }

    /// <summary>
    /// Gets the number of cache hits
    /// </summary>
    public long CacheHits => Interlocked.Read(ref _cacheHits);

    /// <summary>
    /// Gets the number of cache misses
    /// </summary>
    public long CacheMisses => Interlocked.Read(ref _cacheMisses);

    /// <summary>
    /// Gets the cache hit rate as a percentage
    /// </summary>
    public double HitRate
    {
        get
        {
            var hits = CacheHits;
            var misses = CacheMisses;
            return hits + misses == 0 ? 0 : (double)hits / (hits + misses) * 100;
        }
    }

    /// <summary>
    /// Removes a specific entry from the cache
    /// </summary>
    public async Task RemoveFromCacheAsync(string text, CancellationToken cancellationToken = default)
    {
        var cacheKey = ComputeCacheKey(text);
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    private string ComputeCacheKey(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        var hashString = Convert.ToBase64String(hash);
        return $"{_options.KeyPrefix}{hashString}";
    }

    private static byte[] SerializeEmbedding(Embedding<float> embedding)
    {
        // Serialize the vector as JSON
        return JsonSerializer.SerializeToUtf8Bytes(embedding.Vector.ToArray());
    }

    private static Embedding<float> DeserializeEmbedding(byte[] data)
    {
        var vector = JsonSerializer.Deserialize<float[]>(data);
        return new Embedding<float>(vector ?? []);
    }
}

/// <summary>
/// Configuration options for distributed embedding cache
/// </summary>
public class DistributedEmbeddingCacheOptions
{
    /// <summary>
    /// Key prefix for cache entries. Default: "emb:"
    /// </summary>
    public string KeyPrefix { get; set; } = "emb:";

    /// <summary>
    /// Sliding expiration for cache entries. Default: 1 hour
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Absolute expiration relative to now. Default: null (use sliding only)
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
}
