namespace Storage.Backend.Configuration;

/// <summary>
///     Defines the configuration for a storage location in <see cref="StorageOptions.Locations" />.
/// </summary>
internal sealed class StorageLocationOptions
{
    /// <summary>
    ///     Gets or sets the human-readable name of the storage location.
    /// </summary>
    /// <value>A nonblank display name. Defaults to <see cref="string.Empty" />.</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the root directory path of the storage location.
    /// </summary>
    /// <value>A fully qualified, absolute path. Defaults to <see cref="string.Empty" />.</value>
    /// <remarks>
    ///     The configured value is retained as supplied; normalization is performed by
    ///     <c>NormalizedRootPath</c>.
    /// </remarks>
    public string RootPath { get; set; } = string.Empty;
}

/// <summary>
///     Provides path normalization for <see cref="StorageLocationOptions" />.
/// </summary>
internal static class StorageLocationOptionsExtensions
{
    extension(StorageLocationOptions options)
    {
        /// <summary>
        ///     Gets the full root path with any trailing directory separator removed,
        ///     except when the separator is part of the filesystem root.
        /// </summary>
        /// <value>The normalized absolute path derived from <see cref="StorageLocationOptions.RootPath" />.</value>
        /// <remarks>
        ///     The path is computed on each access without changing the configured value or
        ///     checking whether the directory exists. Relative paths are resolved against the
        ///     current working directory, although storage configuration requires an absolute path.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The configured root path is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">The configured root path is empty or invalid.</exception>
        /// <exception cref="NotSupportedException">The configured root path uses an unsupported format.</exception>
        /// <exception cref="PathTooLongException">The configured root path exceeds the system-defined maximum length.</exception>
        public string NormalizedRootPath => Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(options.RootPath));
    }
}
