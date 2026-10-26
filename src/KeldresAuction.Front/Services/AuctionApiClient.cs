using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KeldresAuction.Front.Models;

namespace KeldresAuction.Front.Services;

// Thin wrapper over the API: the front holds no business logic or data of its own.
public sealed class AuctionApiClient(HttpClient http)
{
    // Sends a bids CSV (see the API's CsvBidParser for the format) with the reserve price.
    public async Task<UploadOutcome> UploadCsvAsync(string csv, decimal reservePrice, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent(reservePrice.ToString(CultureInfo.InvariantCulture)), "ReservePrice" },
            { new StringContent(csv, Encoding.UTF8, "text/csv"), "File", "bids.csv" }
        };

        using var response = await http.PostAsync("api/auctions", form, cancellationToken);
        if (response.IsSuccessStatusCode)
            return new UploadOutcome(await response.Content.ReadFromJsonAsync<AuctionDto>(cancellationToken), []);

        return new UploadOutcome(null, await ReadErrorsAsync(response, cancellationToken));
    }

    // Null when no auction exists yet.
    public async Task<AuctionDto?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync("api/auctions/latest", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuctionDto>(cancellationToken);
    }

    public async Task<AuctionResultDto?> GetResultAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"api/auctions/{auctionId}/result", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuctionResultDto>(cancellationToken);
    }

    // Problem details: the "errors" member (ASP.NET validation) lists every message, otherwise fall back to the detail/title.
    private static async Task<IReadOnlyList<string>> ReadErrorsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var messages = new List<string>();
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("errors", out var errors))
                CollectStrings(errors, messages);

            if (messages.Count == 0 && root.ValueKind == JsonValueKind.Object)
            {
                foreach (var member in new[] { "detail", "title" })
                {
                    if (root.TryGetProperty(member, out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        messages.Add(value.GetString()!);
                        break;
                    }
                }
            }

            if (messages.Count > 0)
                return messages;
        }
        catch (JsonException)
        {
            // Not a JSON body: fall through to the generic message.
        }

        return [$"The API answered {(int)response.StatusCode} {response.ReasonPhrase}."];
    }

    private static void CollectStrings(JsonElement element, List<string> messages)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                messages.Add(element.GetString()!);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectStrings(item, messages);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    CollectStrings(property.Value, messages);
                break;
        }
    }
}
