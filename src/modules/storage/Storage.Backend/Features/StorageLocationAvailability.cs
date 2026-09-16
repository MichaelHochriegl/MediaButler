using Domain.Storage;
using Storage.Integration;

namespace Storage.Backend.Features;

internal static class StorageLocationAvailability
{
    public static bool IsAvailable(IStoragePathResolver resolver, StorageLocationId locationId)
    {
        try
        {
            var rootPath = resolver.ResolvePath(locationId, ".");
            // Opening the enumeration also verifies read access without scanning the location.
            using var entries = Directory.EnumerateFileSystemEntries(rootPath).GetEnumerator();
            entries.MoveNext();
            return true;
        }
        catch (Exception exception) when (exception is InvalidStoragePathException or IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
