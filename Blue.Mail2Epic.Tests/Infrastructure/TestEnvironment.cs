using Blue.Mail2Epic.Models.Configuration;
using Microsoft.Extensions.Configuration;

namespace Blue.Mail2Epic.Tests.Infrastructure;

public static class TestEnvironment
{
    public static bool TryGetAzureAiOptions(out AzureAiOptions options)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var bound = config.GetSection(AzureAiOptions.SectionName).Get<AzureAiOptions>();
        if (bound != null && HasAllAzureAiValues(bound))
        {
            options = bound;
            return true;
        }

        var endpoint = Environment.GetEnvironmentVariable("AZUREAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZUREAI_APIKEY");
        var summarizationModel = Environment.GetEnvironmentVariable("AZUREAI_SUMMARIZATIONMODEL") ??
                                 Environment.GetEnvironmentVariable("AZUREAI_SUMMARIZATION_DEPLOYMENTNAME");
        var analysisModel = Environment.GetEnvironmentVariable("AZUREAI_ANALYSISMODEL") ??
                            Environment.GetEnvironmentVariable("AZUREAI_DEPLOYMENTNAME");
        var embedding = Environment.GetEnvironmentVariable("AZUREAI_EMBEDDINGMODEL");

        summarizationModel ??= analysisModel;

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(summarizationModel) ||
            string.IsNullOrWhiteSpace(analysisModel) ||
            string.IsNullOrWhiteSpace(embedding))
        {
            options = null!;
            return false;
        }

        options = new AzureAiOptions
        {
            Endpoint = endpoint,
            ApiKey = apiKey,
            SummarizationModel = summarizationModel,
            AnalysisModel = analysisModel,
            EmbeddingModel = embedding
        };

        return true;
    }

    public static bool TryGetPromptOptions(out PromptOptions options)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var bound = config.GetSection(PromptOptions.SectionName).Get<PromptOptions>();
        if (bound != null && HasAllPromptValues(bound))
        {
            options = bound;
            return true;
        }

        var epicFieldExtractionPrompt = config[$"{PromptOptions.SectionName}:EpicFieldExtractionPrompt"] ??
                                        Environment.GetEnvironmentVariable("PROMPTOPTIONS_EPICFIELDEXTRACTIONPROMPT");
        var emailSummarizationPrompt = config[$"{PromptOptions.SectionName}:EmailSummarizationPrompt"] ??
                                       Environment.GetEnvironmentVariable("PROMPTOPTIONS_EMAILSUMMARIZATIONPROMPT");
        var emailTriagePrompt = config[$"{PromptOptions.SectionName}:EmailTriagePrompt"] ??
                                Environment.GetEnvironmentVariable("PROMPTOPTIONS_EMAILTRIAGEPROMPT");
        var issueUpdatePrompt = config[$"{PromptOptions.SectionName}:IssueUpdatePrompt"] ??
                                Environment.GetEnvironmentVariable("PROMPTOPTIONS_ISSUEUPDATEPROMPT");

        if (string.IsNullOrWhiteSpace(epicFieldExtractionPrompt) ||
            string.IsNullOrWhiteSpace(emailSummarizationPrompt) ||
            string.IsNullOrWhiteSpace(emailTriagePrompt) ||
            string.IsNullOrWhiteSpace(issueUpdatePrompt))
        {
            options = null!;
            return false;
        }

        options = new PromptOptions
        {
            EpicFieldExtractionPrompt = epicFieldExtractionPrompt,
            EmailSummarizationPrompt = emailSummarizationPrompt,
            EmailTriagePrompt = emailTriagePrompt,
            IssueUpdatePrompt = issueUpdatePrompt
        };

        return true;
    }

    private static bool HasAllAzureAiValues(AzureAiOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.Endpoint) &&
               !string.IsNullOrWhiteSpace(options.ApiKey) &&
               !string.IsNullOrWhiteSpace(options.SummarizationModel) &&
               !string.IsNullOrWhiteSpace(options.AnalysisModel) &&
               !string.IsNullOrWhiteSpace(options.EmbeddingModel);
    }

    private static bool HasAllPromptValues(PromptOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.EpicFieldExtractionPrompt) &&
               !string.IsNullOrWhiteSpace(options.EmailSummarizationPrompt) &&
               !string.IsNullOrWhiteSpace(options.EmailTriagePrompt) &&
               !string.IsNullOrWhiteSpace(options.IssueUpdatePrompt);
    }
}
