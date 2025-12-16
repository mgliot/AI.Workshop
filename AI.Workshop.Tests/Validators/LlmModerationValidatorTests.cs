using AI.Workshop.Guardrails;
using AI.Workshop.Guardrails.Validators;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests.Validators;

public class LlmModerationValidatorTests
{
    private readonly LlmModerationValidator _validator = new();

    [Fact]
    public void Validate_WhenDisabled_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions { EnableLlmModeration = false };

        // Act
        var result = _validator.Validate("any content", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenEnabledButNoClient_ReturnsAllowed()
    {
        // Arrange - validator created without a chat client
        var options = new GuardrailsOptions { EnableLlmModeration = true };

        // Act
        var result = _validator.Validate("any content", options);

        // Assert - should be allowed since no client available
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Priority_ShouldBe100()
    {
        // LLM moderation should run last as it's the most expensive
        _validator.Priority.Should().Be(100);
    }
}
