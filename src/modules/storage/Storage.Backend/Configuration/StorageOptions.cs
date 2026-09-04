namespace Storage.Backend.Configuration;

internal sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public Dictionary<string, StorageLocationOptions> Locations { get; set; } = [];
}