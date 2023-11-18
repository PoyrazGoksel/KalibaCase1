using Extensions.DoTween;
using Extensions.Unity.MonoHelper;
using UnityEngine;

namespace Components.Main.UI.Fail
{
    public class FailedTMP : UITMP, ITweenContainerBind
    {
        public ITweenContainer TweenContainer { get; set; }
        [SerializeField] private Transform _myTransform;

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
        }

        public override void SetActive(bool isActive)
        {
            base.SetActive(isActive);

            if (isActive)
            {
                TweenContainer.AddTween = _myTransform.DoScaleInOut(2f, 0.1f, 0.4f);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            TweenContainer.Clear();
        }
    }
}
