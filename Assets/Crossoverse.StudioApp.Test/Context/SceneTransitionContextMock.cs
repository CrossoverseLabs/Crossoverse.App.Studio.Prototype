using UniRx;
using Crossoverse.StudioApp.Context;

namespace Crossoverse.StudioApp.Test.Context
{
    public class SceneTransitionContextMock : ISceneTransitionContext
    {
        public IReadOnlyReactiveProperty<bool> Loading => LoadingRP;
        public readonly ReactiveProperty<bool> LoadingRP = new ReactiveProperty<bool>();
    }
}