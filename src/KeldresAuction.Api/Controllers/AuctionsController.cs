using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using KeldresAuction.Api.Features.Auctions;
using KeldresAuction.Api.Features.Auctions.CreateAuction;
using KeldresAuction.Api.Features.Auctions.GetAuction;
using KeldresAuction.Api.Features.Auctions.GetAuctionResult;
using KeldresAuction.Api.Features.Auctions.GetLatestAuction;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace KeldresAuction.Api.Controllers;

[ApiController]
[Route("api/auctions")]
[TranslateResultToActionResult]
public class AuctionsController(IMediator mediator) : ControllerBase
{
    private const long MaxCsvBytes = 1024 * 1024;

    // Creates an auction from a bids CSV (see CsvBidParser for the format) and a reserve price.
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxCsvBytes + 16 * 1024)]
    public async Task<Result<AuctionDto>> Create([FromForm] CreateAuctionRequest request, CancellationToken cancellationToken)
    {
        if (request.File.Length > MaxCsvBytes)
            return Result<AuctionDto>.Invalid(new ValidationError(nameof(request.File), "The CSV file must not exceed 1 MiB."));

        using var reader = new StreamReader(request.File.OpenReadStream());
        var csv = await reader.ReadToEndAsync(cancellationToken);

        return await mediator.Send(new CreateAuctionCommand(request.ReservePrice, csv), cancellationToken);
    }

    [HttpGet("latest")]
    public async Task<Result<AuctionDto>> GetLatest(CancellationToken cancellationToken) =>
        await mediator.Send(new GetLatestAuctionQuery(), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<Result<AuctionDto>> Get(Guid id, CancellationToken cancellationToken) =>
        await mediator.Send(new GetAuctionQuery(id), cancellationToken);

    [HttpGet("{id:guid}/result")]
    public async Task<Result<AuctionResultDto>> GetResult(Guid id, CancellationToken cancellationToken) =>
        await mediator.Send(new GetAuctionResultQuery(id), cancellationToken);
}
