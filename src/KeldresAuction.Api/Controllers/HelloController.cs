using KeldresAuction.Api.Features.Hello;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace KeldresAuction.Api.Controllers;

[ApiController]
[Route("api/hello")]
public class HelloController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<HelloResponse> Get(CancellationToken cancellationToken) =>
        await mediator.Send(new HelloQuery(), cancellationToken);
}
