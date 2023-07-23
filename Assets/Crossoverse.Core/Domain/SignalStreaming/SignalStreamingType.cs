namespace Crossoverse.Core.Domain.SignalStreaming
{
    public enum SignalType
    {
        // Low Frequency
        LowFreqEvent = 1,

        // High Frequency
        HighFreqEvent = 128,
    }

    public enum StreamingType
    {
        Bidirectional = 1,
        ClientToServer = 2,
        ServerToClient = 3,
    }
}
