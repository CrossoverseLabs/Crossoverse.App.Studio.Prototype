using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UniRx;
using Crossoverse.StudioApp.Configuration;

namespace Crossoverse.StudioApp.Context
{
    public sealed class SceneTransitionContext
    {
        public IReadOnlyReactiveProperty<bool> Loading => _loading;
        private readonly ReactiveProperty<bool> _loading = new ReactiveProperty<bool>();
        
        private readonly SceneConfiguration _sceneConfiguration;
        
        public SceneTransitionContext(SceneConfiguration sceneConfiguration)
        {
            _sceneConfiguration = sceneConfiguration;
        }
        
        public async UniTask InitializeAsync()
        {
            _loading.Value = true;
            await LoadInitialScenesAsync();
            await TransitAsync(_sceneConfiguration.Stages[0]);
        }
        
        public async UniTask TransitAsync(Stage stage)
        {
            _loading.Value = true;
            
            foreach (var sceneName in stage.Scenes)
            {
                await SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);
            }
            if (stage.ActiveSceneName != SceneName.None)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(stage.ActiveSceneName.ToString()));
            }
            
            _loading.Value = false;
        }
        
        private async UniTask LoadInitialScenesAsync()
        {
            foreach (var scene in _sceneConfiguration.InitialScenes)
            {
                await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive);
            }
            if (_sceneConfiguration.InitialActiveScene != SceneName.None)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneConfiguration.InitialActiveScene.ToString()));
            }
        }
    }
}