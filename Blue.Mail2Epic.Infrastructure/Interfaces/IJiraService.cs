using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Models.Responses;

namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface IJiraService
{
    Task<List<JiraIssueResponse>> GetEpics(DateTimeOffset? latestUpdateTime, CancellationToken ct);
    Task<List<JiraIssueResponse>> GetIssuesByKeys(List<string> keys, CancellationToken ct);
    Task<JiraIssueResponse?> GetIssueByKey(string key, CancellationToken ct);
    Task<List<JiraIssueResponse>> GetIssuesByEpic(string epicKey, int count, CancellationToken ct);
    Task<string?> PostIssue(JiraIssuePostRequest issue, bool includeDescription, CancellationToken ct);
    Task PostIssueComment(string body, string issueKey, CancellationToken ct);
}
