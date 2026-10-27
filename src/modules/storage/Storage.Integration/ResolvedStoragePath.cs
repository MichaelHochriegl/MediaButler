using Domain.Storage;

namespace Storage.Integration;

public record ResolvedStoragePath(
    StorageLocationId LocationId,
    StorageRelativePath RelativePath,
    StorageAbsolutePath AbsolutePath);