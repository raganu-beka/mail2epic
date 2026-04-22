using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Models.Configuration;
using Blue.Mail2Epic.Services;
using Blue.Mail2Epic.Tests.TestData;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Blue.Mail2Epic.Tests.Infrastructure;

public sealed class LiveAzureMail2EpicTestContext : IAsyncDisposable
{
    private LiveAzureMail2EpicTestContext(
        AppDbContext dbContext,
        AzureAiService azureAiService,
        NormalizationService normalizationService,
        EpicRelevanceService epicRelevanceService,
        FakeJiraService jiraService,
        EmailTriageService emailTriageService)
    {
        DbContext = dbContext;
        AzureAiService = azureAiService;
        NormalizationService = normalizationService;
        EpicRelevanceService = epicRelevanceService;
        JiraService = jiraService;
        EmailTriageService = emailTriageService;
    }

    public AppDbContext DbContext { get; }
    public AzureAiService AzureAiService { get; }
    public NormalizationService NormalizationService { get; }
    public EpicRelevanceService EpicRelevanceService { get; }
    public FakeJiraService JiraService { get; }
    public EmailTriageService EmailTriageService { get; }

    public static async Task<LiveAzureMail2EpicTestContext> CreateAsync(
        PostgresContainerFixture postgres,
        AzureAiOptions azureAiOptions,
        PromptOptions promptOptions,
        IEnumerable<JiraIssueResponse>? issues = null,
        CancellationToken ct = default)
    {
        var dbContext = await TestDbContextFactory.CreateMigratedDbContextAsync(postgres.ConnectionString, ct);

        var azureAiService = new AzureAiService(
            Options.Create(azureAiOptions),
            Options.Create(promptOptions),
            NullLogger<AzureAiService>.Instance);
        var normalizationService = new NormalizationService(azureAiService, NullLogger<NormalizationService>.Instance);

        await SyntheticEpicCatalog.SeedAsync(dbContext, normalizationService, ct);

        var epicRelevanceService =
            new EpicRelevanceService(NullLogger<EpicRelevanceService>.Instance, azureAiService, dbContext);
        var jiraService = new FakeJiraService(issues);
        var emailTriageService = new EmailTriageService(
            dbContext,
            azureAiService,
            jiraService,
            normalizationService,
            epicRelevanceService);

        return new LiveAzureMail2EpicTestContext(
            dbContext,
            azureAiService,
            normalizationService,
            epicRelevanceService,
            jiraService,
            emailTriageService);
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        AzureAiService.Dispose();
    }
}
