using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using Crossoverse.Toolkit.ResourceProvider;
using Crossoverse.Context;
using Crossoverse.Domain.Content;
using Crossoverse.Infrastructure.Persistence;
using Crossoverse.StudioApp.Configuration;

namespace Crossoverse.StudioApp.Lifecycle
{
    public class ContentStageLifecycle : LifetimeScope
    {
        enum TestMode
        {
            Local = 0,
            LocalServer = 1,
            Remote = 2,
        }

        [Header("Testing / Debugging")]
        [SerializeField] TestMode _testMode = TestMode.Local;
        [SerializeField] GameObject _sceneTestOnlyObjectRoot;
        [SerializeField] SceneConfiguration _sceneConfiguration;
        [SerializeField] private ContentHeaderLocalRepository _contentHeaderLocalRepository;
        [SerializeField] private ResourceInfoLocalRepository _resourceInfoLocalRepository;

        private bool _isSceneTestMode;

        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent != null)
            // Run as an additive scene of multiple scenes.
            {
                Debug.Log($"[{nameof(ContentStageLifecycle)}] Run as an additive scene of multiple scenes.");
                ConfigureCore(builder);
            }
            else
            // Run as a single scene for testing or debugging.
            {
                Debug.Log($"[{nameof(ContentStageLifecycle)}] Run as a single scene for testing or debugging.");

                ConfigureCore(builder);
                ConfigureTestMode(builder);

                _isSceneTestMode = true;
                _sceneTestOnlyObjectRoot.SetActive(true);
            }
        }

        private void ConfigureCore(IContainerBuilder builder)
        {
            builder.Register<ContentStageContext>(Lifetime.Singleton).AsSelf();
        }

        private async UniTask Start()
        {
            if (_isSceneTestMode) await StartTestModeAsync();
            var contentStageContext = Container.Resolve<ContentStageContext>();
            await contentStageContext.InitializeAsync();
        }

        private void ConfigureTestMode(IContainerBuilder builder)
        {
            builder.Register<ContentResourceContext>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<AddressableResourceProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            if (_testMode is TestMode.Local)
            {
                builder.RegisterInstance(_contentHeaderLocalRepository).AsImplementedInterfaces();
                builder.RegisterInstance(_resourceInfoLocalRepository).AsImplementedInterfaces();
            }
            else
            if (_testMode is TestMode.LocalServer)
            {
                builder.RegisterInstance(_contentHeaderLocalRepository).AsImplementedInterfaces();
                builder.RegisterInstance(_resourceInfoLocalRepository).AsImplementedInterfaces();
            }
            else
            if (_testMode is TestMode.Remote)
            {
                builder.RegisterInstance(_contentHeaderLocalRepository).AsImplementedInterfaces();
                builder.RegisterInstance(_resourceInfoLocalRepository).AsImplementedInterfaces();
            }
        }

        private async UniTask StartTestModeAsync()
        {
            if (_isSceneTestMode)
            {
                var contentResourceContext = Container.Resolve<IContentResourceContext>();
                var contentHeaderRepository = Container.Resolve<IContentHeaderRepository>();
                if (_testMode is TestMode.Local)
                {
                    var entity = await contentHeaderRepository.FindByIdAsync("SampleContent01_StreamingAssets");
                    if (entity is not null)
                    {
                        Debug.Log($"[{nameof(ContentStageLifecycle)}] TestMode Entity: {entity.Id}");
                        contentResourceContext.SetNextContentId(entity.Id);
                    }
                }
                else
                if (_testMode is TestMode.LocalServer)
                {
                    var entity = await contentHeaderRepository.FindByIdAsync("SampleContent02_LocalServer");
                    if (entity is not null)
                    {
                        Debug.Log($"[{nameof(ContentStageLifecycle)}] TestMode Entity: {entity.Id}");
                        contentResourceContext.SetNextContentId(entity.Id);
                    }
                }
                else
                if (_testMode is TestMode.Remote)
                {
                    var entity = await contentHeaderRepository.FindByIdAsync("SampleContent01_Remote");
                    // var entity = await contentHeaderRepository.FindByIdAsync("SampleContent02_Remote");
                    if (entity is not null)
                    {
                        Debug.Log($"[{nameof(ContentStageLifecycle)}] TestMode Entity: {entity.Id}");
                        contentResourceContext.SetNextContentId(entity.Id);
                    }
                }
            }
        }
    }
}