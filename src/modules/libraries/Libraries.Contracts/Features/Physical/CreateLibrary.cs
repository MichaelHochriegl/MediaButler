using FluentValidation;

namespace Libraries.Contracts.Features.Physical;

/// <summary>
/// Request body used to create a new media library.
/// </summary>
/// <param name="Name">The unique display name for the library.</param>
/// <param name="Sources">The physical source paths to include in the library, each paired with a media kind.</param>
public record CreatePhysicalLibraryRequest(string Name, IEnumerable<CreatePhysicalLibrarySource> Sources);

/// <summary>
/// Response returned after a library is created.
/// </summary>
/// <param name="Id">The unique identifier of the created library.</param>
public record CreatePhysicalLibraryResponse(Guid Id);

/// <summary>
/// The type of media contained by a library source.
/// </summary>
public enum MediaKind
{
    /// <summary>
    /// Movie media.
    /// </summary>
    Movies = 1,

    /// <summary>
    /// Television show media.
    /// </summary>
    TvShows = 2,
}

/// <summary>
/// Physical source configuration for a library.
/// </summary>
/// <param name="Path">The physical file system path for this source.</param>
/// <param name="Kind">The media kind available from this source.</param>
public record CreatePhysicalLibrarySource(string Path, MediaKind Kind);


public class CreatePhysicalLibraryRequestValidator : AbstractValidator<CreatePhysicalLibraryRequest>
{
    public CreatePhysicalLibraryRequestValidator()
    {
        RuleFor(x => x.Sources).NotEmpty();
    }
}

public class CreatePhysicalLibrarySourceValidator : AbstractValidator<CreatePhysicalLibrarySource>
{
    public CreatePhysicalLibrarySourceValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty();
    }
}