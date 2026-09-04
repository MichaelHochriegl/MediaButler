using Microsoft.Extensions.Hosting;
using Modules.Common;
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

            return builder;
        }
    }
}