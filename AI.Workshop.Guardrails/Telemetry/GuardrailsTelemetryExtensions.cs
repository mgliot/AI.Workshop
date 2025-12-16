using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace AI.Workshop.Guardrails.Telemetry;

/// <summary>
/// Extension methods for integrating guardrails telemetry with OpenTelemetry
/// </summary>
public static class GuardrailsTelemetryExtensions
{
    /// <summary>
    /// Adds the guardrails meter to the MeterProviderBuilder.
    /// Call this in your OpenTelemetry metrics configuration.
    /// </summary>
    /// <example>
    /// builder.Services.AddOpenTelemetry()
    ///     .WithMetrics(metrics => metrics.AddGuardrailsInstrumentation());
    /// </example>
    public static T AddGuardrailsInstrumentation<T>(this T builder) where T : class
    {
        // Use reflection to call AddMeter on MeterProviderBuilder
        // This avoids requiring a direct dependency on OpenTelemetry packages
        var addMeterMethod = builder.GetType().GetMethod("AddMeter", [typeof(string[])]);
        addMeterMethod?.Invoke(builder, [new[] { GuardrailsTelemetry.SourceName }]);

        return builder;
    }

    /// <summary>
    /// Adds the guardrails activity source to the TracerProviderBuilder.
    /// Call this in your OpenTelemetry tracing configuration.
    /// </summary>
    /// <example>
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(tracing => tracing.AddGuardrailsInstrumentation());
    /// </example>
    public static T AddGuardrailsTracing<T>(this T builder) where T : class
    {
        // Use reflection to call AddSource on TracerProviderBuilder
        // This avoids requiring a direct dependency on OpenTelemetry packages
        var addSourceMethod = builder.GetType().GetMethod("AddSource", [typeof(string[])]);
        addSourceMethod?.Invoke(builder, [new[] { GuardrailsTelemetry.SourceName }]);

        return builder;
    }

    /// <summary>
    /// Gets an observable gauge that reports the current block rate
    /// </summary>
    public static void CreateBlockRateGauge(GuardrailsMetrics metrics)
    {
        GuardrailsTelemetry.Meter.CreateObservableGauge(
            "guardrails.block_rate",
            () => metrics.BlockRate,
            unit: "%",
            description: "Current percentage of blocked validations");
    }

    /// <summary>
    /// Gets an observable gauge that reports total validations
    /// </summary>
    public static void CreateTotalValidationsGauge(GuardrailsMetrics metrics)
    {
        GuardrailsTelemetry.Meter.CreateObservableGauge(
            "guardrails.total_validations",
            () => metrics.TotalValidations,
            unit: "{validation}",
            description: "Total number of validations performed");
    }
}
