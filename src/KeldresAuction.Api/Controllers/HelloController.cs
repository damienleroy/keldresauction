using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KeldresAuction.Api.Features.Hello;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace KeldresAuction.Api.Controllers;

[ApiController]
[Route("api/hello")]
[TranslateResultToActionResult] // Result<T> returned by actions is mapped to 200/400/404/409... automatically.
public class HelloController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Result<HelloResponse>> Get(CancellationToken cancellationToken) =>
        await mediator.Send(new HelloQuery(), cancellationToken);
}
