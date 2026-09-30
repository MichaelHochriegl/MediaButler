using FastEndpoints;
using Storage.Contracts.Features;
using Storage.Integration;

namespace Storage.Backend.Features;

public class GetStorageLocationsEndpoint(IStorageDirectoryResolver resolver)
    : EndpointWithoutRequest<GetStorageLocationsResponse>
{
    public override void Configure()
    {
        Get("/storage/locations");
        Summary(s =>
        {
            s.Summary = "List configured storage locations";
            s.Response<GetStorageLocationsResponse>(200, "Location IDs, display names, and current availability.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var locations = new List<StorageLocationResponse>();
        foreach (var location in Resolve<StorageLocationRegistry>().ConfiguredLocations)
        {
            ct.ThrowIfCancellationRequested();
            bool isLocationAvailable;
            try
            {
                resolver.ResolveDirectory(location.Id, ".");
                isLocationAvailable = true;
            }
            catch (Exception exception) when (exception is InvalidStoragePathException
                                              or StorageDirectoryNotFoundException
                                              or StorageDirectoryNotAccessibleException)
            {
                isLocationAvailable = false;
            }

            locations.Add(new StorageLocationResponse(location.Id.Value, location.DisplayName, isLocationAvailable));
        }

        await Send.OkAsync(new GetStorageLocationsResponse(locations), ct);
    }
}
