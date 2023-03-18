using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crossoverse.StudioApp.Configuration
{
    [Serializable]
    [CreateAssetMenu(menuName = "Crossoverse/StudioApp/Create SceneConfiguration", fileName = "SceneConfiguration")]
    public sealed class SceneConfiguration : ScriptableObject
    {
        public List<SceneName> GlobalScenes = new List<SceneName>();
        public SceneName InitialActiveScene = SceneName.DefaultStage;
        
        [Header("Stage Transition")]
        public List<Stage> Stages = new List<Stage>();
    }
}