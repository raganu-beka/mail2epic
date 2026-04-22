using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Dtos.AzureAi;
using Blue.Mail2Epic.Models.Dtos.Email;

namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface IEmailTriageService
{
    Task<(RequiredAction RequiredAction, string? IssueKey)> DetermineRequiredAction(
        EmailDataDto email,
        CancellationToken cancellationToken);
    Task<AzureAiEmailAnalysisResponse> GetEmailAnalysis(EmailDataDto email, CancellationToken ct);
    Task<AzureAiIssueUpdateResponse> GetIssueUpdate(EmailDataDto email, string issueKey, CancellationToken ct);
}
