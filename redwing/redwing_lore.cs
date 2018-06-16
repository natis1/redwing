using System.Collections.Generic;
using Modding;
using UnityEngine;

namespace redwing
{
    public class redwing_lore : MonoBehaviour
    {
        
        // 100% of the lore in this file is completely legit.
        private static readonly Dictionary<string, Dictionary<string, string>> langStrings = new Dictionary<string, Dictionary<string, string>>();
        
        private void Start()
        {
            log("Adding lore to a game without it.");
            setupLoreEN();
            
            ModHooks.Instance.LanguageGetHook += printRealLore;
        }

        // ReSharper disable once InconsistentNaming because ur dumb and EN is a language code
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