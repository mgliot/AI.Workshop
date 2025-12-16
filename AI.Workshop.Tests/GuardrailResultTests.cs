using AI.Workshop.Guardrails;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests;

public class GuardrailResultTests
{
    [Fact]
    public void Allowed_CreatesAllowedResult()
    {
        // Act
        var result = GuardrailResult.Allowed();

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.ViolationType.Should().BeNull();
        result.ViolationMessage.Should().BeNull();
    }

    [Fact]
    public void Blocked_CreatesBlockedResult()
    {
        // Act
        var result = GuardrailResult.Blocked(
            GuardrailViolationType.PromptInjection,
            "Test violation",
            "matched pattern");

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ViolationType.Should().Be(GuardrailViolationType.PromptInjection);
        result.ViolationMessage.Should().Be("Test violation");
        result.MatchedPattern.Should().Be("matched pattern");
    }

    [Fact]
    public void Redacted_CreatesRedactedResult()
    {
        // Act
        var result = GuardrailResult.Redacted(
            GuardrailViolationType.PiiDetected,
            "original content",
            "redacted content",
            "Email");

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.ViolationType.Should().Be(GuardrailViolationType.PiiDetected);
        result.OriginalContent.Should().Be("original content");
        result.RedactedContent.Should().Be("redacted content");
        result.MatchedPattern.Should().Be("Email");
    }
}
