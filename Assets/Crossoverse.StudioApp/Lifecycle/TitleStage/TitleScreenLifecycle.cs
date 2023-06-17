using UnityEngine;
using VContainer;
using VContainer.Unity;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Presentation.TitleScreen;

namespace Crossoverse.StudioApp.Lifecycle
{
    public class TitleScreenLifecycle : LifetimeScope
    {
        [Header("Instances")]
        [SerializeField] TitleScreenView _titleScreenView;

        [Header("Testing / Debugging")]
        [SerializeField] SceneConfiguration _sceneConfiguration;
        [SerializeField] GameObject _sceneTestOnlyObjectRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent != null)
            // Run as an additive scene of multiple scenes.
            {
                Debug.Log($"[{nameof(TitleScreenLifecycle)}] Run as an additive scene of multiple scenes.");
                ConfigureCore(builder);
            }
            else
            // Run as a single scene for testing or debugging.
            {
                Debug.Log($"[{nameof(TitleScreenLifecycle)}] Run as a single scene for testing or debugging.");

                ConfigureCore(builder);

                builder.RegisterInstance(_sceneConfiguration);
                builder.Register<Test.Context.SceneTransitionContextMock>(Lifetime.Singleton).AsImplementedInterfaces();

                _sceneTestOnlyObjectRoot.SetActive(true);
            }
        }

        private void ConfigureCore(IContainerBuilder builder)
        {
            builder.RegisterComponent(_titleScreenView);
            builder.Register<TitleScreenPresenter>(Lifetime.Singleton).AsSelf();
        }

        private void Start()
        {
            var titleScreenPresenter = Container.Resolve<TitleScreenPresenter>();
            titleScreenPresenter.Initialize();
        }
    }
}