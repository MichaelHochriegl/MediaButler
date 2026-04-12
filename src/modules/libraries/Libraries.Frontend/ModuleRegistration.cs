using Microsoft.Extensions.Hosting;
using Modules.Common;

namespace Libraries.Frontend;

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
            ModuleDescriptors.FrontendAssemblies
                .Add(typeof(ModuleRegistration).Assembly);
            
            return builder;
        }
    }
}