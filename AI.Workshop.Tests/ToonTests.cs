using AI.Workshop.Common.Toon;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Xunit;

namespace AI.Workshop.Tests;

public class ToonHelperTests
{
    [Fact]
    public void ToToon_SimpleObject_ReturnsValidToon()
    {
        // Arrange
        var data = new { id = 1, name = "Test", active = true };

        // Act
        var toon = ToonHelper.ToToon(data);

        // Assert
        toon.Should().Contain("id: 1");
        toon.Should().Contain("name: Test");
        toon.Should().Contain("active: true");
    }

    [Fact]
    public void FromToon_ValidToon_ReturnsObject()
    {
        // Arrange - First serialize to get the correct format
        var original = new TestUser { Id = 123, Name = "Alice", Active = true };
        var toon = ToonHelper.ToToon(original);

        // Act
        var result = ToonHelper.FromToon<TestUser>(toon);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Name.Should().Be("Alice");
        result.Active.Should().BeTrue();
    }

    [Fact]
    public void TryFromToon_ValidToon_ReturnsTrueAndObject()
    {
        // Arrange - First serialize to get the correct format
        var original = new TestUser { Id = 456, Name = "Bob", Active = false };
        var toon = ToonHelper.ToToon(original);

        // Act
        var success = ToonHelper.TryFromToon<TestUser>(toon, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Id.Should().Be(456);
    }

    [Fact]
    public void ToToon_NestedObject_ReturnsIndentedToon()
    {
        // Arrange
        var data = new
        {
            user = new { id = 1, name = "Test" }
        };

        // Act
        var toon = ToonHelper.ToToon(data);

        // Assert
        toon.Should().Contain("user:");
        toon.Should().Contain("id: 1");
    }

    [Fact]
    public void ToToon_Array_ReturnsCompactFormat()
    {
        // Arrange
        var data = new { tags = new[] { "admin", "user", "guest" } };

        // Act
        var toon = ToonHelper.ToToon(data);

        // Assert
        toon.Should().Contain("tags[3]:");
    }

    [Fact]
    public void JsonToToon_ValidJson_ConvertsProperly()
    {
        // Arrange
        var json = """{"id":1,"name":"Test"}""";

        // Act
        var toon = ToonHelper.JsonToToon(json);

        // Assert
        toon.Should().Contain("id: 1");
        toon.Should().Contain("name: Test");
        toon.Should().NotContain("{");
        toon.Should().NotContain("}");
    }

    [Fact]
    public void ToonToJson_ValidToon_ConvertsToJson()
    {
        // Arrange
        var toon = """
            id: 42
            name: Charlie
            """;

        // Act
        var json = ToonHelper.ToonToJson(toon);

        // Assert
        json.Should().Contain("\"id\"");
        json.Should().Contain("42");
        json.Should().Contain("\"name\"");
        json.Should().Contain("\"Charlie\"");
    }

    [Fact]
    public void EstimateTokenSavings_WithData_ReturnsSavingsInfo()
    {
        // Arrange
        var data = new[]
        {
            new { id = 1, name = "Alice", role = "admin" },
            new { id = 2, name = "Bob", role = "user" },
            new { id = 3, name = "Charlie", role = "guest" }
        };

        // Act
        var savings = ToonHelper.EstimateTokenSavings(data);

        // Assert
        savings.JsonLength.Should().BeGreaterThan(0);
        savings.ToonLength.Should().BeGreaterThan(0);
        savings.ToonLength.Should().BeLessThan(savings.JsonLength); // TOON should be more compact
        savings.SavingsPercent.Should().BeGreaterThan(0);
    }

    [Fact]
    public void FormatForPrompt_WithLabel_IncludesLabel()
    {
        // Arrange
        var data = new { value = 42 };

        // Act
        var result = ToonHelper.FormatForPrompt(data, "Input Data");

        // Assert
        result.Should().StartWith("Input Data:");
        result.Should().Contain("value: 42");
    }

    [Fact]
    public void FormatAsCodeBlock_ReturnsCodeBlock()
    {
        // Arrange
        var data = new { x = 1, y = 2 };

        // Act
        var result = ToonHelper.FormatAsCodeBlock(data, "Coordinates:");

        // Assert
        result.Should().Contain("```toon");
        result.Should().Contain("```");
        result.Should().Contain("x: 1");
        result.Should().Contain("y: 2");
    }

    [Fact]
    public void ToJsonNode_ValidToon_ReturnsJsonNode()
    {
        // Arrange - Use serialized TOON to ensure correct format
        var data = new { key = "value", number = 42 };
        var toon = ToonHelper.ToToon(data);

        // Act
        var node = ToonHelper.ToJsonNode(toon);

        // Assert
        node.Should().NotBeNull();
        var obj = node!.AsObject();
        obj["key"]!.ToString().Should().Be("value");
        // Numbers may be parsed as double
        Convert.ToInt32(obj["number"]!.GetValue<double>()).Should().Be(42);
    }

    private class TestUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}

public class ToonPromptBuilderTests
{
    [Fact]
    public void Build_WithSystemPrompt_IncludesSystemMessage()
    {
        // Arrange & Act
        var messages = ToonPromptBuilder.Create()
            .WithSystemPrompt("You are a helpful assistant")
            .WithUserPrompt("Hello")
            .Build();

        // Assert
        messages.Should().HaveCount(2);
        messages[0].Role.Should().Be(ChatRole.System);
        messages[0].Text.Should().Contain("You are a helpful assistant");
    }

