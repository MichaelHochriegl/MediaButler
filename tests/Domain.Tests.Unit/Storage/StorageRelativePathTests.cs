using AwesomeAssertions;
using Domain.Storage;

namespace Domain.Tests.Unit.Storage;

// This value stores canonical Linux-relative paths. Storage validates raw input
// (including symlinks) before constructing it; this type performs no filesystem I/O.
public sealed class StorageRelativePathTests
{
    [Theory]
    [InlineData(".")]
    [InlineData("Movies")]
    [InlineData("Movies/Action")]
    [InlineData("Movies/Action/Nested")]
    [InlineData(".hidden/Movies")]
    [InlineData("Movies..Old/Action")]
    [InlineData(".../Movies")]
    [InlineData("Movies With Spaces/Amélie")]
    [InlineData(" Movies ")]
    [InlineData("Movies/ Action ")]
    [InlineData("Movies\\Action")]
    [InlineData("..\\outside")]
    [InlineData("C:\\Movies")]
    [InlineData("Movies:Archive")]
    public void Constructor_WithCanonicalLinuxPath_PreservesValue(string value)
    {
        var path = new StorageRelativePath(value);

        path.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    [InlineData("/")]
    [InlineData("/Movies")]
    [InlineData("//media/Movies")]
    [InlineData("Movies/")]
    [InlineData("Movies//Action")]
    [InlineData("Movies///Action")]
    [InlineData("./Movies")]
    [InlineData("Movies/.")]
    [InlineData("Movies/./Action")]
    [InlineData("./")]
    [InlineData("..")]
    [InlineData("../Movies")]
    [InlineData("Movies/..")]
    [InlineData("Movies/../Shows")]
    [InlineData("Movies/../../outside")]
    [InlineData("link/../Movies")]
    [InlineData("Movies\0/Action")]
    public void Constructor_WithInvalidOrNoncanonicalPath_ThrowsArgumentException(string? value)
    {
        var act = () => new StorageRelativePath(value!);

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Constructor_AtMaximumLength_AcceptsValue()
    {
        var value = new string('a', 1024);

        var path = new StorageRelativePath(value);

        path.Value.Should().Be(value);
    }

    [Fact]
    public void Constructor_OverMaximumLength_ThrowsArgumentException()
    {
        var act = () => new StorageRelativePath(new string('a', 1025));

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Theory]
    [InlineData(".")]
    [InlineData("Movies/Action")]
    public void ToString_ReturnsPathValue(string value)
    {
        var path = new StorageRelativePath(value);

        path.ToString().Should().Be(value);
    }

    [Fact]
    public void Equality_WithIdenticalPaths_UsesValueEqualityAndConsistentHashCodes()
    {
        var first = new StorageRelativePath("Movies/Action");
        var second = new StorageRelativePath("Movies/Action");

        first.Equals(second).Should().BeTrue();
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
        first.GetHashCode().Should().Be(second.GetHashCode());
        new HashSet<StorageRelativePath> { first, second }.Should().ContainSingle();
    }

    [Theory]
    [InlineData("Movies", "movies")]
    [InlineData("Movies", " Movies")]
    [InlineData("Movies", "Movies ")]
    [InlineData("Movies/Action", "Movies\\Action")]
    [InlineData("Movies", "Movies/Action")]
    [InlineData("Amélie", "Ame\u0301lie")]
    public void Equality_WithDistinctLinuxPaths_KeepsThemDistinct(string left, string right)
    {
        var first = new StorageRelativePath(left);
        var second = new StorageRelativePath(right);

        first.Equals(second).Should().BeFalse();
        (first == second).Should().BeFalse();
        (first != second).Should().BeTrue();
        new HashSet<StorageRelativePath> { first, second }.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(".", ".", true)]
    [InlineData(".", "Movies", true)]
    [InlineData(".", "Movies/Action", true)]
    [InlineData("Movies", "Movies", true)]
    [InlineData("Movies", "Movies/Action", true)]
    [InlineData("Movies", "Movies/Action/Nested", true)]
    [InlineData("Movies/Action", "Movies/Action/Nested", true)]
    [InlineData("Movies", "TV Shows", false)]
    [InlineData("Movies", "Movies-Archive", false)]
    [InlineData("Movies", "Movies2/Action", false)]
    [InlineData("Movies/Action", "Movies/Actionable", false)]
    [InlineData("Movies/Action", "Movies/Drama", false)]
    [InlineData("Movies", "movies/Action", false)]
    [InlineData("Movies", "Movies\\Action", false)]
    [InlineData("Movies", "Movies /Action", false)]
    [InlineData(".hidden", ".hidden/Movies", true)]
    [InlineData("Movies\\Action", "Movies\\Action/Nested", true)]
    public void Overlaps_ComparesWholeSegmentsInEitherDirection(string left, string right, bool expected)
    {
        var first = new StorageRelativePath(left);
        var second = new StorageRelativePath(right);

        first.Overlaps(second).Should().Be(expected);
        second.Overlaps(first).Should().Be(expected);
    }
}
