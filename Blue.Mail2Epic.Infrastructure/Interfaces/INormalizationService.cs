using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Models.Dtos.Email;

namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface INormalizationService
{
    Task<Epic?> NormalizeEpic(JiraIssueResponse jiraIssue, CancellationToken ct);
    Task<AzureAiEmailSummarizationResponse?> NormalizeEmail(EmailDataDto email, CancellationToken ct);
}
