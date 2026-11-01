using KeldresAuction.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace KeldresAuction.Api.Infrastructure.Persistence;

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<Auction> Auctions => Set<Auction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionDbContext).Assembly);
}
