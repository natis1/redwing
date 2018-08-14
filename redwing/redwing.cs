using System;
using System.Linq;
using Modding;
using UnityEngine;
using System.IO;
using angleintegration;
using ModCommon;
using On.InControl.NativeProfile;

namespace redwing
{
    // ReSharper disable once InconsistentNaming because it's the name I want to appear on Modding API.
    // ReSharper disable once UnusedMember.Global because it's used implicitly but importing rider extensions is dumb.
    public class redwing : modern_mod<redwing_settings, redwing_global_settings, redwing_flamegen_settings>, ITogglableMod
    {
        private const string VERSION = "1.1.2";
        private const int LOAD_ORDER = 90;
        private const int minApi = 44;


        private bool blackmothExists;
        private bool blackmothError;
        private bool apiTooLow;
        private bool noModCommon;
        private bool shitmothst;
        private int problemCode;

        // ReSharper disable once ArrangeTypeMemberModifiers
        public redwing()
        {
            setupModVars(new modern_mod_vars("Redwing CP1", VERSION, 11, LOAD_ORDER));
        }

        // Version detection code originally by Seanpr, used with permission.
        public override string getVersionAppend()
        {
            string ver = "";
            if (blackmothExists)
            {
                ver += "(Blackmoth)";
            }
            else if (shitmothst)
            {
                ver += "(Shitmoth)";
            }
            else if (globalSettings.useGreymothDashWhenBlackmothMissing)
            {
                ver += "(Greymoth)";
            }
            else
            {
                ver += "(Othermoth?)";
            }

            if (blackmothError)
                ver += " (Error: Blackmoth too old - either remove it or update it to 1.7.2 or newer)";

            if (apiTooLow)
                ver += " (Error: ModAPI too old... Minimum version is 44... seriously)";
            

            if (noModCommon)
                ver += " (Error: Redwing requires ModCommon)";

            return ver;
        }

        public override void Initialize()
        {
            setupSettings();
            
            lore.createLore(globalSettings.overrideBlackmothLore, globalSettings.useEnglishLoreWhenLanguageMissing);
            
            problemCode = 0;

            if (globalSettings.redwingFirstLaunch)
                problemCode += 1;
            
            // report if the user has modcommon.
            noModCommon = !(from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "ModCommon" select type).Any();
            
            
            
            // report if the user has blackmoth.
            blackmothExists = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "BlackmothMod" select type).Any();
            log("does blackmoth exist? " + blackmothExists);
            
            // report if the user is using shitmothst... lol
            shitmothst = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "shitmothst" select type).Any();
            
            redwing_fireball_behavior.fbDamageBase = globalSettings.fireballDamageBase;
            redwing_fireball_behavior.fbDamageScale = globalSettings.fireballDamagePerNailLvl;
            redwing_fireball_behavior.fbmDamageBase = globalSettings.fireballMagmaDamageBase;
            redwing_fireball_behavior.fbmDamageScale = globalSettings.fireballMagmaDamagePerNailLvl;
            redwing_fireball_behavior.fireballMana = globalSettings.fireballSoulAddOnHit;

            redwing_hooks.fbCooldown = globalSettings.fireballCooldownBase;
            redwing_hooks.fsRecharge = globalSettings.shieldCooldownBase;
            redwing_hooks.fsReduceOnHit = globalSettings.shieldCooldownReductionPerNailHit;
            redwing_hooks.laserCooldown = globalSettings.laserCooldownBase;
            redwing_hooks.zeroDmgLaser = globalSettings.lasersWhenShieldBlocksAllDmg;
            redwing_hooks.laserDamageBase = globalSettings.laserDamageBase;
            redwing_hooks.laserDamagePerNail = globalSettings.laserDamagePerNailLvl;
            redwing_hooks.blackmothSymbolsExist = false;
            redwing_hooks.balancedMode = globalSettings.handicapAllNonFireAttacks;
            redwing_hooks.nailmasterGloryNotchCost = globalSettings.nailmasterGloryCost;

            try
            {
                if (blackmothExists)
                {
                    checkBlackmothVersion();
                    //redwing_lore.overrideBlackmothLore = globalSettings.overrideBlackmothLore;
                    if (redwing_hooks.balancedMode)
                    {
                        redwing_hooks.balancedMode = false;
                        log("You cannot handicap yourself with blackmoth installed.");
                        log("Blackmoth makes you a god and there's no handicapping that can change that.");
                        problemCode += 2;
                    }
                }
                else
                {
                    //redwing_lore.overrideBlackmothLore = false;
                }
            }
            catch (Exception e)
            {
                log("Blackmoth not found. Error: " + e);
            }

            redwing_pillar_behavior.damagePriBase = globalSettings.pillarDamageBase;
            redwing_pillar_behavior.damagePriNail = globalSettings.pillarDamagePerNailLvl;
            redwing_pillar_behavior.damageSecBase = globalSettings.pillarSecondaryDamageBase;
            redwing_pillar_behavior.damageSecNail = globalSettings.pillarSecondaryDamagePerNailLvl;
            redwing_pillar_behavior.damageSecondaryTimes = globalSettings.pillarSecondaryAttacks;

