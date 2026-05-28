namespace Libraries.Contracts.Features;

public record GetLibraryByIdRequest(Guid Id);

public record GetLibraryByIdResponse(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);