using System.Text.Json.Serialization;

namespace Blue.Mail2Epic.Tests.Fixtures;

public sealed class EpicFixture
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("fields")]
    public required EpicFixtureFields Fields { get; init; }
}

public sealed class EpicFixtureFields
{
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

