using System.Globalization;
using System.Text;
using KeldresAuction.Front.Models;

namespace KeldresAuction.Front.Services;

// Generates a sample bids CSV (one bidder per line: name, then that bidder's bids) and sends it to the API.
public sealed class AuctionCsvGenerator(AuctionApiClient apiClient)
{
    private static readonly string[] BidderNames =
        ["Alice", "Bob", "Carol", "Dave", "Eve", "Frank", "Grace", "Heidi", "Ivan", "Judy"];

    // Pass a seeded Random to get a reproducible file.
    public string Generate(
        int bidderCount = 5,
        int maxBidsPerBidder = 3,
        int minAmount = 80,
        int maxAmount = 200,
        Random? random = null)
    {
        random ??= Random.Shared;
        bidderCount = Math.Clamp(bidderCount, 1, BidderNames.Length);
        maxBidsPerBidder = Math.Max(maxBidsPerBidder, 1);

        var csv = new StringBuilder();
        foreach (var name in BidderNames.OrderBy(_ => random.Next()).Take(bidderCount))
        {
            csv.Append(name);
            var bidCount = random.Next(1, maxBidsPerBidder + 1);
            for (var i = 0; i < bidCount; i++)
                csv.Append(',').Append(random.Next(minAmount, maxAmount + 1).ToString(CultureInfo.InvariantCulture));
            csv.Append('\n');
        }

        return csv.ToString();
    }

    // Generates a CSV and sends it to the API as a new auction with the given reserve price.
    public Task<UploadOutcome> GenerateAndSendAsync(
        decimal reservePrice,
        int bidderCount = 5,
        int maxBidsPerBidder = 3,
        CancellationToken cancellationToken = default) =>
        apiClient.UploadCsvAsync(Generate(bidderCount, maxBidsPerBidder), reservePrice, cancellationToken);
}
