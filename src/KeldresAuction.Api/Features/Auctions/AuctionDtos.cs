using KeldresAuction.Api.Domain;

namespace KeldresAuction.Api.Features.Auctions;

public sealed record BidDto(string Bidder, decimal Amount);

public sealed record AuctionDto(Guid Id, decimal ReservePrice, DateTimeOffset CreatedAt, IReadOnlyList<BidDto> Bids)
{
    public static AuctionDto From(Auction auction) => new(
        auction.Id,
        auction.ReservePrice,
        auction.CreatedAt,
        auction.Bids.OrderBy(b => b.Sequence).Select(b => new BidDto(b.Bidder, b.Amount)).ToList());
}

// Winner and WinningPrice are null when no bid reaches the reserve price.
public sealed record AuctionResultDto(Guid AuctionId, decimal ReservePrice, string? Winner, decimal? WinningPrice);
