using AwesomeAssertions;
using Domain.Storage;
using Microsoft.Extensions.DependencyInjection;
using Storage.Integration;

namespace Storage.Backend.Tests.Integration.Features.StoragePathResolver;

public sealed class StoragePathResolverFileSystemTests : IDisposable
{
    private static readonly StorageLocationId MediaLocation = new("media");
    private readonly DirectoryInfo _temporaryDirectory = Directory.CreateTempSubdirectory("media-butler-storage-tests-");

    private string RootPath => $"{_temporaryDirectory.FullName}/media";

    public static bool IsLinux => OperatingSystem.IsLinux();

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_ExistingDirectoryBeneathLocation_Should_ResolvePath()
    {
        // Arrange
        Directory.CreateDirectory($"{RootPath}/Movies/Action");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "Movies/Action");

        // Assert
        path.Should().Be($"{RootPath}/Movies/Action");
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_SymlinkOutsideRequestedPath_Should_ResolvePath()
    {
        // Arrange
        Directory.CreateDirectory($"{RootPath}/Movies");
        Directory.CreateDirectory($"{_temporaryDirectory.FullName}/outside");
        Directory.CreateSymbolicLink($"{RootPath}/unrelated", $"{_temporaryDirectory.FullName}/outside");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "Movies/Action");

        // Assert
        path.Should().Be($"{RootPath}/Movies/Action");
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_SymlinkMatchingNestedSegmentOutsideRequestedPath_Should_ResolvePath()
    {
        // Arrange
        Directory.CreateDirectory($"{RootPath}/Movies/Action");
        Directory.CreateDirectory($"{_temporaryDirectory.FullName}/outside");
        Directory.CreateSymbolicLink($"{RootPath}/Action", $"{_temporaryDirectory.FullName}/outside");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "Movies/Action");

        // Assert
        path.Should().Be($"{RootPath}/Movies/Action");
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_NonexistentTarget_Should_ResolveWithoutCreatingDirectories(bool parentExists)
    {
        // Arrange
        Directory.CreateDirectory(RootPath);
        if (parentExists)
            Directory.CreateDirectory($"{RootPath}/Movies");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "Movies/Action");

        // Assert
        path.Should().Be($"{RootPath}/Movies/Action");
        Directory.Exists($"{RootPath}/Movies/Action").Should().BeFalse();
        Directory.Exists($"{RootPath}/Movies").Should().Be(parentExists);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_NonexistentConfiguredRoot_Should_ResolveWithoutCreatingDirectories()
    {
        // Arrange
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "Movies/Action");

