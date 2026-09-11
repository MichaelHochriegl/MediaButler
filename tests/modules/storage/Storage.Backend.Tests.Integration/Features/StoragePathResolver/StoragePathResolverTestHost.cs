using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Storage.Backend.Tests.Integration.Features.StoragePathResolver;

internal static class StoragePathResolverTestHost
{
    public static async Task<IHost> StartAsync(params (string Id, string RootPath)[] locations)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true
        });

        var configuration = new Dictionary<string, string?>();
        foreach (var (id, rootPath) in locations)
        {
            configuration[$"Storage:Locations:{id}:DisplayName"] = id;
            configuration[$"Storage:Locations:{id}:RootPath"] = rootPath;
        }

        builder.Configuration.AddInMemoryCollection(configuration);
        builder.RegisterStorageModule();

        var host = builder.Build();
        try
        {
            await host.StartAsync(TestContext.Current.CancellationToken);
            return host;
        }
        catch
        {
            host.Dispose();
            throw;
        }
    }
}
