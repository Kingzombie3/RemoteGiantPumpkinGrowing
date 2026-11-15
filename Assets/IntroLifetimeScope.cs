using UnityEngine;
using VContainer;
using VContainer.Unity;
using _Project.Scripts.Runtime.UI.Intro;

namespace _Project.Scripts.Runtime.Composition
{
    public class IntroLifetimeScope : LifetimeScope
    {
        [SerializeField] private IntroSequenceController _introController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_introController);
        }
    }
}
