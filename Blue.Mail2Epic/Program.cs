using Blue.Mail2Epic.Infrastructure;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models.Configuration;
using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Services;
using Blue.Mail2Epic.Jobs;
using Blue.Mail2Epic.Models.Configuration;
using Blue.Mail2Epic.Services;
using Blue.Mail2Epic.Workers;
using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using DataProtectionOptions = Blue.Mail2Epic.Infrastructure.Models.Configuration.DataProtectionOptions;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration["ConnectionString"];
if (!string.IsNullOrWhiteSpace(connectionString))
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString, o => o.UseVector()));

builder.Services.AddHttpClient();

var dataProtectionOptions = builder.Configuration
    .GetSection(DataProtectionOptions.SectionName)
    .Get<DataProtectionOptions>()!;

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionOptions.KeyStoragePath))
    .SetApplicationName(Application.Name);
builder.Services.AddScoped<SecretProtector>();

builder.Services.Configure<AdditionalOptions>(builder.Configuration.GetSection(AdditionalOptions.SectionName));

builder.Services.Configure<GoogleOAuthOptions>(builder.Configuration.GetSection(GoogleOAuthOptions.SectionName));
builder.Services.AddScoped<GoogleTokenService>();

builder.Services.AddHttpClient(JiraService.HttpClientName);
builder.Services.Configure<JiraOptions>(builder.Configuration.GetSection(JiraOptions.SectionName));
builder.Services.AddTransient<JiraService>();

builder.Services.Configure<PromptOptions>(builder.Configuration.GetSection(PromptOptions.SectionName));
builder.Services.Configure<AzureAiOptions>(builder.Configuration.GetSection(AzureAiOptions.SectionName));
builder.Services.AddSingleton<AzureAiService>();

builder.Services.AddTransient<NormalizationService>();
builder.Services.AddScoped<EpicRelevanceService>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddTransient<EmailService>();

builder.Services.AddScoped<EmailTriageService>();

builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));

var syncEpicsCronSchedule = builder.Configuration["Quartz:SyncEpicsJobCronSchedule"];
var readEmailsCronSchedule = builder.Configuration["Quartz:ReadEmailsJobCronSchedule"];

if (!string.IsNullOrEmpty(syncEpicsCronSchedule))
{
    builder.Services.AddTransient<SyncEpicsJob>();
    builder.Services.AddQuartz(q =>
    {
        q.ScheduleJob<SyncEpicsJob>(trigger => trigger
            .WithIdentity("Sync Epics Job Trigger")
            .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(7)))
            .WithCronSchedule(syncEpicsCronSchedule)
            .WithDescription("Trigger for Sync Epics Job")
        );
    });
}

if (!string.IsNullOrEmpty(readEmailsCronSchedule))
{
    builder.Services.AddTransient<ReadEmailsJob>();
    builder.Services.AddQuartz(q =>
    {
        q.ScheduleJob<ReadEmailsJob>(trigger => trigger
            .WithIdentity("Read Emails Job Trigger")
            .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(7)))
            .WithCronSchedule(readEmailsCronSchedule)
            .WithDescription("Trigger for Read Emails Job")
        );
    });
}

builder.Services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

builder.Services.AddMassTransit(x =>
{
    var rabbitMqOptions = builder.Configuration
        .GetSection(RabbitMqOptions.SectionName)
        .Get<RabbitMqOptions>()!;

    x.AddConsumer<ProcessEpicWorker>();
    x.AddConsumer<ProcessEmailWorker>();
    x.AddConsumer<ProcessIssueWorker>();
    x.AddConsumer<ProcessIssueUpdateWorker>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.VirtualHost, h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });

        cfg.UseMessageRetry(r =>
        {
            r.Exponential(
                5,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(2));
            r.Handle<HttpRequestException>();
            r.Handle<InvalidOperationException>();
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

await host.RunAsync();