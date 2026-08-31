using AwesomeAssertions;
using Domain.Libraries;
using Domain.Libraries.Physical;

namespace Domain.Tests.Unit.Libraries.Physical;

public sealed class PhysicalLibrarySourceTests
{
    [Fact]
    public void Constructor_InitializesSource()
    {
        var path = new PhysicalPath("/media/movies");
        var beforeCreation = DateTimeOffset.UtcNow;

        var source = new PhysicalLibrarySource(LibraryMediaKind.Movies, path);

        var afterCreation = DateTimeOffset.UtcNow;
        source.Id.Should().NotBeEmpty();
        source.Id.Version.Should().Be(7);
        source.MediaKind.Should().Be(LibraryMediaKind.Movies);
        source.Path.Should().BeSameAs(path);
        source.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        source.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }
}
