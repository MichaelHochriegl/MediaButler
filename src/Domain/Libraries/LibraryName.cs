namespace Domain.Libraries;

public sealed record LibraryName
{
    private const int MaxLength = 100;
    
    public string Value { get; }
    
    public LibraryName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("LibraryName cannot be empty",
                nameof(value));
        
        var trimmed = value.Trim();
        
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"LibraryName cannot exceed {MaxLength} characters",
                nameof(value));
        
        Value = trimmed;
    }
};