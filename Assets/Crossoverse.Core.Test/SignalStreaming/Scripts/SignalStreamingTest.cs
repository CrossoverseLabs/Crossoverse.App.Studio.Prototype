using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using Crossoverse.Core.Context;
using Crossoverse.SignalStreaming;
using Crossoverse.SignalStreaming.LowFreqSignal;
using Crossoverse.SignalStreaming.BufferedSignal;
using Crossoverse.SignalStreaming.Infrastructure;
using Crossoverse.SignalStreaming.Infrastructure.Unity;
using Crossoverse.Core.Unity.Multiplayer;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace Crossoverse.Core.Test
{
    public class SignalStreamingTest : MonoBehaviour
    {
        [SerializeField] TransportConfigLocalRepository _transportConfigRepository;
        [SerializeField] List<GuidComponent> _prefabs;

        private readonly Dictionary<int, GameObject> _instances = new();

        private EventFactory _eventFactory;

        private SignalStreamingContext _signalStreamingContext;
        private ISignalStreamingChannelFactory _streamingChannelFactory;
        private ILowFreqSignalStreamingChannel _lowFreqSignalStreamingChannel;
        private IBufferedSignalStreamingChannel _bufferedSignalStreamingChannel;

        private Dictionary<string, IDisposable> _channelDisposables = new();
        private IDisposable _contextDisposable;

        private Guid _clientId;
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

            var lowFreqSignalChannel = "LowFreqEventTest";
            var bufferedSignalChannel = "BufferedSignalTest";

            await _signalStreamingContext.ConnectAsync(lowFreqSignalChannel, SignalType.LowFreqSignal, StreamingType.Bidirectional);
            await _signalStreamingContext.ConnectAsync(bufferedSignalChannel, SignalType.BufferedSignal, StreamingType.Bidirectional);
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Connected</color>");

            // SendLowFreqEventSignal($"LowFreqSignalTest");

            // await UniTask.WaitUntil(() => _receivedCount > 0);
            // await _signalStreamingContext.DisconnectAsync(channelId);

            // SendLowFreqEventSignal($"LowFreqEventTest2");
            // SendLowFreqEventSignal($"LowFreqEventTest3");

            Debug.Log($"----------");
            for (var i = 0; i < _prefabs.Count; i++)
            {
                Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] prefabs[{i}].Guid: {_prefabs[i].Guid}</color>");
            }
            Debug.Log($"----------");

            var instanceId = SendCreateObjectSignal(_prefabs[0].Guid);
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            SendDestroyObjectSignal(instanceId);
        }

        private void Initialize()
        {
            _clientId = Guid.NewGuid();

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
                        _lowFreqSignalStreamingChannel.OnDestroyObjectSignalReceived
                            .Subscribe(OnDestroyObjectSignalReceivedAsync)
                            .AddTo(channelDisposableBag);
                    }
                    else if (streamingChannel is IBufferedSignalStreamingChannel bufferedSignalStreamingChannel)
                    {
                        _bufferedSignalStreamingChannel = bufferedSignalStreamingChannel;
                        _bufferedSignalStreamingChannel.OnCreateObjectSignalReceived
                            .Subscribe(OnCreateObjectSignalReceivedAsync)
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
                GeneratedBy = _clientId,
                OriginTimestampMilliseconds = timestampMilliseconds,
            };
            _lowFreqSignalStreamingChannel.Send(signal);

            Debug.Log($"----------");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Send message: \"{message}\"</color>");
            Debug.Log($"----------");
        }

        private int SendCreateObjectSignal(Guid originalObjectId)
        {
            var instanceId = BitConverter.ToInt32(XxHash32.Hash(Guid.NewGuid().ToByteArray()));

            var signal = new CreateObjectSignal()
            {
                OriginalObjectId = originalObjectId,
                InstanceId = instanceId,
                FilterKey = instanceId,
                GeneratedBy = _clientId,
                OriginTimestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            };

            _bufferedSignalStreamingChannel.Send(signal);

            return instanceId;
        }

        private void SendDestroyObjectSignal(int instanceId)
        {
            _bufferedSignalStreamingChannel.RemoveBufferedSignal(SignalType.CreateObject, _clientId, instanceId);

            var signal = new DestroyObjectSignal()
            {
                InstanceId = instanceId,
                GeneratedBy = _clientId,
                OriginTimestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            };

            _lowFreqSignalStreamingChannel.Send(signal);
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

        private async void OnCreateObjectSignalReceivedAsync(CreateObjectSignal signal)
        {
            await UniTask.SwitchToMainThread();

            var format = "yyyy/MM/dd HH:mm:ss.fff";
            var originTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(signal.OriginTimestampMilliseconds).ToString(format);
            Debug.Log($"----------");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] CreateObjectSignal</color>");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Original object id: \"{signal.OriginalObjectId}\"</color>");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Instance id: \"{signal.InstanceId}\"</color>");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Origin timestamp: \"{originTimestamp}\"</color>");
            Debug.Log($"----------");

            for (var i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].Guid == signal.OriginalObjectId)
                {
                    Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Instantiate prefab[{i}]</color>");
                    var instance = GameObject.Instantiate(_prefabs[i].gameObject);
                    _instances.TryAdd(signal.InstanceId, instance);
                }
            }
            Debug.Log($"----------");
        }

        private async void OnDestroyObjectSignalReceivedAsync(DestroyObjectSignal signal)
        {
            await UniTask.SwitchToMainThread();

            var format = "yyyy/MM/dd HH:mm:ss.fff";
            var originTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(signal.OriginTimestampMilliseconds).ToString(format);
            Debug.Log($"----------");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] DestroyObjectSignal</color>");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Instance id: \"{signal.InstanceId}\"</color>");
            Debug.Log($"<color=lime>[{nameof(SignalStreamingTest)}] Origin timestamp: \"{originTimestamp}\"</color>");
            Debug.Log($"----------");

            if (_instances.Remove(signal.InstanceId, out var instance))
            {
                GameObject.Destroy(instance);
            }
        }
    }
}