    [Fact]
    public void Build_WithData_IncludesToonFormattedData()
    {
        // Arrange & Act
        var messages = ToonPromptBuilder.Create()
            .WithData(new { id = 1, name = "Test" }, "User Info")
            .WithUserPrompt("What is the user's name?")
            .Build();

        // Assert
        messages.Should().HaveCount(1);
        var content = messages[0].Text;
        content.Should().Contain("User Info:");
        content.Should().Contain("id: 1");
        content.Should().Contain("What is the user's name?");
    }

    [Fact]
    public void Build_WithCodeBlocks_WrapsInCodeBlocks()
    {
        // Arrange & Act
        var messages = ToonPromptBuilder.Create()
            .WithData(new { value = 42 })
            .UseCodeBlocks()
            .WithUserPrompt("Process this data")
            .Build();

        // Assert
        var content = messages[0].Text;
        content.Should().Contain("```toon");
        content.Should().Contain("```");
    }

    [Fact]
    public void Build_WithMultipleDataSections_IncludesAll()
    {
        // Arrange & Act
        var messages = ToonPromptBuilder.Create()
            .WithData(new { type = "input" }, "Input")
            .WithData(new { type = "config" }, "Config")
            .WithUserPrompt("Analyze")
            .Build();

        // Assert
        var content = messages[0].Text;
        content.Should().Contain("Input:");
        content.Should().Contain("Config:");
        content.Should().Contain("type: input");
        content.Should().Contain("type: config");
    }

    [Fact]
    public void GetEstimatedSavings_ReturnsValidInfo()
    {
        // Arrange
        var builder = ToonPromptBuilder.Create()
            .WithData(new { id = 1, name = "Alice", role = "admin" })
            .WithUserPrompt("Describe this user");

        // Act
        var savings = builder.GetEstimatedSavings();

        // Assert
        savings.JsonEquivalentLength.Should().BeGreaterThan(0);
        savings.ToonLength.Should().BeGreaterThan(0);
    }

    [Fact]
    public void WithDataCollection_FormatsAsArray()
    {
        // Arrange
        var items = new[] { "apple", "banana", "cherry" };

        // Act
        var messages = ToonPromptBuilder.Create()
            .WithDataCollection(items, "Fruits")
            .Build();

        // Assert
        var content = messages[0].Text;
        content.Should().Contain("Fruits:");
        content.Should().Contain("items[3]:");
    }
}

public class ToonChatExtensionsTests
{
    [Fact]
    public void CreateToonMessage_ReturnsMessageWithToonData()
    {
        // Arrange
        var data = new { id = 1, name = "Test" };

        // Act
        var message = ToonChatExtensions.CreateToonMessage(
            ChatRole.User, 
            data, 
            "What is the ID?", 
            "Data");

        // Assert
        message.Role.Should().Be(ChatRole.User);
        message.Text.Should().Contain("Data:");
        message.Text.Should().Contain("id: 1");
        message.Text.Should().Contain("What is the ID?");
    }

