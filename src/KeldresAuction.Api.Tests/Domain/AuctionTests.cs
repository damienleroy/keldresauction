using System.Globalization;
using Ardalis.Result;
using KeldresAuction.Api.Domain;

namespace KeldresAuction.Api.Tests.Domain;

public class AuctionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // Bids are written "Name=Amount", separated by spaces, in submission order.
    private static (string Bidder, decimal Amount)[] ParseBids(string bids) =>
        bids.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(bid => bid.Split('='))
            .Select(parts => (parts[0], decimal.Parse(parts[1], CultureInfo.InvariantCulture)))
            .ToArray();

    [Theory]
    // Reference example: E wins and pays the best bid of another bidder (A's 130).
    [InlineData(100, "A=110 A=130 C=125 D=105 D=115 D=90 E=132 E=135 E=140", "E", 130)]
    // The winner's own lower bids are ignored for the price.
    [InlineData(100, "A=150 A=140 B=120", "A", 120)]
    // Only the winner reaches the reserve: the reserve is the price.
    [InlineData(100, "A=150 B=99 C=50", "A", 100)]
    [InlineData(100, "A=120 A=130", "A", 100)]
    // A bid equal to the reserve is eligible.
    [InlineData(100, "A=100 B=100", "A", 100)]
    // Nobody reaches the reserve: no winner.
    [InlineData(100, "A=99 B=50", null, null)]
    // Tie on the highest bid: the first submitted wins.
    [InlineData(100, "A=130 B=130 C=110", "A", 130)]
    // Bidder names are compared ignoring case.
    [InlineData(100, "alice=130 Alice=120 Bob=110", "alice", 110)]
    public void DetermineOutcome_FollowsSecondPriceRules(int reserve, string bids, string? expectedWinner, int? expectedPrice)
    {
        var auction = Auction.Create(reserve, ParseBids(bids), Now).Value;

        var outcome = auction.DetermineOutcome();

        Assert.Equal(expectedWinner, outcome.Winner);
        Assert.Equal(expectedPrice, (int?)outcome.WinningPrice);
    }

    [Theory]
    [InlineData(-1, "A=10")]            // negative reserve
    [InlineData(1.005, "A=10")]         // reserve with more than 2 decimals
    [InlineData(2_000_000_000, "A=10")] // reserve too high
    [InlineData(100, "")]               // no bids
    [InlineData(100, "=10")]            // missing bidder name
    [InlineData(100, "A=0")]            // bid must be positive
    [InlineData(100, "A=-5")]
    [InlineData(100, "A=10.005")]       // bid with more than 2 decimals
    public void Create_RejectsInvalidInput(double reserve, string bids)
    {
        var result = Auction.Create((decimal)reserve, ParseBids(bids), Now);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }
}
