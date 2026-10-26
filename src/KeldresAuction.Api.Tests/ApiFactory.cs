using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace KeldresAuction.Api.Tests;

// Runs the API against a real PostgreSQL started in a container (requires Docker).
// Shared by all API tests through the "Api" collection, so the container starts once.
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:keldresauction"] = _postgres.GetConnectionString() }));

        // Expose the controllers declared in this test assembly (e.g. TestResultsController).
        builder.ConfigureServices(services => services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly));
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    // Explicit implementation: WebApplicationFactory already exposes DisposeAsync() returning ValueTask.
    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition("Api")]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>;
