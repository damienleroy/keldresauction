namespace KeldresAuction.Api.Domain;

public interface IAuctionRepository
{
    /// <summary>
    /// Adds a new auction to the repository.
    /// </summary>
    /// <param name="auction">The auction to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(Auction auction, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an auction by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the auction.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The task result contains the auction if found; otherwise, null.</returns>
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the latest auction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The task result contains the latest auction if found; otherwise, null.</returns>
    Task<Auction?> GetLatestAsync(CancellationToken cancellationToken);
}
