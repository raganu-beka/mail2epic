using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Models.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blue.Mail2Epic.Workers;

public class ProcessEpicWorker(
    ILogger<ProcessEpicWorker> logger,
    AppDbContext dbContext,
    INormalizationService normalizationService) : IConsumer<EpicFetchedEvent>
{
    public async Task Consume(ConsumeContext<EpicFetchedEvent> context)
    {
        var epicDto = context.Message.Epic;
        try
        {
            var epic = await normalizationService.NormalizeEpic(epicDto, context.CancellationToken);

            if (epic != null)
            {
                var existing = await dbContext.Epics.FirstOrDefaultAsync(x => x.Key == epicDto.Key);
                if (existing != null)
                {
                    existing.Summary = epic.Summary;
                    existing.Description = epic.Description;
                    existing.ContactPersonEmail = epic.ContactPersonEmail;
                    existing.Embedding = epic.Embedding;
                    existing.UpdatedAt = epic.UpdatedAt;
                    dbContext.Epics.Update(existing);
                }
                else
                {
                    dbContext.Epics.Add(epic);
                }

                await dbContext.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("Successfully processed epic {Key}", epicDto.Key);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process epic {Key}", epicDto.Key);
            throw;
        }
    }
}
