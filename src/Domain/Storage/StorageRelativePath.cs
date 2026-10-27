namespace Domain.Storage;

public readonly record struct StorageRelativePath
{
    public const int MaxLength = 1024;
    
    public string Value { get; }
    
    public StorageRelativePath(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        
        if (value.Length > MaxLength)
        {
            throw new ArgumentException(
                $"StorageRelativePath must be less than {MaxLength} characters long.",
                nameof(value));
        }

        if (value.Contains('\0'))
        {
            throw new ArgumentException("StorageRelativePath must not contain null characters.", nameof(value));
        }

        if (value != ".")
        {
            var segments = value.Split('/');

            if (segments.Any(s => s is "" or "." or ".."))
            {
                throw new ArgumentException("StorageRelativePath must not contain empty or relative segments.", nameof(value));
            }
        }
        
        Value = value;
    }

    public bool Overlaps(StorageRelativePath other) => 
        Value == other.Value 
        || Value == "." 
        || other.Value == "." 
        || Value.StartsWith(other.Value + "/", StringComparison.Ordinal) 
        || other.Value.StartsWith(Value + "/", StringComparison.Ordinal);
    
    public override string ToString() => Value;
}
