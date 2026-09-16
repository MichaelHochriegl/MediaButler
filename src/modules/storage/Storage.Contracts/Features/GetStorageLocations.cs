namespace Storage.Contracts.Features;

public record GetStorageLocationsResponse(IReadOnlyList<StorageLocationResponse> Locations);

public record StorageLocationResponse(string Id, string DisplayName, bool IsAvailable);
