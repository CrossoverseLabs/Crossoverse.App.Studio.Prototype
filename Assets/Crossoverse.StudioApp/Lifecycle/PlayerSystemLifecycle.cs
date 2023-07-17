using Crossoverse.Core.Context;
using Crossoverse.Core.Domain.ResourceProvider;
using Crossoverse.Infrastructure.Persistence;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Test.Context;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Crossoverse.StudioApp.Application
{
    public class PlayerSystemLifecycle : LifetimeScope
    {
        [Header("Instances")]
        [SerializeField] RuntimeAnimatorController _motionActorAnimatorController;

        enum TestMode
        {
            Local = 0,
            // LocalServer = 1,
            // Remote = 2,
        }

        [Header("Testing / Debugging")]
        [SerializeField] TestMode _testMode = TestMode.Local;
        [SerializeField] GameObject _testModeObjectRoot;
        [SerializeField] SceneConfiguration _sceneConfiguration;
        [SerializeField] AvatarMetadataLocalRepository _avatarMetadataLocalRepository;

        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent != null) // Run as an additive scene of multiple scenes.
            {
                Debug.Log($"[{nameof(PlayerSystemLifecycle)}] Run as an additive scene of multiple scenes.");
                ConfigureCore(builder);
            }
            else // Run as a single scene for testing or debugging.
            {
                Debug.Log($"[{nameof(PlayerSystemLifecycle)}] Run as a single scene for testing or debugging.");

                ConfigureCore(builder);
                ConfigureTestMode(builder);

                _testModeObjectRoot.gameObject.SetActive(true);
            }
        }

        private void ConfigureCore(IContainerBuilder builder)
        {
            builder.Register<AvatarContext>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf()
                .WithParameter<RuntimeAnimatorController>(_motionActorAnimatorController);

            builder.Register<PlayerContext>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private async void Start()
        {
            var playerContext = Container.Resolve<PlayerContext>();
            await playerContext.InitializeAsync();
        }

        private void ConfigureTestMode(IContainerBuilder builder)
        {
            builder.RegisterInstance(_sceneConfiguration);
            builder.Register<SceneTransitionContextMock>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterInstance(_avatarMetadataLocalRepository).AsImplementedInterfaces();

            builder.Register<UrpVrmProvider>(Lifetime.Singleton).AsImplementedInterfaces()
                .WithParameter<IBinaryDataProvider>(new LocalFileBinaryDataProvider());
        }
    }
}