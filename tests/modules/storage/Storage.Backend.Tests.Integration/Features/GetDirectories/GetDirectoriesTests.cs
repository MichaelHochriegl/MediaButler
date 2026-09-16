using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using FastEndpoints.Testing;
using Storage.Contracts.Features;

namespace Storage.Backend.Tests.Integration.Features.GetDirectories;

public class GetDirectoriesTests(GetDirectoriesAppFixture app) : TestBase<GetDirectoriesAppFixture>
{
    public static bool IsLinux => OperatingSystem.IsLinux();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(".")]
    public async Task Given_LocationRoot_Should_ReturnOnlySortedDirectChildDirectories(string? relativePath)
    {
        // Arrange
        var url = GetBrowseUrl("media", relativePath);

        // Act
        using var response = await app.Client.GetAsync(url, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetDirectoriesResponse>(TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.Directories.Should().Equal(
            new StorageDirectoryResponse(".hidden", ".hidden"),
            new StorageDirectoryResponse("Movies", "Movies"),
            new StorageDirectoryResponse("TV Shows", "TV Shows"));

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.Should().NotContain(app.RootPath);
        using var document = JsonDocument.Parse(json);
        foreach (var directory in document.RootElement.GetProperty("directories").EnumerateArray())
            directory.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo("name", "relativePath");
    }

    [Theory]
    [InlineData("Movies")]
    [InlineData("Movies/./Action/..")]
    public async Task Given_RelativePath_Should_ReturnChildrenWithNormalizedLocationRelativePaths(string relativePath)
    {
        // Arrange
        var url = GetBrowseUrl("media", relativePath);

        // Act
        using var response = await app.Client.GetAsync(url, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetDirectoriesResponse>(TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.Directories.Should().Equal(
            new StorageDirectoryResponse("Action", "Movies/Action"),
            new StorageDirectoryResponse("Drama", "Movies/Drama"));
    }

    [Theory]
    [InlineData("Movies/Drama")]
    [InlineData("TV Shows")]
    public async Task Given_EmptyDirectory_Should_ReturnEmptyList(string relativePath)
    {
        // Arrange
        var url = GetBrowseUrl("media", relativePath);

        // Act
        using var response = await app.Client.GetAsync(url, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetDirectoriesResponse>(TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.Directories.Should().BeEmpty();
    }

    [Theory]
    [InlineData("unknown", null, HttpStatusCode.NotFound, "The storage location is not configured.")]
    [InlineData("media", "missing", HttpStatusCode.NotFound, "The directory does not exist.")]
    [InlineData("media", "movie.mkv", HttpStatusCode.NotFound, "The directory does not exist.")]
    [InlineData("offline", null, HttpStatusCode.ServiceUnavailable, "The storage location is unavailable.")]
    [InlineData("offline", "Movies", HttpStatusCode.ServiceUnavailable, "The storage location is unavailable.")]
    [InlineData("file-root", null, HttpStatusCode.ServiceUnavailable, "The storage location is unavailable.")]
    [InlineData("media", "../outside", HttpStatusCode.BadRequest, "The directory path is invalid")]
    [InlineData("media", "/etc", HttpStatusCode.BadRequest, "The directory path is invalid")]
    [InlineData("media", "Movies/../../outside", HttpStatusCode.BadRequest, "The directory path is invalid")]
    [InlineData("media", "bad\0path", HttpStatusCode.BadRequest, "The directory path is invalid")]
    [InlineData(" media", null, HttpStatusCode.BadRequest, "The storage location ID is invalid.")]
    public async Task Given_InvalidOrUnavailableDirectory_Should_ReturnSafeProblemDetails(
        string locationId, string? relativePath, HttpStatusCode expectedStatus, string expectedMessage)
    {
        // Arrange
        var url = GetBrowseUrl(locationId, relativePath);

        // Act
        using var response = await app.Client.GetAsync(url, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(expectedStatus);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.Should().Contain(expectedMessage).And.NotContain(app.RootPath);
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("status").GetInt32().Should().Be((int)expectedStatus);
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData(256)]
    [InlineData(4096)]
    public async Task Given_OverlongDirectoryPath_Should_ReturnBadRequestWithSafeProblemDetails(int pathLength)
    {
        // Arrange
        var relativePath = new string('a', pathLength);
        var url = GetBrowseUrl("media", relativePath);

        // Act
        using var response = await app.Client.GetAsync(url, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.Should().Contain("The directory path is too long.").And.NotContain(app.RootPath);
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("status").GetInt32().Should().Be(400);
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("media", "external-link", HttpStatusCode.BadRequest)]
    [InlineData("media", "internal-link", HttpStatusCode.BadRequest)]
    [InlineData("media", "broken-link", HttpStatusCode.BadRequest)]
    [InlineData("media", "Movies/nested-link", HttpStatusCode.BadRequest)]
    [InlineData("media", "external-link/../Movies", HttpStatusCode.BadRequest)]
    [InlineData("media", "internal-link/Action", HttpStatusCode.BadRequest)]
    [InlineData("linked-root", null, HttpStatusCode.ServiceUnavailable)]
    public async Task Given_SymlinkDirectory_Should_RejectBrowsing(string locationId, string? relativePath, HttpStatusCode expectedStatus)
    {
        // Arrange
        var url = GetBrowseUrl(locationId, relativePath);

        // Act
        using var response = await app.Client.GetAsync(url, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(expectedStatus);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.Should().NotContain(app.RootPath);
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
            using var initialResponse = await app.Client.GetAsync(
                GetBrowseUrl("changing", null), TestContext.Current.CancellationToken);

            // Act
            if (isAvailable)
                Directory.CreateDirectory(app.ChangingRootPath);
            else
                Directory.Delete(app.ChangingRootPath);

            using var response = await app.Client.GetAsync(
                GetBrowseUrl("changing", null), TestContext.Current.CancellationToken);

            // Assert
            initialResponse.StatusCode.Should().Be(isAvailable ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
            response.StatusCode.Should().Be(isAvailable ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            if (Directory.Exists(app.ChangingRootPath))
                Directory.Delete(app.ChangingRootPath);
        }
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_UnreadableDirectory_Should_DistinguishRootUnavailabilityFromChildAccessDenied()
    {
        // Arrange
        if (!OperatingSystem.IsLinux())
            return;

        Directory.CreateDirectory(app.ChangingRootPath);
        var child = Directory.CreateDirectory(Path.Combine(app.ChangingRootPath, "restricted"));
        var rootMode = File.GetUnixFileMode(app.ChangingRootPath);
        var childMode = File.GetUnixFileMode(child.FullName);
        try
        {
            File.SetUnixFileMode(child.FullName, UnixFileMode.None);
            // Privileged test runners may bypass filesystem permissions.
            var canRead = true;
            try
            {
                Directory.GetFileSystemEntries(child.FullName);
            }
            catch (UnauthorizedAccessException)
            {
                canRead = false;
            }
            Assert.SkipWhen(canRead, "The test runner bypasses filesystem permissions.");

            // Act
            using var response = await app.Client.GetAsync(
                GetBrowseUrl("changing", "restricted"), TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
                .Should().Contain("The directory cannot be read.").And.NotContain(app.ChangingRootPath);

            // Arrange
            File.SetUnixFileMode(app.ChangingRootPath, UnixFileMode.None);

            // Act
            using var rootResponse = await app.Client.GetAsync(
                GetBrowseUrl("changing", null), TestContext.Current.CancellationToken);

            // Assert
            rootResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            File.SetUnixFileMode(app.ChangingRootPath, rootMode);
            File.SetUnixFileMode(child.FullName, childMode);
            Directory.Delete(app.ChangingRootPath, recursive: true);
        }
    }

    private static string GetBrowseUrl(string locationId, string? relativePath)
    {
        var url = $"/api/storage/locations/{Uri.EscapeDataString(locationId)}/directories";
        if (relativePath is not null)
            url += $"?relativePath={Uri.EscapeDataString(relativePath)}";
        return url;
    }
}
