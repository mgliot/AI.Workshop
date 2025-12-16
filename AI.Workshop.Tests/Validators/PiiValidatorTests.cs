using AI.Workshop.Guardrails;
using AI.Workshop.Guardrails.Validators;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests.Validators;

public class PiiValidatorTests
{
    private readonly PiiValidator _validator = new();
    private readonly GuardrailsOptions _options = new() { EnablePiiDetection = true };

    [Theory]
    [InlineData("My SSN is 123-45-6789", "SSN")]
    [InlineData("Social security: 987-65-4321", "SSN")]
    [InlineData("My credit card is 4111111111111111", "CreditCard")]
    [InlineData("Card number: 4111-1111-1111-1111", "CreditCard")]
    [InlineData("Email me at test@example.com", "Email")]
    [InlineData("Contact: john.doe@company.org", "Email")]
    [InlineData("Call me at 555-123-4567", "Phone")]
    [InlineData("Phone: (555) 123-4567", "Phone")]
    [InlineData("My IP is 192.168.1.100", "IPAddress")]
    [InlineData("Server at 10.0.0.1", "IPAddress")]
    public void Validate_WithPii_ReturnsBlocked(string input, string expectedPiiType)
    {
        // Act
        var result = _validator.Validate(input, _options);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ViolationType.Should().Be(GuardrailViolationType.PiiDetected);
        result.MatchedPattern.Should().Be(expectedPiiType);
    }

    [Theory]
    [InlineData("Hello, how are you?")]
    [InlineData("The weather is nice today")]
    [InlineData("I need help with my project")]
    [InlineData("What time is it?")]
    [InlineData("Tell me a joke")]
    public void Validate_WithoutPii_ReturnsAllowed(string input)
    {
        // Act
        var result = _validator.Validate(input, _options);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.ViolationType.Should().BeNull();
    }

    [Fact]
    public void Validate_WhenDisabled_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions { EnablePiiDetection = false };
        var inputWithPii = "My SSN is 123-45-6789";

        // Act
        var result = _validator.Validate(inputWithPii, options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithRedactAction_ReturnsRedactedContent()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnablePiiDetection = true,
            DefaultAction = GuardrailAction.Redact
        };
        var input = "My email is test@example.com";

        // Act
        var result = _validator.Validate(input, options);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.RedactedContent.Should().Contain("[REDACTED]");
        result.RedactedContent.Should().NotContain("test@example.com");
    }

    [Fact]
    public void Priority_ShouldBe20()
    {
        _validator.Priority.Should().Be(20);
    }
}
