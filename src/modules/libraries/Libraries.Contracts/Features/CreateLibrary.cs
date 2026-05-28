namespace Libraries.Contracts.Features;

public record CreateLibraryRequest(string Name, LibraryKind Kind, IEnumerable<Source> Sources);

public record CreateLibraryResponse(Guid Id);

public enum LibraryKind
{
    Physical = 1,
    Virtual = 2,
}

public enum MediaKind
{
    Movies = 1,
    TvShows = 2,
}

public record Source(string Path, MediaKind Kind);