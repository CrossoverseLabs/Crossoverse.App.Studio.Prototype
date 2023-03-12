using System;
using UnityEngine;

namespace Crossoverse.StudioApp.Configuration
{
    [Serializable]
    [CreateAssetMenu(menuName = "Crossoverse/StudioApp/Create EngineConfiguration", fileName = "EngineConfiguration")]
    public sealed class EngineConfiguration : ScriptableObject
    {
        public int TargetFrameRate = 60;
        public bool EnableDebugLogger = true;
    }
}