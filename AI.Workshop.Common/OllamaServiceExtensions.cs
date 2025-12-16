using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace AI.Workshop.Common;

/// <summary>
/// Extension methods for registering Ollama services with dependency injection
/// </summary>
public static class OllamaServiceExtensions
{
    /// <summary>
    /// Adds Ollama chat client to the service collection using settings from configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration root</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaChatClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetAISettings();
        return services.AddOllamaChatClient(settings);
    }

    /// <summary>
    /// Adds Ollama chat client to the service collection using provided settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">AI settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaChatClient(
        this IServiceCollection services,
        AISettings settings)
    {
        // Register OllamaApiClient as singleton (thread-safe, reusable)
        services.AddSingleton(sp =>
        {
            var client = new OllamaApiClient(settings.GetOllamaUri(), settings.ChatModel);
            return client;
        });

        // Register IChatClient interface pointing to the same instance
        services.AddSingleton<IChatClient>(sp => sp.GetRequiredService<OllamaApiClient>());

        return services;
    }

    /// <summary>
    /// Adds Ollama embedding generator to the service collection using settings from configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration root</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaEmbeddingGenerator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetAISettings();
        return services.AddOllamaEmbeddingGenerator(settings);
    }

    /// <summary>
    /// Adds Ollama embedding generator to the service collection using provided settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">AI settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaEmbeddingGenerator(
        this IServiceCollection services,
        AISettings settings)
    {
        // Register a separate OllamaApiClient for embeddings (keyed service)
        services.AddKeyedSingleton("EmbeddingClient", (sp, key) =>
        {
            var client = new OllamaApiClient(settings.GetOllamaUri(), settings.EmbeddingModel);
            return client;
        });

        // Register IEmbeddingGenerator interface
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var client = sp.GetRequiredKeyedService<OllamaApiClient>("EmbeddingClient");
            return client;
        });

        return services;
    }

    /// <summary>
    /// Adds both Ollama chat client and embedding generator to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration root</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetAISettings();
        return services.AddOllamaServices(settings);
    }

    /// <summary>
    /// Adds both Ollama chat client and embedding generator to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">AI settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaServices(
        this IServiceCollection services,
        AISettings settings)
    {
        services.AddOllamaChatClient(settings);
        services.AddOllamaEmbeddingGenerator(settings);
        return services;
    }

    /// <summary>
    /// Adds Ollama services with custom configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure AI settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOllamaServices(
        this IServiceCollection services,
        Action<AISettings> configure)
    {
        var settings = new AISettings();
        configure(settings);
        return services.AddOllamaServices(settings);
    }
}
