using Blue.Mail2Epic.Tests.Infrastructure;
using Blue.Mail2Epic.Tests.TestData;

namespace Blue.Mail2Epic.Tests;

[Collection("Postgres")]
public class EpicRelevanceService_ProjectClassificationTests(PostgresContainerFixture postgres) : Mail2EpicTest
{
    [Fact]
    [Trait("Category", "Accuracy")]
    [Trait("Category", "LiveAzure")]
    public async Task GetEmailAnalysis_ProjectClassificationAccuracy_MeetsThreshold()
    {
        Assert.SkipUnless(
            TryGetConfiguration(out var azureOptions, out var promptOptions),
            "Missing Azure AI options or prompt options for live accuracy tests.");

        var scenarios =
            await LabeledDataFixtureLoader.LoadProjectClassificationScenariosAsync(TestContext.Current.CancellationToken);

        await using var context = await LiveAzureMail2EpicTestContext.CreateAsync(
            postgres,
            azureOptions,
            promptOptions,
            ct: TestContext.Current.CancellationToken);

        var correct = 0;
        var results = new List<EvaluationCaseResult>();

        foreach (var scenario in scenarios)
        {
            var analysis = await context.EmailTriageService.GetEmailAnalysis(
                scenario.ToEmailDataDto(),
                TestContext.Current.CancellationToken);

            var isCorrect = string.Equals(
                analysis.EpicKey,
                scenario.ExpectedEpicKey,
                StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                correct++;
            }

            results.Add(new EvaluationCaseResult
            {
                Name = scenario.Name,
                Expected = scenario.ExpectedEpicKey,
                Actual = analysis.EpicKey ?? "<none>",
                IsCorrect = isCorrect,
                Notes = $"confidence={analysis.EpicConfidence?.ToString() ?? "<none>"}"
            });
        }

        var reportPath = await WriteEvaluationReportAsync(
            "project-classification-accuracy",
            results,
            TestContext.Current.CancellationToken);
        var minimumPassingCount = GetMinimumPassingCount(scenarios.Count);

        Assert.True(
            correct >= minimumPassingCount,
            $"Expected at least {minimumPassingCount}/{scenarios.Count} project-classification cases to be correct, but got {correct}/{scenarios.Count}. Report: {reportPath}");
    }
}
