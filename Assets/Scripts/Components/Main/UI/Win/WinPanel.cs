using Events.External;
using Extensions.Unity.MonoHelper;
using UnityEngine;
using Zenject;

namespace Components.Main.UI.Win
{
    public class WinPanel : UIPanel
    {
        [Inject] private PanelEvents PanelEvents { get; set; }
        [SerializeField] private GameObject _titleStarParticleLeftGO;
        [SerializeField] private GameObject _titleStarParticleRightGO;
        [SerializeField] private GameObject _titleGlowParticleCenterGO;

        public override void SetActive(bool isActive)
        {
            base.SetActive(isActive);
            _titleGlowParticleCenterGO.SetActive(isActive);
            _titleStarParticleLeftGO.SetActive(isActive);
            _titleStarParticleRightGO.SetActive(isActive);
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            PanelEvents.WinPanelSetActive += OnWinPanelSetActive;
        }

        private void OnWinPanelSetActive(bool arg0)
        {
            SetActive(arg0);
        }

        protected override void UnRegisterEvents()
        {
            base.UnRegisterEvents();
            PanelEvents.WinPanelSetActive -= OnWinPanelSetActive;
        }
    }
}