using Ardalis.Result;
using KeldresAuction.Api.Domain;

namespace KeldresAuction.Api.Tests.Domain;

public class AuctionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Auction CreateAuction(decimal reserve, params (string Bidder, decimal Amount)[] bids)
    {
        var result = Auction.Create(reserve, bids, Now);
        Assert.True(result.IsSuccess, string.Join("; ", result.ValidationErrors.Select(e => e.ErrorMessage)));
        return result.Value;
    }

    [Fact]
    public void HighestBidderWins_AndPaysTheHighestBidOfAnotherBidder()
    {
        // Reserve 100. A: 110, 130 / C: 125 / D: 105, 115, 90 / E: 132, 135, 140 (B did not bid)
        var auction = CreateAuction(100,
            ("A", 110), ("A", 130),
            ("C", 125),
            ("D", 105), ("D", 115), ("D", 90),
            ("E", 132), ("E", 135), ("E", 140));

        var outcome = auction.DetermineOutcome();

        Assert.Equal("E", outcome.Winner);
        Assert.Equal(130, outcome.WinningPrice);
    }

    [Fact]
    public void WinnersOtherBidsAreIgnoredForThePrice()
    {
        var auction = CreateAuction(100, ("A", 150), ("A", 140), ("B", 120));

        var outcome = auction.DetermineOutcome();

        Assert.Equal("A", outcome.Winner);
        Assert.Equal(120, outcome.WinningPrice);
    }

    [Fact]
    public void PriceIsTheReserve_WhenOnlyTheWinnerReachedIt()
    {
        var auction = CreateAuction(100, ("A", 150), ("B", 99), ("C", 50));

        var outcome = auction.DetermineOutcome();

        Assert.Equal("A", outcome.Winner);
        Assert.Equal(100, outcome.WinningPrice);
    }

    [Fact]
    public void PriceIsTheReserve_WhenThereIsASingleBidder()
    {
        var auction = CreateAuction(100, ("A", 120), ("A", 130));

        var outcome = auction.DetermineOutcome();

        Assert.Equal("A", outcome.Winner);
        Assert.Equal(100, outcome.WinningPrice);
    }

    [Fact]
    public void BidEqualToTheReserveIsEligible()
    {
        var auction = CreateAuction(100, ("A", 100), ("B", 100));

        var outcome = auction.DetermineOutcome();

        Assert.Equal("A", outcome.Winner);
        Assert.Equal(100, outcome.WinningPrice);
    }

    [Fact]
    public void NoWinner_WhenNoBidReachesTheReserve()
    {
        var auction = CreateAuction(100, ("A", 99), ("B", 50));

        var outcome = auction.DetermineOutcome();

        Assert.False(outcome.HasWinner);
        Assert.Null(outcome.Winner);
        Assert.Null(outcome.WinningPrice);
    }

    [Fact]
    public void TieOnTheHighestBid_IsWonByTheFirstSubmittedBid()
    {
        var auction = CreateAuction(100, ("A", 130), ("B", 130), ("C", 110));

        var outcome = auction.DetermineOutcome();

        Assert.Equal("A", outcome.Winner);
        Assert.Equal(130, outcome.WinningPrice);
    }

    [Fact]
    public void BidderNamesAreComparedIgnoringCase()
    {
        var auction = CreateAuction(100, ("alice", 130), ("Alice", 120), ("Bob", 110));

        var outcome = auction.DetermineOutcome();

        Assert.Equal("alice", outcome.Winner);
        Assert.Equal(110, outcome.WinningPrice);
    }

    [Fact]
    public void Create_TrimsBidderNamesAndKeepsSubmissionOrder()
    {
        var auction = CreateAuction(0, ("  Alice ", 10), ("Bob", 20));

        Assert.Equal(["Alice", "Bob"], auction.Bids.OrderBy(b => b.Sequence).Select(b => b.Bidder));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1.005)]
    [InlineData(2_000_000_000)]
    public void Create_RejectsInvalidReservePrice(double reserve)
    {
        var result = Auction.Create((decimal)reserve, [("A", 10)], Now);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public void Create_RejectsAnAuctionWithoutBids()
    {
        var result = Auction.Create(100, [], Now);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("   ", 10)]
    [InlineData("A", 0)]
    [InlineData("A", -5)]
    [InlineData("A", 10.005)]
    public void Create_RejectsInvalidBids(string bidder, double amount)
    {
        var result = Auction.Create(0, [(bidder, (decimal)amount)], Now);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }
}
