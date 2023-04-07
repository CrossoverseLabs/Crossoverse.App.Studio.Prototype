using UnityEngine;
using VContainer;
using VContainer.Unity;
using Crossoverse.StudioApp.Presentation.OutputPort;
using Crossoverse.StudioApp.Presentation.UIView;
using Crossoverse.StudioApp.Test.Context;
using Crossoverse.StudioApp.Test.Presentation;

namespace Crossoverse.StudioApp.Application
{
    public class DefaultScreenLifecycle : LifetimeScope
    {
        [SerializeField] private DefaultScreenView defaultScreenView;
        
        [Header("Testing / Debugging")]
        [SerializeField] private Camera testModeCamera;
        
        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent != null) // Run as an additive scene of multiple scenes.
            {
                Debug.Log($"[{nameof(DefaultScreenLifecycle)}] Run as an additive scene of multiple scenes.");
                ConfigureCore(builder);
            }
            else // Run as a single scene for testing or debugging.
            {
                Debug.Log($"[{nameof(DefaultScreenLifecycle)}] Run as a single scene for testing or debugging.");
                ConfigureCore(builder);
                
                builder.Register<SceneTransitionContextMock>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
                builder.RegisterEntryPoint<DefaultScreenRuntimeTest>();
                
                testModeCamera.gameObject.SetActive(true);
            }
        }
        
        private void ConfigureCore(IContainerBuilder builder)
        {
            builder.RegisterComponent(defaultScreenView);
            builder.RegisterEntryPoint<DefaultScreenPresenter>().AsSelf();
        }
    }
}