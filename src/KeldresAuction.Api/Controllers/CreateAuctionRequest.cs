using System.ComponentModel.DataAnnotations;

namespace KeldresAuction.Api.Controllers;

// Form-level validation (shape of the input). Business rules live in the domain.
public sealed class CreateAuctionRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Range(0, 1_000_000_000)]
    public decimal ReservePrice { get; set; }
}
