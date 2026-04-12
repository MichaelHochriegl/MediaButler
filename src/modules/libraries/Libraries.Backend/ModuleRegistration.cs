using Microsoft.Extensions.Hosting;
using Modules.Common;

namespace Libraries.Backend;

/// <summary>
/// Provides an extension method for registering library modules in a host application builder.
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
        /// Registers the Libraries module by adding its assembly to the
        /// collection of module assemblies in the application's descriptors and registering services.
        /// </summary>
        /// <returns>
        /// The updated <see cref="IHostApplicationBuilder"/> instance to allow
        /// further configuration chaining.
        /// </returns>
        public IHostApplicationBuilder RegisterLibrariesModule()
        {
            ModuleDescriptors.BackendAssemblies
                .Add(typeof(ModuleRegistration).Assembly);
            
            return builder;
        }
    }
}