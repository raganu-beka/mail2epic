using Testcontainers.PostgreSql;

namespace Blue.Mail2Epic.Tests.Infrastructure;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("pgvector/pgvector:pg16")
        .WithDatabase("mail2epic_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public ValueTask InitializeAsync()
    {
        return new ValueTask(_container.StartAsync());
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
}
