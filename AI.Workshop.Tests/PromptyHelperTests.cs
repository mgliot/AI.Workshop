using AI.Workshop.Common;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests;

public class PromptyHelperTests : IDisposable
{
    private readonly string _originalDirectory;

    public PromptyHelperTests()
    {
        // Store the original directory before each test
        _originalDirectory = PromptyHelper.PromptsDirectory;
    }

    public void Dispose()
    {
        // Reset to original after each test
        PromptyHelper.PromptsDirectory = _originalDirectory;
    }

    [Fact]
    public void GetSystemPrompt_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange & Act
        var act = () => PromptyHelper.GetSystemPrompt("NonExistent");

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*NonExistent.prompty*");
    }

    [Fact]
    public void GetSystemPrompt_WithNullName_ThrowsFileNotFoundException()
    {
        // Arrange & Act
        var act = () => PromptyHelper.GetSystemPrompt(null!);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetSystemPrompt_WithEmptyName_ThrowsFileNotFoundException()
    {
        // Arrange & Act
        var act = () => PromptyHelper.GetSystemPrompt(string.Empty);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void PromptsDirectory_CanBeSetAndRetrieved()
    {
        // Arrange
        var customDir = @"C:\CustomPrompts";

        // Act
        PromptyHelper.PromptsDirectory = customDir;

        // Assert
        PromptyHelper.PromptsDirectory.Should().Be(customDir);
    }
}
