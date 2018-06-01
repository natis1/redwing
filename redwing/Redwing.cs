using System;
using System.Linq;
using Modding;
using UnityEngine;
using System.IO;

namespace redwing
{
    public class redwing : Mod <redwingSettings, redwingGlobalSettings>, ITogglableMod
    {

        public static string version = "0.0.3";
        public readonly int loadOrder = 90;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = version;
            int minAPI = 40;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            bool noModCommon = !(from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "ModCommon" select type).Any();

            if (apiTooLow) ver += " (Error: ModAPI too old)";
            if (noModCommon) ver += " (Error: Redwing requires ModCommon)";

            return ver;
        }

        public override void Initialize()
        {
            ModHooks.Instance.AfterSavegameLoadHook += SaveGame;
            ModHooks.Instance.NewGameHook += AddComponent;

            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;
        }

        void SetupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = (GlobalSettings != null && GlobalSettings.SettingsVersion != VersionInfo.SettingsVer);

            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                if (forceReloadGlobalSettings)
                {
                    Log("Settings outdated! Rebuilding.");
                }
                else
                {
                    Log("Settings not found, rebuilding... File will be saved to: " + settingsFilePath);
                }

                GlobalSettings.Reset();
            }
            SaveGlobalSettings();
        }

        private void SaveGame(SaveGameData data)
        {
            AddComponent();
        }

        private void AddComponent()
        {
            Log("Adding Redwing to game.");
            GameManager.instance.gameObject.AddComponent<RedwingFlameGen>();
        }

        public override int LoadPriority()
        {
            return loadOrder;
        }

        public void Unload()
        {
            Log("Disabling! If you see any more non-settings messages by this mod please report as an issue.");
            ModHooks.Instance.AfterSavegameLoadHook -= SaveGame;
            ModHooks.Instance.NewGameHook -= AddComponent;
            
        }

        new public void Log(String str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }


    }
}
