using AI.Workshop.Guardrails;
using AI.Workshop.Guardrails.Validators;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests.Validators;

public class InputLengthValidatorTests
{
    private readonly InputLengthValidator _validator = new();

    [Fact]
    public void Validate_WithContentUnderLimit_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions { MaxInputLength = 100 };
        var input = "This is a short message";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.ViolationType.Should().BeNull();
    }

    [Fact]
    public void Validate_WithContentOverLimit_ReturnsBlocked()
    {
        // Arrange
        var options = new GuardrailsOptions { MaxInputLength = 10 };
        var input = "This is a message that exceeds the limit";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ViolationType.Should().Be(GuardrailViolationType.InputTooLong);
        result.ViolationMessage.Should().Contain("exceeds");
    }

    [Fact]
    public void Validate_WithContentExactlyAtLimit_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions { MaxInputLength = 5 };
        var input = "Hello";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Priority_ShouldBe0()
    {
        _validator.Priority.Should().Be(0);
    }
}
