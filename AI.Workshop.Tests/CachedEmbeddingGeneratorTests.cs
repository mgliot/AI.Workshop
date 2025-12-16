using AI.Workshop.Common.Caching;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace AI.Workshop.Tests;

public class CachedEmbeddingGeneratorTests
{
    private readonly Mock<IEmbeddingGenerator<string, Embedding<float>>> _mockGenerator;
    private readonly CachedEmbeddingGenerator _cachedGenerator;

    public CachedEmbeddingGeneratorTests()
    {
        _mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        _cachedGenerator = new CachedEmbeddingGenerator(_mockGenerator.Object);
    }

    [Fact]
    public async Task GenerateAsync_FirstCall_CallsInnerGenerator()
    {
        // Arrange
        var input = new[] { "test text" };
        var expectedEmbedding = new Embedding<float>(new float[] { 0.1f, 0.2f, 0.3f });
        var expectedResult = new GeneratedEmbeddings<Embedding<float>>([expectedEmbedding]);

        _mockGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _cachedGenerator.GenerateAsync(input);

        // Assert
        result.Should().HaveCount(1);
        _mockGenerator.Verify(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_SecondCallSameText_ReturnsCached()
    {
        // Arrange
        var input = new[] { "cached text" };
        var expectedEmbedding = new Embedding<float>(new float[] { 0.1f, 0.2f, 0.3f });
        var expectedResult = new GeneratedEmbeddings<Embedding<float>>([expectedEmbedding]);

        _mockGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _cachedGenerator.GenerateAsync(input); // First call
        await _cachedGenerator.GenerateAsync(input); // Second call (should be cached)

        // Assert
        _mockGenerator.Verify(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
        _cachedGenerator.CacheHits.Should().Be(1);
        _cachedGenerator.CacheMisses.Should().Be(1);
    }

    [Fact]
    public async Task GenerateAsync_DifferentTexts_CallsGeneratorForEach()
    {
        // Arrange
        var input1 = new[] { "text 1" };
        var input2 = new[] { "text 2" };
        var embedding1 = new Embedding<float>(new float[] { 0.1f });
        var embedding2 = new Embedding<float>(new float[] { 0.2f });

        _mockGenerator
            .SetupSequence(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>([embedding1]))
            .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>([embedding2]));

        // Act
        await _cachedGenerator.GenerateAsync(input1);
        await _cachedGenerator.GenerateAsync(input2);

        // Assert
        _mockGenerator.Verify(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public void ClearCache_ResetsAll()
    {
        // Arrange - populate cache
        _cachedGenerator.ClearCache();

        // Assert
        _cachedGenerator.CacheSize.Should().Be(0);
        _cachedGenerator.CacheHits.Should().Be(0);
        _cachedGenerator.CacheMisses.Should().Be(0);
    }

    [Fact]
    public async Task HitRate_CalculatesCorrectly()
    {
        // Arrange
        var input = new[] { "rate test" };
        var embedding = new Embedding<float>(new float[] { 0.1f });
        var result = new GeneratedEmbeddings<Embedding<float>>([embedding]);

        _mockGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _cachedGenerator.GenerateAsync(input); // Miss
        await _cachedGenerator.GenerateAsync(input); // Hit
        await _cachedGenerator.GenerateAsync(input); // Hit

        // Assert - 2 hits, 1 miss = 66.67%
        _cachedGenerator.HitRate.Should().BeApproximately(66.67, 0.1);
    }

    [Fact]
    public async Task IsInCache_ReturnsTrueForCachedItem()
    {
        // Arrange
        var text = "in cache test";
        var input = new[] { text };
        var embedding = new Embedding<float>(new float[] { 0.1f });
        var result = new GeneratedEmbeddings<Embedding<float>>([embedding]);

        _mockGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _cachedGenerator.GenerateAsync(input);

        // Assert
        _cachedGenerator.IsInCache(text).Should().BeTrue();
        _cachedGenerator.IsInCache("not in cache").Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFromCache_RemovesItem()
    {
        // Arrange
        var text = "remove test";
        var input = new[] { text };
        var embedding = new Embedding<float>(new float[] { 0.1f });
        var result = new GeneratedEmbeddings<Embedding<float>>([embedding]);

        _mockGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        await _cachedGenerator.GenerateAsync(input);
        _cachedGenerator.IsInCache(text).Should().BeTrue();

        // Act
        var removed = _cachedGenerator.RemoveFromCache(text);

        // Assert
        removed.Should().BeTrue();
        _cachedGenerator.IsInCache(text).Should().BeFalse();
    }

    [Fact]
    public async Task GetStats_ReturnsCorrectData()
    {
        // Arrange
        var input = new[] { "stats test" };
        var embedding = new Embedding<float>(new float[] { 0.1f });
        var result = new GeneratedEmbeddings<Embedding<float>>([embedding]);

        _mockGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        await _cachedGenerator.GenerateAsync(input);
        await _cachedGenerator.GenerateAsync(input);

        // Act
        var stats = _cachedGenerator.GetStats();

        // Assert
        stats.CacheSize.Should().Be(1);
        stats.CacheHits.Should().Be(1);
        stats.CacheMisses.Should().Be(1);
        stats.HitRate.Should().Be(50);
    }

    [Fact]
    public void GetService_Type_DelegatesToInnerGenerator()
    {
        // Arrange
        var testService = new object();
        _mockGenerator.Setup(g => g.GetService(typeof(object), null)).Returns(testService);

        // Act
        var result = _cachedGenerator.GetService(typeof(object), null);

        // Assert
        result.Should().Be(testService);
    }

    [Fact]
    public async Task GenerateAsync_WithMultipleInputs_CachesMixCorrectly()
    {
        // Arrange
        var cachedText = "already cached";
        var newText = "new text";
        var embedding1 = new Embedding<float>(new float[] { 0.1f });
        var embedding2 = new Embedding<float>(new float[] { 0.2f });

        _mockGenerator
            .SetupSequence(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>([embedding1]))
            .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>([embedding2]));

        // Cache the first text
        await _cachedGenerator.GenerateAsync(new[] { cachedText });

        // Act - request both (one cached, one new)
        var result = await _cachedGenerator.GenerateAsync(new[] { cachedText, newText });

        // Assert
        result.Should().HaveCount(2);
        _cachedGenerator.CacheHits.Should().Be(1);
        _cachedGenerator.CacheMisses.Should().Be(2); // Initial miss + new text miss
    }
}
