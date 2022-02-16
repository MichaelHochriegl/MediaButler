using MediaButler.Shared.Domain.Common;

namespace MediaButler.Shared.Domain.ValueObjects;

public class Location : ValueObject
{
    public string Path { get; private set; }
    public bool IsVirtual { get; private set; }

    private Location()
    {
        
    }

    public Location(string path)
    {
        Path = path;
        IsVirtual = File.Exists(path);
    }
}