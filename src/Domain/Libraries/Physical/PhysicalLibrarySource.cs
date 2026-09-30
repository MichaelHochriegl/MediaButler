using Domain.Storage;

namespace Domain.Libraries.Physical;

public sealed class PhysicalLibrarySource(
    LibraryMediaKind mediaKind,
    StorageLocationId storageLocationId,
    StorageRelativePath relativePath)
    : LibrarySource(mediaKind)
{
    public StorageLocationId StorageLocationId { get; private set; } = storageLocationId;

    public StorageRelativePath RelativePath { get; private set; } = relativePath;
    
    public bool OverlapsWith(PhysicalLibrarySource other) =>
        StorageLocationId == other.StorageLocationId &&
        RelativePath.Overlaps(other.RelativePath);
}