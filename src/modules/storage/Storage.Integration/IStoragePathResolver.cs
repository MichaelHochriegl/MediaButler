using Domain.Storage;

namespace Storage.Integration;

public interface IStoragePathResolver
{
    /// <summary>Resolves a relative Linux path beneath a configured storage location.</summary>
    /// <exception cref="StorageLocationNotFoundException">The location is not configured.</exception>
    /// <exception cref="InvalidStoragePathException">The path is malformed or unsafe.</exception>
    /// <exception cref="RootedStoragePathException">The supplied path is rooted.</exception>
    /// <exception cref="EscapingStoragePathException">The path escapes the configured location.</exception>
    /// <exception cref="SymlinkStoragePathException">Resolution would traverse a symbolic link.</exception>
    string ResolvePath(StorageLocationId locationId, string relativePath);
}

public sealed class StorageLocationNotFoundException(StorageLocationId locationId)
    : Exception($"Storage location '{locationId}' is not configured.")
{
    public StorageLocationId LocationId { get; } = locationId;
}

public class InvalidStoragePathException : Exception
{
    public StorageLocationId LocationId { get; }
    public string RelativePath { get; }

    public InvalidStoragePathException(
        StorageLocationId locationId,
        string relativePath,
        Exception? innerException = null)
        : this(locationId, relativePath,
            $"Path '{relativePath}' is invalid for storage location '{locationId}'.", innerException)
    {
    }

    protected InvalidStoragePathException(
        StorageLocationId locationId,
        string relativePath,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        LocationId = locationId;
        RelativePath = relativePath;
    }
}

public sealed class RootedStoragePathException(StorageLocationId locationId, string relativePath)
    : InvalidStoragePathException(locationId, relativePath,
        $"Path '{relativePath}' for storage location '{locationId}' must be relative; rooted paths are not allowed.")
{
}

public sealed class EscapingStoragePathException(StorageLocationId locationId, string relativePath)
    : InvalidStoragePathException(locationId, relativePath,
        $"Path '{relativePath}' escapes storage location '{locationId}'.")
{
}

public sealed class SymlinkStoragePathException(StorageLocationId locationId, string relativePath)
    : InvalidStoragePathException(locationId, relativePath,
        $"Path '{relativePath}' for storage location '{locationId}' traverses a symbolic link; symbolic links are not allowed.")
{
}
