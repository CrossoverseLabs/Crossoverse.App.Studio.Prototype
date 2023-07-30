using MessagePipe;

namespace Crossoverse.Core.Domain.SignalStreaming
{
    public interface ILowFreqEventStreamingChannel : ISignalStreamingChannel
    {
        ISubscriber<LowFreqSignal.TextMessageSignal> OnTextMessageReceived { get; }
        void Send<T>(T signal) where T : ILowFreqSignal;
    }
}
