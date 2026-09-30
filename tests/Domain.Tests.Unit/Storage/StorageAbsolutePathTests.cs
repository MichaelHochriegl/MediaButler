using AwesomeAssertions;
using Domain.Storage;

namespace Domain.Tests.Unit.Storage;

// Canonical Linux path syntax only. Storage owns filesystem and containment checks.
public sealed class StorageAbsolutePathTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/media")]
    [InlineData("/media/Movies/Action")]
    [InlineData("/media/.hidden")]
    [InlineData("/media/Movies..Old")]
    [InlineData("/media/.../Movies")]
    [InlineData("/media/Movies With Spaces/Amélie")]
    [InlineData("/media/ Movies ")]
    [InlineData("/media/ /Movies")]
    [InlineData("/media/Movies\\Action")]
    [InlineData("/media/..\\outside")]
    [InlineData("/media/C:\\Movies")]
    [InlineData("/media/Movies:Archive")]
    public void Constructor_WithCanonicalLinuxPath_PreservesValue(string value)
    {
        var path = new StorageAbsolutePath(value);

        path.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("Movies")]
    [InlineData("media/Movies")]
    [InlineData("./Movies")]
    [InlineData("../Movies")]
    [InlineData("~/Movies")]
    [InlineData(" /media/Movies")]
    [InlineData("C:\\Movies")]
    [InlineData("C:/Movies")]
    [InlineData("\\Movies")]
    [InlineData("\\\\server\\share")]
    public void Constructor_WithNonabsoluteLinuxPath_ThrowsArgumentException(string? value)
    {
        var act = () => new StorageAbsolutePath(value!);

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Theory]
    [InlineData("//")]
    [InlineData("//media/Movies")]
    [InlineData("/media/")]
    [InlineData("/media//Movies")]
    [InlineData("/media///Movies")]
    [InlineData("/.")]
    [InlineData("/..")]
    [InlineData("/./media")]
    [InlineData("/media/.")]
    [InlineData("/media/./Movies")]
    [InlineData("/media/..")]
    [InlineData("/media/../Movies")]
    [InlineData("/media/link/../Movies")]
    [InlineData("/media/../../outside")]
    public void Constructor_WithNoncanonicalAbsolutePath_RejectsRatherThanNormalizes(string value)
    {
        var act = () => new StorageAbsolutePath(value);

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Theory]
    [InlineData("/\0")]
    [InlineData("/media/Mov\0ies")]
    public void Constructor_WithNullCharacter_ThrowsArgumentException(string value)
    {
        var act = () => new StorageAbsolutePath(value);

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Constructor_AtMaximumLengthIncludingLeadingSlash_AcceptsValue()
    {
        var value = "/" + new string('a', 1023);

        var path = new StorageAbsolutePath(value);

        path.Value.Should().Be(value);
    }

    [Fact]
    public void Constructor_OverMaximumLengthIncludingLeadingSlash_ThrowsArgumentException()
    {
        var act = () => new StorageAbsolutePath("/" + new string('a', 1024));

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Constructor_WithoutFilesystemSetup_AcceptsSyntacticallyValidPath()
    {
        var value = $"/media-butler-domain-tests-{Guid.NewGuid():N}/missing/Movies";

        var path = new StorageAbsolutePath(value);

        path.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/media/Movies")]
    [InlineData("/media/ Movies ")]
    public void ToString_ReturnsPathValue(string value)
    {
        var path = new StorageAbsolutePath(value);

        path.ToString().Should().Be(value);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/media/Movies")]
    public void Equality_WithIdenticalPaths_UsesValueEqualityAndConsistentHashCodes(string value)
    {
        var first = new StorageAbsolutePath(value);
        var second = new StorageAbsolutePath(new string(value.ToCharArray()));

        first.Equals(second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
        new HashSet<StorageAbsolutePath> { first, second }.Should().ContainSingle();
    }

    [Theory]
    [InlineData("/media/Movies", "/media/movies")]
    [InlineData("/media/Movies", "/MEDIA/Movies")]
    [InlineData("/media/Movies", "/media/ Movies")]
    [InlineData("/media/Movies", "/media/Movies ")]
    [InlineData("/media/Movies/Action", "/media/Movies\\Action")]
    [InlineData("/media/Movies", "/media/Movies/Action")]
    [InlineData("/media/Amélie", "/media/Ame\u0301lie")]
    public void Equality_WithDistinctLinuxPaths_KeepsThemDistinct(string left, string right)
    {
        var first = new StorageAbsolutePath(left);
        var second = new StorageAbsolutePath(right);

        first.Equals(second).Should().BeFalse();
        new HashSet<StorageAbsolutePath> { first, second }.Should().HaveCount(2);
    }
}
