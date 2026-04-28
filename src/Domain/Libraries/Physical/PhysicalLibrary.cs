namespace Domain.Libraries.Physical;

public sealed class PhysicalLibrary : Library
{
    private readonly List<PhysicalLibrarySource> _sources = [];

    public IReadOnlyCollection<PhysicalLibrarySource> Sources => _sources.AsReadOnly();

    private PhysicalLibrary(LibraryName name, IEnumerable<PhysicalLibrarySource> sources)
        : base(name)
    {
        foreach (var source in sources)
        {
            AddSourceInternal(source);
        }

        if (_sources.Count == 0)
        {
            throw new ArgumentException("A physical library needs at least one source");
        }
    }

    public static PhysicalLibrary Create(
        LibraryName name,
        IEnumerable<PhysicalLibrarySource> sources)
    {
        var sourceEntities = sources
            .Select(source => new PhysicalLibrarySource(source.MediaKind, source.Path))
            .ToArray();

        return new PhysicalLibrary(name, sourceEntities);
    }

    public void AddSource(LibraryMediaKind mediaKind, PhysicalPath path)
    {
        AddSourceInternal(new PhysicalLibrarySource(mediaKind, path));
        MarkUpdated();
    }

    public void RemoveSource(LibraryMediaKind mediaKind)
    {
        var source = _sources.SingleOrDefault(x => x.MediaKind == mediaKind);

        if (source is null)
        {
            return;
        }

        if (_sources.Count == 1)
        {
            throw new InvalidOperationException("A physical library must have at least one source");
        }

        _sources.Remove(source);
        MarkUpdated();
    }

    private void AddSourceInternal(PhysicalLibrarySource source)
    {
        if (_sources.Any(x => x.MediaKind != source.MediaKind && x.Path == source.Path))
        {
            throw new InvalidOperationException(
                $"The physical library already has the path '{source.Path.Value}' assigned for the media kind '{source.MediaKind}'");
        }

        if (_sources.Any(x => x.Path == source.Path))
        {
            throw new InvalidOperationException(
                $"The physical library already has the path '{source.Path.Value}' assigned");
        }

        _sources.Add(source);
    }
}