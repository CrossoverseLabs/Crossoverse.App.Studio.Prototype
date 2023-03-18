using System;
using System.Collections.Generic;

namespace Crossoverse.StudioApp.Configuration
{
    [Serializable]
    public class Stage
    {
        public StageName Name;
        public List<SceneName> Scenes = new List<SceneName>();
        public SceneName ActiveSceneName = SceneName.None;
    }
}