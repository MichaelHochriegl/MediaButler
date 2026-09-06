using Domain.Storage;

namespace Storage.Backend.Features;

/// <summary>
///     Represents a normalized storage location configured through <see cref="Configuration.StorageOptions" />.
/// </summary>
/// <remarks>
///     This record is used internally by <c>StorageLocationRegistry</c> as the resolved representation of a
///     configured storage location, so storage backends can consume location details without parsing
///     <see cref="Configuration.StorageOptions" /> directly.
/// </remarks>
/// <param name="Id">
///     The unique identifier of the configured storage location.
/// </param>
/// <param name="DisplayName">
///     The human-readable name of the configured storage location.
/// </param>
/// <param name="RootPath">
///     The root path on disk where the configured storage location begins.
/// </param>
internal sealed record ConfiguredStorageLocation(StorageLocationId Id, string DisplayName, string RootPath);