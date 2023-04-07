using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UniRx;
using Crossoverse.StudioApp.Configuration;

namespace Crossoverse.StudioApp.Context
{
    public interface ISceneTransitionContext
    {
        public IReadOnlyReactiveProperty<bool> Loading { get;  }
    }
    
    public sealed class SceneTransitionContext : ISceneTransitionContext
    {
        public IReadOnlyReactiveProperty<bool> Loading => _loading;
        private readonly ReactiveProperty<bool> _loading = new ReactiveProperty<bool>();
        
        public StageName CurrentStage => _currentStage;
        public StageName PreviousStage => _previousStage;
        
        private readonly SceneConfiguration _sceneConfiguration;
        private readonly HashSet<SceneName> _globalScenes;
        private readonly HashSet<SceneName> _loadedScenes = new HashSet<SceneName>();
        
        private StageName _currentStage = StageName.None;
        private StageName _previousStage = StageName.None;
        
        public SceneTransitionContext(SceneConfiguration sceneConfiguration)
        {
            _sceneConfiguration = sceneConfiguration;
            _globalScenes = sceneConfiguration.GlobalScenes.ToHashSet();
        }
        
        public async UniTask LoadGlobalScenesAndInitialStageAsync()
        {
            await LoadGlobalScenesAsync(true);
            await TransitAsync(_sceneConfiguration.Stages[0]);
        }
        
        public async UniTask LoadGlobalScenesAsync(bool onInitialize = false)
        {
            _loading.Value = true;
            
            foreach (var scene in _globalScenes)
            {
                await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive);
            }
            if (_sceneConfiguration.InitialActiveScene != SceneName.None)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneConfiguration.InitialActiveScene.ToString()));
            }
            
            if(!onInitialize) _loading.Value = false;
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
            
            _previousStage = _currentStage;
            _currentStage = stage.Name;
            
            _loading.Value = false;
        }
    }
}