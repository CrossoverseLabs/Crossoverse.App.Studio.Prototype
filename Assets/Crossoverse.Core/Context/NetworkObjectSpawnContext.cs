using System;
using System.Collections.Generic;
using Crossoverse.Core.Domain.Multiplayer;
using Crossoverse.SignalStreaming;
using Crossoverse.SignalStreaming.LowFreqSignal;
using Crossoverse.SignalStreaming.BufferedSignal;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace Crossoverse.Core.Context
{
    public sealed class NetworkObjectSpawnContext : IDisposable
    {
        public ISubscriber<INetworkObject> OnCreateObject { get; }
        public ISubscriber<INetworkObject> OnDestroyingObject { get; }

        private readonly IDisposablePublisher<INetworkObject> _createObjectEventPublisher;
        private readonly IDisposablePublisher<INetworkObject> _destroyingObjectEventPublisher;

        private readonly List<IGuidComponent> _prefabs = new();
        private readonly Dictionary<int, INetworkObject> _instances = new();

        private readonly SignalStreamingContext _signalStreamingContext;
        private readonly INetworkObjectFactory _networkObjectFactory;

        private IBufferedSignalStreamingChannel _bufferedSignalStreamingChannel;
        private ILowFreqSignalStreamingChannel _lowFreqSignalStreamingChannel;

        private Dictionary<string, IDisposable> _channelDisposables = new();
        private IDisposable _contextDisposable;

        public NetworkObjectSpawnContext
        (
            SignalStreamingContext signalStreamingContext,
            INetworkObjectFactory networkObjectFactory,
            EventFactory eventFactory
        )
        {
            _signalStreamingContext = signalStreamingContext;
            _networkObjectFactory = networkObjectFactory;
            (_createObjectEventPublisher, OnCreateObject) = eventFactory.CreateEvent<INetworkObject>();
            (_destroyingObjectEventPublisher, OnDestroyingObject) = eventFactory.CreateEvent<INetworkObject>();
        }

        public void Initialize()
        {
            InitializeSignalStreaming();
        }

        public void Dispose()
        {
            foreach (var instance in _instances.Values)
            {
                _destroyingObjectEventPublisher.Publish(instance);
                instance.Dispose();
            }

            _instances.Clear();
            _createObjectEventPublisher.Dispose();
            _destroyingObjectEventPublisher.Dispose();

            foreach (var disposable in _channelDisposables.Values)
            {
                disposable.Dispose();
            }
            _channelDisposables.Clear();

            _contextDisposable.Dispose();
        }

        public void AddPrefabs(List<IGuidComponent> prefabs)
        {
            _prefabs.AddRange(prefabs);
        }

        public bool TryGetInstance(int instanceId, out INetworkObject instance)
        {
            return _instances.TryGetValue(instanceId, out instance);
        }

        public int CreateObject(Guid originalObjectId)
        {
            var instanceId = _networkObjectFactory.NewInstanceId();

            var signal = new CreateObjectSignal()
            {
                OriginalObjectId = originalObjectId,
                InstanceId = instanceId,
                FilterKey = instanceId,
                GeneratedBy = _signalStreamingContext.StreamingClientId,
                OriginTimestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            };

            _bufferedSignalStreamingChannel.Send(signal);

            return instanceId;
        }

        public void DestroyObject(int instanceId)
        {
            _bufferedSignalStreamingChannel.RemoveBufferedSignal(SignalType.CreateObject, _signalStreamingContext.StreamingClientId, instanceId);

            var signal = new DestroyObjectSignal()
            {
                InstanceId = instanceId,
                GeneratedBy = _signalStreamingContext.StreamingClientId,
                OriginTimestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            };

            _lowFreqSignalStreamingChannel.Send(signal);
        }

        private async void HandleCreateObjectSignalAsync(CreateObjectSignal signal)
        {
            await UniTask.SwitchToMainThread();

            foreach (var prefab in _prefabs)
            {
                if (prefab.Guid == signal.OriginalObjectId)
                {
                    var instance = _networkObjectFactory.Create(prefab, signal.InstanceId);
                    _instances.TryAdd(signal.InstanceId, instance);
                    _createObjectEventPublisher.Publish(instance);
                }
            }
        }

        private async void HandleDestroyObjectSignalAsync(DestroyObjectSignal signal)
        {
            await UniTask.SwitchToMainThread();

            if (_instances.Remove(signal.InstanceId, out var instance))
            {
                _destroyingObjectEventPublisher.Publish(instance);
                instance.Dispose();
            }
        }

        private void InitializeSignalStreaming()
        {
            var contextDisposableBag = DisposableBag.CreateBuilder();

            _signalStreamingContext.OnStreamingChannelAdded
                .Subscribe(streamingChannel =>
                {
                    var channelDisposableBag = DisposableBag.CreateBuilder();

                    if (streamingChannel is IBufferedSignalStreamingChannel bufferedSignalStreamingChannel)
                    {
                        _bufferedSignalStreamingChannel = bufferedSignalStreamingChannel;

                        _bufferedSignalStreamingChannel.OnCreateObjectSignalReceived
                            .Subscribe(HandleCreateObjectSignalAsync)
                            .AddTo(channelDisposableBag);
                    }
                    else if (streamingChannel is ILowFreqSignalStreamingChannel lowFreqSignalStreamingChannel)
                    {
                        _lowFreqSignalStreamingChannel = lowFreqSignalStreamingChannel;

                        _lowFreqSignalStreamingChannel.OnDestroyObjectSignalReceived
                            .Subscribe(HandleDestroyObjectSignalAsync)
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
                    if (_bufferedSignalStreamingChannel.Id == streamingChannelId)
                    {
                        _bufferedSignalStreamingChannel = null;
                    }
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
    }
}
