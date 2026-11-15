using _Project.Scripts.Runtime.Core.Audio;
using _Project.Scripts.Runtime.Core.SceneManagement;
using _Project.Scripts.Runtime.Infrastructure.Audio;
using _Project.Scripts.Runtime.UI.Transitions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.Runtime.Composition
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private AudioService _audioService;
        [SerializeField] private SceneTransitionService _transitionServicePrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_audioService, Lifetime.Singleton)
                .UnderTransform(transform)
                .As<IAudioService>();

            builder.RegisterComponentInNewPrefab(_transitionServicePrefab, Lifetime.Singleton)
                .UnderTransform(transform)
                .As<ISceneTransitionService>();

            builder.RegisterBuildCallback(container => container.Resolve<IAudioService>());
            //builder.RegisterBuildCallback(container => container.Resolve<ISceneTransitionService>());
        }
    }
}

