using AI.Workshop.Guardrails;
using AI.Workshop.Guardrails.Validators;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests.Validators;

public class TopicRestrictionValidatorTests
{
    private readonly TopicRestrictionValidator _validator = new();

    [Fact]
    public void Validate_WhenDisabled_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions { EnableTopicRestriction = false };

        // Act
        var result = _validator.Validate("anything goes here", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyTopics_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableTopicRestriction = true,
            AllowedTopics = []
        };

        // Act
        var result = _validator.Validate("test content", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMatchingKeyword_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableTopicRestriction = true,
            AllowedTopics = ["programming", "software development", "coding"]
        };

        // Act
        var result = _validator.Validate("I want to learn programming", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPartialKeywordMatch_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableTopicRestriction = true,
            AllowedTopics = ["software development"]
        };

        // Act
        var result = _validator.Validate("Tell me about software engineering", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoMatchingKeyword_ReturnsBlocked()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableTopicRestriction = true,
            AllowedTopics = ["programming", "software", "coding"]
        };

        // Act
        var result = _validator.Validate("What is the weather today?", options);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ViolationType.Should().Be(GuardrailViolationType.OffTopic);
    }

    [Fact]
    public void Validate_CaseInsensitive()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableTopicRestriction = true,
            AllowedTopics = ["PROGRAMMING"]
        };

        // Act
        var result = _validator.Validate("I love programming", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_MultiWordTopic_MatchesAnyWord()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableTopicRestriction = true,
            AllowedTopics = ["machine learning"]
        };

        // Act - should match "machine" or "learning"
        var result = _validator.Validate("I want to learn about machine intelligence", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Priority_ShouldBe35()
    {
        _validator.Priority.Should().Be(35);
    }
}
