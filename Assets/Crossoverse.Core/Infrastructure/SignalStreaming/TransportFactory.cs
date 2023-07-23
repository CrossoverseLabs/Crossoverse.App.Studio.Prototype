using Crossoverse.Core.Configuration;
using Crossoverse.Core.Domain.SignalStreaming;
using Crossoverse.Toolkit.Transports;
#if CROSSOVERSE_PHOTON_TRANSPORT
using Crossoverse.Toolkit.Transports.PhotonRealtime;
#endif

namespace Crossoverse.Core.Infrastructure.SignalStreaming
{
    public sealed class TransportFactory : ITransportFactory
    {
        private readonly IConfigurationRepository<string> _configurationRepository;

        public TransportFactory
        (
            IConfigurationRepository<string> configurationRepository
        )
        {
            _configurationRepository = configurationRepository;
        }

        public ITransport Create(string channelId, SignalType signalType, StreamingType streamingType)
        {
            if (signalType == SignalType.LowFreqEvent)
            {
#if CROSSOVERSE_PHOTON_TRANSPORT
                return CreatePhotonRealtimeTransport(channelId, 5);
#endif
            }
            else if (signalType == SignalType.HighFreqEvent)
            {
#if CROSSOVERSE_PHOTON_TRANSPORT
                return CreatePhotonRealtimeTransport(channelId, 30);
#endif
            }

            DevelopmentOnlyLogger.LogError($"[{nameof(SignalStreamingChannelFactory)}] Could not create transport. StreamingType: {streamingType}, SignalType: {signalType}.");
            return null;
        }

#if CROSSOVERSE_PHOTON_TRANSPORT
        private PhotonRealtimeTransport CreatePhotonRealtimeTransport(string channelId, int updateRatePerSecond)
        {
            var punVersion = _configurationRepository.Find("Transport:PhotonRealtime:PunVersion");
            var appVersion = _configurationRepository.Find("Transport:PhotonRealtime:AppVersion");
            var region = _configurationRepository.Find("Transport:PhotonRealtime:Region");
            var appId = _configurationRepository.Find("Transport:PhotonRealtime:AppId");

            var appSettings = new Photon.Realtime.AppSettings()
            {
                AppVersion = appVersion,
                AppIdRealtime = appId,
                FixedRegion = region,
            };

            return new PhotonRealtimeTransport
            (
                punVersion: punVersion,
                connectParameters: new PhotonRealtimeConnectParameters(){ AppSettings = appSettings },
                joinParameters: new PhotonRealtimeJoinParameters(){ RoomName = channelId },
                targetFrameRate: updateRatePerSecond,
                isBackgroundThread: true,
                protocol: ExitGames.Client.Photon.ConnectionProtocol.Udp,
                receiverGroup: Photon.Realtime.ReceiverGroup.All
                // receiverGroup: Photon.Realtime.ReceiverGroup.Others
            );
        }
#endif
    }
}
