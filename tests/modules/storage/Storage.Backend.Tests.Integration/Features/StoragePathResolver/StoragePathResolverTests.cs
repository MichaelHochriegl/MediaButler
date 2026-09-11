using AwesomeAssertions;
using Domain.Storage;
using Microsoft.Extensions.DependencyInjection;
using Storage.Integration;

namespace Storage.Backend.Tests.Integration.Features.StoragePathResolver;

public sealed class StoragePathResolverTests
{
    private static readonly StorageLocationId MediaLocation = new("media");
    private readonly string _rootPath = $"/media-butler-tests/{Guid.NewGuid():N}/media";

    public static bool IsLinux => OperatingSystem.IsLinux();

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_RegisteredStorageModule_Should_ProvidePathResolver()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));

        // Act
        var resolver = host.Services.GetService<IStoragePathResolver>();

        // Assert
        resolver.Should().NotBeNull();
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("Movies", "Movies")]
    [InlineData("Movies/Action", "Movies/Action")]
    [InlineData("Movies//Action", "Movies/Action")]
    [InlineData("Movies///Action", "Movies/Action")]
    [InlineData("Movies/Action/", "Movies/Action/")]
    [InlineData("Movies//Action///", "Movies/Action/")]
    [InlineData("movies/Action", "movies/Action")]
    [InlineData("Movies With Spaces/Amélie", "Movies With Spaces/Amélie")]
    [InlineData(".hidden/Movies", ".hidden/Movies")]
    [InlineData("Movies..Old/Action", "Movies..Old/Action")]
    [InlineData("Movies\\Action", "Movies\\Action")]
    [InlineData("..\\outside", "..\\outside")]
    public async Task Given_RelativeLinuxPath_Should_ResolveBeneathConfiguredLocation(
        string relativePath,
        string expectedRelativePath)
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        path.Should().Be($"{_rootPath}/{expectedRelativePath}");
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("./Movies", "Movies")]
    [InlineData("Movies/./Action", "Movies/Action")]
    [InlineData("Movies/../Shows", "Shows")]
    [InlineData("Movies/Action/../../Shows", "Shows")]
    public async Task Given_DotSegmentsRemainingInsideLocation_Should_NormalizePath(
        string relativePath,
        string expectedRelativePath)
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        path.Should().Be($"{_rootPath}/{expectedRelativePath}");
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData(".")]
    [InlineData("Movies/..")]
    public async Task Given_DotSegmentsResolvingToLocationRoot_Should_ResolveRoot(string relativePath)
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        path.Should().Be(_rootPath);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_EmptyRelativePath_Should_ResolveLocationRoot()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "");

        // Assert
        path.Should().Be(_rootPath);
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("")]
    [InlineData(".")]
    public async Task Given_FilesystemRootLocationAndRootRelativePath_Should_ResolveFilesystemRoot(string relativePath)
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", "/"));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        path.Should().Be("/");
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_FilesystemRootLocationAndRelativeChild_Should_ResolveChild()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", "/"));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        var relativePath = $"media-butler-tests-{Guid.NewGuid():N}/Movies";

        // Act
        var path = resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        path.Should().Be($"/{relativePath}");
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_SeveralLocations_Should_ResolveAgainstRequestedLocation()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(
            ("media", _rootPath), ("archive", $"{_rootPath}-archive"));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(new StorageLocationId("archive"), "Movies");

        // Assert
        path.Should().Be($"{_rootPath}-archive/Movies");
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_ConfiguredRootWithTrailingSeparator_Should_ResolveWithoutDuplicateSeparator()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", $"{_rootPath}/"));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "Movies/Action");

        // Assert
        path.Should().Be($"{_rootPath}/Movies/Action");
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("/")]
    [InlineData("/etc")]
    [InlineData("//etc/passwd")]
    public async Task Given_AbsoluteLinuxPath_Should_RejectPath(string relativePath)
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<RootedStoragePathException>()
            .WithMessage("*must be relative*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_AbsolutePathInsideConfiguredLocation_Should_RejectPath()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        var absolutePath = $"{_rootPath}/Movies";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, absolutePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<RootedStoragePathException>()
            .WithMessage("*must be relative*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(absolutePath);
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("..")]
    [InlineData("../outside")]
    [InlineData("./../outside")]
    [InlineData("Movies/../../outside")]
    [InlineData("Movies/./../../outside")]
    public async Task Given_TraversalOutsideConfiguredLocation_Should_RejectPath(string relativePath)
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<EscapingStoragePathException>()
            .WithMessage("*escapes storage location*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_PathContainingNullCharacter_Should_RejectPath()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        const string relativePath = "Movies/Act\0ion";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().Throw<InvalidStoragePathException>()
            .WithMessage("*is invalid for storage location*").Which;
        exception.InnerException.Should().BeOfType<ArgumentException>();
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_UnknownLocation_Should_ReportMissingLocation()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", _rootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        var unknownLocation = new StorageLocationId("unknown");

        // Act
        var resolve = () => resolver.ResolvePath(unknownLocation, "Movies");

        // Assert
        resolve.Should().ThrowExactly<StorageLocationNotFoundException>()
            .WithMessage("Storage location 'unknown' is not configured.")
            .Which.LocationId.Should().Be(unknownLocation);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_NoConfiguredLocations_Should_ReportMissingLocation()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync();
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, "Movies");

        // Assert
        resolve.Should().ThrowExactly<StorageLocationNotFoundException>()
            .WithMessage("Storage location 'media' is not configured.")
            .Which.LocationId.Should().Be(MediaLocation);
    }
}
