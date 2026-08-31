using AwesomeAssertions;
using Domain.Libraries;

namespace Domain.Tests.Unit.Libraries;

public sealed class LibraryNameTests
{
    [Fact]
    public void Constructor_TrimsValue()
    {
        var name = new LibraryName("  Movies  ");

        name.Value.Should().Be("Movies");
    }

    [Fact]
    public void Constructor_AcceptsValueAtMaximumLength()
    {
        var value = new string('a', LibraryName.MaxLength);

        var name = new LibraryName(value);

        name.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithEmptyValue_ThrowsArgumentException(string? value)
    {
        var act = () => new LibraryName(value!);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }

    [Fact]
    public void Constructor_WithValueOverMaximumLength_ThrowsArgumentException()
    {
        var value = new string('a', LibraryName.MaxLength + 1);

        var act = () => new LibraryName(value);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }
}
