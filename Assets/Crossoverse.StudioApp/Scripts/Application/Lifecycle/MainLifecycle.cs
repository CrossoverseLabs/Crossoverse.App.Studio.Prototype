using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Crossoverse.StudioApp.Application
{
    /// <summary>
    /// Entry point
    /// </summary>
    public class MainLifecycle : LifetimeScope
    {
        [Header("Crossoverse.StudioApp")]
        
        [SerializeField] private SceneConfiguration sceneConfiguration;
        
        protected override void Configure(IContainerBuilder builder)
        {
        }
        
        private async void Start()
        {
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