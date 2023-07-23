using System.Buffers;
using System.Threading;
using Crossoverse.Toolkit.Transports;
using Crossoverse.Toolkit.Serialization;
using Crossoverse.Core.Domain.SignalStreaming;
using Cysharp.Threading.Tasks;
using MessagePack;
using MessagePipe;

namespace Crossoverse.Core.Infrastructure.SignalStreaming
{
    public sealed class LowFreqEventStreamingChannel : ILowFreqEventStreamingChannel
    {
        public string Id => _id;
        public SignalType SignalType => SignalType.LowFreqEvent;
        public StreamingType StreamingType => StreamingType.Bidirectional;

        public bool IsConnected => _isConnected;

        public IBufferedSubscriber<bool> ConnectionStateSubscriber { get; }
        public ISubscriber<LowFreqEventSignal> OnEventReceived { get; }

        private readonly IDisposablePublisher<LowFreqEventSignal> _eventPublisher;
        private readonly IDisposableBufferedPublisher<bool> _connectionStatePublisher;

        private readonly IMessageSerializer _messageSerializer = new MessagePackMessageSerializer();
        private readonly ITransport _transport;
        private readonly string _id;

        private bool _isConnected;
        private bool _initialized;

        public LowFreqEventStreamingChannel
        (
            string id,
            ITransport transport,
            EventFactory eventFactory
        )
        {
            _id = id;
            _transport = transport;
            (_eventPublisher, OnEventReceived) = eventFactory.CreateEvent<LowFreqEventSignal>();
            (_connectionStatePublisher, ConnectionStateSubscriber) = eventFactory.CreateBufferedEvent<bool>(_isConnected);
        }

        public void Initialize()
        {
            _transport.OnReceiveMessage += OnMessageReceived;
            _initialized = true;
        }

        public void Dispose()
        {
            _transport.OnReceiveMessage -= OnMessageReceived;
            _eventPublisher.Dispose();
            _connectionStatePublisher.Dispose();
        }

        public async UniTask<bool> ConnectAsync(CancellationToken token = default)
        {
            if (!_initialized) Initialize();

            if (_isConnected)
            {
                DevelopmentOnlyLogger.Log($"<color=orange>[{nameof(LowFreqEventStreamingChannel)}] Already connected.</color>");
                return true;
            }

            _isConnected = await _transport.ConnectAsync(_id);
            _connectionStatePublisher.Publish(_isConnected);

            return _isConnected;
        }

        public async UniTask DisconnectAsync()
        {
            if (!_isConnected) return;

            await _transport.DisconnectAsync();
            _isConnected = false;

            _connectionStatePublisher.Publish(_isConnected);
        }

        public void SendEvent(LowFreqEventSignal signal)
        {
            DevelopmentOnlyLogger.Log($"<color=lime>[{nameof(LowFreqEventStreamingChannel)}] SendEvent</color>");

            using var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter();

            var signalId = (int)SignalType.LowFreqEvent;

            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(signalId);
            writer.Write(_transport.ClientId);
            writer.Flush();

            _messageSerializer.Serialize(buffer, signal);

            _transport.Send(buffer.WrittenSpan.ToArray());
        }

        private void OnMessageReceived(byte[] serializedMessage)
        {
            DevelopmentOnlyLogger.Log($"<color=lime>[{nameof(LowFreqEventStreamingChannel)}] OnMessageReceived</color>");

            var messagePackReader = new MessagePackReader(serializedMessage);

            var arrayLength = messagePackReader.ReadArrayHeader();
            if (arrayLength != 3)
            {
                DevelopmentOnlyLogger.LogError($"[{nameof(LowFreqEventStreamingChannel)}] The received message is unsupported format.");
            }

            var signalId = messagePackReader.ReadInt32();
            var networkClientId = messagePackReader.ReadInt32();
            var offset = (int)messagePackReader.Consumed;

            var signal = _messageSerializer.Deserialize<LowFreqEventSignal>(new ReadOnlySequence<byte>(serializedMessage, offset, serializedMessage.Length - offset));
            _eventPublisher.Publish(signal);
        }
    }
}