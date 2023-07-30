using System;
using System.Collections.Generic;
using Crossoverse.Core.Context;
using Crossoverse.Core.Domain.SignalStreaming;
using Crossoverse.Core.Domain.SignalStreaming.LowFreqSignal;
using Crossoverse.Core.Infrastructure.SignalStreaming;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace Crossoverse.Core.Test
{
    public class SignalStreamingTest : MonoBehaviour
    {
        [SerializeField] TransportConfigLocalRepository _transportConfigRepository;

        private EventFactory _eventFactory;

        private SignalStreamingContext _signalStreamingContext;
        private ISignalStreamingChannelFactory _streamingChannelFactory;
        private ILowFreqSignalStreamingChannel _lowFreqSignalStreamingChannel;

        private Dictionary<string, IDisposable> _channelDisposables = new();
        private IDisposable _contextDisposable;

        private int _receivedCount;

        void Awake()
        {
            var builder = new BuiltinContainerBuilder();
            builder.AddMessagePipe();

            var provider = builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);

            _eventFactory = provider.GetRequiredService<EventFactory>();
        }

        void OnDestroy()
        {
            _contextDisposable?.Dispose();
            _signalStreamingContext.Dispose();
        }

        async void Start()
        {
            Initialize();

            var channelId = "LowFreqEventTest";

            await _signalStreamingContext.ConnectAsync(channelId, SignalType.LowFreqSignal, StreamingType.Bidirectional);

            SendLowFreqEventSignal($"LowFreqSignalTest");

            await UniTask.WaitUntil(() => _receivedCount > 0);
            await _signalStreamingContext.DisconnectAsync(channelId);

            SendLowFreqEventSignal($"LowFreqEventTest2");
        }

        private void Initialize()
        {
            _streamingChannelFactory = new SignalStreamingChannelFactory(new TransportFactory(_transportConfigRepository), _eventFactory);
            _signalStreamingContext = new SignalStreamingContext(_streamingChannelFactory, _eventFactory);

            var contextDisposableBag = DisposableBag.CreateBuilder();

            _signalStreamingContext.OnStreamingChannelAdded
                .Subscribe(streamingChannel =>
                {
                    var channelDisposableBag = DisposableBag.CreateBuilder();

                    if (streamingChannel is ILowFreqSignalStreamingChannel lowFreqSignalStreamingChannel)
                    {
                        _lowFreqSignalStreamingChannel = lowFreqSignalStreamingChannel;
                        _lowFreqSignalStreamingChannel.OnTextMessageReceived
                            .Subscribe(OnTextMessageReceived)
                            .AddTo(channelDisposableBag);
                    }

                    if (_channelDisposables.TryGetValue(streamingChannel.Id, out var channelDisposable))
                    {
                        channelDisposableBag.Add(channelDisposable);
                    }

                    _channelDisposables[streamingChannel.Id] = channelDisposableBag.Build();
                })
                .AddTo(contextDisposableBag);

            _signalStreamingContext.OnStreamingChannelRemoved
                .Subscribe(streamingChannelId =>
                {
                    if (_lowFreqSignalStreamingChannel.Id == streamingChannelId)
                    {
                        _lowFreqSignalStreamingChannel = null;
                    }

                    _channelDisposables.Remove(streamingChannelId, out var disposable);
                    disposable?.Dispose();
                })
                .AddTo(contextDisposableBag);

            _contextDisposable = contextDisposableBag.Build();
        }

        private void SendLowFreqEventSignal(string message)
        {
            var timestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var signal = new TextMessageSignal()
            {
                Message = message,
                GeneratedBy = Guid.NewGuid(),
                OriginTimestampMilliseconds = timestampMilliseconds,
            };
            _lowFreqSignalStreamingChannel.Send(signal);

            Debug.Log($"----------");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Send message: \"{message}\"</color>");
            Debug.Log($"----------");
        }

        private void OnTextMessageReceived(TextMessageSignal signal)
        {
            var format = "yyyy/MM/dd HH:mm:ss.fff";
            var originTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(signal.OriginTimestampMilliseconds).ToString(format);
            var nowTimestamp = DateTime.Now.ToString(format);
            _receivedCount++;

            Debug.Log($"----------");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Received message: \"{signal.Message}\"</color>");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Origin timestamp: {originTimestamp}</color>");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Now timestamp: {nowTimestamp}</color>");
            Debug.Log($"----------");
        }
    }
}
