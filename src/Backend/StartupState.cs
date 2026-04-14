namespace Backend;

/// <summary>
/// Represents the state of the application during startup.
/// </summary>
internal sealed class StartupState
{
    /// <summary>
    /// Indicates whether the database is ready for use.
    /// This property is set during the application's startup process,
    /// specifically after a successful database migration, to signal
    /// that the application can safely interact with the database.
    /// </summary>
    public bool DatabaseReady { get; set; }
}