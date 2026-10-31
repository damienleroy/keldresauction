using KeldresAuction.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeldresAuction.Api.Infrastructure.Persistence;

public sealed class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable("auctions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.ReservePrice).HasPrecision(18, 2);
        builder.HasIndex(a => a.CreatedAt);

        builder.HasMany(a => a.Bids)
            .WithOne()
            .HasForeignKey("AuctionId");
    }
}

public sealed class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("bids");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.Bidder).HasMaxLength(Bid.BidderMaxLength).IsRequired();
        builder.Property(b => b.Amount).HasPrecision(18, 2);
    }
}
