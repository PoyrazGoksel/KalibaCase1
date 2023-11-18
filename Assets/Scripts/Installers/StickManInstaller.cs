using Events.Internal;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class StickManInstaller : MonoInstaller<StickManInstaller>
    {
        public override void Start()
        {}

        public override void InstallBindings()
        {
            Container.Bind<GridAgentEvents>()
            .AsSingle();
        }
    }
}