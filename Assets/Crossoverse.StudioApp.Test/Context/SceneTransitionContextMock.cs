using Cysharp.Threading.Tasks;
using UniRx;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Context;

namespace Crossoverse.StudioApp.Test.Context
{
    public class SceneTransitionContextMock : ISceneTransitionContext
    {
        public IReadOnlyReactiveProperty<bool> Loading => LoadingRP;
        public readonly ReactiveProperty<bool> LoadingRP = new();

        public async UniTask LoadStageAsync(StageName stageName)
        {
            UnityEngine.Debug.Log($"[SceneTransitionContextMock] LoadStageAsync: {stageName}");
        }
    }
}