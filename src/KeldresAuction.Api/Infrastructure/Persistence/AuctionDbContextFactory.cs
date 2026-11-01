using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KeldresAuction.Api.Infrastructure.Persistence;

// Used by `dotnet ef` only (migrations); the connection string is never opened at design time.
public sealed class AuctionDbContextFactory : IDesignTimeDbContextFactory<AuctionDbContext>
{
    public AuctionDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=keldresauction")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AuctionDbContext(options);
    }
}
