using Mediator;

namespace KeldresAuction.Api.Features.Hello;

public sealed record HelloQuery : IQuery<HelloResponse>;

public sealed record HelloResponse(string Message);

public sealed class HelloQueryHandler : IQueryHandler<HelloQuery, HelloResponse>
{
    public ValueTask<HelloResponse> Handle(HelloQuery query, CancellationToken cancellationToken) =>
        ValueTask.FromResult(new HelloResponse("Hello World from KeldresAuction API"));
}
