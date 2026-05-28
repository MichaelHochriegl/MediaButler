using System.Collections.Concurrent;
using System.Reflection;

namespace Modules.Common;

/// <summary>
/// Provides a centralized registry for managing module assemblies within the application,
/// allowing dynamic composition.
/// </summary>
public static class ModuleDescriptors
{
    /// <summary>
    /// Gets a collection of assemblies that includes all module assemblies.
    /// </summary>
    public static IEnumerable<Assembly> Assemblies => BackendAssemblies
        .Concat(FrontendAssemblies)
        .Concat(PersistenceAssemblies);

    /// <summary>
    /// A thread-safe collection of assemblies representing the backend modules of the application.
    /// </summary>
    /// <remarks>
    /// This property holds a thread-safe, concurrent collection of assemblies
    /// that are associated with backend modules. It is used to organize and
    /// manage backend-related assemblies for module registration and
    /// dependency resolution within the application.
    /// </remarks>
    public static ConcurrentBag<Assembly> BackendAssemblies { get; } = [];

    /// <summary>
    /// A thread-safe collection of assemblies representing the frontend modules of the application.
    /// </summary>
    /// <remarks>
    /// This property holds a thread-safe, concurrent collection of assemblies
    /// that are associated with frontend modules. It is used to organize and
    /// manage frontend-related assemblies for module registration and
    /// dependency resolution within the application.
    /// </remarks>
    public static ConcurrentBag<Assembly> FrontendAssemblies { get; } = [];
    
    public static ConcurrentBag<Assembly> PersistenceAssemblies { get; } = [];
}