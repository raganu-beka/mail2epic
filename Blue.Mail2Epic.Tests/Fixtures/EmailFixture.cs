using System.Text.Json.Serialization;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Dtos.Email;

namespace Blue.Mail2Epic.Tests.Fixtures;

public sealed class EmailFixture
{
    [JsonPropertyName("email")]
    public required EmailFixtureEmail Email { get; init; }

    [JsonPropertyName("labels")]
    public required EmailFixtureLabels Labels { get; init; }

    [JsonPropertyName("seedMappings")]
    public List<EmailMappingFixture>? SeedMappings { get; init; }

    public EmailDataDto ToEmailDataDto()
    {
        return new EmailDataDto
        {
            MessageId = Email.MessageId,
            InReplyTo = Email.InReplyTo,
            References = Email.References ?? [],
            From = Email.From,
            Subject = Email.Subject,
            Body = Email.Body,
            Date = Email.Date
        };
    }
}

public sealed class EmailFixtureEmail
{
    [JsonPropertyName("messageId")]
    public required string MessageId { get; init; }

    [JsonPropertyName("inReplyTo")]
    public string? InReplyTo { get; init; }

    [JsonPropertyName("references")]
    public List<string>? References { get; init; }

    [JsonPropertyName("from")]
    public required string From { get; init; }

    [JsonPropertyName("subject")]
    public required string Subject { get; init; }

    [JsonPropertyName("body")]
    public required string Body { get; init; }

    [JsonPropertyName("date")]
    public required DateTimeOffset Date { get; init; }
}

public sealed class EmailFixtureLabels
{
    [JsonPropertyName("requiresAction")]
    public bool RequiresAction { get; init; }

    [JsonPropertyName("expectedEpicKey")]
    public string? ExpectedEpicKey { get; init; }
}

public sealed class EmailMappingFixture
{
    [JsonPropertyName("messageId")]
    public required string MessageId { get; init; }

    [JsonPropertyName("threadRootMessageId")]
    public string? ThreadRootMessageId { get; init; }

    [JsonPropertyName("jiraIssueKey")]
    public required string JiraIssueKey { get; init; }

    [JsonPropertyName("actionTaken")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RequiredAction ActionTaken { get; init; } = RequiredAction.CreateNewIssue;
}

