using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeldresAuction.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Refactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bids_auctions_auction_id",
                table: "bids");

            migrationBuilder.AddForeignKey(
                name: "fk_bids_auctions_auction_id",
                table: "bids",
                column: "auction_id",
                principalTable: "auctions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bids_auctions_auction_id",
                table: "bids");

            migrationBuilder.AddForeignKey(
                name: "fk_bids_auctions_auction_id",
                table: "bids",
                column: "auction_id",
                principalTable: "auctions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
