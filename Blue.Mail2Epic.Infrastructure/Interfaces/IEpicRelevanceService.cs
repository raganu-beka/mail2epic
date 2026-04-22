using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Dtos.Email;

namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface IEpicRelevanceService
{
    Task<List<Epic>> FindRelevantEpics(AzureAiEmailSummarizationResponse emailSummary, CancellationToken ct);
}
