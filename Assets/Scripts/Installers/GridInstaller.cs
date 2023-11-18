using Events.Internal;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class GridInstaller : MonoInstaller<GridInstaller>
    {
        private GridInternalEvents _gridInternalEvents;
        
        public override void InstallBindings()
        {
            Container.Bind<GridInternalEvents>()
            .AsSingle();
        }

        public override void Start()
        {
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            _gridInternalEvents = Container.Resolve<GridInternalEvents>();

            _gridInternalEvents.InstantiatePrefab += OnCreatePrefabInstance;
        }

        private GameObject OnCreatePrefabInstance(GameObject arg)
        {
            
            return Container.InstantiatePrefab(arg);
        }

        private void UnRegisterEvents()
        {
            _gridInternalEvents.InstantiatePrefab -= OnCreatePrefabInstance;
        }
    }
}