using System;
using System.Collections.Generic;
using System.Reflection;
using Modding;
// ReSharper disable UnusedMember.Global

namespace angleintegration
{
    internal struct short_lang_string
    {
        public readonly string value;
        public readonly int priority;
        public readonly bool allowOverride;

        public short_lang_string(string value, int priority, bool allowOverride)
        {
            this.value = value;
            this.priority = priority;
            this.allowOverride = allowOverride;
        }
    }

    internal struct short_player_bool
    {
        public readonly bool value;
        public readonly int priority;
        public readonly bool allowOverride;

        public short_player_bool(bool value, int priority, bool allowOverride)
        {
            this.value = value;
            this.priority = priority;
            this.allowOverride = allowOverride;
        }
    }

    internal struct short_player_int
    {
        public readonly int value;
        public readonly int priority;
        public readonly bool allowOverride;

        public short_player_int(int value, int priority, bool allowOverride)
        {
            this.value = value;
            this.priority = priority;
            this.allowOverride = allowOverride;
        }
    }
    
    
    // ReSharper disable once InconsistentNaming
    public class angleint : modern_mod
    {
        private static Dictionary<string, Dictionary<string, short_lang_string>> languageStrings = 
            new Dictionary<string, Dictionary<string, short_lang_string>>();
        
        private static Dictionary<string, short_player_bool> playerBools = new Dictionary<string, short_player_bool>();
        
        private static Dictionary<string, short_player_int> playerInts = new Dictionary<string, short_player_int>();
        
        
        public static List<modern_mod_vars> modernMods { get; internal set; } = new List<modern_mod_vars>();

        public angleint()
        {
            setupModVars(new modern_mod_vars("Angle Integration", "0.0.1", 1, int.MinValue));
        }

        public override void Initialize()
        {
            Log("Adding modern language get hook.");
            ModHooks.Instance.LanguageGetHook += modernLanguageGet;
            ModHooks.Instance.GetPlayerBoolHook += modernPlayerBoolGet;
            
        }

        public override string getVersionAppend()
        {
            const int minApi = 40;
            string ver = "";
            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minApi;
            if (apiTooLow) ver += " (Error: ModAPI too old)";
            Log("For debugging purposes, version is " + ver);
            return ver;
        }

        public static void clearLanguageGet()
        {
            Logger.Log("[Angle Integration] Warning! Clearing all language strings.");
            languageStrings = new Dictionary<string, Dictionary<string, short_lang_string>>();
        }

        public static void clearPlayerDataOverrides()
        {
            Logger.Log("[Angle Integration] Warning! Clearing all player ints and player bools.");
            playerBools = new Dictionary<string, short_player_bool>();
            playerInts = new Dictionary<string, short_player_int>();
        }
        

        public static void addPlayerInt(player_int i)
        {
            if (!playerInts.ContainsKey(i.intKey))
            {
                playerInts[i.intKey] = new short_player_int(i.value, i.priority, i.allowOverride);
            }
            else
            {
                short_player_int oldI = playerInts[i.intKey];
                if (oldI.allowOverride || oldI.priority < i.priority)
                {
                    playerInts[i.intKey] = new short_player_int(i.value, i.priority, i.allowOverride);
                }
                else
                {
                    Logger.Log("[Angle Integration] Unable to add player int "  +
                               "because priority lower than existing mod at " +
                               i.intKey + " You may force remove this bool using removePlayerInt!");
                }
            }
        }

        public static void removePlayerInt(string key)
        {
            playerInts.Remove(key);
        }
        
        private int modernPlayerIntGet(string originalset)
        {
            return playerInts.ContainsKey(originalset) ?
                playerInts[originalset].value : PlayerData.instance.GetIntInternal(originalset);
        }

        public static void addPlayerBool(player_bool b)
        {
            if (!playerBools.ContainsKey(b.boolKey))
            {
                playerBools[b.boolKey] = new short_player_bool(b.state, b.priority, b.allowOverride);
            }
            else
            {
                short_player_bool oldB = playerBools[b.boolKey];
                if (oldB.allowOverride || oldB.priority < b.priority)
                {
                    playerBools[b.boolKey] = new short_player_bool(b.state, b.priority, b.allowOverride);
                }
                else
                {
                    Logger.Log("[Angle Integration] Unable to add player bool "  +
                               "because priority lower than existing mod at " +
                               b.boolKey + " You may force remove this bool using removePlayerBool!");
                }
            }
        }

        public static void removePlayerBool(string key)
        {
            playerBools.Remove(key);
        }
        
        private bool modernPlayerBoolGet(string originalset)
        {
            return playerBools.ContainsKey(originalset) ?
                playerBools[originalset].value : PlayerData.instance.GetBoolInternal(originalset);
        }

        public static void addLanguageString(language_string l)
        {
            //Logger.Log("Adding language string [" + l.sheetTitle + "] [" + l.key + "]");
            if (!languageStrings.ContainsKey(l.sheetTitle))
            {
                languageStrings.Add(l.sheetTitle, new Dictionary<string, short_lang_string>());
            }

            if (!languageStrings[l.sheetTitle].ContainsKey(l.key))
            {
                languageStrings[l.sheetTitle][l.key] = new short_lang_string(l.value, l.priority, l.allowOverride);
            }
            else
            {
                short_lang_string oldLangString = languageStrings[l.sheetTitle][l.key];
                if (oldLangString.allowOverride || oldLangString.priority < l.priority)
                {
                    languageStrings[l.sheetTitle][l.key] = new short_lang_string(l.value, l.priority, l.allowOverride);
                }
                else
                {
                    Logger.Log("[Angle Integration] Unable to add language string "  +
                                       "because priority lower than existing mod string at " +
                                       l.sheetTitle + " " + l.key);
                }
            }
        }

        public static void removeLanguageString(string sheetTitle, string key)
        {
            if (!languageStrings.ContainsKey(sheetTitle)) return;
            languageStrings[sheetTitle].Remove(key);
        }
        
        private static string modernLanguageGet(string key, string sheettitle)
        {
            if (!languageStrings.ContainsKey(sheettitle)) return Language.Language.GetInternal(key, sheettitle);
            
            return languageStrings[sheettitle].ContainsKey(key) ?
                languageStrings[sheettitle][key].value : Language.Language.GetInternal(key, sheettitle);
        }
    }
}