using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Workshop.Common.Caching;

/// <summary>
/// Extension methods for adding cached embedding generation
/// </summary>
public static class EmbeddingCacheExtensions
{
    /// <summary>
    /// Wraps an embedding generator with caching
    /// </summary>
    /// <param name="generator">The embedding generator to wrap</param>
    /// <param name="options">Cache configuration options</param>
    /// <returns>A cached embedding generator</returns>
    public static CachedEmbeddingGenerator WithCaching(
        this IEmbeddingGenerator<string, Embedding<float>> generator,
        EmbeddingCacheOptions? options = null)
    {
        return new CachedEmbeddingGenerator(generator, options);
    }

    /// <summary>
    /// Wraps an embedding generator with caching using configuration action
    /// </summary>
    /// <param name="generator">The embedding generator to wrap</param>
    /// <param name="configure">Configuration action for cache options</param>
    /// <returns>A cached embedding generator</returns>
    public static CachedEmbeddingGenerator WithCaching(
        this IEmbeddingGenerator<string, Embedding<float>> generator,
        Action<EmbeddingCacheOptions> configure)
    {
        var options = new EmbeddingCacheOptions();
        configure(options);
        return new CachedEmbeddingGenerator(generator, options);
    }

    /// <summary>
    /// Adds Ollama embedding generator with caching to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="cacheOptions">Cache configuration options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaEmbeddingGeneratorWithCaching(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        EmbeddingCacheOptions? cacheOptions = null)
    {
        var options = cacheOptions ?? new EmbeddingCacheOptions();
        services.AddSingleton(options);

        // Register AISettings first if not already done
        var aiSettings = configuration.GetAISettings();
        services.AddSingleton(aiSettings);

        // Register the cached embedding generator directly
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var settings = sp.GetRequiredService<AISettings>();
            var innerClient = new OllamaSharp.OllamaApiClient(
                settings.GetOllamaUri(), 
                settings.EmbeddingModel);
            return new CachedEmbeddingGenerator(innerClient, options);
        });

        return services;
    }

    /// <summary>
    /// Adds Ollama embedding generator with caching using configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="configure">Configuration action for cache options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaEmbeddingGeneratorWithCaching(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        Action<EmbeddingCacheOptions> configure)
    {
        var options = new EmbeddingCacheOptions();
        configure(options);
        return services.AddOllamaEmbeddingGeneratorWithCaching(configuration, options);
    }
}
