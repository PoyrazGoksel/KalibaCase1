using Datas.Players;
using Extensions.Unity.MonoHelper;
using Zenject;

namespace Components.Main.UI.GamePlay
{
    public class LevelCountTMP : UITMP
    {
        [Inject] private IPlayerData PlayerData { get; set; }
    
        public override void SetActive(bool isActive)
        {
            base.SetActive(isActive);
            UpdateText("Level " + PlayerData.CurrentLevel);
        }
    }
}
