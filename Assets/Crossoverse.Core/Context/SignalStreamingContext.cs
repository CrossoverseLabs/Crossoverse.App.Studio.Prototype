using System;
using System.Collections.Generic;
using System.Threading;
using Crossoverse.SignalStreaming;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace Crossoverse.Core.Context
{
    public sealed class SignalStreamingContext : IDisposable
    {
        public ISubscriber<ISignalStreamingChannel> OnStreamingChannelAdded { get; }
        public ISubscriber<string> OnStreamingChannelRemoved { get; }

        public Guid StreamingClientId => _streamingClientId;

        private readonly IDisposablePublisher<ISignalStreamingChannel> _streamingChannelAddedEventPublisher;
        private readonly IDisposablePublisher<string> _streamingChannelRemovedEventPublisher;

        private readonly ISignalStreamingChannelFactory _streamingChannelFactory;
        private readonly Dictionary<string, ISignalStreamingChannel> _streamingChannels = new();

        private Guid _streamingClientId = new();

        public SignalStreamingContext
        (
            ISignalStreamingChannelFactory streamingChannelFactory,
            EventFactory eventFactory
        )
        {
            _streamingChannelFactory = streamingChannelFactory;
            (_streamingChannelAddedEventPublisher, OnStreamingChannelAdded) = eventFactory.CreateEvent<ISignalStreamingChannel>();
            (_streamingChannelRemovedEventPublisher, OnStreamingChannelRemoved) = eventFactory.CreateEvent<string>();
        }

        public async UniTask<bool> ConnectAsync(string channelId, SignalType signalType, StreamingType streamingType, CancellationToken cancellationToken = default)
        {
            if (_streamingChannels.TryGetValue(channelId, out var streamingChannel))
            {
                return await streamingChannel.ConnectAsync(cancellationToken);
            }
            else
            {
                streamingChannel = _streamingChannelFactory.Create(channelId, signalType, streamingType);
                if (_streamingChannels.TryAdd(streamingChannel.Id, streamingChannel))
                {
                    _streamingChannelAddedEventPublisher.Publish(streamingChannel);
                    return await streamingChannel.ConnectAsync(cancellationToken);
                }
            }

            return false;
        }

        public async UniTask DisconnectAsync(string channelId)
        {
            if (_streamingChannels.TryGetValue(channelId, out var streamingChannel))
            {
                await streamingChannel.DisconnectAsync();
            }
        }

        public void RemoveChannel(string channelId)
        {
            if (_streamingChannels.Remove(channelId, out var streamingChannel))
            {
                _streamingChannelRemovedEventPublisher.Publish(streamingChannel.Id);
                streamingChannel.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var streamingChannel in _streamingChannels.Values)
            {
                _streamingChannelRemovedEventPublisher.Publish(streamingChannel.Id);
                streamingChannel.Dispose();
            }

            _streamingChannels.Clear();
            _streamingChannelAddedEventPublisher.Dispose();
            _streamingChannelRemovedEventPublisher.Dispose();
        }
    }
}
