using UnityEngine;
using Cysharp.Threading.Tasks;
using Crossoverse.Toolkit.ResourceProvider;
using Crossoverse.Domain.Content;

namespace Crossoverse.Context
{
    public interface IContentResourceContext
    {
        string CurrentContentName { get; }
        string CurrentContentId { get; }
        string NextContentId { get; }
        void SetNextContentId(string contentId);
        UniTask<bool> LoadContentResourceInfoAsync(string contentId);
        UniTask LoadSkyboxAsync();
    }

    public sealed class ContentResourceContext : IContentResourceContext
    {
        private readonly IContentHeaderRepository _contentHeaderRepository;
        private readonly IResourceInfoRepository _resourceInfoRepository;
        private readonly IResourceProvider _resourceProvider;

        private readonly ContentEntity _contentEntity = new();

        public string CurrentContentName { get; }
        public string CurrentContentId { get; }
        public string NextContentId => _nextContentId;

        private string _nextContentId;

        public ContentResourceContext
        (
            IContentHeaderRepository contentHeaderRepository,
            IResourceInfoRepository resourceInfoRepository,
            IResourceProvider resourceProvider
        )
        {
            _contentHeaderRepository = contentHeaderRepository;
            _resourceInfoRepository = resourceInfoRepository;
            _resourceProvider = resourceProvider;
        }

        public void SetNextContentId(string contentId)
        {
            _nextContentId = contentId;
        }

        public async UniTask<bool> LoadContentResourceInfoAsync(string contentId)
        {
            var contentHeader = await _contentHeaderRepository.FindByIdAsync(contentId);
            if (contentHeader is null) return false;

            var contentResourceInfo = await _resourceInfoRepository.FindByContentIdAsync(contentId);
            if (contentResourceInfo is null) return false;

            var catalogPath = "";

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var platform = "StandaloneWindows64";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            var platform = "StandaloneOSX";
#elif UNITY_IOS
            var platform = "iOS";
#elif UNITY_ANDROID
            var platform = "Android";
#endif

            if (contentResourceInfo.CatalogPathType is CatalogPathType.StreamingAssets)
            {
                catalogPath = System.IO.Path.Combine(Application.streamingAssetsPath, contentResourceInfo.Subdirectory, platform, contentResourceInfo.CatalogFilename);
            }
            else if (contentResourceInfo.CatalogPathType is CatalogPathType.Server)
            {
                catalogPath = System.IO.Path.Combine(contentResourceInfo.ServerUrl, contentResourceInfo.Subdirectory, platform, contentResourceInfo.CatalogFilename);
            }

            if (_resourceProvider is AddressableResourceProvider addressalbeResourceProvider)
            {
                Debug.Log($"<color=cyan>[ContentResourceContext] ContentCatalogPath: {catalogPath}</color>");
                await addressalbeResourceProvider.LoadContentCatalogAsync(catalogPath);
            }

            foreach (var resourceInfo in contentResourceInfo.ResourceInfoEntities)
            {
                if (resourceInfo.ResourceType is ResourceType.SkyboxMaterial)
                {
                    _contentEntity.SkyboxMaterial = resourceInfo;
                }
                else if (resourceInfo.ResourceType is ResourceType.Prefab)
                {
                    _contentEntity.Prefabs.Add(resourceInfo);
                }
            }

            return true;
        }

        public async UniTask LoadSkyboxAsync()
        {
            var resourceInfo = _contentEntity.SkyboxMaterial;
            var resourcePath = $"{resourceInfo.ResourceBasePath}/{resourceInfo.ResourceName}";
            var skyboxMaterial = await _resourceProvider.LoadResourceAsync(resourcePath).As<Material>();
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }
}