namespace Crossoverse.Core.Domain.SignalStreaming
{
    public enum SignalType
    {
        // Buffered
        BufferedSignal = 0,

        // Low Frequency
        LowFreqSignal = 64,
        TextMessage = LowFreqSignal + 1,

        // High Frequency
        HighFreqSignal = 128,
    }
}
