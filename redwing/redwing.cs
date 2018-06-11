using System;
using System.Linq;
using Modding;
using UnityEngine;
using System.IO;

namespace redwing
{
    public class redwing : Mod <redwing_settings, redwing_global_settings>, ITogglableMod
    {
        private const string VERSION = "0.0.3";
        private const int LOAD_ORDER = 90;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = VERSION;
            const int minApi = 40;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minApi;
            bool noModCommon = !(from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "ModCommon" select type).Any();

            if (apiTooLow) ver += " (Error: ModAPI too old)";
            if (noModCommon) ver += " (Error: Redwing requires ModCommon)";

            return ver;
        }

        public override void Initialize()
        {
            ModHooks.Instance.AfterSavegameLoadHook += saveGame;
            ModHooks.Instance.NewGameHook += addComponent;

            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;
        }

        private void setupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = (GlobalSettings != null && GlobalSettings.settingsVersion != version_info.SETTINGS_VER);

            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                if (forceReloadGlobalSettings)
                {
                    log("Settings outdated! Rebuilding.");
                }
                else
                {
                    log("Settings not found, rebuilding... File will be saved to: " + settingsFilePath);
                }

                GlobalSettings.reset();
            }
            SaveGlobalSettings();
        }

        private void saveGame(SaveGameData data)
        {
            addComponent();
        }

        private void addComponent()
        {
            log("Adding Redwing to game.");
            
            GameManager.instance.gameObject.AddComponent<greymoth>();
            GameManager.instance.gameObject.AddComponent<redwing_flame_gen>();
            GameManager.instance.gameObject.AddComponent<redwing_hooks>();
            
        }

        public override int LoadPriority()
        {
            return LOAD_ORDER;
        }

        public void Unload()
        {
            log("Disabling! If you see any more non-settings messages by this mod please report as an issue.");
            ModHooks.Instance.AfterSavegameLoadHook -= saveGame;
            ModHooks.Instance.NewGameHook -= addComponent;
            
        }

        public void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }


    }
}
