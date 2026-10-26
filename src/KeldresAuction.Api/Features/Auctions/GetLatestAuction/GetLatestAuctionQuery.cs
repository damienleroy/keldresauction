using Ardalis.Result;
using KeldresAuction.Api.Domain;
using Mediator;

namespace KeldresAuction.Api.Features.Auctions.GetLatestAuction;

public sealed record GetLatestAuctionQuery : IQuery<Result<AuctionDto>>;

public sealed class GetLatestAuctionQueryHandler(IAuctionRepository repository)
    : IQueryHandler<GetLatestAuctionQuery, Result<AuctionDto>>
{
    public async ValueTask<Result<AuctionDto>> Handle(GetLatestAuctionQuery query, CancellationToken cancellationToken)
    {
        var auction = await repository.GetLatestAsync(cancellationToken);
        return auction is null ? Result<AuctionDto>.NotFound() : AuctionDto.From(auction);
    }
}
