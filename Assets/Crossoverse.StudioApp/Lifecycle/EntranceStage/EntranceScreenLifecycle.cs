using UnityEngine;
using VContainer;
using VContainer.Unity;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Presentation.EntranceScreen;

namespace Crossoverse.StudioApp.Lifecycle
{
    public class EntranceScreenLifecycle : LifetimeScope
    {
        [Header("Instances")]
        [SerializeField] EntranceScreenView _entranceScreenView;

        [Header("Testing / Debugging")]
        [SerializeField] SceneConfiguration _sceneConfiguration;
        [SerializeField] GameObject _sceneTestOnlyObjectRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent != null)
            // Run as an additive scene of multiple scenes.
            {
                Debug.Log($"[{nameof(EntranceScreenLifecycle)}] Run as an additive scene of multiple scenes.");
                ConfigureCore(builder);
            }
            else
            // Run as a single scene for testing or debugging.
            {
                Debug.Log($"[{nameof(EntranceScreenLifecycle)}] Run as a single scene for testing or debugging.");

                ConfigureCore(builder);

                builder.RegisterInstance(_sceneConfiguration);
                builder.Register<Test.Context.SceneTransitionContextMock>(Lifetime.Singleton).AsImplementedInterfaces();

                builder.Register<Test.Context.ContentResourceContextMock>(Lifetime.Singleton).AsImplementedInterfaces();

                _sceneTestOnlyObjectRoot.SetActive(true);
            }
        }

        private void ConfigureCore(IContainerBuilder builder)
        {
            builder.RegisterComponent(_entranceScreenView);
            builder.Register<EntranceScreenPresenter>(Lifetime.Singleton).AsSelf();
        }

        private void Start()
        {
            var entranceScreenPresenter = Container.Resolve<EntranceScreenPresenter>();
            entranceScreenPresenter.Initialize();
        }
    }
}