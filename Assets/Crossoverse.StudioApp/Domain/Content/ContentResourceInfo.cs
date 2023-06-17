using System;
using System.Collections.Generic;

namespace Crossoverse.Domain.Content
{
    [Serializable]
    public class ContentResourceInfo
    {
        public string ContentId;
        public CatalogPathType CatalogPathType;
        public string ServerUrl;
        public string Subdirectory;
        public string CatalogFilename;
        public List<ResourceInfoEntity> ResourceInfoEntities = new List<ResourceInfoEntity>();
    }

    public enum CatalogPathType
    {
        StreamingAssets = 0,
        Server = 1,
    }
}