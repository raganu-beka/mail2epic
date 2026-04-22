using Blue.Mail2Epic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Blue.Mail2Epic.Tests.Infrastructure;

public static class TestDbContextFactory
{
    public static async Task<AppDbContext> CreateMigratedDbContextAsync(
        string connectionString,
        CancellationToken ct = default)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, builder => builder.UseVector())
            .Options;

        var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureDeletedAsync(ct);
        await dbContext.Database.MigrateAsync(ct);
        return dbContext;
    }
}
