using KeldresAuction.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace KeldresAuction.Api.Infrastructure.Persistence.Repositories;

public sealed class AuctionRepository(AuctionDbContext dbContext) : IAuctionRepository
{
    public async Task AddAsync(Auction auction, CancellationToken cancellationToken)
    {
        dbContext.Auctions.Add(auction);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Query().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Auction?> GetLatestAsync(CancellationToken cancellationToken) =>
        Query().OrderByDescending(a => a.CreatedAt).FirstOrDefaultAsync(cancellationToken);

    private IQueryable<Auction> Query() =>
        dbContext.Auctions
            .AsNoTracking()
            .Include(a => a.Bids.OrderBy(b => b.Sequence));
}
