using UnityEngine;
using VContainer;
using VContainer.Unity;
using Crossoverse.Context;
using Crossoverse.Infrastructure.Persistence;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Context;
using Crossoverse.Core.Domain.ResourceProvider;
using Crossoverse.Toolkit.ResourceProvider;

namespace Crossoverse.StudioApp.Application
{
    /// <summary>
    /// Entry point
    /// </summary>
    public class MainLifecycle : LifetimeScope
    {
        [Header("Crossoverse.StudioApp")]
        
        [SerializeField] private EngineConfiguration engineConfiguration;
        [SerializeField] private SceneConfiguration sceneConfiguration;

        [SerializeField] private AvatarMetadataLocalRepository _avatarMetadataLocalRepository;
        [SerializeField] private ContentHeaderLocalRepository _contentHeaderLocalRepository;
        [SerializeField] private ResourceInfoLocalRepository _resourceInfoLocalRepository;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(engineConfiguration);
            builder.RegisterInstance(sceneConfiguration);
            builder.Register<ApplicationContext>(Lifetime.Singleton);
            builder.Register<SceneTransitionContext>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Context
            builder.Register<ContentResourceContext>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Repository
            builder.RegisterInstance(_avatarMetadataLocalRepository).AsImplementedInterfaces();
            builder.RegisterInstance(_contentHeaderLocalRepository).AsImplementedInterfaces();
            builder.RegisterInstance(_resourceInfoLocalRepository).AsImplementedInterfaces();

            // ResourceProvider
            builder.Register<AddressableResourceProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<UrpVrmProvider>(Lifetime.Singleton).AsImplementedInterfaces()
                    .WithParameter<IBinaryDataProvider>(new LocalFileBinaryDataProvider());
        }
        
        private async void Start()
        {
            var applicationContext = Container.Resolve<ApplicationContext>();
            var sceneTransitionContext = Container.Resolve<SceneTransitionContext>();
            
            applicationContext.Initialize();
            await sceneTransitionContext.LoadGlobalScenesAndInitialStageAsync();
        }
    }
}