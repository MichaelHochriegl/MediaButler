using AwesomeAssertions;
using Domain.Storage;

namespace Domain.Tests.Unit.Storage;

public sealed class StorageLocationIdTests
{
    [Fact]
    public void Constructor_WithValidValue_PreservesValue()
    {
        // Arrange
        const string value = "primary-media";

        // Act
        var storageLocationId = new StorageLocationId(value);

        // Assert
        storageLocationId.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithEmptyValue_ThrowsArgumentException(string? value)
    {
        // Act
        var act = () => new StorageLocationId(value!);

        // Assert
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
        // Act
        var act = () => new StorageLocationId(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(value));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var storageLocationId = new StorageLocationId("primary-media");

        // Act & Assert
        storageLocationId.ToString().Should().Be(storageLocationId.Value);
    }
}