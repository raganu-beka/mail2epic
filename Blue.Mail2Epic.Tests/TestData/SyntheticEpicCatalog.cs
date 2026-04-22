using System.Text.Json;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Services;

namespace Blue.Mail2Epic.Tests.TestData;

public static class SyntheticEpicCatalog
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        NormalizationService normalizationService,
        CancellationToken ct)
    {
        foreach (var issue in await LoadIssuesAsync(ct))
        {
            var normalized = await normalizationService.NormalizeEpic(issue, ct);
            if (normalized is not null)
            {
                dbContext.Epics.Add(normalized);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task<IReadOnlyList<JiraIssueResponse>> LoadIssuesAsync(CancellationToken ct)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "TestData", "LabeledData", "epic-catalog.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Labeled epic catalog was not found.", filePath);
        }

        var json = await File.ReadAllTextAsync(filePath, ct);
        var fixtures = JsonSerializer.Deserialize<List<AccuracyEpicFixture>>(json, LabeledDataFixtureLoader.SerializerOptions) ?? [];

        return fixtures
            .Select(fixture => new JiraIssueResponse
            {
                Key = fixture.Key,
                Fields = new JiraIssueFields
                {
                    Summary = fixture.Fields.Summary,
                    Description = fixture.Fields.Description,
                    UpdatedAt = fixture.Fields.UpdatedAt
                }
            })
            .ToList();
    }
}
