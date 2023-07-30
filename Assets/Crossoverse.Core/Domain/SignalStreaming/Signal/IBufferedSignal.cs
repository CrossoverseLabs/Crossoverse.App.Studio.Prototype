using System;

namespace Crossoverse.Core.Domain.SignalStreaming
{
    public interface IBufferedSignal
    {
        Guid GeneratedBy { get; }
        long OriginTimestampMilliseconds { get; } // UnixTimeMilliseconds
    }
}