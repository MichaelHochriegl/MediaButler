namespace ServiceDiscovery;

/// <summary>
/// Provides constants representing service descriptor identifiers used for
/// service discovery and application configuration mechanisms.
/// </summary>
public static class Descriptors
{
    /// <summary>
    ///     Represents the descriptor identifier for the "media-root-path" parameter
    ///     within the service discovery mechanism. This constant is used to uniquely
    ///     name and reference the root path for media content.
    /// </summary>
    public const string MediaRootPath = "media-root-path";
    
    /// <summary>
    /// Represents the descriptor identifier for the "database-server" service
    /// within the service discovery mechanism. This constant is used to uniquely
    /// name and reference the database server when configuring distributed
    /// applications or assigning service dependencies.
    /// </summary>
    public const string DatabaseServer = "database-server";

    /// <summary>
    /// Represents the descriptor identifier for the "database volume" resource
    /// within the service discovery mechanism. This constant is used to uniquely
    /// name and reference the data volume associated with the database server,
    /// enabling persistent storage configuration in distributed applications.
    /// </summary>
    public const string DatabaseVolume = "database-volume";

    /// <summary>
    /// Represents the descriptor identifier for the "database" component within the
    /// service discovery mechanism. This constant is used to uniquely name and
    /// reference the specific database instance when configuring and interacting with
    /// distributed application components.
    /// </summary>
    public const string Database = "database";
    
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