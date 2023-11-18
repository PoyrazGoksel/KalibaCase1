using JetBrains.Annotations;
using UnityEngine.Events;

namespace Events.External
{
    
    [UsedImplicitly]
    public class PanelEvents
    {
        public UnityAction<bool> WinPanelSetActive;
        public UnityAction<bool> FailPanelSetActive;
        public UnityAction<bool> GamePlayPanelSetActive;
    }
}