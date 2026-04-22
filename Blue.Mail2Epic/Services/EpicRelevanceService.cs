using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Dtos.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector.EntityFrameworkCore;

namespace Blue.Mail2Epic.Services;

public class EpicRelevanceService(
    ILogger<EpicRelevanceService> logger,
    IAzureAiService aiService,
    AppDbContext dbContext) : IEpicRelevanceService
{
    public async Task<List<Epic>> FindRelevantEpics(AzureAiEmailSummarizationResponse emailSummary,
        CancellationToken ct)
    {
        var nearestEpics = await FindNearestEpicsWithRecencyBias(emailSummary, ct);
        var senderEpics = await FindEpicsBySender(emailSummary.Participants, ct);

        return nearestEpics
            .Concat(senderEpics)
            .DistinctBy(e => e.Id)
            .ToList();
    }

    private async Task<List<Epic>> FindNearestEpicsWithRecencyBias(AzureAiEmailSummarizationResponse emailSummary,
        CancellationToken ct,
        int topK = 10, double recencyWeight = 0.3)
    {
        var keyEntities = string.Join("\n", emailSummary.KeyEntities ?? []);
        var emailEmbedding =
            await aiService.GetEmbedding($"{emailSummary.Subject}\n{emailSummary.Summary}\n{keyEntities}", ct);
        if (emailEmbedding == null) return [];

        var now = DateTimeOffset.UtcNow;
        var maxAgeDays = 365.0;

        var candidates = await dbContext.Epics
            .Where(x => x.Embedding != null && x.UpdatedAt != null)
            .OrderBy(x => x.Embedding!.CosineDistance(emailEmbedding))
            .Take(topK * 3)
            .Select(e => new
            {
                Epic = e,
                Distance = e.Embedding!.CosineDistance(emailEmbedding)
            })
            .ToListAsync(ct);

        return candidates
            .Select(x => new
            {
                x.Epic,
                SimilarityScore = 1 - x.Distance,
                RecencyScore = 1 - Math.Min((now - x.Epic.UpdatedAt!.Value).TotalDays / maxAgeDays, 1)
            })
            .Select(x => new
            {
                x.Epic,
                CombinedScore = (1 - recencyWeight) * x.SimilarityScore + recencyWeight * x.RecencyScore
            })
            .OrderByDescending(x => x.CombinedScore)
            .Take(topK)
            .Select(x => x.Epic)
            .ToList();
    }

    private async Task<List<Epic>> FindEpicsBySender(List<EmailParticipant> emailParticipants, CancellationToken ct)
    {
        if (emailParticipants.Count == 0) return [];

        var senderEmails = emailParticipants
            .Select(p => p.Email)
            .Where(e => !string.IsNullOrEmpty(e) && e.Contains('@'))
            .Select(e => e!.Split('@')[1].ToLower())
            .Distinct()
            .ToList();

        if (senderEmails.Count == 0) return [];

        return await dbContext.Epics
            .Where(x => x.ContactPersonEmail != null &&
                        senderEmails.Any(domain => EF.Functions.Like(
                            x.ContactPersonEmail.ToLower(), "%@" + domain)))
            .ToListAsync(ct);
    }
}
