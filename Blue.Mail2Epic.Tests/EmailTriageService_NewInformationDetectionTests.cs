using Blue.Mail2Epic.Tests.Infrastructure;
using Blue.Mail2Epic.Tests.TestData;

namespace Blue.Mail2Epic.Tests;

[Collection("Postgres")]
public class EmailTriageService_NewInformationDetectionTests(PostgresContainerFixture postgres) : Mail2EpicTest
{
    [Fact]
    [Trait("Category", "Accuracy")]
    [Trait("Category", "LiveAzure")]
    public async Task GetIssueUpdate_NewInformationAccuracy_MeetsThreshold()
    {
        Assert.SkipUnless(
            TryGetConfiguration(out var azureOptions, out var promptOptions),
            "Missing Azure AI options or prompt options for live accuracy tests.");

        var scenarios =
            await LabeledDataFixtureLoader.LoadNewInformationScenariosAsync(TestContext.Current.CancellationToken);
        var issues = await LabeledDataFixtureLoader.LoadIssuesAsync(TestContext.Current.CancellationToken);

        await using var context = await LiveAzureMail2EpicTestContext.CreateAsync(
            postgres,
            azureOptions,
            promptOptions,
            issues.Values,
            TestContext.Current.CancellationToken);

        var correct = 0;
        var results = new List<EvaluationCaseResult>();

        foreach (var scenario in scenarios)
        {
            Assert.True(
                issues.ContainsKey(scenario.IssueKey),
                $"Issue '{scenario.IssueKey}' was not found for scenario '{scenario.Name}'.");

            var update = await context.EmailTriageService.GetIssueUpdate(
                scenario.ToEmailDataDto(),
                scenario.IssueKey,
                TestContext.Current.CancellationToken);

            var isCorrect = update.RequiresAction == scenario.ExpectedHasNewInformation;
            if (isCorrect)
            {
                correct++;
            }

            results.Add(new EvaluationCaseResult
            {
                Name = scenario.Name,
                Expected = scenario.ExpectedHasNewInformation.ToString(),
                Actual = update.RequiresAction.ToString(),
                IsCorrect = isCorrect,
                Notes = $"commentPresent={!string.IsNullOrWhiteSpace(update.Comment)}"
            });
        }

        var reportPath = await WriteEvaluationReportAsync(
            "new-information-accuracy",
            results,
            TestContext.Current.CancellationToken);
        var minimumPassingCount = GetMinimumPassingCount(scenarios.Count);

        Assert.True(
            correct >= minimumPassingCount,
            $"Expected at least {minimumPassingCount}/{scenarios.Count} new-information cases to be correct, but got {correct}/{scenarios.Count}. Report: {reportPath}");
    }
}
