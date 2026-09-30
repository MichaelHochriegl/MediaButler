using Domain.Storage;

namespace Storage.Integration;

public interface IStorageDirectoryResolver
{
    /// <exception cref="StorageDirectoryNotFoundException">The directory does not exist.</exception>
    /// <exception cref="StorageDirectoryNotAccessibleException">The directory cannot be read.</exception>
    ResolvedStoragePath ResolveDirectory(StorageLocationId locationId, string relativePath);
}

public sealed class StorageDirectoryNotFoundException(
    StorageLocationId locationId,
    StorageRelativePath relativePath,
    DirectoryNotFoundException innerException)
    : Exception($"Directory '{relativePath}' does not exist in storage location '{locationId}'.", innerException)
{
    public StorageLocationId LocationId { get; } = locationId;
    public StorageRelativePath RelativePath { get; } = relativePath;
}

public sealed class StorageDirectoryNotAccessibleException(
    StorageLocationId locationId,
    StorageRelativePath relativePath,
    Exception innerException)
    : Exception($"Directory '{relativePath}' cannot be read in storage location '{locationId}'.", innerException)
{
    public StorageLocationId LocationId { get; } = locationId;
    public StorageRelativePath RelativePath { get; } = relativePath;
}
