using System;
using MessagePack;

namespace Crossoverse.Core.Domain.SignalStreaming.BufferedSignal
{
    [MessagePackObject]
    public sealed class CreateObjectSignal : IBufferedSignal
    {
        [Key(0)]
        public Guid OriginalObjectId { get; }

        [Key(1)]
        public int InstanceId { get; }

        [Key(2)]
        public int OwnerClientId { get; }

        [Key(3)]
        public Guid GeneratedBy { get; }

        [Key(4)]
        public long OriginTimestampMilliseconds { get; }
    }
}
