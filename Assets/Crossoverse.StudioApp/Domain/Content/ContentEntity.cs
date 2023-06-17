using System;
using System.Collections.Generic;

namespace Crossoverse.Domain.Content
{
    [Serializable]
    public class ContentEntity
    {
        public string Name;
        public string Id;
        public string Category;
        public ResourceInfoEntity SkyboxMaterial;
        public List<ResourceInfoEntity> Prefabs = new List<ResourceInfoEntity>();
    }
}