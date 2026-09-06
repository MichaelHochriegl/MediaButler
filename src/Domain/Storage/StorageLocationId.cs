namespace Domain.Storage;

/// <summary>
///     Represents a unique identifier for a storage location within the system.
/// </summary>
public readonly record struct StorageLocationId
{
    /// <summary>
    ///     Gets the unique identifier value for the storage location.
    /// </summary>
    public string Value { get; }

    public StorageLocationId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            throw new ArgumentException(
                "Storage location IDs must not contain leading or trailing whitespace.",
                nameof(value));

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}