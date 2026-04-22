using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Models.Dtos.AzureAi;
using Blue.Mail2Epic.Models.Dtos.Email;
using Vector = Pgvector.Vector;

namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface IAzureAiService
{
    Task<AzureAiEpicFieldExtractionResponse> ExtractJiraEpicFields(JiraIssueResponse issueResponse, CancellationToken ct);
    Task<Vector?> GetEmbedding(string text, CancellationToken ct);
    Task<AzureAiEmailAnalysisResponse> AnalyzeEmail(
        EmailDataDto email,
        AzureAiEmailSummarizationResponse summary,
        List<Epic> relevantEpics,
        CancellationToken ct);
    Task<AzureAiEmailSummarizationResponse> SummarizeEmail(EmailDataDto email, CancellationToken ct);
    Task<AzureAiIssueUpdateResponse> GetIssueUpdate(EmailDataDto email, JiraIssueResponse issue, CancellationToken ct);
}
