using AI.Workshop.Common;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Xunit;

namespace AI.Workshop.Tests;

public class TokenUsageTrackerTests
{
    [Fact]
    public void NewTracker_ShouldHaveZeroValues()
    {
        // Arrange & Act
        var tracker = new TokenUsageTracker();

        // Assert
        tracker.TotalInputTokens.Should().Be(0);
        tracker.TotalOutputTokens.Should().Be(0);
        tracker.TotalTokens.Should().Be(0);
        tracker.RequestCount.Should().Be(0);
        tracker.AverageTokensPerRequest.Should().Be(0);
    }

    [Fact]
    public void RecordUsage_ShouldAccumulateTokens()
    {
        // Arrange
        var tracker = new TokenUsageTracker();

        // Act
        tracker.RecordUsage(100, 50);
        tracker.RecordUsage(200, 75);

        // Assert
        tracker.TotalInputTokens.Should().Be(300);
        tracker.TotalOutputTokens.Should().Be(125);
        tracker.TotalTokens.Should().Be(425);
        tracker.RequestCount.Should().Be(2);
    }

    [Fact]
    public void RecordUsage_WithNullValues_ShouldTreatAsZero()
    {
        // Arrange
        var tracker = new TokenUsageTracker();

        // Act
        tracker.RecordUsage(null, 50);
        tracker.RecordUsage(100, null);

        // Assert
        tracker.TotalInputTokens.Should().Be(100);
        tracker.TotalOutputTokens.Should().Be(50);
        tracker.RequestCount.Should().Be(2);
    }

    [Fact]
    public void RecordUsage_WithUsageDetails_ShouldExtractValues()
    {
        // Arrange
        var tracker = new TokenUsageTracker();
        var usage = new UsageDetails
        {
            InputTokenCount = 150,
            OutputTokenCount = 75
        };

        // Act
        tracker.RecordUsage(usage);

        // Assert
        tracker.TotalInputTokens.Should().Be(150);
        tracker.TotalOutputTokens.Should().Be(75);
        tracker.RequestCount.Should().Be(1);
    }

    [Fact]
    public void RecordUsage_WithNullUsageDetails_ShouldNotThrow()
    {
        // Arrange
        var tracker = new TokenUsageTracker();

        // Act
        tracker.RecordUsage((UsageDetails?)null);

        // Assert
        tracker.TotalInputTokens.Should().Be(0);
        tracker.TotalOutputTokens.Should().Be(0);
        tracker.RequestCount.Should().Be(0);
    }

    [Fact]
    public void Reset_ShouldClearAllValues()
    {
        // Arrange
        var tracker = new TokenUsageTracker();
        tracker.RecordUsage(100, 50);
        tracker.RecordUsage(200, 75);

        // Act
        tracker.Reset();

        // Assert
        tracker.TotalInputTokens.Should().Be(0);
        tracker.TotalOutputTokens.Should().Be(0);
        tracker.TotalTokens.Should().Be(0);
        tracker.RequestCount.Should().Be(0);
    }

    [Fact]
    public void AverageTokensPerRequest_ShouldCalculateCorrectly()
    {
        // Arrange
        var tracker = new TokenUsageTracker();

        // Act
        tracker.RecordUsage(100, 50); // 150 total
        tracker.RecordUsage(200, 100); // 300 total
        // Total: 450 tokens, 2 requests = 225 average

        // Assert
        tracker.AverageTokensPerRequest.Should().Be(225);
    }

    [Fact]
    public void GetSummary_ShouldReturnFormattedString()
    {
        // Arrange
        var tracker = new TokenUsageTracker();
        tracker.RecordUsage(1000, 500);

        // Act
        var summary = tracker.GetSummary();

        // Assert (use regex to handle locale differences in number formatting)
        summary.Should().Contain("in");
        summary.Should().Contain("out");
        summary.Should().Contain("total");
        summary.Should().Contain("1 requests");
        // Verify the actual numbers are present (regardless of formatting)
        summary.Should().MatchRegex(@"1[,.\s]?000.*in");
        summary.Should().MatchRegex(@"500.*out");
    }

    [Fact]
    public void FormatUsage_ShouldFormatCorrectly()
    {
        // Act
        var result = TokenUsageTracker.FormatUsage(1500, 750);

        // Assert (use regex to handle locale differences)
        result.Should().Contain("in");
        result.Should().Contain("out");
        result.Should().Contain("total");
        result.Should().MatchRegex(@"1[,.\s]?500.*in");
        result.Should().MatchRegex(@"750.*out");
    }

    [Fact]
    public void FormatUsage_WithNullValues_ShouldReturnNA()
    {
        // Act
        var result = TokenUsageTracker.FormatUsage(null, null);

        // Assert
        result.Should().Be("N/A");
    }

    [Fact]
    public void FormatUsage_WithUsageDetails_ShouldFormatCorrectly()
    {
        // Arrange
        var usage = new UsageDetails
        {
            InputTokenCount = 500,
            OutputTokenCount = 250
        };

        // Act
        var result = TokenUsageTracker.FormatUsage(usage);

        // Assert
        result.Should().Contain("500 in");
        result.Should().Contain("250 out");
    }

    [Fact]
    public void FormatUsage_WithNullUsageDetails_ShouldReturnNA()
    {
        // Act
        var result = TokenUsageTracker.FormatUsage((UsageDetails?)null);

        // Assert
        result.Should().Be("N/A");
    }
}
