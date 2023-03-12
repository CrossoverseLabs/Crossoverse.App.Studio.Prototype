using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crossoverse.StudioApp.Configuration
{
    [Serializable]
    [CreateAssetMenu(menuName = "Crossoverse/StudioApp/Create SceneConfiguration", fileName = "SceneConfiguration")]
    public sealed class SceneConfiguration : ScriptableObject
    {
        public List<SceneName> InitialScenes = new List<SceneName>();
        public SceneName InitialActiveScene = SceneName.DefaultStage;
    }
}