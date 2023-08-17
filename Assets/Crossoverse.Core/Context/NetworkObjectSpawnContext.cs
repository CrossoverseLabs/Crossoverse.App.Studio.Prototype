using System;
using System.Collections.Generic;
using System.IO.Hashing;
using Crossoverse.Core.Domain.SignalStreaming;
using Crossoverse.Core.Domain.SignalStreaming.BufferedSignal;
using Crossoverse.Core.Domain.SignalStreaming.LowFreqSignal;
using Crossoverse.Core.Unity.Behaviour;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace Crossoverse.Core.Context
{
    public sealed class NetworkObjectSpawnContext : IDisposable
    {
        public ISubscriber<GameObject> OnCreateObject { get; }
        public ISubscriber<GameObject> OnDestroyingObject { get; }

        private readonly IDisposablePublisher<GameObject> _createObjectEventPublisher;
        private readonly IDisposablePublisher<GameObject> _destroyingObjectEventPublisher;

        private readonly List<GuidComponent> _prefabs = new();
        private readonly Dictionary<int, GameObject> _instances = new();

        private readonly SignalStreamingContext _signalStreamingContext;

        private IBufferedSignalStreamingChannel _bufferedSignalStreamingChannel;
        private ILowFreqSignalStreamingChannel _lowFreqSignalStreamingChannel;

        private Dictionary<string, IDisposable> _channelDisposables = new();
        private IDisposable _contextDisposable;

        public NetworkObjectSpawnContext
        (
            SignalStreamingContext signalStreamingContext,
            EventFactory eventFactory
        )
        {
            _signalStreamingContext = signalStreamingContext;
            (_createObjectEventPublisher, OnCreateObject) = eventFactory.CreateEvent<GameObject>();
            (_destroyingObjectEventPublisher, OnDestroyingObject) = eventFactory.CreateEvent<GameObject>();
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
                GameObject.Destroy(instance);
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

        public void AddPrefabs(List<GuidComponent> prefabs)
        {
            _prefabs.AddRange(prefabs);
        }

        public bool TryGetInstance(int instanceId, out GameObject instance)
        {
            return _instances.TryGetValue(instanceId, out instance);
        }

        public int CreateObject(Guid originalObjectId)
        {
            var instanceId = BitConverter.ToInt32(XxHash32.Hash(Guid.NewGuid().ToByteArray()));

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

        private async void OnCreateObjectSignalReceivedAsync(CreateObjectSignal signal)
        {
            await UniTask.SwitchToMainThread();

            foreach (var prefab in _prefabs)
            {
                if (prefab.Guid == signal.OriginalObjectId)
                {
                    var instance = GameObject.Instantiate(prefab.gameObject);
                    _instances.TryAdd(signal.InstanceId, instance);
                    _createObjectEventPublisher.Publish(instance);
                }
            }
        }

        private async void OnDestroyObjectSignalReceivedAsync(DestroyObjectSignal signal)
        {
            await UniTask.SwitchToMainThread();

            if (_instances.Remove(signal.InstanceId, out var instance))
            {
                _destroyingObjectEventPublisher.Publish(instance);
                GameObject.Destroy(instance);
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
                            .Subscribe(OnCreateObjectSignalReceivedAsync)
                            .AddTo(channelDisposableBag);
                    }
                    else if (streamingChannel is ILowFreqSignalStreamingChannel lowFreqSignalStreamingChannel)
                    {
                        _lowFreqSignalStreamingChannel = lowFreqSignalStreamingChannel;

                        _lowFreqSignalStreamingChannel.OnDestroyObjectSignalReceived
                            .Subscribe(OnDestroyObjectSignalReceivedAsync)
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
