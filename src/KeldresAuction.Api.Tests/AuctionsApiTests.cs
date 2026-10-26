using System.Net;
using System.Net.Http.Json;
using System.Text;
using KeldresAuction.Api.Features.Auctions;

namespace KeldresAuction.Api.Tests;

[Collection("Api")]
public class AuctionsApiTests(ApiFactory factory)
{
    private const string KataCsv = "A,110,130\nB\nC,125\nD,105,115,90\nE,132,135,140\n";

    private readonly HttpClient _client = factory.CreateClient();

    private static MultipartFormDataContent Form(string csv, string reservePrice)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(reservePrice), "ReservePrice" },
            { new StringContent(csv, Encoding.UTF8, "text/csv"), "File", "bids.csv" }
        };
        return form;
    }

    [Fact]
    public async Task CreateAuction_PersistsBidsAndComputesTheResult()
    {
        var created = await _client.PostAsync("/api/auctions", Form(KataCsv, "100"));

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var auction = await created.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.NotNull(auction);
        Assert.Equal(100, auction.ReservePrice);
        Assert.Equal(9, auction.Bids.Count);
        Assert.Equal(new BidDto("A", 110), auction.Bids[0]);

        var fetched = await _client.GetFromJsonAsync<AuctionDto>($"/api/auctions/{auction.Id}");
        Assert.Equal(auction.Bids, fetched!.Bids);

        var result = await _client.GetFromJsonAsync<AuctionResultDto>($"/api/auctions/{auction.Id}/result");
        Assert.Equal("E", result!.Winner);
        Assert.Equal(130, result.WinningPrice);
    }

    [Fact]
    public async Task CreateAuction_AcceptsADecimalReservePriceWithADot()
    {
        var response = await _client.PostAsync("/api/auctions", Form("A,120.5", "100.50"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal(100.50m, auction!.ReservePrice);
        Assert.Equal(120.5m, auction.Bids[0].Amount);
    }

    [Fact]
    public async Task GetLatest_ReturnsTheMostRecentAuction()
    {
        await _client.PostAsync("/api/auctions", Form("A,10", "1"));
        var second = await (await _client.PostAsync("/api/auctions", Form("B,20", "2"))).Content.ReadFromJsonAsync<AuctionDto>();

        var latest = await _client.GetFromJsonAsync<AuctionDto>("/api/auctions/latest");

        Assert.Equal(second!.Id, latest!.Id);
    }

    [Fact]
    public async Task Result_HasNoWinner_WhenNoBidReachesTheReserve()
    {
        var auction = await (await _client.PostAsync("/api/auctions", Form("A,10\nB,20", "100"))).Content.ReadFromJsonAsync<AuctionDto>();

        var result = await _client.GetFromJsonAsync<AuctionResultDto>($"/api/auctions/{auction!.Id}/result");

        Assert.Null(result!.Winner);
        Assert.Null(result.WinningPrice);
    }

    [Theory]
    [InlineData("A,abc", "100")]
    [InlineData("", "100")]
    [InlineData("A,10", "-5")]
    public async Task CreateAuction_RejectsInvalidInput(string csv, string reservePrice)
    {
        var response = await _client.PostAsync("/api/auctions", Form(csv, reservePrice));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_RejectsARequestWithoutFile()
    {
        var response = await _client.PostAsync("/api/auctions", new MultipartFormDataContent { { new StringContent("100"), "ReservePrice" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnknownAuction_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/auctions/{Guid.NewGuid()}/result");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
