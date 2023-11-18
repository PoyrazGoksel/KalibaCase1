using Extensions.Unity;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Utils
{
    [UsedImplicitly]
    public class InputListener : IInitializable
    {
        private Camera _mainSceneCamera;
        private RoutineHelper _inputListenerRoutine;

        public void Initialize()
        {
            RegisterEvents();
        }

        private void RegisterEvents()
        {
        }
        
        private void InputListenerUpdate()
        {
        }
    }
}