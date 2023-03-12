using UnityEngine;
using VContainer;
using VContainer.Unity;
using Crossoverse.StudioApp.Presentation.OutputPort;
using Crossoverse.StudioApp.Presentation.UIView;

namespace Crossoverse.StudioApp.Application
{
    public class DefaultScreenLifecycle : LifetimeScope
    {
        [SerializeField] private DefaultScreenView defaultScreenView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(defaultScreenView);
            builder.RegisterEntryPoint<DefaultScreenPresenter>();
        }
    }
}