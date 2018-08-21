using System.Collections.Generic;
using angleintegration;
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

        private void Start()
        {
            if (englishWarnings || englishLore || lore.isEnglish())
            {
                setupWarningEN();
            }
            log("Warning code is " + redwingProblemCode);
        }
        
        private static void setupWarningEN()
        {
            if (redwingProblemCode != 0)
            {
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_04", "Thank you for your cooperation and enjoy Redwing!", false, 100));
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_AUTHOR", "- Avenging Angle, on behalf of the Redwing team", false, 100));
            }

            if ((redwingProblemCode & 4) != 0)
            {
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_01", "ERROR: One of your mods is either out of date or missing!", false, 100));
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_02", "Redwing CANNOT RUN with your current mod configuration.", false, 100));
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_03", "Please read modlog.txt for more details!", false, 100));
            } else if ( (redwingProblemCode & 1) != 0)
            {
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_01", "WARNING. PLEASE READ THE REDWING README BEFORE PLAYING.", false, 100));
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_02", "Also check out the global config file: 'Redwing.settings' located with your save files", false, 100));
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_03", "This message will disappear on relaunching the game! Please relaunch to see lore here.", false, 100));

                // Note: add "Please relaunch to see lore here!" when CP1 is out.
            } else if ((redwingProblemCode & 2) != 0)
            {
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_01", "WARNING. ERROR ENABLING HANDICAP: You cannot handicap yourself with Blackmoth installed.", false, 100));
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_02", "Blackmoth simply makes you too much of a God to be nerfed.", false, 100));
                angleint.addLanguageString(new language_string("General", "PROLOGUE_EXCERPT_03", "The handicap has been disabled ingame but not in the settings file...", false, 100));
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