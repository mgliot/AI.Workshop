using AI.Workshop.Guardrails;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests;

public class GuardrailsMetricsTests
{
    [Fact]
    public void RecordValidation_IncrementsCounters()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();
        var allowedResult = GuardrailResult.Allowed();
        var blockedResult = GuardrailResult.Blocked(GuardrailViolationType.PromptInjection, "test");

        // Act
        metrics.RecordValidation(allowedResult, "TestValidator", TimeSpan.FromMilliseconds(10));
        metrics.RecordValidation(blockedResult, "TestValidator", TimeSpan.FromMilliseconds(20));

        // Assert
        metrics.TotalValidations.Should().Be(2);
        metrics.TotalAllowed.Should().Be(1);
        metrics.TotalBlocked.Should().Be(1);
    }

    [Fact]
    public void RecordValidation_TracksViolationTypes()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();

        // Act
        metrics.RecordValidation(
            GuardrailResult.Blocked(GuardrailViolationType.PromptInjection, "test"),
            "V1", TimeSpan.Zero);
        metrics.RecordValidation(
            GuardrailResult.Blocked(GuardrailViolationType.PromptInjection, "test"),
            "V1", TimeSpan.Zero);
        metrics.RecordValidation(
            GuardrailResult.Blocked(GuardrailViolationType.PiiDetected, "test"),
            "V2", TimeSpan.Zero);

        // Assert
        metrics.ViolationCounts[GuardrailViolationType.PromptInjection].Should().Be(2);
        metrics.ViolationCounts[GuardrailViolationType.PiiDetected].Should().Be(1);
    }

    [Fact]
    public void RecordValidation_TracksValidatorTriggers()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();

        // Act
        metrics.RecordValidation(GuardrailResult.Allowed(), "ValidatorA", TimeSpan.Zero);
        metrics.RecordValidation(GuardrailResult.Allowed(), "ValidatorA", TimeSpan.Zero);
        metrics.RecordValidation(GuardrailResult.Allowed(), "ValidatorB", TimeSpan.Zero);

        // Assert
        metrics.ValidatorTriggerCounts["ValidatorA"].Should().Be(2);
        metrics.ValidatorTriggerCounts["ValidatorB"].Should().Be(1);
    }

    [Fact]
    public void BlockRate_CalculatesCorrectly()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();

        // Act - 3 allowed, 1 blocked = 25% block rate
        metrics.RecordValidation(GuardrailResult.Allowed(), "V", TimeSpan.Zero);
        metrics.RecordValidation(GuardrailResult.Allowed(), "V", TimeSpan.Zero);
        metrics.RecordValidation(GuardrailResult.Allowed(), "V", TimeSpan.Zero);
        metrics.RecordValidation(
            GuardrailResult.Blocked(GuardrailViolationType.ToxicContent, "test"),
            "V", TimeSpan.Zero);

        // Assert
        metrics.BlockRate.Should().Be(25);
    }

    [Fact]
    public void BlockRate_WhenNoValidations_ReturnsZero()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();

        // Assert
        metrics.BlockRate.Should().Be(0);
    }

    [Fact]
    public void RecentEvents_LimitsToMaxEvents()
    {
        // Arrange
        var metrics = new GuardrailsMetrics(maxRecentEvents: 5);

        // Act - Add 10 events
        for (int i = 0; i < 10; i++)
        {
            metrics.RecordValidation(GuardrailResult.Allowed(), $"V{i}", TimeSpan.Zero);
        }

        // Assert - Should only keep last 5
        metrics.RecentEvents.Should().HaveCount(5);
    }

    [Fact]
    public void Reset_ClearsAllMetrics()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();
        metrics.RecordValidation(GuardrailResult.Allowed(), "V", TimeSpan.Zero);
        metrics.RecordValidation(
            GuardrailResult.Blocked(GuardrailViolationType.ToxicContent, "test"),
            "V", TimeSpan.Zero);

        // Act
        metrics.Reset();

        // Assert
        metrics.TotalValidations.Should().Be(0);
        metrics.TotalAllowed.Should().Be(0);
        metrics.TotalBlocked.Should().Be(0);
        metrics.ViolationCounts.Should().BeEmpty();
        metrics.RecentEvents.Should().BeEmpty();
    }

    [Fact]
    public void GetSummary_ReturnsCorrectData()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();
        metrics.RecordValidation(GuardrailResult.Allowed(), "V", TimeSpan.Zero);
        metrics.RecordValidation(
            GuardrailResult.Blocked(GuardrailViolationType.PromptInjection, "test"),
            "V", TimeSpan.Zero);

        // Act
        var summary = metrics.GetSummary();

        // Assert
        summary.TotalValidations.Should().Be(2);
        summary.TotalAllowed.Should().Be(1);
        summary.TotalBlocked.Should().Be(1);
        summary.BlockRate.Should().Be(50);
    }

    [Fact]
    public void RecordValidation_TracksRedactedContent()
    {
        // Arrange
        var metrics = new GuardrailsMetrics();
        var redactedResult = GuardrailResult.Redacted(
            GuardrailViolationType.PiiDetected,
            "original",
            "redacted");

        // Act
        metrics.RecordValidation(redactedResult, "V", TimeSpan.Zero);

        // Assert
        metrics.TotalRedacted.Should().Be(1);
        metrics.TotalAllowed.Should().Be(0); // Redacted counts separately
    }
}
