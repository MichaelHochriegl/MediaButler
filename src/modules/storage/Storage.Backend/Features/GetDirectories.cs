using Domain.Storage;
using FastEndpoints;
using Storage.Contracts.Features;
using Storage.Integration;

namespace Storage.Backend.Features;

public class GetDirectoriesEndpoint(IStoragePathResolver resolver)
    : Endpoint<GetDirectoriesRequest, GetDirectoriesResponse>
{
    public override void Configure()
    {
        Get("/storage/locations/{locationId}/directories");
        Summary(s =>
        {
            s.Summary = "Browse direct child directories in a storage location";
            s.Description = "Omit relativePath to browse the root. Symbolic links and files are excluded. " +
                            "Results are sorted by name using ordinal ordering.";
            s.Response<GetDirectoriesResponse>(200, "Directory names and location-relative paths.");
            s.Response(400, "The location ID or relative path is invalid or unsafe.");
            s.Response(403, "The requested directory cannot be read.");
            s.Response(404, "The location is unknown or the directory does not exist.");
            s.Response(503, "The configured location or filesystem is unavailable.");
        });
    }

    public override async Task HandleAsync(GetDirectoriesRequest req, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        StorageLocationId locationId;
        try
        {
            locationId = new StorageLocationId(req.LocationId);
        }
        catch (ArgumentException)
        {
            ThrowError(r => r.LocationId, "The storage location ID is invalid.");
            return;
        }

        if (!Resolve<StorageLocationRegistry>().TryGet(locationId, out _))
        {
            ThrowError(r => r.LocationId, "The storage location is not configured.", 404);
            return;
        }

        if (!StorageLocationAvailability.IsAvailable(resolver, locationId))
        {
            ThrowError("The storage location is unavailable.", 503);
            return;
        }

        try
        {
            var rootPath = resolver.ResolvePath(locationId, ".");
            var path = resolver.ResolvePath(locationId,
                string.IsNullOrEmpty(req.RelativePath) ? "." : req.RelativePath);
            var directories = new List<StorageDirectoryResponse>();
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.ReparsePoint
            };

            foreach (var child in Directory.EnumerateDirectories(path, "*", options))
            {
                ct.ThrowIfCancellationRequested();
                directories.Add(new StorageDirectoryResponse(Path.GetFileName(child),
                    Path.GetRelativePath(rootPath, child)));
            }

            directories.Sort((left, right) => StringComparer.Ordinal.Compare(left.Name, right.Name));
            await Send.OkAsync(new GetDirectoriesResponse(directories), ct);
        }
        catch (InvalidStoragePathException)
        {
            ThrowError(r => r.RelativePath, "The directory path is invalid or traverses a symbolic link.");
        }
        catch (DirectoryNotFoundException)
        {
            if (StorageLocationAvailability.IsAvailable(resolver, locationId))
                ThrowError(r => r.RelativePath, "The directory does not exist.", 404);
            else
                ThrowError("The storage location is unavailable.", 503);
        }
        catch (UnauthorizedAccessException)
        {
            ThrowError("The directory cannot be read.", 403);
        }
        catch (PathTooLongException)
        {
            ThrowError("The directory path is too long.", 400);
        }
        catch (IOException)
        {
            ThrowError("The filesystem is unavailable.", 503);
        }
    }
}
