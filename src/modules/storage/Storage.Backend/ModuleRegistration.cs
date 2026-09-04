using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Modules.Common;
using Storage.Backend.Configuration;
using Storage.Contracts;

namespace Storage.Backend;

/// <summary>
///     Registers storage modules in a host application builder.
/// </summary>
public static class ModuleRegistration
{
    /// <summary>
    ///     Provides extension methods for registering modules
    ///     in a host application builder.
    /// </summary>
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers the Libraries module with the host application builder.
        /// </summary>
        /// <returns>
        ///     The same <see cref="IHostApplicationBuilder" /> instance for chaining.
        /// </returns>
        public IHostApplicationBuilder RegisterStorageModule()
        {
            ModuleDescriptors.BackendAssemblies
                .Add(typeof(ModuleRegistration).Assembly);
            ModuleDescriptors.ContractAssemblies
                .Add(typeof(IStorageContractsMarker).Assembly);

            builder.Services.AddSingleton<
                IValidateOptions<StorageOptions>,
                StorageOptionsValidator>();

            builder.Services.AddOptions<StorageOptions>()
                .Bind(builder.Configuration.GetSection(StorageOptions.SectionName))
                .ValidateOnStart();

            return builder;
        }
    }
}