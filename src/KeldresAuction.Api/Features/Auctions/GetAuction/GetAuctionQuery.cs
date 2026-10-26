using Ardalis.Result;
using KeldresAuction.Api.Domain;
using Mediator;

namespace KeldresAuction.Api.Features.Auctions.GetAuction;

public sealed record GetAuctionQuery(Guid Id) : IQuery<Result<AuctionDto>>;

public sealed class GetAuctionQueryHandler(IAuctionRepository repository)
    : IQueryHandler<GetAuctionQuery, Result<AuctionDto>>
{
    public async ValueTask<Result<AuctionDto>> Handle(GetAuctionQuery query, CancellationToken cancellationToken)
    {
        var auction = await repository.GetByIdAsync(query.Id, cancellationToken);
        return auction is null ? Result<AuctionDto>.NotFound() : AuctionDto.From(auction);
    }
}