            redwing_trail_behavior.damagePriBase = globalSettings.trailDamageBase;
            redwing_trail_behavior.damagePriNail = globalSettings.trailDamagePerNailLvl;
            redwing_trail_behavior.damageSecBase = 0;
            redwing_trail_behavior.damageSecNail = 0;

            redwing_error.englishLore = globalSettings.useEnglishLoreWhenLanguageMissing;
            redwing_error.englishWarnings = globalSettings.useEnglishWarningInfoWhenLanguageMissing;

            napalm.damageExponent = (float) globalSettings.napalmDamageExponent;
            napalm.damageMultiplier = (float) globalSettings.napalmDamageMultiplier;
            
            apiTooLow = (Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minApi);
            if (noModCommon || blackmothError || apiTooLow)
                problemCode = 4;
            
            
            redwing_error.redwingProblemCode = problemCode;

            redwing_flame_gen.flameIntensityCurve = new[]
            {
                new Color(secondarySettings.flameColorR1, secondarySettings.flameColorG1,
                    secondarySettings.flameColorB1),
                new Color(secondarySettings.flameColorR2, secondarySettings.flameColorG2,
                    secondarySettings.flameColorB2),
                new Color(secondarySettings.flameColorR3, secondarySettings.flameColorG3,
                    secondarySettings.flameColorB3),
                new Color(secondarySettings.flameColorR4, secondarySettings.flameColorG4,
                    secondarySettings.flameColorB4)
            };
            redwing_flame_gen.flameIntensityThresholds = new[]
            {
                (double) secondarySettings.flameColor1Threshold,
                (double) secondarySettings.flameColor2Threshold,
                (double) secondarySettings.flameColor3Threshold,
                (double) secondarySettings.flameColor4Threshold
            };
            
            ModHooks.Instance.AfterSavegameLoadHook += saveGame;
            ModHooks.Instance.NewGameHook += addComponent;
            ModHooks.Instance.ApplicationQuitHook += saveGlobalSettings;
            ModHooks.Instance.ApplicationQuitHook += saveSecondarySettings;
            printErrors();
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
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + "Redwing.settings.json";

            bool forceReloadGlobalSettings = (globalSettings != null && globalSettings.settingsVersion != version_info.SETTINGS_VER);

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

                globalSettings?.reset();
            }
            saveGlobalSettings();
            
            
            settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + "Redwing.flamegen.json";
            forceReloadGlobalSettings = (secondarySettings != null && secondarySettings.settingsVersion != version_info.SETTINGS_VER);

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

                secondarySettings?.reset();
            }
            
            saveSecondarySettings();
        }

        private void saveGame(SaveGameData data)
        {
            addComponent();
        }

        private void printErrors()
        {
            if (noModCommon)
            {
                GameManager.instance.gameObject.AddComponent<redwing_error>();
            }
            else
            {
                modcommonAddRedwingError();
            }

            if (problemCode == 1)
            {
                globalSettings.redwingFirstLaunch = false;
            }
        }

        private void addComponent()
        {
            log("Adding Redwing to game.");

            if ( ( !blackmothExists && !shitmothst) && globalSettings.useGreymothDashWhenBlackmothMissing)
            {
                GameManager.instance.gameObject.AddComponent<greymoth>();
                // no blackmoth so no need to override it.
                redwing_hooks.overrideBlackmothNailDmg = false;
                log("Unable to find Blackmoth, loading Greymoth instead.");
            }
            else if (blackmothExists)
            {
                log("Found Blackmoth...");
                log(globalSettings.overrideBlackmothNailDamage
                    ? "The God of fire and void has arrived."
                    : "Enter the knight, on flaming wings.");

                redwing_hooks.overrideBlackmothNailDmg = globalSettings.overrideBlackmothNailDamage;
            }
            else
            {
                log("Not adding any dash manager. I hope you have one loaded...");
            }

            GameManager.instance.gameObject.AddComponent<redwing_flame_gen>();
            GameManager.instance.gameObject.AddComponent<redwing_hooks>();
            GameManager.instance.gameObject.AddComponent<room_checker>();
            
            if (!noModCommon)
                modcommonAddRedwingError();
            
            log(Language.Language.CurrentLanguage() + " is your current language.");
        }

        // put in separate function to avoid weird errors caused by optimization.
        private static void modcommonAddRedwingError()
        {
            GameManager.instance.gameObject.GetOrAddComponent<redwing_error>();
        }

        public void Unload()
        {
            log("Disabling! If you see any more non-settings messages by this mod please report as an issue.");
            ModHooks.Instance.AfterSavegameLoadHook -= saveGame;
            ModHooks.Instance.NewGameHook -= addComponent;

        }

        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}
