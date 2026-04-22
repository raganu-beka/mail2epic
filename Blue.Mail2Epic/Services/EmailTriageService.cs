using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Dtos.AzureAi;
using Blue.Mail2Epic.Models.Dtos.Email;
using Microsoft.EntityFrameworkCore;

namespace Blue.Mail2Epic.Services;

public class EmailTriageService(
    AppDbContext dbContext,
    IAzureAiService aiService,
    IJiraService jiraService,
    INormalizationService normalizationService,
    IEpicRelevanceService epicRelevanceService) : IEmailTriageService
{
    public async Task<(RequiredAction RequiredAction, string? IssueKey)> DetermineRequiredAction(EmailDataDto email,
        CancellationToken cancellationToken)
    {
        var existingIssueKey = await FindExistingIssue(email, cancellationToken);
        if (string.IsNullOrEmpty(existingIssueKey)) return (RequiredAction.CreateNewIssue, null);
        var action = email.IsThreadRoot ? RequiredAction.NoAction : RequiredAction.AppendExistingIssue;
        return (action, existingIssueKey);
    }

    public async Task<AzureAiEmailAnalysisResponse> GetEmailAnalysis(EmailDataDto email, CancellationToken ct)
    {
        var emailSummary = await normalizationService.NormalizeEmail(email, ct);
        if (emailSummary == null) throw new InvalidOperationException("Email summary was not generated.");

        var nearestEpics = await epicRelevanceService.FindRelevantEpics(emailSummary, ct);
        return await aiService.AnalyzeEmail(email, emailSummary, nearestEpics, ct);
    }

    public async Task<AzureAiIssueUpdateResponse> GetIssueUpdate(EmailDataDto email, string issueKey,
        CancellationToken ct)
    {
        var issue = await jiraService.GetIssueByKey(issueKey, ct);
        if (issue is null) throw new InvalidOperationException("Issue not found for key: " + issueKey);
        return await aiService.GetIssueUpdate(email, issue, ct);
    }

    private async Task<string?> FindExistingIssue(EmailDataDto email, CancellationToken ct)
    {
        var threadRootMatch = await dbContext.EmailMappings
            .Where(x => x.ThreadRootMessageId.Equals(email.ThreadRootMessageId) && x.JiraIssueKey != null)
            .Select(x => x.JiraIssueKey)
            .FirstOrDefaultAsync(ct);

        return string.IsNullOrEmpty(threadRootMatch) ? null : threadRootMatch;
    }
}
