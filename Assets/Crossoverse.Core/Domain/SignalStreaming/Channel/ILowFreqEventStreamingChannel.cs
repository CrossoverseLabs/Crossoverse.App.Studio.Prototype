using MessagePipe;

namespace Crossoverse.Core.Domain.SignalStreaming
{
    public interface ILowFreqEventStreamingChannel : ISignalStreamingChannel
    {
        ISubscriber<LowFreqEventSignal> OnEventReceived { get; }
        void SendEvent(LowFreqEventSignal signal);
    }
}
