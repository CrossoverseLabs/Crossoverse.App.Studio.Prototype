using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using Crossoverse.StudioApp.Test.Context;

namespace Crossoverse.StudioApp.Test.Presentation
{
    public class DefaultScreenRuntimeTest : IStartable
    {
        private readonly SceneTransitionContextMock _contextMock;
        
        [Inject]
        public DefaultScreenRuntimeTest(SceneTransitionContextMock sceneTransitionContextMock)
        {
            _contextMock = sceneTransitionContextMock;
        }
        
        public async void Start()
        {
            Debug.Log($"[{nameof(DefaultScreenRuntimeTest)} Start");
            
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            _contextMock.LoadingRP.Value = true;
            
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            _contextMock.LoadingRP.Value = false;
            
            Debug.Log($"[{nameof(DefaultScreenRuntimeTest)} End");
        }
    }
}