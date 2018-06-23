using System;
using System.Linq;
using Modding;
using UnityEngine;
using System.IO;

namespace redwing
{
    public class redwing : Mod <redwing_settings, redwing_global_settings>, ITogglableMod
    {
        private const string VERSION = "0.0.8";
        private const int LOAD_ORDER = 90;

        private bool blackmothExists = false;
        private bool blackmothError = false;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = VERSION;
            const int minApi = 43;


            if (blackmothExists)
            {
                ver += " (Blackmoth)";
            }
            else if (GlobalSettings.useGreymothDashWhenBlackmothMissing)
            {
                ver += " (Greymoth)";
            }
            else
            {
                ver += " (Othermoth?)";
            }

            if (blackmothError)
            {
                ver += " (Error: Blackmoth too old - either remove it or update it)";
            }
            
            

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minApi;
            bool noModCommon = !(from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "ModCommon" select type).Any();

            if (apiTooLow) ver += " (Error: ModAPI too old)";
            if (noModCommon) ver += " (Error: Redwing requires ModCommon)";

            return ver;
        }

        public override void Initialize()
        {
            setupSettings();
            
            // report if the user has blackmoth.
            blackmothExists = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "BlackmothMod" select type).Any();
            log("does blackmoth exist? " + blackmothExists);
            
            redwing_fireball_behavior.fbDamageBase = GlobalSettings.fireballDamageBase;
            redwing_fireball_behavior.fbDamageScale = GlobalSettings.fireballDamagePerNailLvl;
            redwing_fireball_behavior.fbmDamageBase = GlobalSettings.fireballMagmaDamageBase;
            redwing_fireball_behavior.fbmDamageScale = GlobalSettings.fireballMagmaDamagePerNailLvl;

            redwing_hooks.fbCooldown = GlobalSettings.fireballCooldownBase;
            redwing_hooks.fsRecharge = GlobalSettings.shieldCooldownBase;
            redwing_hooks.fsReduceOnHit = GlobalSettings.shieldCooldownReductionPerNailHit;
            redwing_hooks.laserCooldown = GlobalSettings.laserCooldownBase;
            redwing_hooks.zeroDmgLaser = GlobalSettings.lasersWhenShieldBlocksAllDmg;
            redwing_hooks.laserDamageBase = GlobalSettings.laserDamageBase;
            redwing_hooks.laserDamagePerNail = GlobalSettings.laserDamagePerNailLvl;
            redwing_hooks.blackmothSymbolsExist = false;

            try
            {
                if (blackmothExists)
                {
                    checkBlackmothVersion();
                }
            }
            catch (Exception e)
            {
                log("Blackmoth not found. Error: " + e);
            }

            redwing_pillar_behavior.damagePriBase = GlobalSettings.pillarDamageBase;
            redwing_pillar_behavior.damagePriNail = GlobalSettings.pillarDamagePerNailLvl;
            redwing_pillar_behavior.damageSecBase = GlobalSettings.pillarSecondaryDamageBase;
            redwing_pillar_behavior.damageSecNail = GlobalSettings.pillarSecondaryDamagePerNailLvl;
            redwing_pillar_behavior.damageSecondaryTimes = GlobalSettings.pillarSecondaryAttacks;

            redwing_trail_behavior.damagePriBase = GlobalSettings.trailDamageBase;
            redwing_trail_behavior.damagePriNail = GlobalSettings.trailDamagePerNailLvl;
            redwing_trail_behavior.damageSecBase = GlobalSettings.trailSecondaryDamageBase;
            redwing_trail_behavior.damageSecNail = GlobalSettings.trailSecondaryDamagePerNailLvl;
            
            ModHooks.Instance.AfterSavegameLoadHook += saveGame;
            ModHooks.Instance.NewGameHook += addComponent;

            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;
        }

        private void checkBlackmothVersion()
        {
            Version blackmothVers = new Version(BlackmothMod.Blackmoth.Instance.GetVersion());
            Version blackmothNeeded = new Version("1.7.2");
            if (blackmothNeeded.CompareTo(blackmothVers) > 0)
            {
                log("ERROR: Blackmoth found but too old to work with redwing!" +
                    " Please update to Blackmoth 1.7.2 or newer");
                blackmothError = true;
            }
            else
            {
                redwing_hooks.blackmothSymbolsExist = true;
            }

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
            
            redwing_lore.overrideBlackmothLore = GlobalSettings.overrideBlackmothLore;

            try
            {
                if (!blackmothExists && GlobalSettings.useGreymothDashWhenBlackmothMissing)
                {
                    GameManager.instance.gameObject.AddComponent<redwing_greymoth>();
                    // no blackmoth so no need to override it.
                    redwing_hooks.overrideBlackmothNailDmg = false;
                    log("Unable to find Blackmoth, loading Greymoth instead.");
                }
                else if (blackmothExists)
                {
                    log("Found Blackmoth...");
                    log(GlobalSettings.overrideBlackmothNailDamage
                        ? "The God of fire and void has arrived."
                        : "Enter the knight, on flaming wings.");

                    redwing_hooks.overrideBlackmothNailDmg = GlobalSettings.overrideBlackmothNailDamage;
                }
                else
                {
                    log("Not adding any dash manager. I hope you have one loaded...");
                }

                GameManager.instance.gameObject.AddComponent<redwing_flame_gen>();
                GameManager.instance.gameObject.AddComponent<redwing_hooks>();

                GameManager.instance.gameObject.AddComponent<redwing_lore>();
            }
            catch (Exception e)
            {
                log("what the fuck? " + e);
            }

            log(Language.Language.CurrentLanguage() + " is your current language.");
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
