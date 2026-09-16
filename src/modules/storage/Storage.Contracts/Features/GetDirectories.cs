namespace Storage.Contracts.Features;

/// <param name="LocationId">The configured storage location ID.</param>
/// <param name="RelativePath">A location-relative path; omitted or empty selects the root.</param>
public record GetDirectoriesRequest(string LocationId, string? RelativePath = null);

public record GetDirectoriesResponse(IReadOnlyList<StorageDirectoryResponse> Directories);

public record StorageDirectoryResponse(string Name, string RelativePath);
