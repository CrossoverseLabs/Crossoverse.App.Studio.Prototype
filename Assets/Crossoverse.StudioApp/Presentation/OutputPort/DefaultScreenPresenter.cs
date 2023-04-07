using System;
using UnityEngine;
using UniRx;
using VContainer.Unity;
using Crossoverse.StudioApp.Context;
using Crossoverse.StudioApp.Presentation.UIView;

namespace Crossoverse.StudioApp.Presentation.OutputPort
{
    public class DefaultScreenPresenter : IInitializable, IDisposable
    {
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        
        public DefaultScreenPresenter
        (
            DefaultScreenView view,
            ISceneTransitionContext sceneTransitionContext
        )
        {
            Debug.Log($"[{nameof(DefaultScreenPresenter)}] Constructor");
            
            sceneTransitionContext
                .Loading
                // .CombineLatest() // 合成前のストリームのいずれかに値が流れてきたら、合成前の全てのストリームの最新の値をリストにして合成後のストリームに流す。
                .Subscribe(loading =>
                {
                    Debug.Log($"[{nameof(DefaultScreenPresenter)}] Loading: {loading}");
                    view.SetText(loading ? "Loading" : "");
                })
                .AddTo(_disposable);
        }
        
        public void Initialize()
        {
            Debug.Log($"[{nameof(DefaultScreenPresenter)}] Initialize");
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
            Debug.Log($"[{nameof(DefaultScreenPresenter)}] Disposed");
        }
    }
}