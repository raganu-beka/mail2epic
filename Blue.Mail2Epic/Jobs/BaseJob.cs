using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Blue.Mail2Epic.Jobs;

public abstract class BaseJob : IJob
{
    public abstract Task Execute(IJobExecutionContext context);

    protected async Task<DateTimeOffset?> GetLastExecutionTimeAsync(DbContext dbContext)
    {
        var jobName = GetType().Name;
        return await dbContext.Set<JobExecutionInfo>()
            .Where(x => x.JobName == jobName)
            .Select(x => (DateTimeOffset?)x.LastExecutionTime)
            .FirstOrDefaultAsync();
    }
    
    protected async Task UpdateJobLastRunTime(DateTimeOffset jobStartTime, AppDbContext dbContext)
    {
        var jobName = GetType().Name;
        var jobInfo = await dbContext.JobExecutionInfos
            .FirstOrDefaultAsync(x => x.JobName == jobName);
        
        if (jobInfo == null)
        {
            dbContext.JobExecutionInfos.Add(new JobExecutionInfo
            {
                JobName = jobName,
                LastExecutionTime = jobStartTime.UtcDateTime
            });
        }
        else
        {
            jobInfo.LastExecutionTime = jobStartTime.UtcDateTime;
            dbContext.JobExecutionInfos.Update(jobInfo);
        }
        
        await dbContext.SaveChangesAsync();
    }
}