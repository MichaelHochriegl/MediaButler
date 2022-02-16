using MediaButler.Shared.Domain.Common;
using MediaButler.Shared.Domain.Enums;

namespace MediaButler.Shared.Domain.ValueObjects;

public class VideoStream : ValueObject
{
    public double FrameRate { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public AspectRatio AspectRatio { get; private set; }
    public bool Interlaced { get; private set; }
    public string Format { get; private set; }
    public TimeSpan Duration { get; private set; }
    public Hdr Hdr { get; private set; }
    public int BitDepth { get; private set; }
    public string CodecName { get; private set; }
    public string Resolution { get; private set; }

    private VideoStream()
    {
        
    }
    
    public VideoStream(double frameRate,
        int width,
        int height,
        AspectRatio aspectRatio,
        bool interlaced,
        string format,
        TimeSpan duration,
        Hdr hdr,
        int bitDepth,
        string codecName,
        string resolution)
    {
        FrameRate = frameRate;
        Width = width;
        Height = height;
        AspectRatio = aspectRatio;
        Interlaced = interlaced;
        Format = format;
        Duration = duration;
        Hdr = hdr;
        BitDepth = bitDepth;
        CodecName = codecName;
        Resolution = resolution;
    }
}