        // Assert
        path.Should().Be($"{RootPath}/Movies/Action");
        Directory.Exists(RootPath).Should().BeFalse();
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("media-backup")]
    [InlineData("MEDIA")]
    public async Task Given_TraversalToSiblingDirectory_Should_RejectPath(string siblingName)
    {
        // Arrange
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory($"{_temporaryDirectory.FullName}/{siblingName}/Movies");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        var relativePath = $"../{siblingName}/Movies";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<EscapingStoragePathException>()
            .WithMessage("*escapes storage location*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("media/real", true, "link")]
    [InlineData("media/real", true, "link/Movies")]
    [InlineData("outside", true, "link")]
    [InlineData("outside", true, "link/Movies")]
    [InlineData("missing", false, "link")]
    [InlineData("missing", false, "link/Movies")]
    [InlineData("outside", true, "link/../Movies")]
    [InlineData("outside", true, "link//Movies")]
    [InlineData("outside", true, "link/")]
    [InlineData("outside", true, "link//..//Movies/")]
    public async Task Given_DirectorySymlinkInPath_Should_RejectPath(
        string targetRelativeToTemporaryDirectory,
        bool targetExists,
        string relativePath)
    {
        // Arrange
        Directory.CreateDirectory(RootPath);
        var target = $"{_temporaryDirectory.FullName}/{targetRelativeToTemporaryDirectory}";
        if (targetExists)
            Directory.CreateDirectory(target);
        Directory.CreateSymbolicLink($"{RootPath}/link", target);
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<SymlinkStoragePathException>()
            .WithMessage("*symbolic links are not allowed*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_NestedSymlinkWithRelativeTarget_Should_RejectPath()
    {
        // Arrange
        Directory.CreateDirectory($"{RootPath}/Movies");
        Directory.CreateDirectory($"{_temporaryDirectory.FullName}/outside");
        Directory.CreateSymbolicLink($"{RootPath}/Movies/link", "../../outside");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        const string relativePath = "Movies/link/Action";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<SymlinkStoragePathException>()
            .WithMessage("*symbolic links are not allowed*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_SymlinkLoopInPath_Should_RejectPath()
    {
        // Arrange
        Directory.CreateDirectory(RootPath);
        Directory.CreateSymbolicLink($"{RootPath}/link", "link");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        const string relativePath = "link/Movies";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<SymlinkStoragePathException>()
            .WithMessage("*symbolic links are not allowed*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_FileSymlinkInPath_Should_RejectPath(bool targetExists)
    {
        // Arrange
        Directory.CreateDirectory(RootPath);
        var target = $"{_temporaryDirectory.FullName}/movie.mkv";
        if (targetExists)
            await File.WriteAllTextAsync(target, "test", TestContext.Current.CancellationToken);
        File.CreateSymbolicLink($"{RootPath}/movie.mkv", target);
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        const string relativePath = "movie.mkv";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<SymlinkStoragePathException>()
            .WithMessage("*symbolic links are not allowed*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Theory(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    [InlineData("")]
    [InlineData("/nested")]
    public async Task Given_SymlinkInConfiguredRoot_Should_RejectResolution(string rootSuffix)
    {
        // Arrange
        Directory.CreateDirectory($"{_temporaryDirectory.FullName}/outside{rootSuffix}");
        Directory.CreateSymbolicLink(RootPath, $"{_temporaryDirectory.FullName}/outside");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", $"{RootPath}{rootSuffix}"));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        const string relativePath = "Movies";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<SymlinkStoragePathException>()
            .WithMessage("*symbolic links are not allowed*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Fact(Skip = "Unsure how to handle this, might need to disallow symbolic links during startup altogether")]
    public async Task Given_ConfiguredRootWithSymlinkFollowedByParentSegment_Should_RejectResolution()
    {
        // Arrange
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory($"{_temporaryDirectory.FullName}/outside");
        Directory.CreateSymbolicLink($"{_temporaryDirectory.FullName}/link", $"{_temporaryDirectory.FullName}/outside");
        var configuredRoot = $"{_temporaryDirectory.FullName}/link/../media";
        using var host = await StoragePathResolverTestHost.StartAsync(("media", configuredRoot));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        const string relativePath = "Movies";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<SymlinkStoragePathException>()
            .WithMessage("*symbolic links are not allowed*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_PathLeavingAndReenteringLocationWithoutSymlinks_Should_ResolveContainedPath()
    {
        // Arrange
        Directory.CreateDirectory($"{RootPath}/Movies");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();

        // Act
        var path = resolver.ResolvePath(MediaLocation, "../media/Movies");

        // Assert
        path.Should().Be($"{RootPath}/Movies");
    }

    [Fact(Skip = "Storage paths require Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_PathLeavingAndReenteringLocationThroughSymlink_Should_RejectPath()
    {
        // Arrange
        Directory.CreateDirectory($"{RootPath}/Movies");
        Directory.CreateDirectory($"{_temporaryDirectory.FullName}/outside");
        Directory.CreateSymbolicLink($"{_temporaryDirectory.FullName}/link", $"{_temporaryDirectory.FullName}/outside");
        using var host = await StoragePathResolverTestHost.StartAsync(("media", RootPath));
        var resolver = host.Services.GetRequiredService<IStoragePathResolver>();
        const string relativePath = "../link/../media/Movies";

        // Act
        var resolve = () => resolver.ResolvePath(MediaLocation, relativePath);

        // Assert
        var exception = resolve.Should().ThrowExactly<SymlinkStoragePathException>()
            .WithMessage("*symbolic links are not allowed*").Which;
        exception.LocationId.Should().Be(MediaLocation);
        exception.RelativePath.Should().Be(relativePath);
    }

    public void Dispose()
    {
        _temporaryDirectory.Delete(recursive: true);
    }
}
