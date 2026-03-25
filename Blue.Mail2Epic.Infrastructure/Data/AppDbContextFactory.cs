using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Blue.Mail2Epic.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configurationPath)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("DefaultConnection not found in appsettings.json");

        optionsBuilder.UseNpgsql(connectionString, options => options.UseVector());

        return new AppDbContext(optionsBuilder.Options);
    }
}