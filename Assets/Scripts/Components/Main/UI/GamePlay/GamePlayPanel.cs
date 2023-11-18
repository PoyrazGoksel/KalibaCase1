using Events.External;
using Extensions.Unity.MonoHelper;
using Zenject;

namespace Components.Main.UI.GamePlay
{
    public class GamePlayPanel : UIPanel
    {
        [Inject] private PanelEvents PanelEvents { get; set; }
        [Inject] private UIEvents UIEvents { get; set; }

        
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            PanelEvents.GamePlayPanelSetActive += OnGamePlayPanelSetActive;
            UIEvents.PlayButton += OnPlayButton;
        }

        private void OnGamePlayPanelSetActive(bool arg0)
        {
            SetActive(arg0);
        }

        private void OnPlayButton()
        {
            SetActive(true);
        }

        protected override void UnRegisterEvents()
        {
            base.UnRegisterEvents();
            PanelEvents.GamePlayPanelSetActive -= OnGamePlayPanelSetActive;
            UIEvents.PlayButton -= OnPlayButton;
        }
    }
}