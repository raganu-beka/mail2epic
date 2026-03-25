using Blue.Mail2Epic.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Blue.Mail2Epic.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Epic> Epics { get; set; }
    public DbSet<EmailMapping> EmailMappings { get; set; }
    public DbSet<JobExecutionInfo> JobExecutionInfos { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<GoogleMailboxAccount> GoogleMailboxAccounts { get; set; }
    public DbSet<EmailMappingRecipient> EmailMappingRecipients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Epic>()
            .HasIndex(e => e.Key)
            .IsUnique();

        modelBuilder.Entity<Epic>()
            .Property(e => e.Embedding)
            .HasColumnType("vector(1536)");

        modelBuilder.Entity<JobExecutionInfo>()
            .HasIndex(e => e.JobName)
            .IsUnique();

        modelBuilder.Entity<EmailMapping>()
            .HasIndex(e => e.MessageId)
            .IsUnique();

        modelBuilder.Entity<EmailMapping>()
            .HasIndex(e => e.ThreadRootMessageId);

        modelBuilder.Entity<EmailMapping>()
            .Property(e => e.ActionTaken)
            .HasConversion<string>();

        modelBuilder.Entity<UserAccount>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<GoogleMailboxAccount>()
            .HasIndex(e => e.GoogleSubject).IsUnique();

        modelBuilder.Entity<UserAccount>()
            .HasOne(u => u.GoogleMailboxAccount)
            .WithOne(g => g.UserAccount)
            .HasForeignKey<GoogleMailboxAccount>(g => g.UserAccountId);

        modelBuilder.Entity<EmailMappingRecipient>()
            .HasKey(r => new { r.EmailMappingId, r.UserAccountId });

        modelBuilder.Entity<EmailMappingRecipient>()
            .HasOne(r => r.EmailMapping)
            .WithMany(e => e.Recipients)
            .HasForeignKey(r => r.EmailMappingId);

        modelBuilder.Entity<EmailMappingRecipient>()
            .HasOne(r => r.UserAccount)
            .WithMany(u => u.EmailMappingRecipients)
            .HasForeignKey(r => r.UserAccountId);
    }

    public async Task<bool> EmailMappingExistsAsync(string messageId, CancellationToken ct = default)
    {
        return await EmailMappings
            .AsNoTracking()
            .AnyAsync(x => x.MessageId == messageId, ct);
    }
}