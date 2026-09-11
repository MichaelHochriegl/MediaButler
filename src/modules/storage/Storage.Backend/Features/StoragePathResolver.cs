using Domain.Storage;
using Storage.Integration;

namespace Storage.Backend.Features;

internal sealed class StoragePathResolver(StorageLocationRegistry locationRegistry) : IStoragePathResolver
{
    public string ResolvePath(StorageLocationId locationId, string relativePath)
    {
        if (!locationRegistry.TryGet(locationId, out var location))
        {
            throw new StorageLocationNotFoundException(locationId);
        }

        if (!IsRelativePath(relativePath))
        {
            throw new RootedStoragePathException(locationId, relativePath);
        }

        string resolvedPath;
        try
        {
            resolvedPath = Path.GetFullPath(relativePath, location.RootPath);
            
            if (!IsContainedWithin(location.RootPath, resolvedPath))
            {
                throw new EscapingStoragePathException(locationId, relativePath);
            }

            if (!IsSymlinkFree(location, relativePath))
            {
                throw new SymlinkStoragePathException(location.Id, relativePath);
            }
        }
        catch (Exception exception)
            when(exception is ArgumentException or PathTooLongException)
        {
            throw new InvalidStoragePathException(locationId, relativePath, exception);
        }


        return resolvedPath;
    }

    private static bool IsContainedWithin(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);

        return relative != ".."
               && !relative.StartsWith(
                   $"..{Path.DirectorySeparatorChar}",
                   StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
    }

    private static bool IsRelativePath(string relativePath)
    {
        return !Path.IsPathRooted(relativePath);
    }

    private static bool IsSymlinkFree(ConfiguredStorageLocation location, string relativePath)
    {
        var fullPath = Path.Combine(location.RootPath, relativePath);

        var segments = fullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        var path = Path.DirectorySeparatorChar.ToString();
        foreach (var pathSegment in segments)
        {
            path = Path.GetFullPath(pathSegment, path);
            if (IsSymlink(path)) return false;
        }

        return true;
    }

    private static bool IsSymlink(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.LinkTarget is not null;
    }
}
