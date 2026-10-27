using FastEndpoints.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using ServiceDiscovery;
using Testcontainers.PostgreSql;

namespace Libraries.Backend.Tests.Integration.Features.Physical.CreateLibrary;

public class CreateLibraryAppFixture : AppFixture<Program>
{
    private readonly DirectoryInfo _temporaryDirectory =
        Directory.CreateTempSubdirectory("media-butler-library-tests-");
    private PostgreSqlContainer _databaseContainer = null!;

    public string MediaRootPath => Path.Combine(_temporaryDirectory.FullName, "media");
    public string ArchiveRootPath => Path.Combine(_temporaryDirectory.FullName, "archive");
    public string OfflineRootPath => Path.Combine(_temporaryDirectory.FullName, "offline");
    public string FilePath => Path.Combine(MediaRootPath, "movie.mkv");
    public string SymlinkPath => Path.Combine(MediaRootPath, "linked-movies");
    
    protected override async ValueTask PreSetupAsync()
    {
        CreateStorageDirectories(MediaRootPath);
        CreateStorageDirectories(ArchiveRootPath);
        await File.WriteAllTextAsync(FilePath, "test", TestContext.Current.CancellationToken);

        if (OperatingSystem.IsLinux())
        {
            Directory.CreateSymbolicLink(SymlinkPath, Path.Combine(MediaRootPath, "Movies"));
        }

        _databaseContainer = new PostgreSqlBuilder($"postgres:{Tags.PostgresTag}")
            .WithDatabase("media-butler-libraries-test")
            .WithUsername("libraries-user")
            .WithPassword("libraries-password")
            .Build();

        await _databaseContainer.StartAsync();
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseSetting($"ConnectionStrings:{Descriptors.Database}", _databaseContainer.GetConnectionString());
        AddStorageLocation(a, "media", "Media", MediaRootPath);
        AddStorageLocation(a, "archive", "Archive", ArchiveRootPath);
        AddStorageLocation(a, "offline", "Offline", OfflineRootPath);
    }
    
    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync("""
                                             TRUNCATE TABLE "LibrarySource", "Library" RESTART IDENTITY CASCADE;
                                             """);
    }

    protected override async ValueTask TearDownAsync()
    {
        await _databaseContainer.DisposeAsync();
        _temporaryDirectory.Delete(recursive: true);
    }

    private static void CreateStorageDirectories(string rootPath)
    {
        Directory.CreateDirectory(Path.Combine(rootPath, "Movies", "Action"));
        Directory.CreateDirectory(Path.Combine(rootPath, "Movies-Archive"));
        Directory.CreateDirectory(Path.Combine(rootPath, "TV Shows"));
    }

    private static void AddStorageLocation(
        IWebHostBuilder builder,
        string id,
        string displayName,
        string rootPath)
    {
        builder.UseSetting($"Storage:Locations:{id}:DisplayName", displayName);
        builder.UseSetting($"Storage:Locations:{id}:RootPath", rootPath);
    }
}
