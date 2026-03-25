using System.Text.Json;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Services;
using Microsoft.EntityFrameworkCore;

namespace Blue.Mail2Epic.Tests.Fixtures;

public static class EpicFixtureSeeder
{
    public static async Task<IReadOnlyList<Epic>> SeedFromFileAsync(
        AppDbContext dbContext,
        NormalizationService normalizationService,
        string filePath,
        ITestOutputHelper output,
        CancellationToken ct)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Epic fixture file not found.", filePath);
        }

        var json = await File.ReadAllTextAsync(filePath, ct);
        var fixtures = JsonSerializer.Deserialize<List<EpicFixture>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<EpicFixture>();

        var seeded = new List<Epic>();

        foreach (var fixture in fixtures)
        {
            output.WriteLine("Processing epic: {0}", fixture.Key);
            var exists = await dbContext.Epics.AnyAsync(e => e.Key == fixture.Key, ct);
            if (exists) continue;

            var dto = new JiraIssueResponse
            {
                Key = fixture.Key,
                Fields = new JiraIssueFields
                {
                    Summary = fixture.Fields.Summary,
                    Description = fixture.Fields.Description,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var normalized = await normalizationService.NormalizeEpic(dto, ct);
            if (normalized == null) continue;

            dbContext.Epics.Add(normalized);
            seeded.Add(normalized);
        }

        await dbContext.SaveChangesAsync(ct);
        return seeded;
    }
}
