using Ardalis.Result;
using KeldresAuction.Api.Domain;
using Mediator;

namespace KeldresAuction.Api.Features.Auctions.CreateAuction;

public sealed record CreateAuctionCommand(decimal ReservePrice, string Csv) : ICommand<Result<AuctionDto>>;

public sealed class CreateAuctionCommandHandler(IAuctionRepository repository, TimeProvider timeProvider)
    : ICommandHandler<CreateAuctionCommand, Result<AuctionDto>>
{
    public async ValueTask<Result<AuctionDto>> Handle(CreateAuctionCommand command, CancellationToken cancellationToken)
    {
        var bids = CsvBidParser.Parse(command.Csv);
        if (!bids.IsSuccess)
            return Result<AuctionDto>.Invalid(bids.ValidationErrors);

        var auction = Auction.Create(command.ReservePrice, bids.Value, timeProvider.GetUtcNow());
        if (!auction.IsSuccess)
            return Result<AuctionDto>.Invalid(auction.ValidationErrors);

        await repository.AddAsync(auction.Value, cancellationToken);

        return Result<AuctionDto>.Created(AuctionDto.From(auction.Value), $"/api/auctions/{auction.Value.Id}");
    }
}
