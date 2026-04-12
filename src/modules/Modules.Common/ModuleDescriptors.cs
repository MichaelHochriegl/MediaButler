using System.Collections.Concurrent;
using System.Reflection;

namespace Modules.Common;

/// <summary>
/// Provides a static class for managing module descriptors within an application.
/// This class maintains a collection of assemblies that can be used for configuring
/// modules and their dependencies.
/// </summary>
public static class ModuleDescriptors
{
    /// <summary>
    /// Represents a collection of assemblies used for module registration and descriptor configuration.
    /// </summary>
    /// <remarks>
    /// This property is a thread-safe collection that stores the assemblies required for registering modules
    /// and configuring various aspects of the application. It is a shared resource, ensuring that all modules
    /// have access to the necessary assembly metadata during runtime.
    /// </remarks>
    public static ConcurrentBag<Assembly> Assemblies { get; } = [];
}