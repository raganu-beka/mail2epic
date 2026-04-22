using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Models.Responses;

namespace Blue.Mail2Epic.Tests.Infrastructure;

public sealed class FakeJiraService(IEnumerable<JiraIssueResponse>? issues = null) : IJiraService
{
    private readonly Dictionary<string, JiraIssueResponse> _issues = (issues ?? [])
        .Where(issue => !string.IsNullOrWhiteSpace(issue.Key))
        .ToDictionary(issue => issue.Key!, StringComparer.OrdinalIgnoreCase);

    public List<(string Body, string IssueKey)> PostedComments { get; } = [];

    public Task<List<JiraIssueResponse>> GetEpics(DateTimeOffset? latestUpdateTime, CancellationToken ct)
    {
        return Task.FromResult(new List<JiraIssueResponse>());
    }

    public Task<List<JiraIssueResponse>> GetIssuesByKeys(List<string> keys, CancellationToken ct)
    {
        var issues = keys
            .Where(key => _issues.ContainsKey(key))
            .Select(key => _issues[key])
            .ToList();

        return Task.FromResult(issues);
    }

    public Task<JiraIssueResponse?> GetIssueByKey(string key, CancellationToken ct)
    {
        _issues.TryGetValue(key, out var issue);
        return Task.FromResult(issue);
    }

    public Task<List<JiraIssueResponse>> GetIssuesByEpic(string epicKey, int count, CancellationToken ct)
    {
        return Task.FromResult(new List<JiraIssueResponse>());
    }

    public Task<string?> PostIssue(JiraIssuePostRequest issue, bool includeDescription, CancellationToken ct)
    {
        return Task.FromResult<string?>($"{issue.EpicKey}-SIM");
    }

    public Task PostIssueComment(string body, string issueKey, CancellationToken ct)
    {
        PostedComments.Add((body, issueKey));
        return Task.CompletedTask;
    }
}
