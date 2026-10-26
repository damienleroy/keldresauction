using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace KeldresAuction.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        // Expose the controllers declared in this test assembly (e.g. TestResultsController).
        builder.ConfigureServices(services => services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly));
}
