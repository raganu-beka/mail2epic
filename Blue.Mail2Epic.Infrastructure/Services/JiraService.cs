using System.Net.Http.Headers;
using System.Text;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models.Exceptions;
using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blue.Mail2Epic.Infrastructure.Services;

public class JiraService : IJiraService
{
    public static readonly string HttpClientName = "jira";

    private readonly HttpClient _httpClient;
    private readonly ILogger<JiraService> _logger;
    private readonly JiraOptions _options;

    public JiraService(IOptions<JiraOptions> options, ILogger<JiraService> logger, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;

        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
    }

    public async Task<List<JiraIssueResponse>>
        GetEpics(DateTimeOffset? latestUpdateTime,
            CancellationToken ct)
    {
        var query = $"project = {_options.ProjectKey} AND issuetype = Epic";
        if (latestUpdateTime.HasValue)
            query += $" AND updated >= '{latestUpdateTime.Value:yyyy-MM-dd HH:mm}'";
        query += " ORDER BY key ASC";

        _logger.LogInformation("Getting Epics for {ProjectKey}", _options.ProjectKey);

        var allIssues = new List<JiraIssueResponse>();
        var startAt = 0;
        const int maxResults = 1000;

        while (true)
        {
            var request = new
            {
                jql = query,
                startAt,
                maxResults,
                fields = new[] { "key", "summary", "description", "updated" }
            };
            var json = JsonConvert.SerializeObject(request);
            var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/rest/api/2/search", requestBody, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new InvalidResponseException("Invalid response from Jira API when fetching epics.");

            var searchResponse = JsonConvert.DeserializeObject<JiraSearchResponse>(responseBody);
            var issues = searchResponse?.Issues ?? [];

            allIssues.AddRange(issues);
            if (issues.Count < maxResults) break;
            startAt += maxResults;
        }

        return allIssues;
    }

    public async Task<List<JiraIssueResponse>> GetIssuesByKeys(List<string> keys, CancellationToken ct)
    {
        if (keys.Count == 0) return [];

        var keyString = string.Join(", ", keys);
        var query = $"key in ({keyString})";
        var request = new
        {
            jql = query,
            startAt = 0,
            maxResults = 1000,
            fields = new[] { "key", "summary", "description", "updated", _options.EpicLinkFieldId }
        };
        var json = JsonConvert.SerializeObject(request);
        var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Getting issues by keys {KeyString}", keyString);
        var response = await _httpClient.PostAsync($"{_options.BaseUrl}/rest/api/2/search", requestBody, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(responseBody))
            throw new InvalidResponseException("Invalid response from Jira API when fetching epics.");
        var serialisedResponse = JsonConvert.DeserializeObject<JiraSearchResponse>(responseBody);
        return serialisedResponse?.Issues ?? [];
    }

    public async Task<JiraIssueResponse?> GetIssueByKey(string key, CancellationToken ct)
    {
        _logger.LogInformation("Getting issue by key {Key}", key);
        var response = await _httpClient.GetAsync($"{_options.BaseUrl}/rest/api/2/issue/{key}", ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(responseBody))
            throw new InvalidResponseException($"Invalid response from Jira API when fetching epic with key {key}.");
        var serialisedResponse = JsonConvert.DeserializeObject<JiraIssueResponse>(responseBody);
        return serialisedResponse;
    }

    public async Task<List<JiraIssueResponse>> GetIssuesByEpic(string epicKey, int count, CancellationToken ct)
    {
        var query = $"\"Epic Link\" = {epicKey} OR parent = {epicKey} ORDER BY created DESC";
        var request = new
        {
            jql = query,
            startAt = 0,
            maxResults = count,
            fields = new[] { "key", "summary", "reporter", "created" }
        };
        var json = JsonConvert.SerializeObject(request);
        var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Getting last {Count} issues for Epic {EpicKey}", count, epicKey);
        var response = await _httpClient.PostAsync($"{_options.BaseUrl}/rest/api/2/search", requestBody, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(responseBody))
            throw new InvalidResponseException("Invalid response from Jira API when fetching epic issues.");
        var serialisedResponse = JsonConvert.DeserializeObject<JiraSearchResponse>(responseBody);
        return serialisedResponse?.Issues ?? [];
    }

    public async Task<string?> PostIssue(JiraIssuePostRequest issue, bool includeDescription, CancellationToken ct)
    {
        var fields = new Dictionary<string, object?>
        {
            { "project", new { key = _options.ProjectKey } },
            { "summary", issue.Summary },
            { "description", includeDescription ? issue.Description : string.Empty },
            { "issuetype", new { name = _options.IssueType } },
            { "assignee", new { name = issue.Assignee } },
            { "reporter", new { name = issue.Assignee } },
            { _options.EpicLinkFieldId, issue.EpicKey },
            { "labels", new[] { _options.IssueLabel } }
        };
        var request = new { fields };
        var json = JsonConvert.SerializeObject(request);
        var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Posting issue with summary: {Summary}", issue.Summary);
        var response = await _httpClient.PostAsync($"{_options.BaseUrl}/rest/api/2/issue", requestBody, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("Issue created: {ResponseBody}", responseBody);

        var createResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
        return createResponse?.key;
    }

    public async Task PostIssueComment(string body, string issueKey, CancellationToken ct)
    {
        var comment = new { body };
        var json = JsonConvert.SerializeObject(comment);
        var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Posting issue comment for {IssueKey}", issueKey);
        var response =
            await _httpClient.PostAsync($"{_options.BaseUrl}/rest/api/2/issue/{issueKey}/comment", requestBody, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("Comment created: {ResponseBody}", responseBody);
    }
}
