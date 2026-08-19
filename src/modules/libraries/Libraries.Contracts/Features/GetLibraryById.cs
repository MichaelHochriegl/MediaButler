namespace Libraries.Contracts.Features;

/// <summary>
/// Request used to retrieve a library by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the library to retrieve.</param>
public record GetLibraryByIdRequest(Guid Id);

/// <summary>
/// Response returned when a library is found.
/// </summary>
/// <param name="Id">The unique identifier of the library.</param>
/// <param name="Name">The display name of the library.</param>
/// <param name="CreatedAt">The date and time when the library was created.</param>
/// <param name="UpdatedAt">The date and time when the library was last updated, if it has been updated.</param>
public record GetLibraryByIdResponse(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
