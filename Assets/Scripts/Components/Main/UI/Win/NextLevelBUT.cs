using Events.External;
using Extensions.Unity.MonoHelper;
using Zenject;

namespace Components.Main.UI.Win
{
    public class NextLevelBUT : UIButtonTMP
    {
        [Inject] private UIEvents UIEvents { get; set; }
        
        protected override void OnClick()
        {
            UIEvents.NextLevelBut?.Invoke();
        }
    }
}