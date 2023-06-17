using System;
using Cysharp.Threading.Tasks;
using UniRx;
using Crossoverse.StudioApp.Configuration;
using Crossoverse.StudioApp.Context;

namespace Crossoverse.StudioApp.Presentation.TitleScreen
{
    public sealed class TitleScreenPresenter : IDisposable
    {
        private readonly CompositeDisposable _disposable = new();
        private readonly TitleScreenView _titleScreenView;
        private readonly LicenseView _licenseView;
        private readonly ISceneTransitionContext _sceneTransitionContext;

        public TitleScreenPresenter
        (
            TitleScreenView titleScreenView,
            // LicenseView licenseView, 
            ISceneTransitionContext sceneTransitionContext
        )
        {
            _titleScreenView = titleScreenView;
            // _licenseView = licenseView;
            _sceneTransitionContext = sceneTransitionContext;
        }

        public void Dispose() => _disposable.Dispose();

        public void Initialize()
        {
            _titleScreenView.OnClickStartButton
                .Subscribe(_ =>
                {
                    _sceneTransitionContext.LoadStageAsync(StageName.EntranceStage).Forget();
                })
                .AddTo(_disposable);
            
            _titleScreenView.OnClickLicenseButton
                .Subscribe(_ => 
                {
                    _licenseView.Show();
                })
                .AddTo(_disposable);
        }
    }
}