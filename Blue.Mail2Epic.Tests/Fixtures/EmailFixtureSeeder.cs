using System.Text.Json;
using System.Text.Json.Serialization;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Blue.Mail2Epic.Tests.Fixtures;

public static class EmailFixtureSeeder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<IReadOnlyList<EmailFixture>> LoadFromFileAsync(string filePath, CancellationToken ct)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Email fixture file not found.", filePath);
        }

        var json = await File.ReadAllTextAsync(filePath, ct);
        return JsonSerializer.Deserialize<List<EmailFixture>>(json, SerializerOptions) ?? [];
    }

    public static async Task SeedMappingsAsync(AppDbContext dbContext, IEnumerable<EmailFixture> fixtures, CancellationToken ct)
    {
        var mappings = fixtures
            .Where(f => f.SeedMappings != null)
            .SelectMany(f => f.SeedMappings!)
            .ToList();

        if (mappings.Count == 0) return;

        foreach (var mapping in mappings)
        {
            var exists = await dbContext.EmailMappings.AnyAsync(e => e.MessageId == mapping.MessageId, ct);
            if (exists) continue;

            dbContext.EmailMappings.Add(new EmailMapping
            {
                MessageId = mapping.MessageId,
                ThreadRootMessageId = mapping.ThreadRootMessageId ?? mapping.MessageId,
                JiraIssueKey = mapping.JiraIssueKey,
                ActionTaken = mapping.ActionTaken,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(ct);
    }
}