    [Fact]
    public void AddToonMessage_AddsToList()
    {
        // Arrange
        var messages = new List<ChatMessage>();
        var data = new { value = 42 };

        // Act
        messages.AddToonMessage(ChatRole.User, data, "Process this");

        // Assert
        messages.Should().HaveCount(1);
        messages[0].Text.Should().Contain("value: 42");
    }
}

public class ToonResponseParserTests
{
    [Fact]
    public void Parse_ToonCodeBlock_ExtractsAndParsesData()
    {
        // Arrange
        var response = """
            Here is the analysis:

            ```toon
            Id: 123
            Name: Test Product
            Price: 29.99
            ```

            Let me know if you need more information.
            """;

        // Act
        var result = ToonResponseParser.Parse<ProductResponse>(response);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(123);
        result.Data.Name.Should().Be("Test Product");
        result.Data.Price.Should().BeApproximately(29.99, 0.01);
        result.AdditionalText.Should().Contain("Here is the analysis");
    }

    [Fact]
    public void Parse_PlainToon_ParsesSuccessfully()
    {
        // Arrange - Use serialized format
        var original = new ProductResponse { Id = 456, Name = "Widget", Price = 9.99 };
        var toon = ToonHelper.ToToon(original);

        // Act
        var result = ToonResponseParser.Parse<ProductResponse>(toon);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(456);
    }

    [Fact]
    public void TryParse_ValidToon_ReturnsTrueWithData()
    {
        // Arrange
        var original = new ProductResponse { Id = 789, Name = "Gadget", Price = 19.99 };
        var response = $"```toon\n{ToonHelper.ToToon(original)}\n```";

        // Act
        var success = ToonResponseParser.TryParse<ProductResponse>(response, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Id.Should().Be(789);
    }

    [Fact]
    public void TryParse_InvalidContent_ReturnsFalse()
    {
        // Arrange
        var response = "This is just plain text with no structured data.";

        // Act
        var success = ToonResponseParser.TryParse<ProductResponse>(response, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractAllToonBlocks_MultipleBlocks_ExtractsAll()
    {
        // Arrange
        var response = """
            First block:
            ```toon
            id: 1
            name: First
            ```

            Second block:
            ```toon
            id: 2
            name: Second
            ```
            """;

        // Act
        var blocks = ToonResponseParser.ExtractAllToonBlocks(response);

        // Assert
        blocks.Should().HaveCount(2);
        blocks[0].Should().Contain("id: 1");
        blocks[1].Should().Contain("id: 2");
    }

    [Fact]
    public void CreateToonResponsePrompt_IncludesSchema()
    {
        // Arrange
        var schema = "id: number, name: string, active: boolean";

        // Act
        var prompt = ToonResponseParser.CreateToonResponsePrompt(schema);

        // Assert
        prompt.Should().Contain("TOON");
        prompt.Should().Contain(schema);
        prompt.Should().Contain("```toon");
    }

    [Fact]
    public void CreateToonResponsePromptWithExample_IncludesSerializedExample()
    {
        // Arrange
        var example = new { status = "success", count = 5 };

        // Act
        var prompt = ToonResponseParser.CreateToonResponsePromptWithExample(example, "Status report");

        // Assert
        prompt.Should().Contain("status: success");
        prompt.Should().Contain("count: 5");
        prompt.Should().Contain("Status report");
    }

    [Fact]
    public void ParseResult_GetDataOrThrow_ThrowsOnFailure()
    {
        // Arrange
        var result = ToonParseResult<ProductResponse>.Failed("Test error");

        // Act & Assert
        var act = () => result.GetDataOrThrow();
        act.Should().Throw<InvalidOperationException>().WithMessage("Test error");
    }

    [Fact]
    public void ParseResult_GetDataOrDefault_ReturnsDefaultOnFailure()
    {
        // Arrange
        var result = ToonParseResult<ProductResponse>.Failed("Test error");
        var defaultValue = new ProductResponse { Id = 999, Name = "Default", Price = 0 };

        // Act
        var data = result.GetDataOrDefault(defaultValue);

        // Assert
        data.Should().Be(defaultValue);
    }

    [Fact]
    public void Parse_GenericCodeBlock_FallsBackToGenericParsing()
    {
        // Arrange
        var original = new ProductResponse { Id = 111, Name = "Generic", Price = 5.0 };
        var response = $"```\n{ToonHelper.ToToon(original)}\n```";

        // Act
        var result = ToonResponseParser.Parse<ProductResponse>(response);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(111);
    }

    private class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
    }
}
