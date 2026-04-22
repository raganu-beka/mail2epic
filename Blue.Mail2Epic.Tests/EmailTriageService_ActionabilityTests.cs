using Blue.Mail2Epic.Tests.Infrastructure;
using Blue.Mail2Epic.Tests.TestData;

namespace Blue.Mail2Epic.Tests;

[Collection("Postgres")]
public class EmailTriageService_ActionabilityTests(PostgresContainerFixture postgres) : Mail2EpicTest
{
    [Fact]
    [Trait("Category", "Accuracy")]
    [Trait("Category", "LiveAzure")]
    public async Task GetEmailAnalysis_ActionabilityAccuracy_MeetsThreshold()
    {
        Assert.SkipUnless(
            TryGetConfiguration(out var azureOptions, out var promptOptions),
            "Missing Azure AI options or prompt options for live accuracy tests.");

        var scenarios = await LabeledDataFixtureLoader.LoadActionabilityScenariosAsync(TestContext.Current.CancellationToken);

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

            var isCorrect = analysis.RequiresAction == scenario.ExpectedRequiresAction;
            if (isCorrect)
            {
                correct++;
            }

            results.Add(new EvaluationCaseResult
            {
                Name = scenario.Name,
                Expected = scenario.ExpectedRequiresAction.ToString(),
                Actual = analysis.RequiresAction.ToString(),
                IsCorrect = isCorrect,
                Notes = $"epicKey={analysis.EpicKey ?? "<none>"}"
            });
        }

        var reportPath = await WriteEvaluationReportAsync(
            "actionability-accuracy",
            results,
            TestContext.Current.CancellationToken);
        var minimumPassingCount = GetMinimumPassingCount(scenarios.Count);

        Assert.True(
            correct >= minimumPassingCount,
            $"Expected at least {minimumPassingCount}/{scenarios.Count} actionability cases to be correct, but got {correct}/{scenarios.Count}. Report: {reportPath}");
    }
}
