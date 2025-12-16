using AI.Workshop.Guardrails;
using AI.Workshop.Guardrails.Validators;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests.Validators;

public class RateLimitValidatorTests
{
    private readonly RateLimitValidator _validator = new();

    [Fact]
    public void Validate_WhenDisabled_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions { EnableRateLimiting = false };

        // Act
        var result = _validator.Validate("test", options);

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithinLimit_ReturnsAllowed()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableRateLimiting = true,
            RateLimitMaxRequests = 5,
            RateLimitWindowSeconds = 60,
            RateLimitClientId = "test-client-1"
        };
        _validator.ResetClient("test-client-1");

        // Act - Make 5 requests (at the limit)
        for (int i = 0; i < 5; i++)
        {
            var result = _validator.Validate("test", options);
            result.IsAllowed.Should().BeTrue($"Request {i + 1} should be allowed");
        }
    }

    [Fact]
    public void Validate_ExceedsLimit_ReturnsBlocked()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableRateLimiting = true,
            RateLimitMaxRequests = 3,
            RateLimitWindowSeconds = 60,
            RateLimitClientId = "test-client-2"
        };
        _validator.ResetClient("test-client-2");

        // Act - Make 3 requests (at the limit)
        for (int i = 0; i < 3; i++)
        {
            _validator.Validate("test", options);
        }

        // 4th request should be blocked
        var result = _validator.Validate("test", options);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ViolationType.Should().Be(GuardrailViolationType.RateLimitExceeded);
    }

    [Fact]
    public void Validate_DifferentClients_IndependentLimits()
    {
        // Arrange
        var options1 = new GuardrailsOptions
        {
            EnableRateLimiting = true,
            RateLimitMaxRequests = 2,
            RateLimitWindowSeconds = 60,
            RateLimitClientId = "client-a"
        };
        var options2 = new GuardrailsOptions
        {
            EnableRateLimiting = true,
            RateLimitMaxRequests = 2,
            RateLimitWindowSeconds = 60,
            RateLimitClientId = "client-b"
        };
        _validator.ResetClient("client-a");
        _validator.ResetClient("client-b");

        // Act - Exhaust client-a's limit
        _validator.Validate("test", options1);
        _validator.Validate("test", options1);
        var clientAResult = _validator.Validate("test", options1);

        // client-b should still be allowed
        var clientBResult = _validator.Validate("test", options2);

        // Assert
        clientAResult.IsAllowed.Should().BeFalse();
        clientBResult.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void GetCurrentCount_ReturnsCorrectCount()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableRateLimiting = true,
            RateLimitMaxRequests = 10,
            RateLimitWindowSeconds = 60,
            RateLimitClientId = "count-test"
        };
        _validator.ResetClient("count-test");

        // Act
        _validator.Validate("test", options);
        _validator.Validate("test", options);
        _validator.Validate("test", options);

        // Assert
        _validator.GetCurrentCount("count-test").Should().Be(3);
    }

    [Fact]
    public void ResetClient_ClearsCounter()
    {
        // Arrange
        var options = new GuardrailsOptions
        {
            EnableRateLimiting = true,
            RateLimitMaxRequests = 2,
            RateLimitWindowSeconds = 60,
            RateLimitClientId = "reset-test"
        };
        _validator.ResetClient("reset-test");

        // Exhaust limit
        _validator.Validate("test", options);
        _validator.Validate("test", options);

        // Act
        _validator.ResetClient("reset-test");

        // Assert - should be allowed again
        var result = _validator.Validate("test", options);
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Priority_ShouldBe5()
    {
        _validator.Priority.Should().Be(5);
    }
}
