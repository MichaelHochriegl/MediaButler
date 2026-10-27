using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Storage.Backend.Tests.Integration.Configuration;

public class StorageConfigurationTests
{
    [Fact]
    public async Task Given_EmptyConfiguration_Should_Start()
    {
        // Act
        var startHost = () => StartHostAsync([]);

        // Assert
        await startHost.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_ValidLocation_Should_Start()
    {
        // Arrange
        var configuration = Location("media", "Media", "/media");

        // Act
        var startHost = () => StartHostAsync(configuration);

        // Assert
        await startHost.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_SeveralValidLocations_Should_Start()
    {
        // Arrange
        var configuration = new Dictionary<string, string?>
        {
            ["Storage:Locations:movies:DisplayName"] = "Movies",
            ["Storage:Locations:movies:RootPath"] = "/media/movies",
            ["Storage:Locations:shows:DisplayName"] = "TV Shows",
            ["Storage:Locations:shows:RootPath"] = "/media/shows",
            ["Storage:Locations:music:DisplayName"] = "Music",
            ["Storage:Locations:music:RootPath"] = "/media/music"
        };

        // Act
        var startHost = () => StartHostAsync(configuration);

        // Assert
        await startHost.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_RelativeRootPath_Should_RejectStartup()
    {
        // Arrange
        var configuration = Location("media", "Media", "media");

        // Act
        var startHost = () => StartHostAsync(configuration);

        // Assert
        await startHost.Should().ThrowAsync<OptionsValidationException>();
    }

    [Theory]
    [MemberData(nameof(BlankLocationValues))]
    public async Task Given_BlankRequiredLocationValue_Should_RejectStartup(
        IReadOnlyDictionary<string, string?> configuration)
    {
        // Act
        var startHost = () => StartHostAsync(configuration);

        // Assert
        await startHost.Should().ThrowAsync<OptionsValidationException>();
    }

    public static TheoryData<IReadOnlyDictionary<string, string?>> BlankLocationValues => new()
    {
        Location("", "Media", "/media"),
        Location("media", " ", "/media"),
        Location("media", "Media", " ")
    };

    [Fact]
    public async Task Given_RootPathsDifferingOnlyByTrailingSeparator_Should_RejectStartup()
    {
        // Arrange
        var configuration = new Dictionary<string, string?>
        {
            ["Storage:Locations:media:DisplayName"] = "Media",
            ["Storage:Locations:media:RootPath"] = "/media",
            ["Storage:Locations:duplicate:DisplayName"] = "Duplicate",
            ["Storage:Locations:duplicate:RootPath"] = "/media/"
        };

        // Act
        var startHost = () => StartHostAsync(configuration);

        // Assert
        await startHost.Should().ThrowAsync<OptionsValidationException>();
    }

    [Theory]
    [InlineData("/mnt/media", "/mnt/media/Movies")]
    [InlineData("/mnt/media/Movies", "/mnt/media")]
    [InlineData("/mnt/media/", "/mnt/media/Movies/Action/..")]
    [InlineData("/", "/mnt/media")]
    [InlineData("/mnt/media", "/")]
    public async Task Given_NestedRootPaths_Should_RejectStartup(string firstRoot, string secondRoot)
    {
        var configuration = Location("media", "Media", firstRoot);
        foreach (var entry in Location("movies", "Movies", secondRoot))
            configuration.Add(entry.Key, entry.Value);

        var startHost = () => StartHostAsync(configuration);

        var exception = await startHost.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Failures.Should().ContainSingle()
            .Which.Should().Contain("media").And.Contain("movies").And.Contain("overlap");
    }

    [Theory]
    [InlineData("/media", "/media-archive")]
    [InlineData("/media/movies", "/media/shows")]
    public async Task Given_NonOverlappingRootPaths_Should_Start(string firstRoot, string secondRoot)
    {
        var configuration = Location("media", "Media", firstRoot);
        foreach (var entry in Location("archive", "Archive", secondRoot))
            configuration.Add(entry.Key, entry.Value);

        var startHost = () => StartHostAsync(configuration);

        await startHost.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_ValidNonexistentAbsoluteRootPath_Should_Start()
    {
        // Arrange
        var configuration = Location(
            "missing",
            "Missing directory",
            $"/media-butler-tests/{Guid.NewGuid():N}/does-not-exist");

        // Act
        var startHost = () => StartHostAsync(configuration);

        // Assert
        await startHost.Should().NotThrowAsync();
    }

    private static Dictionary<string, string?> Location(
        string id,
        string displayName,
        string rootPath)
    {
        return new Dictionary<string, string?>
        {
            [$"Storage:Locations:{id}:DisplayName"] = displayName,
            [$"Storage:Locations:{id}:RootPath"] = rootPath
        };
    }

    private static async Task StartHostAsync(
        IEnumerable<KeyValuePair<string, string?>> configuration)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true
        });

        builder.Configuration.AddInMemoryCollection(configuration);
        builder.RegisterStorageModule();

        using var host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);
    }
}