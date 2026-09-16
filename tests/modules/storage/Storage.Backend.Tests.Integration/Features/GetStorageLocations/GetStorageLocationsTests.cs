using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using FastEndpoints.Testing;
using Storage.Contracts.Features;

namespace Storage.Backend.Tests.Integration.Features.GetStorageLocations;

public class GetStorageLocationsTests(GetStorageLocationsAppFixture app) : TestBase<GetStorageLocationsAppFixture>
{
    public static bool IsLinux => OperatingSystem.IsLinux();

    [Fact]
    public async Task Given_ConfiguredLocations_Should_ReturnNamesAndAvailabilityWithoutRootPaths()
    {
        // Arrange
        const string url = "/api/storage/locations";

        // Act
        using var response = await app.Client.GetAsync(url, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetStorageLocationsResponse>(TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.Locations.Should().Contain(new StorageLocationResponse("media", "Media library", true));
        result.Locations.Should().Contain(new StorageLocationResponse("offline", "Offline storage", false));
        result.Locations.Should().Contain(new StorageLocationResponse("file-root", "Invalid mount", false));
        if (IsLinux)
            result.Locations.Should().Contain(new StorageLocationResponse("linked-root", "Linked storage", false));

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.Should().NotContain(app.RootPath);
        using var document = JsonDocument.Parse(json);
        foreach (var location in document.RootElement.GetProperty("locations").EnumerateArray())
            location.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo("id", "displayName", "isAvailable");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_LocationAvailabilityChanges_Should_ReportCurrentAvailability(bool isAvailable)
    {
        // Arrange
        if (!isAvailable)
            Directory.CreateDirectory(app.ChangingRootPath);

        try
        {
            var initialResult = await app.Client.GetFromJsonAsync<GetStorageLocationsResponse>(
                "/api/storage/locations", TestContext.Current.CancellationToken);

            // Act
            if (isAvailable)
                Directory.CreateDirectory(app.ChangingRootPath);
            else
                Directory.Delete(app.ChangingRootPath);

            var result = await app.Client.GetFromJsonAsync<GetStorageLocationsResponse>(
                "/api/storage/locations", TestContext.Current.CancellationToken);

            // Assert
            initialResult.Should().NotBeNull();
            initialResult.Locations.Should().Contain(new StorageLocationResponse("changing", "Removable storage", !isAvailable));
            result.Should().NotBeNull();
            result.Locations.Should().Contain(new StorageLocationResponse("changing", "Removable storage", isAvailable));
        }
        finally
        {
            if (Directory.Exists(app.ChangingRootPath))
                Directory.Delete(app.ChangingRootPath);
        }
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_UnreadableLocation_Should_ReportUnavailable()
    {
        // Arrange
        if (!OperatingSystem.IsLinux())
            return;

        Directory.CreateDirectory(app.ChangingRootPath);
        var mode = File.GetUnixFileMode(app.ChangingRootPath);
        try
        {
            File.SetUnixFileMode(app.ChangingRootPath, UnixFileMode.None);
            // Privileged test runners may bypass filesystem permissions.
            var canRead = true;
            try
            {
                Directory.GetFileSystemEntries(app.ChangingRootPath);
            }
            catch (UnauthorizedAccessException)
            {
                canRead = false;
            }
            Assert.SkipWhen(canRead, "The test runner bypasses filesystem permissions.");

            // Act
            var result = await app.Client.GetFromJsonAsync<GetStorageLocationsResponse>(
                "/api/storage/locations", TestContext.Current.CancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Locations.Should().Contain(new StorageLocationResponse("changing", "Removable storage", false));
        }
        finally
        {
            File.SetUnixFileMode(app.ChangingRootPath, mode);
            Directory.Delete(app.ChangingRootPath);
        }
    }
}
