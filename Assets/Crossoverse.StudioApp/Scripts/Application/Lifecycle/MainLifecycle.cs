using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
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
            builder.Register<ApplicationContext>(Lifetime.Singleton);
        }
        
        private async void Start()
        {
            var applicationContext = Container.Resolve<ApplicationContext>();
            
            applicationContext.Initialize();
            
            await LoadScenesAsync();
        }
        
        private async UniTask LoadScenesAsync()
        {
            foreach (var scene in sceneConfiguration.InitialScenes)
            {
                await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive);
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneConfiguration.InitialActiveScene.ToString()));
        }
    }
}