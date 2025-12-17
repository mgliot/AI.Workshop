using System.Text.Json.Serialization;

namespace AI.Workshop.ConsoleApps.Agents;

internal record PersonInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }
}
