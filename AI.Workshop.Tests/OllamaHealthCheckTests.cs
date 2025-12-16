using AI.Workshop.Common;
using Xunit;

namespace AI.Workshop.Tests;

/// <summary>
/// Tests for OllamaHealthCheck functionality
/// </summary>
public class OllamaHealthCheckTests
{
    [Fact]
    public async Task CheckAsync_WithInvalidUri_ReturnsUnhealthy()
    {
        // Arrange - use a port that's unlikely to be in use
        var invalidUri = "http://localhost:59999/";

        // Act
        var result = await OllamaHealthCheck.CheckAsync(invalidUri);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Contains("Cannot connect", result.Message);
    }

    [Fact]
    public async Task CheckAsync_WithUri_ReturnsHealthCheckResult()
    {
        // Arrange
        var uri = new Uri("http://localhost:59999/");

        // Act
        var result = await OllamaHealthCheck.CheckAsync(uri);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OllamaHealthCheck.HealthCheckResult>(result);
    }

    [Fact]
    public async Task WaitForOllamaAsync_WithInvalidUri_ReturnsUnhealthyAfterRetries()
    {
        // Arrange
        var invalidUri = "http://localhost:59998/";
        var maxRetries = 2;
        var delay = TimeSpan.FromMilliseconds(100);

        // Act
        var result = await OllamaHealthCheck.WaitForOllamaAsync(
            invalidUri, 
            maxRetries, 
            delay);

        // Assert
        Assert.False(result.IsHealthy);
    }

    [Fact]
    public async Task WaitForOllamaAsync_WithCancellation_ReturnsCancelledResult()
    {
        // Arrange
        var invalidUri = "http://localhost:59997/";
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var result = await OllamaHealthCheck.WaitForOllamaAsync(
            invalidUri, 
            maxRetries: 10, 
            delayBetweenRetries: TimeSpan.FromSeconds(1),
            cancellationToken: cts.Token);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Contains("cancelled", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HealthCheckResult_HasCorrectProperties()
    {
        // Arrange & Act
        var result = new OllamaHealthCheck.HealthCheckResult(
            IsHealthy: true, 
            Message: "Ollama is running", 
            Version: "Ollama is running");

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Ollama is running", result.Message);
        Assert.Equal("Ollama is running", result.Version);
    }

    [Fact]
    public void HealthCheckResult_WithoutVersion_HasNullVersion()
    {
        // Arrange & Act
        var result = new OllamaHealthCheck.HealthCheckResult(
            IsHealthy: false, 
            Message: "Connection failed");

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal("Connection failed", result.Message);
        Assert.Null(result.Version);
    }
}
