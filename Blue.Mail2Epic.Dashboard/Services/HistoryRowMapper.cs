using System.Text.RegularExpressions;
using Blue.Mail2Epic.Dashboard.Models;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Blue.Mail2Epic.Dashboard.Services;

public class HistoryRowMapper(
    IOptions<FrontendOptions> frontendOptions,
    IOptions<JiraOptions> jiraOptions,
    IMemoryCache cache,
    IJiraService jiraService)
{
    private const string EpicCachePrefix = "issue-epic:";

    private readonly FrontendOptions _frontendOptions = frontendOptions.Value;
    private readonly JiraOptions _jiraOptions = jiraOptions.Value;
    private readonly Regex _senderRegex = new(frontendOptions.Value.MessageSenderRegex, RegexOptions.Compiled);

    public HistoryRow Map(EmailMapping emailMapping, JiraIssueResponse? epic)
    {
        var row = HistoryRow.From(emailMapping);

        row.GmailEmailLink = string.Format(_frontendOptions.GmailEmailLinkTemplate, emailMapping.MessageId);

        var senderMatch = _senderRegex.Match(emailMapping.MessageSenderAddress);
        row.MessageSender = senderMatch.Success ? senderMatch.Value : emailMapping.MessageSenderAddress;

        if (emailMapping.JiraIssueKey is null) return row;

        row.JiraIssueLink = string.Format(_frontendOptions.JiraIssueLinkTemplate, emailMapping.JiraIssueKey);

        if (epic is not null)
            row.IssueEpic = IssueEpic.From(epic, string.Format(_frontendOptions.JiraIssueLinkTemplate, epic.Key));

        return row;
    }

    public async Task<Dictionary<string, JiraIssueResponse>> GetIssuesEpicData(
        List<EmailMapping> mappings,
        CancellationToken ct)
    {
        var issueKeys = mappings
            .Select(x => x.JiraIssueKey)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToList();

        var result = new Dictionary<string, JiraIssueResponse>();
        var uncachedIssueKeys = new List<string>();

        foreach (var key in issueKeys)
            if (cache.TryGetValue($"{EpicCachePrefix}{key}", out JiraIssueResponse? cached) && cached is not null)
                result[key] = cached;
            else
                uncachedIssueKeys.Add(key);

        if (uncachedIssueKeys.Count == 0) return result;

        var issues = await jiraService.GetIssuesByKeys(uncachedIssueKeys, ct);

        var epicKeys = issues
            .Select(x => x.Fields?.AdditionalFields?[_jiraOptions.EpicLinkFieldId]?.Value<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToList();

        var epics = epicKeys.Count > 0
            ? (await jiraService.GetIssuesByKeys(epicKeys,
                ct))
            .Where(x => x.Key is not null)
            .ToDictionary(x => x.Key!, x => x)
            : new Dictionary<string, JiraIssueResponse>();

        foreach (var issue in issues)
        {
            if (issue.Key is null) continue;
            var epicKey = issue.Fields?.AdditionalFields?[_jiraOptions.EpicLinkFieldId]?.Value<string>();
            if (epicKey is not null && epics.TryGetValue(epicKey, out var epic))
            {
                result[issue.Key] = epic;
                cache.Set($"{EpicCachePrefix}{issue.Key}", epic, TimeSpan.FromMinutes(5));
            }
        }

        return result;
    }
}
