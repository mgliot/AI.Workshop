using AI.Workshop.Guardrails;
using AI.Workshop.Guardrails.Telemetry;
using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace AI.Workshop.Tests;

public class GuardrailsTelemetryTests
{
    [Fact]
    public void ActivitySource_HasCorrectName()
    {
        GuardrailsTelemetry.SourceName.Should().Be("AI.Workshop.Guardrails");
    }

    [Fact]
    public void ActivitySource_HasCorrectVersion()
    {
        GuardrailsTelemetry.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void StartInputValidation_CreatesActivity()
    {
        // Arrange - add listener to capture activities
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == GuardrailsTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        using var activity = GuardrailsTelemetry.StartInputValidation("test-client");

        // Assert
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("guardrails.validate_input");
        activity.GetTagItem("guardrails.direction").Should().Be("input");
        activity.GetTagItem("guardrails.client_id").Should().Be("test-client");
    }

    [Fact]
    public void StartOutputValidation_CreatesActivity()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == GuardrailsTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        using var activity = GuardrailsTelemetry.StartOutputValidation();

        // Assert
        activity.Should().NotBeNull();
        activity!.GetTagItem("guardrails.direction").Should().Be("output");
    }

    [Fact]
    public void StartValidatorActivity_CreatesActivityWithValidatorName()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == GuardrailsTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        using var activity = GuardrailsTelemetry.StartValidatorActivity("PromptInjectionValidator");

        // Assert
        activity.Should().NotBeNull();
        activity!.DisplayName.Should().Be("guardrails.validator.PromptInjectionValidator");
        activity.GetTagItem("guardrails.validator").Should().Be("PromptInjectionValidator");
    }

    [Fact]
    public void RecordActivityResult_SetsAllowedTag()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == GuardrailsTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);
        using var activity = GuardrailsTelemetry.StartInputValidation();
        var result = GuardrailResult.Allowed();

        // Act
        GuardrailsTelemetry.RecordActivityResult(activity, result);

        // Assert
        activity!.GetTagItem("guardrails.allowed").Should().Be(true);
    }

    [Fact]
    public void RecordActivityResult_SetsBlockedTagsAndStatus()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == GuardrailsTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);
        using var activity = GuardrailsTelemetry.StartInputValidation();
        var result = GuardrailResult.Blocked(GuardrailViolationType.PromptInjection, "Injection detected");

        // Act
        GuardrailsTelemetry.RecordActivityResult(activity, result);

        // Assert
        activity!.GetTagItem("guardrails.allowed").Should().Be(false);
        activity.GetTagItem("guardrails.violation_type").Should().Be("PromptInjection");
        activity.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void RecordValidation_DoesNotThrow()
    {
        // Arrange
        var result = GuardrailResult.Allowed();

        // Act & Assert - should not throw
        var act = () => GuardrailsTelemetry.RecordValidation(result, "TestValidator", TimeSpan.FromMilliseconds(10));
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordValidation_WithBlockedResult_DoesNotThrow()
    {
        // Arrange
        var result = GuardrailResult.Blocked(GuardrailViolationType.RateLimitExceeded, "Rate limit exceeded");

        // Act & Assert
        var act = () => GuardrailsTelemetry.RecordValidation(result, "RateLimitValidator", TimeSpan.FromMilliseconds(5), "client-1");
        act.Should().NotThrow();
    }

    [Fact]
    public void GuardrailsService_WithTelemetryEnabled_HasTelemetryProperty()
    {
        // Arrange & Act
        var service = new GuardrailsService(enableTelemetry: true);

        // Assert
        service.TelemetryEnabled.Should().BeTrue();
    }

    [Fact]
    public void GuardrailsService_WithTelemetryDisabled_HasTelemetryProperty()
    {
        // Arrange & Act
        var service = new GuardrailsService(enableTelemetry: false);

        // Assert
        service.TelemetryEnabled.Should().BeFalse();
    }
}
