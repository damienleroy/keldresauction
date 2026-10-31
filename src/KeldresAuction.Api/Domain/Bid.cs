namespace KeldresAuction.Api.Domain;

public sealed class Bid
{
    public const int BidderMaxLength = 100;

    // Used by EF Core.
    private Bid() => Bidder = string.Empty;

    internal Bid(string bidder, decimal amount, int sequence)
    {
        Id = Guid.NewGuid();
        Bidder = bidder;
        Amount = amount;
        Sequence = sequence;
    }

    public Guid Id { get; private set; }
    public string Bidder { get; private set; }
    public decimal Amount { get; private set; }

    // Order in which the bid was submitted; used to break ties between equal bids.
    public int Sequence { get; private set; }

    public bool IsFrom(string bidder) => StringComparer.OrdinalIgnoreCase.Equals(Bidder, bidder);
}
