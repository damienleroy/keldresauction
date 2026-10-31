using System.Globalization;
using Ardalis.Result.AspNetCore;
using KeldresAuction.Api.Domain;
using KeldresAuction.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
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

// PostgreSQL through EF Core. The connection string is resolved lazily so it can be overridden
// by configuration set after startup (integration tests).
builder.Services.AddDbContext<AuctionDbContext>((serviceProvider, options) => options
    .UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("keldresauction"))
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddSingleton(TimeProvider.System);
// Readiness only: the "live" check used by /alive stays independent from the database.
builder.Services.AddHealthChecks().AddDbContextCheck<AuctionDbContext>("database");
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
