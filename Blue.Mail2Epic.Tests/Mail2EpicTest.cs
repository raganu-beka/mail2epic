using System.Text.Json;
using Blue.Mail2Epic.Models.Configuration;
using Blue.Mail2Epic.Tests.Infrastructure;

namespace Blue.Mail2Epic.Tests;

public abstract class Mail2EpicTest
{
    private static readonly JsonSerializerOptions ReportSerializerOptions = new()
    {
        WriteIndented = true
    };

    protected static int GetMinimumPassingCount(int totalCases)
    {
        return Math.Max(1, totalCases - 1);
    }

    protected static bool TryGetConfiguration(out AzureAiOptions azureOptions, out PromptOptions promptOptions)
    {
        var hasAzure = TestEnvironment.TryGetAzureAiOptions(out azureOptions);
        var hasPrompts = TestEnvironment.TryGetPromptOptions(out promptOptions);
        return hasAzure && hasPrompts;
    }

    protected static async Task<string> WriteEvaluationReportAsync(
        string suiteName,
        IReadOnlyList<EvaluationCaseResult> cases,
        CancellationToken ct)
    {
        var timestamp = DateTimeOffset.UtcNow;
        var safeSuiteName = suiteName
            .Replace(' ', '-')
            .Replace('_', '-')
            .ToLowerInvariant();

        var resultsDirectory = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(resultsDirectory);

        var filePath = Path.Combine(
            resultsDirectory,
            $"{timestamp:yyyyMMdd-HHmmss-fff}-{safeSuiteName}.json");

        var report = new EvaluationReport
        {
            SuiteName = suiteName,
            GeneratedAtUtc = timestamp,
            TotalCases = cases.Count,
            CorrectCases = cases.Count(result => result.IsCorrect),
            Cases = cases
        };

        await File.WriteAllTextAsync(
            filePath,
            JsonSerializer.Serialize(report, ReportSerializerOptions),
            ct);

        return filePath;
    }
}

public sealed class EvaluationReport
{
    public required string SuiteName { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
    public required int TotalCases { get; init; }
    public required int CorrectCases { get; init; }
    public required IReadOnlyList<EvaluationCaseResult> Cases { get; init; }
}

public sealed class EvaluationCaseResult
{
    public required string Name { get; init; }
    public required string Expected { get; init; }
    public required string Actual { get; init; }
    public required bool IsCorrect { get; init; }
    public string? Notes { get; init; }
}
