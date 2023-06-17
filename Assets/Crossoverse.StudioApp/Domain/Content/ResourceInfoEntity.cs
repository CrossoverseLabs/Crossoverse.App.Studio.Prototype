using System;

namespace Crossoverse.Domain.Content
{
    [Serializable]
    public class ResourceInfoEntity
    {
        public string Title;
        public string EntityId;
        public ResourceType ResourceType;
        public string ResourceBasePath;
        public string ResourceName;
    }
}