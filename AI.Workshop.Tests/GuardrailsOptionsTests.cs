using AI.Workshop.Guardrails;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests;

public class GuardrailsOptionsTests
{
    [Fact]
    public void GuardrailsOptions_HasCorrectDefaults()
    {
        // Arrange & Act
        var options = new GuardrailsOptions();

        // Assert
        options.MaxInputLength.Should().Be(10000);
        options.MaxOutputLength.Should().Be(50000);
        options.EnablePromptInjectionDetection.Should().BeTrue();
        options.EnablePiiDetection.Should().BeTrue();
        options.EnableToxicityFiltering.Should().BeTrue();
        options.EnableTopicRestriction.Should().BeFalse();
        options.AllowedTopics.Should().BeEmpty();
        options.BlockedKeywords.Should().BeEmpty();
        options.BlockedPatterns.Should().BeEmpty();
        options.DefaultAction.Should().Be(GuardrailAction.Block);
        options.BlockedResponseMessage.Should().Contain("cannot process");
    }

    [Fact]
    public void GuardrailsOptions_CanBeModified()
    {
        // Arrange
        var options = new GuardrailsOptions();

        // Act
        options.MaxInputLength = 5000;
        options.EnablePiiDetection = false;
        options.BlockedKeywords = ["test", "keyword"];
        options.DefaultAction = GuardrailAction.LogOnly;

        // Assert
        options.MaxInputLength.Should().Be(5000);
        options.EnablePiiDetection.Should().BeFalse();
        options.BlockedKeywords.Should().HaveCount(2);
        options.DefaultAction.Should().Be(GuardrailAction.LogOnly);
    }
}
