using AI.Workshop.Common;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests;

public class AISettingsTests
{
    [Fact]
    public void AISettings_HasCorrectDefaults()
    {
        // Arrange & Act
        var settings = new AISettings();

        // Assert
        settings.OllamaUri.Should().Be(AIConstants.DefaultOllamaUri);
        settings.ChatModel.Should().Be(AIConstants.DefaultChatModel);
        settings.EmbeddingModel.Should().Be(AIConstants.DefaultEmbeddingModel);
        settings.VectorDimensions.Should().Be(AIConstants.VectorDimensions);
    }

    [Fact]
    public void GetOllamaUri_ReturnsValidUri()
    {
        // Arrange
        var settings = new AISettings
        {
            OllamaUri = "http://custom-server:11434/"
        };

        // Act
        var uri = settings.GetOllamaUri();

        // Assert
        uri.Should().NotBeNull();
        uri.Host.Should().Be("custom-server");
        uri.Port.Should().Be(11434);
    }

    [Fact]
    public void SectionName_IsAI()
    {
        // Assert
        AISettings.SectionName.Should().Be("AI");
    }

    [Fact]
    public void AISettings_CanBeModified()
    {
        // Arrange
        var settings = new AISettings();

        // Act
        settings.OllamaUri = "http://localhost:8080/";
        settings.ChatModel = "mistral";
        settings.EmbeddingModel = "nomic-embed";
        settings.VectorDimensions = 768;

        // Assert
        settings.OllamaUri.Should().Be("http://localhost:8080/");
        settings.ChatModel.Should().Be("mistral");
        settings.EmbeddingModel.Should().Be("nomic-embed");
        settings.VectorDimensions.Should().Be(768);
    }
}
