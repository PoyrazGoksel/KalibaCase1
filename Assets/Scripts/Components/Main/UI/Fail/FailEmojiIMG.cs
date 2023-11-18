using Extensions.DoTween;
using Extensions.Unity.MonoHelper;
using UnityEngine;

namespace Components.Main.UI.Fail
{
    public class FailEmojiIMG : UIIMG, ITweenContainerBind
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
                if (Application.isPlaying)
                {
                    TweenContainer.AddTween = _myTransform.DoYoYo(1.1f, 0.5f);
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            TweenContainer.Clear();
        }
    }
}
