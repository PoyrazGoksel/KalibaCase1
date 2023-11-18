using Events.External;
using Extensions.Unity.MonoHelper;
using Zenject;

namespace Components.Main.UI.Fail
{
    public class FailPanel : UIPanel
    {
        [Inject] private PanelEvents PanelEvents { get; set; }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();

            PanelEvents.FailPanelSetActive += OnFailPanelSetActive;
        }

        private void OnFailPanelSetActive(bool arg0)
        {
            SetActive(arg0);
        }

        protected override void UnRegisterEvents()
        {
            base.UnRegisterEvents();
            PanelEvents.FailPanelSetActive -= OnFailPanelSetActive;
        }
    }
}
