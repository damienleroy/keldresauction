using Ardalis.Result;
using KeldresAuction.Api.Features.Auctions;

namespace KeldresAuction.Api.Tests.Features;

public class CsvBidParserTests
{
    [Fact]
    public void Parse_ReadsOneBidderPerLineWithSeveralBids()
    {
        var result = CsvBidParser.Parse("Alice,110,130\nBob\nCarol,125.5\n");

        Assert.True(result.IsSuccess);
        Assert.Equal([("Alice", 110m), ("Alice", 130m), ("Carol", 125.5m)], result.Value);
    }

    [Fact]
    public void Parse_IgnoresBlankLinesTrailingCommasWhitespaceAndBom()
    {
        var result = CsvBidParser.Parse("﻿Alice , 110 ,130,\r\n\r\n  \r\nBob,90,\r\n");

        Assert.True(result.IsSuccess);
        Assert.Equal([("Alice", 110m), ("Alice", 130m), ("Bob", 90m)], result.Value);
    }

    [Fact]
    public void Parse_ReportsEveryInvalidLineWithItsNumber()
    {
        var result = CsvBidParser.Parse("Alice,110\nBob,abc\n,50\nCarol,12,1o");

        Assert.Equal(ResultStatus.Invalid, result.Status);
        var messages = result.ValidationErrors.Select(e => e.ErrorMessage).ToList();
        Assert.Contains(messages, m => m.StartsWith("Line 2:") && m.Contains("'abc'"));
        Assert.Contains(messages, m => m.StartsWith("Line 3:") && m.Contains("name"));
        Assert.Contains(messages, m => m.StartsWith("Line 4:") && m.Contains("'1o'"));
    }

    [Fact]
    public void Parse_AcceptsAnEmptyFileAndLeavesTheEmptinessToTheDomain()
    {
        var result = CsvBidParser.Parse("");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
