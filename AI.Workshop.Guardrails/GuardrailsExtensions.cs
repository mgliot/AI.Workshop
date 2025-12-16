using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Workshop.Guardrails;

/// <summary>
/// Extension methods for registering guardrails with dependency injection
/// </summary>
public static class GuardrailsExtensions
{
    /// <summary>
    /// Adds guardrails middleware to the chat client pipeline
    /// </summary>
    public static ChatClientBuilder UseGuardrails(
        this ChatClientBuilder builder,
        GuardrailsOptions? options = null,
        Action<GuardrailResult>? onViolation = null)
    {
        return builder.Use(innerClient => new GuardrailsChatClient(innerClient, options, onViolation));
    }

    /// <summary>
    /// Adds guardrails middleware with configuration action
    /// </summary>
    public static ChatClientBuilder UseGuardrails(
        this ChatClientBuilder builder,
        Action<GuardrailsOptions> configure,
        Action<GuardrailResult>? onViolation = null)
    {
        var options = new GuardrailsOptions();
        configure(options);
        return builder.UseGuardrails(options, onViolation);
    }

    /// <summary>
    /// Wraps an existing IChatClient with guardrails
    /// </summary>
    public static IChatClient WithGuardrails(
        this IChatClient chatClient,
        GuardrailsOptions? options = null,
        Action<GuardrailResult>? onViolation = null)
    {
        return new GuardrailsChatClient(chatClient, options, onViolation);
    }

    /// <summary>
    /// Wraps an existing IChatClient with guardrails using configuration action
    /// </summary>
    public static IChatClient WithGuardrails(
        this IChatClient chatClient,
        Action<GuardrailsOptions> configure,
        Action<GuardrailResult>? onViolation = null)
    {
        var options = new GuardrailsOptions();
        configure(options);
        return chatClient.WithGuardrails(options, onViolation);
    }

    /// <summary>
    /// Registers GuardrailsService in the DI container
    /// </summary>
    public static IServiceCollection AddGuardrails(
        this IServiceCollection services,
        Action<GuardrailsOptions>? configure = null)
    {
        var options = new GuardrailsOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<GuardrailsService>();

        return services;
    }

    /// <summary>
    /// Registers GuardrailsService with metrics in the DI container
    /// </summary>
    public static IServiceCollection AddGuardrailsWithMetrics(
        this IServiceCollection services,
        Action<GuardrailsOptions>? configure = null,
        int maxRecentEvents = 100)
    {
        var options = new GuardrailsOptions();
        configure?.Invoke(options);

        var metrics = new GuardrailsMetrics(maxRecentEvents);

        services.AddSingleton(options);
        services.AddSingleton(metrics);
        services.AddSingleton(sp => new GuardrailsService(
            sp.GetRequiredService<GuardrailsOptions>(),
            sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>(),
            sp.GetService<IChatClient>(),
            sp.GetRequiredService<GuardrailsMetrics>()));

        return services;
    }

    /// <summary>
    /// Registers GuardrailsService with advanced validators (semantic topic matching, LLM moderation)
    /// </summary>
    public static IServiceCollection AddAdvancedGuardrails(
        this IServiceCollection services,
        Action<GuardrailsOptions>? configure = null,
        bool enableMetrics = true,
        int maxRecentEvents = 100)
    {
        var options = new GuardrailsOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        if (enableMetrics)
        {
            services.AddSingleton(new GuardrailsMetrics(maxRecentEvents));
        }

        services.AddSingleton(sp =>
        {
            var embeddingGenerator = sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            var moderationClient = options.EnableLlmModeration 
                ? sp.GetService<IChatClient>() 
                : null;
            var metrics = enableMetrics ? sp.GetService<GuardrailsMetrics>() : null;

            return new GuardrailsService(options, embeddingGenerator, moderationClient, metrics);
        });

        return services;
    }
}
