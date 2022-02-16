using MediaButler.Shared.Domain.Common;

namespace MediaButler.Shared.Domain.ValueObjects;

public class AudioStream : ValueObject
{
    public string CodecFriendly { get; private set; }
    public int Bitrate { get; private set; }
    public int Channel { get; private set; }
    public int SamplingRate { get; private set; }
    public int BitDepth { get; private set; }
    public string CodecName { get; private set; }
    public string AudioChannelsFriendly { get; private set; }
    public string Language { get; private set; }

    private AudioStream()
    {
        
    }
    
    public AudioStream(string codecFriendly,
        int bitrate,
        int channel,
        int samplingRate,
        int bitDepth,
        string codecName,
        string audioChannelsFriendly, string language)
    {
        CodecFriendly = codecFriendly;
        Bitrate = bitrate;
        Channel = channel;
        SamplingRate = samplingRate;
        BitDepth = bitDepth;
        CodecName = codecName;
        AudioChannelsFriendly = audioChannelsFriendly;
        Language = language;
    }
}