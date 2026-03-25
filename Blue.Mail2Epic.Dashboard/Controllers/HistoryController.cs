using Blue.Mail2Epic.Dashboard.Extensions;
using Blue.Mail2Epic.Dashboard.Services;
using Blue.Mail2Epic.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blue.Mail2Epic.Dashboard.Controllers;

[Authorize]
[ApiController]
[Route("api/history")]
public class HistoryController(AppDbContext dbContext, HistoryRowMapper historyRowMapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int maxResults = 25,
        CancellationToken ct = default)
    {
        if (maxResults > 100) maxResults = 100;
        var userId = User.GetUserId();

        var emailMappings = await dbContext.EmailMappings
            .AsNoTracking()
            .Where(x => x.Recipients.Any(r => r.UserAccountId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * maxResults)
            .Take(maxResults)
            .ToListAsync(ct);

        var issueEpicData = await historyRowMapper.GetIssuesEpicData(emailMappings, ct);
        var historyRows = emailMappings.Select(x =>
            historyRowMapper.Map(x,
                x.JiraIssueKey is not null &&
                issueEpicData.TryGetValue(x.JiraIssueKey, out var epic)
                    ? epic
                    : null
            ));

        return Ok(historyRows);
    }
}