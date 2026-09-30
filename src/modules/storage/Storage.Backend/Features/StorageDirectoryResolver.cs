using Domain.Storage;
using Storage.Integration;

namespace Storage.Backend.Features;

internal class StorageDirectoryResolver(IStoragePathResolver pathResolver) : IStorageDirectoryResolver
{
    public ResolvedStoragePath ResolveDirectory(StorageLocationId locationId, string relativePath)
    {
        var resolvedPath =
            pathResolver.ResolvePath(locationId, relativePath);

        EnsureReadableDirectory(resolvedPath);

        return resolvedPath;
    }

    private static void EnsureReadableDirectory(
        ResolvedStoragePath path)
    {
        try
        {
            using var enumerator =
                Directory
                    .EnumerateFileSystemEntries(path.AbsolutePath.Value)
                    .GetEnumerator();

            // Actually opening/advancing the enumeration checks
            // that the directory can be read without enumerating
            // the complete directory.
            enumerator.MoveNext();
        }
        catch (DirectoryNotFoundException exception)
        {
            throw new StorageDirectoryNotFoundException(
                path.LocationId,
                path.RelativePath,
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new StorageDirectoryNotAccessibleException(
                path.LocationId,
                path.RelativePath,
                exception);
        }
        catch (IOException exception) when (exception is not PathTooLongException)
        {
            throw new StorageDirectoryNotAccessibleException(
                path.LocationId,
                path.RelativePath,
                exception);
        }
    }
}