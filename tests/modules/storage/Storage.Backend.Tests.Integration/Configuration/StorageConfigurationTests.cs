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
        var startHost = () => StartHostAsync([]);

        await startHost.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_ValidLocation_Should_Start()
    {
        var configuration = Location("media", "Media", "/media");

        var startHost = () => StartHostAsync(configuration);

        await startHost.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_SeveralValidLocations_Should_Start()
    {
        var configuration = new Dictionary<string, string?>
        {
            ["Storage:Locations:movies:DisplayName"] = "Movies",
            ["Storage:Locations:movies:RootPath"] = "/media/movies",
            ["Storage:Locations:shows:DisplayName"] = "TV Shows",
            ["Storage:Locations:shows:RootPath"] = "/media/shows",
            ["Storage:Locations:music:DisplayName"] = "Music",
            ["Storage:Locations:music:RootPath"] = "/media/music"
        };

        var startHost = () => StartHostAsync(configuration);

        await startHost.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_RelativeRootPath_Should_RejectStartup()
    {
        var configuration = Location("media", "Media", "media");

        var startHost = () => StartHostAsync(configuration);

        await startHost.Should().ThrowAsync<OptionsValidationException>();
    }

    [Theory]
    [MemberData(nameof(BlankLocationValues))]
    public async Task Given_BlankRequiredLocationValue_Should_RejectStartup(
        IReadOnlyDictionary<string, string?> configuration)
    {
        var startHost = () => StartHostAsync(configuration);

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
        var configuration = new Dictionary<string, string?>
        {
            ["Storage:Locations:media:DisplayName"] = "Media",
            ["Storage:Locations:media:RootPath"] = "/media",
            ["Storage:Locations:duplicate:DisplayName"] = "Duplicate",
            ["Storage:Locations:duplicate:RootPath"] = "/media/"
        };

        var startHost = () => StartHostAsync(configuration);

        await startHost.Should().ThrowAsync<OptionsValidationException>();
    }

    [Fact]
    public async Task Given_ValidNonexistentAbsoluteRootPath_Should_Start()
    {
        var configuration = Location(
            "missing",
            "Missing directory",
            $"/media-butler-tests/{Guid.NewGuid():N}/does-not-exist");

        var startHost = () => StartHostAsync(configuration);

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