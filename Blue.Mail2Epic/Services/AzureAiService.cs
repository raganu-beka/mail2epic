using Azure;
using Azure.AI.OpenAI;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Models.Configuration;
using Blue.Mail2Epic.Models.Dtos.AzureAi;
using Blue.Mail2Epic.Models.Dtos.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI.Chat;
using Vector = Pgvector.Vector;

namespace Blue.Mail2Epic.Services;

public class AzureAiService(
    IOptions<AzureAiOptions> options,
    IOptions<PromptOptions> promptOptions,
    ILogger<AzureAiService> logger) : IAzureAiService, IDisposable
{
    private const string EmailTriageUserTemplate = """
                                                   EMAIL SUMMARY:
                                                   {0}

                                                   RAW EMAIL:
                                                   {1}

                                                   CANDIDATE EPICS:
                                                   {2}
                                                   """;

    private const string IssueUpdateUserTemplate = """
                                                   EMAIL BODY:
                                                   {0}

                                                   JIRA ISSUE DESCRIPTION:
                                                   {1}

                                                   JIRA ISSUE COMMENTS:
                                                   {2}
                                                   """;

    private readonly AzureOpenAIClient _client = new(
        new Uri(options.Value.Endpoint),
        new AzureKeyCredential(options.Value.ApiKey),
        new AzureOpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromSeconds(options.Value.NetworkTimeoutSeconds)
        }
    );

    private readonly AzureAiOptions _options = options.Value;
    private readonly PromptOptions _prompts = promptOptions.Value;

    private readonly SemaphoreSlim _semaphore = new(
        options.Value.MaxConcurrentRequests,
        options.Value.MaxConcurrentRequests
    );

    public async Task<AzureAiEpicFieldExtractionResponse> ExtractJiraEpicFields(JiraIssueResponse issueResponse,
        CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            if (string.IsNullOrWhiteSpace(issueResponse.Fields?.Description))
                return new AzureAiEpicFieldExtractionResponse();

            var chatClient = _client.GetChatClient(_options.SummarizationModel);

            var completionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_prompts.EpicFieldExtractionPrompt),
                new UserChatMessage(issueResponse.Fields?.Description)
            };

            var response = await chatClient.CompleteChatAsync(messages, completionOptions, ct);
            var content = response.Value.Content[0].Text;

            if (!string.IsNullOrWhiteSpace(content))
                return JsonConvert.DeserializeObject<AzureAiEpicFieldExtractionResponse>(content) ??
                       new AzureAiEpicFieldExtractionResponse();

            logger.LogWarning("Azure AI returned empty content for epic field extraction.");
            return new AzureAiEpicFieldExtractionResponse();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Vector?> GetEmbedding(string text, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var embeddingClient = _client.GetEmbeddingClient(_options.EmbeddingModel);

            logger.LogInformation("Creating embedding for text of length {Length}", text.Length);
            var response = await embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: ct);
            var embedding = response?.Value;

            if (embedding != null) return new Vector(embedding.ToFloats().ToArray());

            logger.LogWarning("Azure AI returned empty content for text embedding.");
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<AzureAiEmailAnalysisResponse> AnalyzeEmail(
        EmailDataDto email,
        AzureAiEmailSummarizationResponse summary,
        List<Epic> relevantEpics,
        CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var chatClient = _client.GetChatClient(_options.AnalysisModel);

            var completionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_prompts.EmailTriagePrompt),
                new UserChatMessage(PrepareEmailTriagePrompt(email, summary, relevantEpics))
            };

            var response = await chatClient.CompleteChatAsync(messages, completionOptions, ct);
            var content = response.Value.Content[0].Text;

            if (!string.IsNullOrWhiteSpace(content))
                return JsonConvert.DeserializeObject<AzureAiEmailAnalysisResponse>(content) ??
                       new AzureAiEmailAnalysisResponse();

            logger.LogWarning("Azure AI returned empty content for email triage.");
            return new AzureAiEmailAnalysisResponse();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<AzureAiEmailSummarizationResponse> SummarizeEmail(EmailDataDto email, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var chatClient = _client.GetChatClient(_options.SummarizationModel);

            var completionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_prompts.EmailSummarizationPrompt),
                new UserChatMessage(email.Body)
            };

            var response = await chatClient.CompleteChatAsync(messages, completionOptions, ct);
            var content = response.Value.Content[0].Text;

            if (!string.IsNullOrWhiteSpace(content))
                return JsonConvert.DeserializeObject<AzureAiEmailSummarizationResponse>(content) ??
                       new AzureAiEmailSummarizationResponse();

            logger.LogWarning("Azure AI returned empty content for email summarization.");
            return new AzureAiEmailSummarizationResponse();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<AzureAiIssueUpdateResponse> GetIssueUpdate(EmailDataDto email, JiraIssueResponse issue,
        CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var chatClient = _client.GetChatClient(_options.AnalysisModel);

            var completionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_prompts.IssueUpdatePrompt),
                new UserChatMessage(PrepareIssueUpdatePrompt(email, issue))
            };

            var response = await chatClient.CompleteChatAsync(messages, completionOptions, ct);
            var content = response.Value.Content[0].Text;

            if (!string.IsNullOrWhiteSpace(content))
                return JsonConvert.DeserializeObject<AzureAiIssueUpdateResponse>(content) ??
                       new AzureAiIssueUpdateResponse();

            logger.LogWarning("Azure AI returned empty content for issue update.");
            return new AzureAiIssueUpdateResponse();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static string PrepareEmailTriagePrompt(
        EmailDataDto email,
        AzureAiEmailSummarizationResponse summary,
        List<Epic> relevantEpics)
    {
        var participantsText = summary.Participants.Count > 0
            ? string.Join("\n", summary.Participants.Select(p => $"- {p.Name} ({p.Email})"))
            : "No participants";

        var keyEntitiesText = summary.KeyEntities != null && summary.KeyEntities.Count > 0
            ? string.Join(", ", summary.KeyEntities)
            : "No key entities";

        var summaryText =
            $"{summary.Subject}\n{summary.Summary}\n\nParticipants:\n{participantsText}\n\nKey entities:\n{keyEntitiesText}";

        var epicsText = relevantEpics.Count > 0
            ? string.Join("\n\n", relevantEpics.Select(e =>
                $"- {e.Summary ?? "N/A"} ({e.Key})\n  Description: {e.Description ?? "N/A"}\n  Contact: {e.ContactPersonEmail ?? "N/A"}"))
            : "No relevant epics found";

        return string.Format(EmailTriageUserTemplate, summaryText, email.Body, epicsText);
    }

    private static string PrepareIssueUpdatePrompt(EmailDataDto email, JiraIssueResponse issue)
    {
        var description = string.IsNullOrWhiteSpace(issue.Fields?.Description)
            ? "No description"
            : issue.Fields.Description;

        var comments = issue.Fields?.Comment?.Comments != null && issue.Fields.Comment.Comments.Count > 0
            ? string.Join("\n\n", issue.Fields.Comment.Comments.Select(c => c.Body))
            : "No comments";

        return string.Format(IssueUpdateUserTemplate, email.Body, description, comments);
    }

    public void Dispose() => _semaphore.Dispose();
}
