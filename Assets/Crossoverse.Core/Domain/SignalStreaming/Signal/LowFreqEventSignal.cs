using System;
using MessagePack;

namespace Crossoverse.Core.Domain.SignalStreaming
{
    [MessagePackObject]
    public readonly struct LowFreqEventSignal
    {
        [Key(0)]
        public readonly ArraySegment<byte> Data;

        public LowFreqEventSignal(ArraySegment<byte> data)
        {
            Data = data;
        }
    }
}
