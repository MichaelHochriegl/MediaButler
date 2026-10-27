using Domain.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Persistence.Locks;

namespace Libraries.Backend.Features.Physical;

/// <summary>
/// Coordinates physical library source changes through transaction-scoped storage location locks.
/// </summary>
public static class PhysicalLibraryLocks
{
    extension (DatabaseFacade db)
    {
        /// <summary>
        /// Waits for exclusive advisory locks on the specified physical library source locations.
        /// </summary>
        /// <param name="locationIds">The storage locations whose use as physical library sources must be coordinated.</param>
        /// <param name="cancellationToken">A token used to cancel waiting for the locks.</param>
        /// <returns>A task that completes when all requested location locks have been acquired.</returns>
        /// <remarks>
        /// Uses a resource namespace reserved for physical library sources. Duplicate locations are locked once,
        /// and locks are acquired in a consistent key order. Locks remain held until the current transaction ends.
        /// Callers must acquire these locks before checking or changing source assignments for the locations.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The database has no current transaction.</exception>
        /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
        public Task AcquirePhysicalLibrarySourceLocks(
            IEnumerable<StorageLocationId> locationIds,
            CancellationToken cancellationToken = default)
        {
            var resources = locationIds.Select(id => $"MediaButler:Libraries:PhysicalSources:{id.Value}");
            
            return db.AcquireLockAsync(resources, cancellationToken);
        }
    }
}
