using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Models.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Blue.Mail2Epic.Jobs;

[DisallowConcurrentExecution]
public class SyncEpicsJob(
    ILogger<SyncEpicsJob> logger,
    AppDbContext dbContext,
    IJiraService jiraService,
    IBus bus) : BaseJob
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var jobStartTime = DateTimeOffset.Now;
        logger.LogInformation("SyncEpics job started at: {Time}", jobStartTime);

        var lastExecutionTime = await GetLastExecutionTimeAsync(dbContext);

        var epics = await jiraService.GetEpics(lastExecutionTime, context.CancellationToken);
        logger.LogInformation("Fetched {Count} epics from Jira", epics.Count);

        foreach (var epic in epics) await bus.Publish(new EpicFetchedEvent { Epic = epic }, context.CancellationToken);

        if (epics.Count > 0)
            await UpdateJobLastRunTime(jobStartTime, dbContext);
    }
}
