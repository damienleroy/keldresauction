using System.Net;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace KeldresAuction.Api.Tests;

// Checks that Result<T> failures returned by controller actions are translated into the right HTTP status.
public class ResultConventionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/test-results/ok", HttpStatusCode.OK)]
    [InlineData("/test-results/not-found", HttpStatusCode.NotFound)]
    [InlineData("/test-results/invalid", HttpStatusCode.BadRequest)]
    [InlineData("/test-results/conflict", HttpStatusCode.Conflict)]
    public async Task ResultIsTranslatedToHttpStatus(string url, HttpStatusCode expected)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(expected, response.StatusCode);
    }
}

[ApiController]
[Route("test-results")]
[TranslateResultToActionResult]
public class TestResultsController : ControllerBase
{
    [HttpGet("ok")]
    public Result<string> Success() => Result.Success("ok");

    [HttpGet("not-found")]
    public Result<string> NotFoundResult() => Result.NotFound();

    [HttpGet("invalid")]
    public Result<string> Invalid() => Result.Invalid(new ValidationError("field", "bad value"));

    [HttpGet("conflict")]
    public Result<string> ConflictResult() => Result.Conflict();
}
