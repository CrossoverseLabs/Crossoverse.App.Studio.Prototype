using System;
using System.Collections.Generic;

namespace Crossoverse.StudioApp.Configuration
{
    [Serializable]
    public class Stage
    {
        public string Name;
        public List<SceneName> Scenes = new List<SceneName>();
        public SceneName ActiveSceneName = SceneName.None;
    }
}