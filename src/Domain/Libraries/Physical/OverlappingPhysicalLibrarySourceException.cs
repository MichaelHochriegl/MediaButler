using Domain.Storage;

namespace Domain.Libraries.Physical;

public sealed class OverlappingPhysicalLibrarySourceException(
    StorageLocationId storageLocationId,
    StorageRelativePath relativePath)
    : InvalidOperationException(
        $"Source '{relativePath}' in storage location '{storageLocationId}' overlaps an existing source in this library.")
{
    public StorageLocationId StorageLocationId { get; } = storageLocationId;
    public StorageRelativePath RelativePath { get; } = relativePath;
}
