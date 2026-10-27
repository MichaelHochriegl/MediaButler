using FastEndpoints.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using ServiceDiscovery;
using Testcontainers.PostgreSql;

namespace Libraries.Backend.Tests.Integration.Features.GetLibraryById;

public class GetLibraryByIdAppFixture : AppFixture<Program>
{
    private readonly DirectoryInfo _temporaryDirectory =
        Directory.CreateTempSubdirectory("media-butler-get-library-tests-");
    private PostgreSqlContainer _databaseContainer = null!;

    protected override async ValueTask PreSetupAsync()
    {
        Directory.CreateDirectory(Path.Combine(_temporaryDirectory.FullName, "Movies"));

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
        a.UseSetting("Storage:Locations:media:DisplayName", "Media");
        a.UseSetting("Storage:Locations:media:RootPath", _temporaryDirectory.FullName);
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
}
