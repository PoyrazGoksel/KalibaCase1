using Events.External;
using Extensions.Unity.MonoHelper;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Login
{
    public class LoadingSlider : UIIMG
    {
        [Inject] private LoadingScreenEvents LoadingScreenEvents { get; set; }
        [SerializeField] private Slider _loadingSlider;
        [SerializeField] private UITMP _loadingTMP;

        protected override void RegisterEvents()
        {
            LoadingScreenEvents.LoadingProgress += OnLoadingProgress;
        }

        private void OnLoadingProgress(float arg0)
        {
            _loadingSlider.value = arg0;
            _loadingTMP.UpdateText($"{arg0 * 100}%");
        }

        protected override void UnRegisterEvents()
        {
            LoadingScreenEvents.LoadingProgress -= OnLoadingProgress;
        }
    }
}
