using AwesomeAssertions;
using Domain.Libraries;
using Domain.Libraries.Physical;

namespace Domain.Tests.Unit.Libraries.Physical;

public sealed class PhysicalLibraryTests
{
    [Fact]
    public void Create_WithSource_InitializesLibrary()
    {
        var name = new LibraryName("Movies");
        var source = CreateSource("/media/movies");
        var beforeCreation = DateTimeOffset.UtcNow;

        var library = PhysicalLibrary.Create(name, [source]);

        var afterCreation = DateTimeOffset.UtcNow;
        library.Id.Should().NotBeEmpty();
        library.Id.Version.Should().Be(7);
        library.Name.Should().BeSameAs(name);
        library.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        library.CreatedAt.Should().BeOnOrBefore(afterCreation);
        library.UpdatedAt.Should().BeNull();
        library.Sources.Should().ContainSingle()
            .Which.Should().BeSameAs(source);
    }

    [Fact]
    public void Create_ExposesReadOnlySources()
    {
        var library = CreateLibrary();
        var sources = (ICollection<PhysicalLibrarySource>)library.Sources;

        var act = () => sources.Add(CreateSource("/media/tv"));

        act.Should().Throw<NotSupportedException>();
        library.Sources.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithoutSources_ThrowsArgumentException()
    {
        var act = () => PhysicalLibrary.Create(
            new LibraryName("Movies"),
            []);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDuplicatePath_ThrowsInvalidOperationException()
    {
        var first = CreateSource("/media/movies");
        var duplicate = CreateSource(" /media/movies/ ");

        var act = () => PhysicalLibrary.Create(
            new LibraryName("Movies"),
            [first, duplicate]);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rename_WithDifferentName_ChangesNameAndMarksLibraryAsUpdated()
    {
        var library = CreateLibrary();
        var newName = new LibraryName("Favorite Movies");
        var beforeRename = DateTimeOffset.UtcNow;

        library.Rename(newName);

        var afterRename = DateTimeOffset.UtcNow;
        library.Name.Should().BeSameAs(newName);
        library.UpdatedAt.Should().NotBeNull();
        library.UpdatedAt!.Value.Should().BeOnOrAfter(beforeRename);
        library.UpdatedAt.Value.Should().BeOnOrBefore(afterRename);
    }

    [Fact]
    public void Rename_WithEqualName_DoesNotChangeLibrary()
    {
        var originalName = new LibraryName("Movies");
        var library = CreateLibrary(originalName);

        library.Rename(new LibraryName(" Movies "));

        library.Name.Should().BeSameAs(originalName);
        library.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void AddSource_WithUniquePath_AddsSourceAndMarksLibraryAsUpdated()
    {
        var library = CreateLibrary();
        var path = new PhysicalPath("/media/tv");
        var beforeAddition = DateTimeOffset.UtcNow;

        library.AddSource(LibraryMediaKind.TvShows, path);

        var afterAddition = DateTimeOffset.UtcNow;
        library.Sources.Should().HaveCount(2);
        library.Sources.Should().ContainSingle(source =>
            source.MediaKind == LibraryMediaKind.TvShows &&
            source.Path == path);
        library.UpdatedAt.Should().NotBeNull();
        library.UpdatedAt!.Value.Should().BeOnOrAfter(beforeAddition);
        library.UpdatedAt.Value.Should().BeOnOrBefore(afterAddition);
    }

    [Fact]
    public void AddSource_WithDuplicatePath_ThrowsWithoutChangingLibrary()
    {
        var library = CreateLibrary();

        var act = () => library.AddSource(
            LibraryMediaKind.TvShows,
            new PhysicalPath(" /media/movies/ "));

        act.Should().Throw<InvalidOperationException>();
        library.Sources.Should().ContainSingle();
        library.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void RemoveSource_WithExistingSource_RemovesSourceAndMarksLibraryAsUpdated()
    {
        var movies = CreateSource("/media/movies");
        var tvShows = CreateSource("/media/tv", LibraryMediaKind.TvShows);
        var library = PhysicalLibrary.Create(
            new LibraryName("Videos"),
            [movies, tvShows]);
        var beforeRemoval = DateTimeOffset.UtcNow;

        library.RemoveSource(movies);

        var afterRemoval = DateTimeOffset.UtcNow;
        library.Sources.Should().ContainSingle()
            .Which.Should().BeSameAs(tvShows);
        library.UpdatedAt.Should().NotBeNull();
        library.UpdatedAt!.Value.Should().BeOnOrAfter(beforeRemoval);
        library.UpdatedAt.Value.Should().BeOnOrBefore(afterRemoval);
    }

    [Fact]
    public void RemoveSource_WithUnknownSource_ThrowsWithoutChangingLibrary()
    {
        var existing = CreateSource("/media/movies");
        var library = PhysicalLibrary.Create(
            new LibraryName("Movies"),
            [existing, CreateSource("/media/archive")]);

        var act = () => library.RemoveSource(CreateSource("/media/unknown"));

        act.Should().Throw<InvalidOperationException>();
        library.Sources.Should().HaveCount(2);
        library.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void RemoveSource_WithOnlySource_ThrowsWithoutChangingLibrary()
    {
        var source = CreateSource("/media/movies");
        var library = PhysicalLibrary.Create(new LibraryName("Movies"), [source]);

        var act = () => library.RemoveSource(source);

        act.Should().Throw<InvalidOperationException>();
        library.Sources.Should().ContainSingle()
            .Which.Should().BeSameAs(source);
        library.UpdatedAt.Should().BeNull();
    }

    private static PhysicalLibrary CreateLibrary(LibraryName? name = null)
    {
        return PhysicalLibrary.Create(
            name ?? new LibraryName("Movies"),
            [CreateSource("/media/movies")]);
    }

    private static PhysicalLibrarySource CreateSource(
        string path,
        LibraryMediaKind mediaKind = LibraryMediaKind.Movies)
    {
        return new PhysicalLibrarySource(mediaKind, new PhysicalPath(path));
    }
}
