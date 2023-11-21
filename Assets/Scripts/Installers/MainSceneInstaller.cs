using System;
using System.Collections.Generic;
using Datas.Levels;
using Datas.Players;
using Datas.Settings;
using Events.External;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MainSceneInstaller : MonoInstaller<MainSceneInstaller>
    {
        [Inject] private PanelEvents PanelEvents { get; set; }
        [Inject] private MainSceneSettings MainSceneSettings { get; set; }
        [Inject] private UIEvents UIEvents { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }
        [Inject] private GameStateEvents GameStateEvents { get; set; }
        [Inject] private IPlayerData PlayerData { get; set; }
        private Settings _mySettings;

        public override void Start()
        {
            base.Start();
            _mySettings = MainSceneSettings.Settings;
            SetUIStartState();
            CreateLevel();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        public override void InstallBindings() {}

        private void SetUIStartState()
        {
            PanelEvents.FailPanelSetActive?.Invoke(false);
            PanelEvents.WinPanelSetActive?.Invoke(false);
            PanelEvents.GamePlayPanelSetActive?.Invoke(false);
        }

        private void CreateLevel()
        {
            Container.InstantiatePrefab(_mySettings.GridPefab);

            LevelData currLevel = _mySettings.LevelList[PlayerData.CurrentLevel];

            IGridAccess gridAccess = currLevel;

            GridEvents.LoadGrid?.Invoke(gridAccess.GridDataClone);
        }

        private void RegisterEvents()
        {
            UIEvents.PlayButton += OnPlayButton;
            GameStateEvents.LevelSuccess += OnLevelSuccess;
            GameStateEvents.LevelFail += OnLevelFail;
        }

        private void OnPlayButton()
        {
            CreateLevel();
        }

        private void OnLevelSuccess()
        {
            PanelEvents.WinPanelSetActive?.Invoke(true);
        }

        private void OnLevelFail()
        {
            PanelEvents.FailPanelSetActive?.Invoke(true);
        }

        private void UnRegisterEvents()
        {
            UIEvents.PlayButton -= OnPlayButton;
            GameStateEvents.LevelSuccess -= OnLevelSuccess;
            GameStateEvents.LevelFail -= OnLevelFail;
        }

        [Serializable]
        public class Settings
        {
            [InlineEditor][SerializeField] private List<LevelData> _levelList;
            [SerializeField] private GameObject _gridPrefab;
            public List<LevelData> LevelList => _levelList;
            public GameObject GridPefab => _gridPrefab;
        }
    }
}