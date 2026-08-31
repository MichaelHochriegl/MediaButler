using Libraries.Contracts.Features.Physical;
using Libraries.Persistence;
using Microsoft.Extensions.Hosting;
using Modules.Common;

namespace Libraries.Backend;

/// <summary>
/// Registers library modules in a host application builder.
/// </summary>
public static class ModuleRegistration
{
    /// <summary>
    /// Provides extension methods for registering modules
    /// in a host application builder.
    /// </summary>
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the Libraries module with the host application builder.
        /// </summary>
        /// <returns>
        /// The same <see cref="IHostApplicationBuilder"/> instance for chaining.
        /// </returns>
        public IHostApplicationBuilder RegisterLibrariesModule()
        {
            ModuleDescriptors.BackendAssemblies
                .Add(typeof(ModuleRegistration).Assembly);
            ModuleDescriptors.PersistenceAssemblies
                .Add(typeof(LibrariesDb).Assembly);
            ModuleDescriptors.ContractAssemblies
                .Add(typeof(CreatePhysicalLibraryRequest).Assembly);
            
            return builder;
        }
    }
}