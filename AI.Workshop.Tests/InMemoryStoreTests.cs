using AI.Workshop.VectorStore;
using FluentAssertions;
using Xunit;

namespace AI.Workshop.Tests;

public class VectorModelTests
{
    [Fact]
    public void VectorModel_HasDefaultValues()
    {
        // Arrange & Act
        var model = new VectorModel();

        // Assert
        model.Key.Should().Be(0);
        model.Name.Should().BeEmpty();
        model.Description.Should().BeEmpty();
    }

    [Fact]
    public void VectorModel_CanSetProperties()
    {
        // Arrange
        var embedding = CreateTestEmbedding();

        // Act
        var model = new VectorModel
        {
            Key = 1,
            Name = "Test Service",
            Description = "A test service description",
            Vector = embedding
        };

        // Assert
        model.Key.Should().Be(1);
        model.Name.Should().Be("Test Service");
        model.Description.Should().Be("A test service description");
        model.Vector.Length.Should().Be(384);
    }

    [Fact]
    public void VectorModel_VectorDimensionsMatch()
    {
        // The VectorModel should have 384 dimensions (all-minilm)
        var embedding = CreateTestEmbedding();
        var model = new VectorModel { Vector = embedding };

        model.Vector.Length.Should().Be(384);
    }

    private static ReadOnlyMemory<float> CreateTestEmbedding()
    {
        var embedding = new float[384];
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(i * 0.001);
        }
        return new ReadOnlyMemory<float>(embedding);
    }
}

public class SampleDataTests
{
    [Fact]
    public void CloudServices_ContainsData()
    {
        // Act
        var services = SampleData.CloudServices;

        // Assert
        services.Should().NotBeEmpty();
        services.Should().AllSatisfy(s =>
        {
            s.Name.Should().NotBeNullOrEmpty();
            s.Description.Should().NotBeNullOrEmpty();
        });
    }
}

