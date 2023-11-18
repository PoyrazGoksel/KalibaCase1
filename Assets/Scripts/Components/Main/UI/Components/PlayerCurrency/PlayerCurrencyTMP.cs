using Datas.Players;
using Events.External;
using Extensions.Unity.MonoHelper;
using Zenject;

namespace Components.Main.UI.Components.PlayerCurrency
{
    public class PlayerCurrencyTMP : UITMP
    {
        [Inject] private IPlayerData PlayerData { get; set; }
        [Inject] private PlayerEvents PlayerEvents { get; set; }

        private void Start()
        {
            UpdateText(PlayerData.Currency.ToString());
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            PlayerEvents.PlayerCurrencyChanged += OnPlayerCurrencyChanged;
        }

        private void OnPlayerCurrencyChanged(int arg0)
        {
            UpdateText(arg0.ToString());
        }

        protected override void UnRegisterEvents()
        {
            base.UnRegisterEvents();
            PlayerEvents.PlayerCurrencyChanged -= OnPlayerCurrencyChanged;
        }
    }
}
