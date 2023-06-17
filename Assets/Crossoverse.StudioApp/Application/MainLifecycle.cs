using UnityEngine;
using VContainer;
using VContainer.Unity;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Context;

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
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(engineConfiguration);
            builder.RegisterInstance(sceneConfiguration);
            builder.Register<ApplicationContext>(Lifetime.Singleton);
            builder.Register<SceneTransitionContext>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.Register<Test.Context.ContentResourceContextMock>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
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