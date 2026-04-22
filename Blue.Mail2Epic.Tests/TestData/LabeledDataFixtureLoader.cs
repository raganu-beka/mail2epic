using System.Text.Json;
using System.Text.Json.Serialization;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Models.Dtos.Email;

namespace Blue.Mail2Epic.Tests.TestData;

public static class LabeledDataFixtureLoader
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static Task<IReadOnlyList<ActionabilityScenarioFixture>> LoadActionabilityScenariosAsync(CancellationToken ct)
    {
        return LoadAsync<ActionabilityScenarioFixture>("actionability-scenarios.json", ct);
    }

    public static Task<IReadOnlyList<ProjectClassificationScenarioFixture>> LoadProjectClassificationScenariosAsync(
        CancellationToken ct)
    {
        return LoadAsync<ProjectClassificationScenarioFixture>("project-classification-scenarios.json", ct);
    }

    public static Task<IReadOnlyList<NewInformationScenarioFixture>> LoadNewInformationScenariosAsync(CancellationToken ct)
    {
        return LoadAsync<NewInformationScenarioFixture>("new-information-scenarios.json", ct);
    }

    public static async Task<IReadOnlyDictionary<string, JiraIssueResponse>> LoadIssuesAsync(CancellationToken ct)
    {
        var fixtures = await LoadAsync<IssueFixture>("issues.json", ct);

        return fixtures.ToDictionary(
            fixture => fixture.IssueKey,
            fixture => new JiraIssueResponse
            {
                Key = fixture.IssueKey,
                Fields = new JiraIssueFields
                {
                    Summary = fixture.Summary,
                    Description = fixture.Description,
                    Comment = new JiraIssueComments
                    {
                        Comments = fixture.Comments
                            .Select(comment => new JiraComment { Body = comment })
                            .ToList()
                    }
                }
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<IReadOnlyList<TFixture>> LoadAsync<TFixture>(string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "TestData", "LabeledData", fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Labeled fixture file was not found.", filePath);
        }

        var json = await File.ReadAllTextAsync(filePath, ct);
        return JsonSerializer.Deserialize<List<TFixture>>(json, SerializerOptions) ?? [];
    }
}

public sealed class AccuracyEpicFixture
{
    public required string Key { get; init; }
    public required AccuracyEpicFieldsFixture Fields { get; init; }
}

public sealed class AccuracyEpicFieldsFixture
{
    public string? Summary { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ActionabilityScenarioFixture
{
    public required string Name { get; init; }
    public required AccuracyEmailFixture Email { get; init; }
    public bool ExpectedRequiresAction { get; init; }

    public EmailDataDto ToEmailDataDto() => Email.ToEmailDataDto();
}

public sealed class ProjectClassificationScenarioFixture
{
    public required string Name { get; init; }
    public required AccuracyEmailFixture Email { get; init; }
    public required string ExpectedEpicKey { get; init; }

    public EmailDataDto ToEmailDataDto() => Email.ToEmailDataDto();
}

public sealed class NewInformationScenarioFixture
{
    public required string Name { get; init; }
    public required string IssueKey { get; init; }
    public required AccuracyEmailFixture Email { get; init; }
    public bool ExpectedHasNewInformation { get; init; }

    public EmailDataDto ToEmailDataDto() => Email.ToEmailDataDto();
}

public sealed class AccuracyEmailFixture
{
    public required string MessageId { get; init; }
    public int UserAccountId { get; init; } = 1;
    public string? InReplyTo { get; init; }
    public List<string>? References { get; init; }
    public required string From { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public required DateTimeOffset Date { get; init; }

    public EmailDataDto ToEmailDataDto()
    {
        return new EmailDataDto
        {
            MessageId = MessageId,
            UserAccountId = UserAccountId,
            InReplyTo = InReplyTo,
            References = References ?? [],
            From = From,
            Subject = Subject,
            Body = Body,
            Date = Date
        };
    }
}

public sealed class IssueFixture
{
    public required string IssueKey { get; init; }
    public required string Summary { get; init; }
    public required string Description { get; init; }
    public List<string> Comments { get; init; } = [];
}
