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
        var deployment = Environment.GetEnvironmentVariable("AZUREAI_DEPLOYMENTNAME");
        var embedding = Environment.GetEnvironmentVariable("AZUREAI_EMBEDDINGMODEL");

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment) ||
            string.IsNullOrWhiteSpace(embedding))
        {
            options = null!;
            return false;
        }

        options = new AzureAiOptions
        {
            Endpoint = endpoint,
            ApiKey = apiKey,
            DeploymentName = deployment,
            EmbeddingModel = embedding
        };

        return true;
    }

    private static bool HasAllAzureAiValues(AzureAiOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.Endpoint) &&
               !string.IsNullOrWhiteSpace(options.ApiKey) &&
               !string.IsNullOrWhiteSpace(options.DeploymentName) &&
               !string.IsNullOrWhiteSpace(options.EmbeddingModel);
    }
}
