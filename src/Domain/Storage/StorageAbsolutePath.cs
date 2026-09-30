namespace Domain.Storage;

public readonly record struct StorageAbsolutePath
{
    private const int MaxLength = 1024;
    
    public string Value { get; }
    
    public StorageAbsolutePath(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        
        if (value.Length > MaxLength)
        {
            throw new ArgumentException(
                $"StorageAbsolutePath must be less than {MaxLength} characters long.",
                nameof(value));
        }

        if (value.Contains('\0'))
        {
            throw new ArgumentException("StorageAbsolutePath must not contain null characters.", nameof(value));
        }

        if (!Path.IsPathRooted(value))
        {
            throw new ArgumentException("StorageAbsolutePath must be rooted.", nameof(value));
        }
        
        if (value != "/" && value.Split('/').Skip(1).Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException("StorageAbsolutePath must not contain empty or relative segments.", nameof(value));
        }
        
        Value = value;
    }

    public override string ToString() => Value;
}