using System.IO;
using Datas.Settings;

namespace Utils
{
    public static class EnvironmentVariables
    {
        public const string SettingsPath = "Settings/";
        public const string LevelsPath = SettingsPath + "Levels/";
        public const string SaveFileExt = ".sav";
        public const string TileSettingsPath = SettingsPath + "TileSettings";
        public const string Project = "Kaliba";
        public const string LevelSoPrefix = "LevelData";
        public const string SoSuffix = ".asset";
        public const string AssetsPath = "Assets/";
        public const string ResourcesPath = "Resources/";
        public const string MainSceneSettingsPath = SettingsPath + nameof(MainSceneSettings);
        public const string GridPrefabPath = "Assets/Prefabs/Grid/Grid.prefab";
        public const string LevelGeneratorScenePath = "Assets/Scenes/LevelGenerator.unity";
        public const string TileItemsSoPath = SettingsPath + "Grid/TileItems";
        public static string GetProjectPath(string resourcePath)
        {
            return AssetsPath + ResourcesPath + resourcePath;
        }

        public static string GetResourcesPath(string projectPath)
        {
            string ext = Path.GetExtension(projectPath);
            string assetPath = projectPath.Replace(ext, "");
            return assetPath.Replace(AssetsPath + ResourcesPath, "");
        }
    }
}