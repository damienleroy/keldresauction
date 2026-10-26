using System.Net;
using System.Text;
using Bunit;
using KeldresAuction.Front.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace KeldresAuction.Front.Tests;

public class HomeTests : BunitContext
{
    [Fact]
    public void Home_DisplaysMessageFromApi()
    {
        Services.AddScoped(_ => new HttpClient(new StubHandler("""{"message":"Hello World"}"""))
        {
            BaseAddress = new Uri("http://localhost")
        });

        var cut = Render<Home>();

        cut.WaitForAssertion(() => Assert.Equal("Hello World", cut.Find("#api-message").TextContent));
    }

    private sealed class StubHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }
}
