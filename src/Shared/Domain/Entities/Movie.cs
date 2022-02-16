using MediaButler.Shared.Domain.Common;
using MediaButler.Shared.Domain.ValueObjects;

namespace MediaButler.Shared.Domain.Entities;

public class Movie : BaseEntity
{
    private readonly List<AudioStream> _audioStreams;
    public string Name { get; private set; }
    public FourDigitYear Year { get; private set; }
    public Location Location { get; private set; }
    public VideoStream VideoStream { get; private set; }

    public IEnumerable<AudioStream> AudioStreams => _audioStreams;

    private Movie()
    {
        
    }

    public Movie(string name, FourDigitYear year, Location location, VideoStream videoStream, List<AudioStream> audioStreams)
    {
        _audioStreams = audioStreams;
        Name = name;
        Year = year;
        Location = location;
        VideoStream = videoStream;
    }
}