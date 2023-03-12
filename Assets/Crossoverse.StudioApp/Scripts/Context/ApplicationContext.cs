using Crossoverse.StudioApp.Configuration;

namespace Crossoverse.StudioApp.Context
{
    public sealed class ApplicationContext
    {
        private EngineConfiguration _engineConfiguration;
        
        public ApplicationContext(EngineConfiguration engineConfiguration)
        {
            _engineConfiguration = engineConfiguration;
        }
        
        public void Initialize()
        {
            UnityEngine.QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = _engineConfiguration.TargetFrameRate;
            UnityEngine.Debug.unityLogger.logEnabled = _engineConfiguration.EnableDebugLogger;
            UnityEngine.Debug.Log($"[{nameof(ApplicationContext)}] Initialized");
        }
    }
}