using AI.Workshop.Guardrails;
using AI.Workshop.Guardrails.Validators;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests.Validators;

public class BlockedKeywordValidatorTests
{
    private readonly BlockedKeywordValidator _validator = new();

    [Theory]
    [InlineData("This contains a secret word", "secret")]
    [InlineData("CONFIDENTIAL information here", "confidential")]
    [InlineData("The password is 12345", "password")]
    public void Validate_WithBlockedKeyword_ReturnsBlocked(string input, string keyword)
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            BlockedKeywords = ["secret", "confidential", "password"]
        };

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ViolationType.Should().Be(GuardrailViolationType.BlockedKeyword);
        result.MatchedPattern.Should().BeEquivalentTo(keyword);
    }

    [Fact]
    public void Validate_WithoutBlockedKeyword_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            BlockedKeywords = ["secret", "confidential"]
        };
        var input = "This is a normal message";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithBlockedPattern_ReturnsBlocked()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            BlockedPatterns = [@"\b\d{4}-\d{4}-\d{4}\b"]  // Matches patterns like 1234-5678-9012
        };
        var input = "My code is 1234-5678-9012";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ViolationType.Should().Be(GuardrailViolationType.BlockedPattern);
    }

    [Fact]
    public void Validate_WithEmptyBlockedLists_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            BlockedKeywords = [],
            BlockedPatterns = []
        };
        var input = "Any content here";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidRegexPattern_SkipsPattern()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            BlockedPatterns = ["[invalid regex("]  // Invalid regex
        };
        var input = "Some content";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeTrue();  // Should not throw, just skip
    }

    [Fact]
    public void Priority_ShouldBe40()
    {
        _validator.Priority.Should().Be(40);
    }
}
