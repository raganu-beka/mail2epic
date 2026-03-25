using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Services;
using Blue.Mail2Epic.Services;
using Blue.Mail2Epic.Tests.Fixtures;
using Blue.Mail2Epic.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Blue.Mail2Epic.Tests;

[Collection("Postgres")]
public class EmailAccuracyHarnessTests(PostgresContainerFixture postgres, ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task EmailFixtures_EndToEndAccuracy()
    {
        Assert.SkipUnless(TestEnvironment.TryGetAzureAiOptions(out var azureOptions), "Missing Azure AI environment variables.");

        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString, o => o.UseVector())
            .Options;

        await using var dbContext = new AppDbContext(dbOptions);
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);

        var azureAiService = new AzureAiService(Options.Create(azureOptions), NullLogger<AzureAiService>.Instance);
        var normalizationService = new NormalizationService(azureAiService, NullLogger<NormalizationService>.Instance);
        var epicRelevanceService = new EpicRelevanceService(NullLogger<EpicRelevanceService>.Instance, azureAiService, dbContext);

        var jiraOptions = Options.Create(new JiraOptions
        {
            BaseUrl = "http://localhost",
            AccessToken = "test",
            ProjectKey = "TEST",
            IssueType = "Task",
            EpicLinkFieldId = "customfield_10000",
            IssueLabel = "mail2epic"
        });
        var stubHandler = new JiraStubHandler();
        var httpClientFactory = new StubHttpClientFactory(new HttpClient(stubHandler) { BaseAddress = new Uri("http://localhost") });

        var triageService = new EmailTriageService(
            NullLogger<EmailTriageService>.Instance,
            dbContext,
            azureAiService,
            new JiraService(jiraOptions, NullLogger<JiraService>.Instance, httpClientFactory),
            normalizationService,
            epicRelevanceService);

        var epicFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "epics.json");
        await EpicFixtureSeeder.SeedFromFileAsync(dbContext, normalizationService, epicFilePath, output, CancellationToken.None);

        var emailFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "emails.json");
        var fixtures = await EmailFixtureSeeder.LoadFromFileAsync(emailFilePath, CancellationToken.None);
        await EmailFixtureSeeder.SeedMappingsAsync(dbContext, fixtures, CancellationToken.None);

        var emailAmountTotal = 0;
        var epicKeyCorrect = 0;
        var epicKeyRequiresActionTotal = 0;
        var epicKeyRequiresActionCorrect = 0;
        var requiredActionCorrect = 0;

        foreach (var fixture in fixtures)
        {
            var email = fixture.ToEmailDataDto();
            output.WriteLine(
                "Assessing email: {0} | {1} | {2}",
                email.MessageId,
                email.From,
                email.Subject);

            var (requiredAction, _) = await triageService.DetermineRequiredAction(email, CancellationToken.None);

            emailAmountTotal++;

            string? actualEpicKey = null;
            bool actualRequiresAction;

            if (requiredAction == RequiredAction.CreateNewIssue)
            {
                var classification = await triageService.GetEmailAnalysis(email, CancellationToken.None);
                actualRequiresAction = classification.RequiresAction;
                actualEpicKey = classification.EpicKey;
            }
            else
            {
                actualRequiresAction = requiredAction == RequiredAction.AppendExistingIssue;
            }

            if (actualRequiresAction == fixture.Labels.RequiresAction)
            {
                requiredActionCorrect++;
            }

            if (string.Equals(actualEpicKey, fixture.Labels.ExpectedEpicKey, StringComparison.OrdinalIgnoreCase))
            {
                epicKeyCorrect++;
            }

            if (actualRequiresAction && !string.IsNullOrWhiteSpace(fixture.Labels.ExpectedEpicKey))
            {
                epicKeyRequiresActionTotal++;
                if (string.Equals(actualEpicKey, fixture.Labels.ExpectedEpicKey, StringComparison.OrdinalIgnoreCase))
                {
                    epicKeyRequiresActionCorrect++;
                }
            }
        }

        var requiredActionAccuracy = emailAmountTotal == 0 ? 0 : (double)requiredActionCorrect / emailAmountTotal;
        var epicKeyAccuracy = emailAmountTotal == 0 ? 0 : (double)epicKeyCorrect / emailAmountTotal;
        var epicKeyRequiresActionAccuracy = epicKeyRequiresActionTotal == 0
            ? 0
            : (double)epicKeyRequiresActionCorrect / epicKeyRequiresActionTotal;

        output.WriteLine(
            "Required action accuracy: {0:P2} ({1}/{2})",
            requiredActionAccuracy,
            requiredActionCorrect,
            emailAmountTotal);
        output.WriteLine(
            "Epic key accuracy: {0:P2} ({1}/{2})",
            epicKeyAccuracy,
            epicKeyCorrect,
            emailAmountTotal);
        output.WriteLine(
            "Epic key accuracy (requires action): {0:P2} ({1}/{2})",
            epicKeyRequiresActionAccuracy,
            epicKeyRequiresActionCorrect,
            epicKeyRequiresActionTotal);

        Assert.True(emailAmountTotal > 0, "No fixtures were found.");
    }
}
