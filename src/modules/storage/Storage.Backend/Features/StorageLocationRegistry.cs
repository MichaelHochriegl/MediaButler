using Domain.Storage;
using Microsoft.Extensions.Options;
using Storage.Backend.Configuration;

namespace Storage.Backend.Features;

/// <summary>
///     Provides access to the storage locations configured for the application.
/// </summary>
/// <remarks>
///     The registry converts <see cref="StorageOptions" /> into normalized
///     <see cref="ConfiguredStorageLocation" /> instances once, allowing storage backends
///     to resolve configured locations without parsing or interpreting the raw options themselves.
/// </remarks>
internal sealed class StorageLocationRegistry
{
    private readonly Dictionary<StorageLocationId, ConfiguredStorageLocation> _configuredLocations;

    /// <summary>
    ///     Gets all configured storage locations known to the registry.
    /// </summary>
    public IReadOnlyCollection<ConfiguredStorageLocation> ConfiguredLocations => _configuredLocations.Values;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageLocationRegistry" /> class.
    /// </summary>
    /// <param name="options">
    ///     The configured storage options from which the registry builds its normalized storage location entries.
    /// </param>
    public StorageLocationRegistry(IOptions<StorageOptions> options)
    {
        _configuredLocations = options.Value.Locations.ToDictionary(l => new StorageLocationId(l.Key),
            l => new ConfiguredStorageLocation(new StorageLocationId(l.Key),
                l.Value.DisplayName,
                l.Value.NormalizedRootPath));
    }

    /// <summary>
    ///     Attempts to resolve a configured storage location by its identifier.
    /// </summary>
    /// <param name="id">
    ///     The identifier of the configured storage location to resolve.
    /// </param>
    /// <param name="location">
    ///     When this method returns, contains the configured storage location associated with <paramref name="id" />,
    ///     if one was found; otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when a configured storage location exists for <paramref name="id" />;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public bool TryGet(StorageLocationId id, out ConfiguredStorageLocation? location)
    {
        return _configuredLocations.TryGetValue(id, out location);
    }
}