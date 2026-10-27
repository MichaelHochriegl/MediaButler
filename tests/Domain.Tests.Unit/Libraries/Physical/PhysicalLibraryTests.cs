using AwesomeAssertions;
using Domain.Libraries;
using Domain.Libraries.Physical;
using Domain.Storage;

namespace Domain.Tests.Unit.Libraries.Physical;

public sealed class PhysicalLibraryTests
{
    [Fact]
    public void Create_WithSource_InitializesLibrary()
    {
        var name = new LibraryName("Movies");
        var source = CreateSource("movies");
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

        var act = () => sources.Add(CreateSource("tv"));

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

    [Theory]
    [InlineData("movies", "movies")]
    [InlineData("movies", "movies/action")]
    [InlineData("movies/action", "movies")]
    public void Create_WithOverlappingSource_ThrowsDomainException(string firstPath, string secondPath)
    {
        var first = CreateSource(firstPath);
        var duplicate = CreateSource(secondPath);

        var act = () => PhysicalLibrary.Create(
            new LibraryName("Movies"),
            [first, duplicate]);

        var exception = act.Should().ThrowExactly<OverlappingPhysicalLibrarySourceException>().Which;
        exception.StorageLocationId.Should().Be(duplicate.StorageLocationId);
        exception.RelativePath.Should().Be(duplicate.RelativePath);
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
        var storageLocationId = new StorageLocationId("media");
        var storageRelativePath = new StorageRelativePath("tv");
        var beforeAddition = DateTimeOffset.UtcNow;

        library.AddSource(LibraryMediaKind.TvShows, storageLocationId, storageRelativePath);

        var afterAddition = DateTimeOffset.UtcNow;
        library.Sources.Should().HaveCount(2);
        library.Sources.Should().ContainSingle(source =>
            source.MediaKind == LibraryMediaKind.TvShows &&
            source.StorageLocationId == storageLocationId &&
            source.RelativePath == storageRelativePath);
        library.UpdatedAt.Should().NotBeNull();
        library.UpdatedAt!.Value.Should().BeOnOrAfter(beforeAddition);
        library.UpdatedAt.Value.Should().BeOnOrBefore(afterAddition);
    }

    [Theory]
    [InlineData("movies")]
    [InlineData("movies/action")]
    [InlineData(".")]
    public void AddSource_WithOverlappingPath_ThrowsWithoutChangingLibrary(string relativePath)
    {
        var library = CreateLibrary();
        var storageLocationId = new StorageLocationId("media");
        var storageRelativePath = new StorageRelativePath(relativePath);

        var act = () => library.AddSource(
            LibraryMediaKind.TvShows, storageLocationId, storageRelativePath);

        var exception = act.Should().ThrowExactly<OverlappingPhysicalLibrarySourceException>().Which;
        exception.StorageLocationId.Should().Be(storageLocationId);
        exception.RelativePath.Should().Be(storageRelativePath);
        library.Sources.Should().ContainSingle();
        library.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void RemoveSource_WithExistingSource_RemovesSourceAndMarksLibraryAsUpdated()
    {
        var movies = CreateSource("movies");
        var tvShows = CreateSource("tv", LibraryMediaKind.TvShows);
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
        var existing = CreateSource("movies");
        var library = PhysicalLibrary.Create(
            new LibraryName("Movies"),
            [existing, CreateSource("archive")]);

        var act = () => library.RemoveSource(CreateSource("unknown"));

        act.Should().Throw<InvalidOperationException>();
        library.Sources.Should().HaveCount(2);
        library.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void RemoveSource_WithOnlySource_ThrowsWithoutChangingLibrary()
    {
        var source = CreateSource("movies");
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
            [CreateSource("movies")]);
    }

    private static PhysicalLibrarySource CreateSource(
        string relativePath,
        LibraryMediaKind mediaKind = LibraryMediaKind.Movies)
    {
        return new PhysicalLibrarySource(mediaKind, new StorageLocationId("media"), new StorageRelativePath(relativePath));
    }
}
