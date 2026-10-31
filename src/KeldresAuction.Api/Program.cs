using System.Globalization;
using Ardalis.Result.AspNetCore;
using KeldresAuction.Api.Domain;
using KeldresAuction.Api.Infrastructure.Persistence;
using KeldresAuction.Api.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

// The API speaks the invariant culture ('.' decimal separator), whatever the host machine's culture is.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Service defaults: OpenTelemetry (traces, metrics, logs), health checks, resilience.
builder.AddServiceDefaults();

builder.Services.AddControllers(options => options.AddDefaultResultConvention());
builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuctionDbContext>((serviceProvider, options) => options
    .UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("keldresauction"))
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddSingleton(TimeProvider.System);

// In development the Blazor front runs on its own dev server, so allow its origin.
const string DevCorsPolicy = "DevFront";
builder.Services.AddCors(options => options.AddPolicy(DevCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// Apply pending EF Core migrations at startup.
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AuctionDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseForwardedHeaders();
app.MapDefaultEndpoints();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "KeldresAuction API v1"));
    app.UseCors(DevCorsPolicy);
}

app.MapControllers();

// Unknown API routes must stay 404 instead of falling back to the Blazor index.html.
app.Map("/api/{**rest}", () => Results.NotFound());

// The Blazor WebAssembly front is published into wwwroot and served by this same host.
app.UseDefaultFiles();
// .dat (ICU globalization data) has no default MIME mapping, so it would be a 404 otherwise.
var contentTypes = new FileExtensionContentTypeProvider();
contentTypes.Mappings[".dat"] = "application/octet-stream";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypes });
app.MapFallbackToFile("index.html");

app.Run();
