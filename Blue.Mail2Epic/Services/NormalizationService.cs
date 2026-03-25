using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Models.Dtos.Email;
using Microsoft.Extensions.Logging;

namespace Blue.Mail2Epic.Services;

public class NormalizationService(AzureAiService azureAiService, ILogger<NormalizationService> logger)
{
    public async Task<Epic?> NormalizeEpic(JiraIssueResponse jiraIssue, CancellationToken ct)
    {
        if (jiraIssue.Key == null) return null;

        logger.LogInformation("Extracting JSON fields for epic {Key}", jiraIssue.Key);
        var epicFields = await azureAiService.ExtractJiraEpicFields(jiraIssue, ct);
        
        logger.LogInformation("Creating embedding for epic {Key}", jiraIssue.Key);
        var epicEmbedding = await azureAiService.GetEmbedding(
            $"{jiraIssue.Fields?.Summary?.Trim() ?? string.Empty}\n{epicFields.Description?.Trim() ?? string.Empty}",
            ct);
        
        return new Epic()
        {
            Key = jiraIssue.Key,
            Summary = jiraIssue.Fields?.Summary?.Trim(),
            Description = epicFields.Description?.Trim(),
            ContactPersonEmail = epicFields.ContactPersonEmail?.Trim(),
            UpdatedAt = jiraIssue.Fields?.UpdatedAt.GetValueOrDefault().ToUniversalTime(),
            Embedding = epicEmbedding
        };
    }
    
    public async Task<AzureAiEmailSummarizationResponse?> NormalizeEmail(EmailDataDto email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email.Body)) return null;
        return await azureAiService.SummarizeEmail(email, ct);
    }
}

