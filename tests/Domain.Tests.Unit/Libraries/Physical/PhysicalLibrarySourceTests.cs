using AwesomeAssertions;
using Domain.Libraries;
using Domain.Libraries.Physical;
using Domain.Storage;

namespace Domain.Tests.Unit.Libraries.Physical;

public sealed class PhysicalLibrarySourceTests
{
    [Fact]
    public void Constructor_InitializesSource()
    {
        var storageLocationId = new StorageLocationId("media");
        var storageRelativePath = new StorageRelativePath("movies");
        var beforeCreation = DateTimeOffset.UtcNow;

        var source = new PhysicalLibrarySource(LibraryMediaKind.Movies, storageLocationId, storageRelativePath);

        var afterCreation = DateTimeOffset.UtcNow;
        source.Id.Should().NotBeEmpty();
        source.Id.Version.Should().Be(7);
        source.MediaKind.Should().Be(LibraryMediaKind.Movies);
        source.StorageLocationId.Should().Be(storageLocationId);
        source.RelativePath.Should().Be(storageRelativePath);
        source.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        source.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }
}
