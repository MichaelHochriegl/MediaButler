namespace ServiceDiscovery;

/// <summary>
/// Provides constants representing service descriptor identifiers used for
/// service discovery and application configuration mechanisms.
/// </summary>
public static class Descriptors
{
    /// <summary>
    /// Represents the descriptor identifier for the "backend" project component
    /// within the service discovery mechanism. This constant is used to uniquely
    /// name and reference the backend project when configuring distributed
    /// applications or service routing.
    /// </summary>
    public const string Backend = "backend";

    /// <summary>
    /// Represents the descriptor identifier for the "frontend" project component
    /// within the service discovery mechanism. This constant is used to uniquely
    /// name and reference the frontend project when configuring distributed
    /// applications or service routing.
    /// </summary>
    public const string Frontend = "frontend";
}