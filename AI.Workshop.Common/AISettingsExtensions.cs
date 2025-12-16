using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Workshop.Common;

/// <summary>
/// Extension methods for registering AI settings with dependency injection
/// </summary>
public static class AISettingsExtensions
{
    /// <summary>
    /// Adds AISettings to the service collection, binding from configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration root</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAISettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AISettings>(
            configuration.GetSection(AISettings.SectionName));

        // Also register AISettings directly for simple injection
        services.AddSingleton(sp =>
        {
            var settings = new AISettings();
            configuration.GetSection(AISettings.SectionName).Bind(settings);
            return settings;
        });

        return services;
    }

    /// <summary>
    /// Adds AISettings to the service collection with manual configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAISettings(
        this IServiceCollection services,
        Action<AISettings> configure)
    {
        var settings = new AISettings();
        configure(settings);

        services.AddSingleton(settings);
        services.Configure<AISettings>(opt =>
        {
            opt.OllamaUri = settings.OllamaUri;
            opt.ChatModel = settings.ChatModel;
            opt.EmbeddingModel = settings.EmbeddingModel;
            opt.VectorDimensions = settings.VectorDimensions;
        });

        return services;
    }

    /// <summary>
    /// Gets AISettings from configuration, or returns defaults if section is missing
    /// </summary>
    /// <param name="configuration">The configuration root</param>
    /// <returns>AISettings instance</returns>
    public static AISettings GetAISettings(this IConfiguration configuration)
    {
        var settings = new AISettings();
        configuration.GetSection(AISettings.SectionName).Bind(settings);
        return settings;
    }
}
