namespace KeldresAuction.Front.Models;

// Mirrors the API contracts (JSON, camelCase).
public sealed record BidDto(string Bidder, decimal Amount);

public sealed record AuctionDto(Guid Id, decimal ReservePrice, DateTimeOffset CreatedAt, IReadOnlyList<BidDto> Bids);

// Winner and WinningPrice are null when no bid reaches the reserve price.
public sealed record AuctionResultDto(Guid AuctionId, decimal ReservePrice, string? Winner, decimal? WinningPrice);

// Outcome of sending a CSV to the API: the created auction, or the errors reported by the API.
public sealed record UploadOutcome(AuctionDto? Auction, IReadOnlyList<string> Errors)
{
    public bool Succeeded => Auction is not null;
}
