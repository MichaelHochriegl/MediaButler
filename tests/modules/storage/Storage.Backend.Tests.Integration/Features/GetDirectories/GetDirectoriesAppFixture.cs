using FastEndpoints.Testing;
using Microsoft.AspNetCore.Hosting;
using ServiceDiscovery;
using Testcontainers.PostgreSql;

namespace Storage.Backend.Tests.Integration.Features.GetDirectories;

public class GetDirectoriesAppFixture : AppFixture<Program>
{
    private readonly DirectoryInfo _temporaryDirectory = Directory.CreateTempSubdirectory("media-butler-browsing-tests-");
    private PostgreSqlContainer _databaseContainer = null!;

    public string RootPath => Path.Combine(_temporaryDirectory.FullName, "media");
    public string ChangingRootPath => Path.Combine(_temporaryDirectory.FullName, "changing");

    protected override async ValueTask PreSetupAsync()
    {
        Directory.CreateDirectory(Path.Combine(RootPath, "Movies", "Action", "Nested"));
        Directory.CreateDirectory(Path.Combine(RootPath, "Movies", "Drama"));
        Directory.CreateDirectory(Path.Combine(RootPath, "TV Shows"));
        Directory.CreateDirectory(Path.Combine(RootPath, ".hidden"));
        await File.WriteAllTextAsync(Path.Combine(RootPath, "movie.mkv"), "test");
        await File.WriteAllTextAsync(Path.Combine(RootPath, "Movies", "film.mkv"), "test");

        if (OperatingSystem.IsLinux())
        {
            var outside = Directory.CreateDirectory(Path.Combine(_temporaryDirectory.FullName, "outside"));
            Directory.CreateSymbolicLink(Path.Combine(RootPath, "external-link"), outside.FullName);
            Directory.CreateSymbolicLink(Path.Combine(RootPath, "internal-link"), Path.Combine(RootPath, "Movies"));
            Directory.CreateSymbolicLink(Path.Combine(RootPath, "broken-link"), Path.Combine(_temporaryDirectory.FullName, "missing"));
            Directory.CreateSymbolicLink(Path.Combine(_temporaryDirectory.FullName, "linked-root"), RootPath);
            Directory.CreateSymbolicLink(Path.Combine(RootPath, "Movies", "nested-link"), outside.FullName);
        }

        _databaseContainer = new PostgreSqlBuilder($"postgres:{Tags.PostgresTag}")
            .WithDatabase("media-butler-storage-test")
            .WithUsername("storage-user")
            .WithPassword("storage-password")
            .Build();
        await _databaseContainer.StartAsync();
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseSetting($"ConnectionStrings:{Descriptors.Database}", _databaseContainer.GetConnectionString());
        AddLocation(a, "media", "Media library", RootPath);
        AddLocation(a, "offline", "Offline storage", Path.Combine(_temporaryDirectory.FullName, "missing"));
        AddLocation(a, "file-root", "Invalid mount", Path.Combine(RootPath, "movie.mkv"));
        AddLocation(a, "changing", "Removable storage", ChangingRootPath);
        if (OperatingSystem.IsLinux())
            AddLocation(a, "linked-root", "Linked storage", Path.Combine(_temporaryDirectory.FullName, "linked-root"));
    }

    private static void AddLocation(IWebHostBuilder builder, string id, string displayName, string rootPath)
    {
        builder.UseSetting($"Storage:Locations:{id}:DisplayName", displayName);
        builder.UseSetting($"Storage:Locations:{id}:RootPath", rootPath);
    }

    protected override async ValueTask TearDownAsync()
    {
        await _databaseContainer.DisposeAsync();
        _temporaryDirectory.Delete(recursive: true);
    }
}
