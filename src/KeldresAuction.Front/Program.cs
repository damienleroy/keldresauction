using KeldresAuction.Front;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// In production the API serves the front, so the API lives on the same origin.
// In development, wwwroot/appsettings.Development.json points to the API's own address.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(string.IsNullOrWhiteSpace(apiBaseUrl) ? builder.HostEnvironment.BaseAddress : apiBaseUrl)
});

builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
