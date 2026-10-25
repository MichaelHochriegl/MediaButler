using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Persistence.Locks;

/// <summary>
/// Provides PostgreSQL transaction-scoped advisory locks for named resources.
/// </summary>
public static class AdvisoryLocks
{
    extension(DatabaseFacade db)
    {
        /// <summary>
        /// Waits for exclusive advisory locks on the specified resources in the current transaction.
        /// </summary>
        /// <param name="resources">Non-empty, case-sensitive resource names to lock.</param>
        /// <param name="cancellationToken">A token used to cancel waiting for the locks.</param>
        /// <returns>A task that completes when all requested locks have been acquired.</returns>
        /// <remarks>
        /// Resource names are hashed into stable 64-bit keys. Duplicate keys are acquired once,
        /// in ascending order to avoid deadlocks between callers acquiring the same set of locks.
        /// PostgreSQL releases the locks when the transaction ends; they are not released when this method returns.
        /// Advisory locks coordinate only callers using the same locking convention in the same database.
        /// Hash collisions can cause unrelated resources to share a lock.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The database has no current transaction.</exception>
        /// <exception cref="ArgumentNullException">A resource name is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A resource name is empty.</exception>
        /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
        public async Task AcquireLockAsync(IEnumerable<string> resources, CancellationToken cancellationToken = default)
        {
            if (db.CurrentTransaction is null)
            {
                throw new InvalidOperationException("Cannot acquire advisory lock outside of a transaction.");
            }
            
            var keys = resources.Select(GetLockKey).Distinct().Order().ToArray();
            
            foreach (var key in keys)
            {
                await db.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", cancellationToken);
            }
        }
    }

    /// <summary>
    /// Derives a stable lock key from the first eight bytes of the resource name's SHA-256 hash.
    /// </summary>
    /// <param name="resource">The non-empty resource name encoded as UTF-8 before hashing.</param>
    /// <returns>The hash prefix interpreted as a signed, big-endian 64-bit integer.</returns>
    private static long GetLockKey(string resource)
    {
        ArgumentException.ThrowIfNullOrEmpty(resource);
        
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(resource));
        
        return BinaryPrimitives.ReadInt64BigEndian(hash);
    }
}
