using Ardalis.Result;
using Mediator;

namespace KeldresAuction.Api.Features.Hello;

public sealed record HelloQuery : IQuery<Result<HelloResponse>>;

public sealed record HelloResponse(string Message);

public sealed class HelloQueryHandler : IQueryHandler<HelloQuery, Result<HelloResponse>>
{
    public ValueTask<Result<HelloResponse>> Handle(HelloQuery query, CancellationToken cancellationToken) =>
        ValueTask.FromResult(Result.Success(new HelloResponse("Hello World from KeldresAuction API")));
}
