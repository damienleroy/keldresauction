using Ardalis.Result;
using KeldresAuction.Api.Domain;
using Mediator;

namespace KeldresAuction.Api.Features.Auctions.GetAuctionResult;

public sealed record GetAuctionResultQuery(Guid Id) : IQuery<Result<AuctionResultDto>>;

public sealed class GetAuctionResultQueryHandler(IAuctionRepository repository)
    : IQueryHandler<GetAuctionResultQuery, Result<AuctionResultDto>>
{
    public async ValueTask<Result<AuctionResultDto>> Handle(GetAuctionResultQuery query, CancellationToken cancellationToken)
    {
        var auction = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (auction is null)
            return Result<AuctionResultDto>.NotFound();

        var outcome = auction.DetermineOutcome();
        return new AuctionResultDto(auction.Id, auction.ReservePrice, outcome.Winner, outcome.WinningPrice);
    }
}
