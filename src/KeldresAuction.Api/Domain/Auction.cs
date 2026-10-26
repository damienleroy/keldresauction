using Ardalis.Result;

namespace KeldresAuction.Api.Domain;

// A sealed-bid second-price auction for a single item.
// Rules:
//  - the winner is the bidder with the highest bid, provided it reaches the reserve price;
//  - the winning price is the highest bid at or above the reserve placed by a *different* bidder,
//    or the reserve price when there is none;
//  - ties on the highest bid are won by the bidder who submitted it first.
public sealed class Auction
{
    public const decimal MaxAmount = 1_000_000_000m;

    private readonly List<Bid> _bids = [];

    // Used by EF Core.
    private Auction() { }

    public Guid Id { get; private set; }
    public decimal ReservePrice { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<Bid> Bids => _bids;

    public static Result<Auction> Create(
        decimal reservePrice,
        IEnumerable<(string Bidder, decimal Amount)> bids,
        DateTimeOffset createdAt)
    {
        var errors = new List<ValidationError>();

        if (reservePrice < 0 || reservePrice > MaxAmount || HasMoreThanTwoDecimals(reservePrice))
            errors.Add(new ValidationError(nameof(ReservePrice), $"The reserve price must be between 0 and {MaxAmount:0} with at most 2 decimals."));

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = reservePrice,
            CreatedAt = createdAt
        };

        foreach (var (bidder, amount) in bids)
        {
            var name = bidder.Trim();
            var position = auction._bids.Count + 1;

            if (name.Length is 0 or > Bid.BidderMaxLength)
                errors.Add(new ValidationError("Bids", $"Bid {position}: the bidder name is required and must not exceed {Bid.BidderMaxLength} characters."));
            else if (amount <= 0 || amount > MaxAmount || HasMoreThanTwoDecimals(amount))
                errors.Add(new ValidationError("Bids", $"Bid {position} ({name}): the amount must be between 0 (excluded) and {MaxAmount:0} with at most 2 decimals."));
            else
                auction._bids.Add(new Bid(name, amount, auction._bids.Count));
        }

        if (errors.Count == 0 && auction._bids.Count == 0)
            errors.Add(new ValidationError("Bids", "At least one bid is required."));

        return errors.Count > 0 ? Result<Auction>.Invalid(errors) : auction;
    }

    public AuctionOutcome DetermineOutcome()
    {
        var eligibleBids = _bids.Where(b => b.Amount >= ReservePrice).ToList();
        if (eligibleBids.Count == 0)
            return AuctionOutcome.NoWinner;

        var winningBid = eligibleBids
            .OrderByDescending(b => b.Amount)
            .ThenBy(b => b.Sequence)
            .First();

        // The price comes from the other bidders; the reserve price is the floor.
        var winningPrice = eligibleBids
            .Where(b => !b.IsFrom(winningBid.Bidder))
            .Select(b => b.Amount)
            .DefaultIfEmpty(ReservePrice)
            .Max();

        return new AuctionOutcome(winningBid.Bidder, winningPrice);
    }

    private static bool HasMoreThanTwoDecimals(decimal value) => decimal.Round(value, 2) != value;
}
