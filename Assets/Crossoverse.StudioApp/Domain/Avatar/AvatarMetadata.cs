using System;

namespace Crossoverse.Domain.Avatar
{
    [Serializable]
    public class AvatarMetadata
    {
        public string Name;
        public string Id;
        public string OwnerId;
        public string ResourcePath;
    }
}