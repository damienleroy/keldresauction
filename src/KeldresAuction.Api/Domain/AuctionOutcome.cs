namespace KeldresAuction.Api.Domain;

// Winner and winning price of a sealed-bid second-price auction.
// Winner and WinningPrice are null when no bid reaches the reserve price.
public sealed record AuctionOutcome(string? Winner, decimal? WinningPrice)
{
    public static readonly AuctionOutcome NoWinner = new(null, null);

    public bool HasWinner => Winner is not null;
}
