using AwesomeAssertions;
using Domain.Libraries.Physical;

namespace Domain.Tests.Unit.Libraries.Physical;

public sealed class PhysicalPathTests
{
    [Fact]
    public void Constructor_NormalizesValue()
    {
        var separator = Path.DirectorySeparatorChar;
        var value = $"  {separator}media{separator}movies{separator}  ";

        var path = new PhysicalPath(value);

        path.Value.Should().Be($"{separator}media{separator}movies");
    }

    [Fact]
    public void Constructor_AcceptsValueAtMaximumLength()
    {
        var value = new string('a', 1024);

        var path = new PhysicalPath(value);

        path.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithEmptyValue_ThrowsArgumentException(string? value)
    {
        var act = () => new PhysicalPath(value!);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }

    [Fact]
    public void Constructor_WithValueOverMaximumLength_ThrowsArgumentException()
    {
        var value = new string('a', 1025);

        var act = () => new PhysicalPath(value);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }

    [Fact]
    public void Constructor_WithNullCharacter_ThrowsArgumentException()
    {
        var value = $"media{Path.DirectorySeparatorChar}mov\0ies";

        var act = () => new PhysicalPath(value);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        var separator = Path.DirectorySeparatorChar;
        var path = new PhysicalPath($"media{separator}movies{separator}");

        path.ToString().Should().Be($"media{separator}movies");
    }
}
