using System;
using System.Collections.Generic;
using Language;
using Modding;
using UnityEngine;
// ReSharper disable InconsistentNaming because EN is a language code and so are the other ones

namespace redwing
{
    public class redwing_lore : MonoBehaviour
    {
        
        // 100% of the lore in this file is completely legit.
        private static readonly Dictionary<string, Dictionary<string, string>> langStrings = new Dictionary<string, Dictionary<string, string>>();

        public static bool overrideBlackmothLore;
        public static bool englishLore;

        private void OnDestroy()
        {
            ModHooks.Instance.LanguageGetHook -= printRealLore;
        }

        private void Start()
        {
            if (englishLore || isEnglish())
            {
                setupLoreEN();
                if (overrideBlackmothLore)
                {
                    setupBlackmothLoreEN();
                }
            }


            log("Added lore to a game without it.");

            ModHooks.Instance.LanguageGetHook += printRealLore;
        }
        
        public static bool isEnglish()
        {
            LanguageCode locale = Language.Language.CurrentLanguage();
            return (locale == LanguageCode.EN || locale == LanguageCode.EN_AU || locale == LanguageCode.EN_CA
                    || locale == LanguageCode.EN_CB || locale == LanguageCode.EN_GB || locale == LanguageCode.EN_IE
                    || locale == LanguageCode.EN_JM || locale == LanguageCode.EN_NZ || locale == LanguageCode.EN_TT
                    || locale == LanguageCode.EN_US || locale == LanguageCode.EN_ZA);
        }
        
        private static void setupLoreEN()
        {
            
            // Testing only. Not actual lore.
            /*langStrings["Titles"] = new Dictionary<string, string>
            {
                ["FINAL_BOSS_SUPER"] = "KDE",
                ["FINAL_BOSS_MAIN"] = "Plasma"
            };

            langStrings["General"] = new Dictionary<string, string>
            {
                ["GAME_TITLE"] = "Redwing Testing"
            };
            */

        }
        
        // only used once blackmoth actually gets lore
        private static void setupBlackmothLoreEN()
        {
            log("Replacing blackmoth lore with redwing lore.");
            
        }

        private static string printRealLore(string smallKey, string key)
        {
            if (langStrings.ContainsKey(key) && langStrings[key].ContainsKey(smallKey))
            {
                return langStrings[key][smallKey];
            }
            
            //log("Text requested from: [" + key + "] [" + smallKey + "]");
            string realLore = Language.Language.GetInternal(smallKey, key);
            //log("Text displayed is: " + realLore);
            
            return realLore;
        }


        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}