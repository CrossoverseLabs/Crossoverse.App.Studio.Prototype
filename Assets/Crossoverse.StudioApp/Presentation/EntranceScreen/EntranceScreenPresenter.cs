using System;
using Cysharp.Threading.Tasks;
using UniRx;
using Crossoverse.Context;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Context;

namespace Crossoverse.StudioApp.Presentation.EntranceScreen
{
    public sealed class EntranceScreenPresenter : IDisposable
    {
        private readonly CompositeDisposable _disposable = new();
        private readonly EntranceScreenView _entranceScreenView;
        private readonly ISceneTransitionContext _sceneTransitionContext;
        private readonly IContentResourceContext _contentResourceContext;

        public EntranceScreenPresenter
        (
            EntranceScreenView EntranceScreenView,
            ISceneTransitionContext sceneTransitionContext,
            IContentResourceContext contentResourceContext
        )
        {
            _entranceScreenView = EntranceScreenView;
            _sceneTransitionContext = sceneTransitionContext;
            _contentResourceContext = contentResourceContext;
        }

        public void Dispose() => _disposable.Dispose();

        public void Initialize()
        {
            _entranceScreenView.OnClickEnterButton
                .Subscribe(contentId =>
                {
                    _contentResourceContext.SetNextContentId(contentId);
                    _sceneTransitionContext.LoadStageAsync(StageName.ContentStage).Forget();
                })
                .AddTo(_disposable);
        }
    }
}