using System.Globalization;
using Ardalis.Result;

namespace KeldresAuction.Api.Features.Auctions;

// Parses the bids CSV. One bidder per line: the name first, then that bidder's bids.
//
//   Alice,110,130
//   Bob
//   Carol,125
//
// Rules: comma separated, no header, amounts use '.' as decimal separator, blank lines and
// trailing commas are ignored, a bidder may appear on several lines.
public static class CsvBidParser
{
    public static Result<IReadOnlyList<(string Bidder, decimal Amount)>> Parse(string csv)
    {
        var bids = new List<(string Bidder, decimal Amount)>();
        var errors = new List<ValidationError>();

        var lines = csv.TrimStart('﻿').Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        for (var index = 0; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
                continue;

            var lineNumber = index + 1;
            var fields = lines[index].Split(',').Select(f => f.Trim()).ToArray();
            var bidder = fields[0];

            if (bidder.Length == 0)
            {
                errors.Add(new ValidationError("Csv", $"Line {lineNumber}: the bidder name is missing."));
                continue;
            }

            foreach (var field in fields.Skip(1).Where(f => f.Length > 0))
            {
                if (decimal.TryParse(field, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount))
                    bids.Add((bidder, amount));
                else
                    errors.Add(new ValidationError("Csv", $"Line {lineNumber}: '{field}' is not a valid amount."));
            }
        }

        return errors.Count > 0
            ? Result<IReadOnlyList<(string Bidder, decimal Amount)>>.Invalid(errors)
            : Result<IReadOnlyList<(string Bidder, decimal Amount)>>.Success(bids);
    }
}
