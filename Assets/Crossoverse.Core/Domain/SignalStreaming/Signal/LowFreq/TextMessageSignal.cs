using System;
using MessagePack;

namespace Crossoverse.Core.Domain.SignalStreaming
{
    [MessagePackObject]
    public sealed class TextMessageSignal : ILowFreqSignal
    {
        [Key(0)]
        public string Message { get; set; }

        [Key(1)]
        public Guid GeneratedBy { get; set; }

        [Key(2)]
        public long OriginTimestampMilliseconds { get; set; }
    }
}
