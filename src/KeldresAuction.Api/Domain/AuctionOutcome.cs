namespace KeldresAuction.Api.Domain;

public sealed record AuctionOutcome(string? Winner, decimal? WinningPrice)
{
    public static readonly AuctionOutcome NoWinner = new(null, null);

    public bool HasWinner => Winner is not null;
}
