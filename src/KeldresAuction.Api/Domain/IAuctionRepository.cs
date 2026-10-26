namespace KeldresAuction.Api.Domain;

public interface IAuctionRepository
{
    Task AddAsync(Auction auction, CancellationToken cancellationToken);

    // Read side: auctions are returned with their bids in submission order.
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Auction?> GetLatestAsync(CancellationToken cancellationToken);
}
