using Events.External;
using Extensions.Unity.MonoHelper;
using Zenject;

namespace Components.Main.UI.Fail
{
    public class RetryBUT : UIButtonTMP
    {
        [Inject] private UIEvents UIEvents { get; set; }

        protected override void OnClick()
        {
            UIEvents.RestartLevelBut?.Invoke();
        }
    }
}
