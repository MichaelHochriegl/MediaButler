namespace Domain.Libraries.Physical;

public sealed record PhysicalPath
{
    private const int MaxLength = 1024;
    
    public string Value { get; }

    public PhysicalPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("PhysicalPath cannot be empty",
                nameof(value));
        }

        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"PhysicalPath cannot exceed {MaxLength} characters",
                nameof(value));
        }

        if (normalized.Contains('\0'))
        {
            throw new ArgumentException("PhysicalPath cannot contain null characters",
                nameof(value));
        }
        
        Value = normalized;
    }


    public override string ToString() => Value;

    private static string Normalize(string value)
    {
        var trimmed = value.Trim();

        while (HasTrailingDirectorySeparator(trimmed) && !IsFileSystemRoot(trimmed))
        {
            trimmed = Path.TrimEndingDirectorySeparator(trimmed);
        }
        
        return trimmed;
    }

    private static bool IsFileSystemRoot(string trimmed)
    {
        return string.Equals(trimmed, Path.GetPathRoot(trimmed), StringComparison.Ordinal);
    }

    private static bool HasTrailingDirectorySeparator(string trimmed)
    {
        return Path.EndsInDirectorySeparator(trimmed);
    }
}