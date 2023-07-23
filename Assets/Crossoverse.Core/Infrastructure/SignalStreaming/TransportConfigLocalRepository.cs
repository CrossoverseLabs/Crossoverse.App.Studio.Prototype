using System;
using System.Threading.Tasks;
using Crossoverse.Core.Configuration;
using UnityEngine;

namespace Crossoverse.Core.Infrastructure.SignalStreaming
{
    [CreateAssetMenu(
        menuName = "Crossoverse/LocalRepository/" + nameof(TransportConfigLocalRepository),
        fileName = nameof(TransportConfigLocalRepository))]
    public class TransportConfigLocalRepository : ScriptableObject, IConfigurationRepository<string>
    {
        [SerializeField] string _photonRealtime_PunVersion = "2.4.0";
        [SerializeField] string _photonRealtime_AppVersion = "";
        [SerializeField] string _photonRealtime_Region = "jp";
        [SerializeField] string _photonRealtime_AppId = "";

        public string Find(string key)
        {
            var value = key switch
            {
                "Transport:PhotonRealtime:PunVersion" => _photonRealtime_PunVersion,
                "Transport:PhotonRealtime:AppVersion" => _photonRealtime_AppVersion,
                "Transport:PhotonRealtime:Region" => _photonRealtime_Region,
                "Transport:PhotonRealtime:AppId" => _photonRealtime_AppId,
                _ => "Unknown",
            };

            return value;
        }

        public async Task<string> FindAsync(string key)
        {
            throw new NotImplementedException();
        }

        public void Save(string key, string value)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SaveAsync(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
