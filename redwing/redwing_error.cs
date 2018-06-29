using System.Collections.Generic;
using Language;
using Modding;
using UnityEngine;
// ReSharper disable InconsistentNaming because language codes.

namespace redwing
{
    public class redwing_error : MonoBehaviour
    {
        // 100% of the errors in this file are completely legit.
        private static readonly Dictionary<string, Dictionary<string, string>> langStrings = new Dictionary<string, Dictionary<string, string>>();
        
        public static int redwingProblemCode;
        public static bool englishWarnings;
        public static bool englishLore;
        
        private void OnDestroy()
        {
            ModHooks.Instance.LanguageGetHook -= printError;
        }

        private void Start()
        {
            if (englishWarnings || englishLore || redwing_lore.isEnglish())
            {
                setupWarningEN();
            }
            log("Warning code is " + redwingProblemCode);
            ModHooks.Instance.LanguageGetHook += printError;
        }
        
        private static void setupWarningEN()
        {
            langStrings["General"] = new Dictionary<string, string>();

            if (redwingProblemCode != 0)
            {
                langStrings["General"]["PROLOGUE_EXCERPT_04"] = "Thank you for your cooperation and enjoy Redwing!";
                langStrings["General"]["PROLOGUE_EXCERPT_AUTHOR"] = "- Avenging Angle, on behalf of the Redwing team";
            }

            if ((redwingProblemCode & 4) != 0)
            {
                langStrings["General"]["PROLOGUE_EXCERPT_01"] =
                    "ERROR: One of your mods is either out of date or missing!";
                langStrings["General"]["PROLOGUE_EXCERPT_02"] =
                    "Redwing CANNOT RUN with your current mod configuration.";
                langStrings["General"]["PROLOGUE_EXCERPT_03"] =
                    "Please read modlog.txt for more details!";
            } else if ( (redwingProblemCode & 1) != 0)
            {
                langStrings["General"]["PROLOGUE_EXCERPT_01"] =
                    "WARNING. PLEASE READ THE REDWING README BEFORE PLAYING.";
                langStrings["General"]["PROLOGUE_EXCERPT_02"] =
                    "Also check out the global config file: 'Redwing.GlobalSettings' located with your save files";
                langStrings["General"]["PROLOGUE_EXCERPT_03"] =
                    "This message will disappear on relaunching the game!";
                // Note: add "Please relaunch to see lore here!" when CP1 is out.
            } else if ((redwingProblemCode & 2) != 0)
            {
                langStrings["General"]["PROLOGUE_EXCERPT_01"] =
                    "WARNING. ERROR ENABLING HANDICAP: You cannot handicap yourself with Blackmoth installed.";
                langStrings["General"]["PROLOGUE_EXCERPT_02"] =
                    "Blackmoth simply makes you too much of a God to be nerfed.";
                langStrings["General"]["PROLOGUE_EXCERPT_03"] =
                    "The handicap has been disabled...";
            } 
            
            
        }
        
        private static string printError(string smallKey, string key)
        {
            //log("Attempting to read [" + key + "] [" + smallKey + "]" + "with value " + Language.Language.GetInternal(smallKey, key));
            
            
            if (langStrings.ContainsKey(key) && langStrings[key].ContainsKey(smallKey))
            {
                return langStrings[key][smallKey];
            }

            return Language.Language.GetInternal(smallKey, key);
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }

        
    }
}