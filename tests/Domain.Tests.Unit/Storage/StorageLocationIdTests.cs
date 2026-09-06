using AwesomeAssertions;
using Domain.Storage;

namespace Domain.Tests.Unit.Storage;

public sealed class StorageLocationIdTests
{
    [Fact]
    public void Constructor_WithValidValue_PreservesValue()
    {
        const string value = "primary-media";

        var storageLocationId = new StorageLocationId(value);

        storageLocationId.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithEmptyValue_ThrowsArgumentException(string? value)
    {
        var act = () => new StorageLocationId(value!);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }

    [Theory]
    [InlineData(" primary-media")]
    [InlineData("primary-media ")]
    [InlineData("\tprimary-media")]
    [InlineData("primary-media\r\n")]
    public void Constructor_WithLeadingOrTrailingWhitespace_ThrowsArgumentException(string value)
    {
        var act = () => new StorageLocationId(value);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var storageLocationId = new StorageLocationId("primary-media");

        storageLocationId.ToString().Should().Be(storageLocationId.Value);
    }
}