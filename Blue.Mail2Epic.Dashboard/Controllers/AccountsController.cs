using Blue.Mail2Epic.Dashboard.Extensions;
using Blue.Mail2Epic.Dashboard.Models;
using Blue.Mail2Epic.Dashboard.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blue.Mail2Epic.Dashboard.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts")]
public class AccountsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var accounts = await dbContext.UserAccounts
            .AsNoTracking()
            .Include(a => a.GoogleMailboxAccount)
            .OrderBy(a => a.Email)
            .ToListAsync(ct);

        var lastEmailProcessingDates = await dbContext.EmailMappingRecipients
            .AsNoTracking()
            .GroupBy(r => r.UserAccountId)
            .Select(g => new { UserAccountId = g.Key, LastAt = g.Max(r => (DateTimeOffset?)r.EmailMapping.CreatedAt) })
            .ToDictionaryAsync(x => x.UserAccountId, x => x.LastAt, ct);

        var result = accounts.Select(a =>
            AccountData.From(a, lastEmailProcessingDates.GetValueOrDefault(a.Id)));

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateAllowedAccountRequest request, CancellationToken ct)
    {
        var exists = await dbContext.UserAccounts
            .AsNoTracking()
            .AnyAsync(a => a.Email == request.Email, ct);
        if (exists) return Conflict(new { message = $"An account with email '{request.Email}' already exists." });

        var account = new UserAccount
        {
            Email = request.Email,
            IsActive = request.IsActive,
            IsAdmin =  request.IsAdmin,
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        dbContext.UserAccounts.Add(account);
        await dbContext.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), null, AccountData.From(account));
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserAccountRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var account = await dbContext.UserAccounts
            .Include(a => a.GoogleMailboxAccount)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (account is null) return NotFound();

        if (account.Id == userId) return StatusCode(403, "You cannot update your own account.");

        account.IsActive = request.IsActive;
        account.IsAdmin = request.IsAdmin;
        account.ModifiedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var lastEmailProcessingAt = await dbContext.EmailMappings
            .AsNoTracking()
            .Where(x => x.Recipients.Any(r => r.UserAccountId == userId))
            .MaxAsync(em => (DateTimeOffset?)em.CreatedAt, ct);

        return Ok(AccountData.From(account, lastEmailProcessingAt));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var account = await dbContext.UserAccounts.FindAsync([id], ct);
        if (account is null) return NotFound();
        if (account.Id == userId) return StatusCode(403, "You cannot delete your own account.");
        if (account.IsAdmin) return BadRequest("Admin accounts cannot be deleted.");

        dbContext.UserAccounts.Remove(account);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserData(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var user = await dbContext.UserAccounts
            .AsNoTracking()
            .Include(x => x.GoogleMailboxAccount)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user?.GoogleMailboxAccount is null) return Unauthorized();

        var lastEmailProcessingAt = await dbContext.EmailMappings
            .AsNoTracking()
            .Where(x => x.Recipients.Any(r => r.UserAccountId == userId))
            .MaxAsync(em => (DateTimeOffset?)em.CreatedAt, ct);

        return Ok(AccountData.From(user, lastEmailProcessingAt));
    }
}