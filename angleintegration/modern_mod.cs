using System;
using System.IO;
using System.Reflection;
using angleintegration;
using Modding;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace angleintegration
{
    public struct modern_mod_vars
    {
        public readonly string modName;
        public readonly string version;

        // ReSharper disable once MemberCanBePrivate.Global
        public readonly int modVersionInt;
        public readonly int loadPriority;

        public modern_mod_vars(string modName, string version, int modVersionInt, int loadPriority)
        {
            this.modName = modName;
            this.version = version;
            this.modVersionInt = modVersionInt;
            this.loadPriority = loadPriority;
        }
    }

    public class modern_mod : Mod
    {
        protected modern_mod_vars modVars;

        protected modern_mod()
        {
            setupModVars(new modern_mod_vars("UNNAMED MOD PLEASE NAME", "UNKNOWN_VERSION", 0, -5));
        }

        protected void setupModVars(modern_mod_vars v)
        {
            modVars = v;
            FieldInfo field = typeof(modern_mod).GetField
                ("Name", BindingFlags.Instance | BindingFlags.Public);
            field?.SetValue(this, modVars.modName);

            Log("modname is " + Name);
            
            angleint.modernMods.Add(modVars);
        }

        public override string GetVersion()
        {
            return modVars.version;
        }

        public override int LoadPriority()
        {
            Log("Load prio is " + modVars.loadPriority);
            return modVars.loadPriority;
        }

        public virtual string getVersionAppend()
        {
            return "";
        }
    }


    public class modern_mod<TSaveSettings, TGlobalSettings> :
        modern_mod<TSaveSettings> where TSaveSettings : IModSettings, new() where TGlobalSettings : IModSettings, new()
    {
        private readonly string globalSettingsFilename;
        private TGlobalSettings modGlobalSettings;

        protected modern_mod()
        {
            globalSettingsFilename = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name +
                                     ".GlobalSettings.json";
            loadGlobalSettings();
        }

        public TGlobalSettings globalSettings
        {
            get
            {
                TGlobalSettings gSettings = this.modGlobalSettings;
                if ((object) gSettings != null)
                    return gSettings;
                return this.modGlobalSettings = Activator.CreateInstance<TGlobalSettings>();
            }
            set { modGlobalSettings = value; }
        }

        public void saveGlobalSettings()
        {
            Log("Saving Global Settings");
            if (File.Exists(globalSettingsFilename + ".bak"))
                File.Delete(globalSettingsFilename + ".bak");
            if (File.Exists(globalSettingsFilename))
                File.Move(globalSettingsFilename, globalSettingsFilename + ".bak");
            using (FileStream fileStream = File.Create(globalSettingsFilename))
            {
                using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream))
                {
                    string json = JsonUtility.ToJson((object) globalSettings, true);
                    streamWriter.Write(json);
                }
            }
        }

        public void loadGlobalSettings()
        {
            Log("Loading Global Settings");
            if (!File.Exists(globalSettingsFilename))
                return;
            using (FileStream fileStream = File.OpenRead(globalSettingsFilename))
            {
                using (StreamReader streamReader = new StreamReader((Stream) fileStream))
                    modGlobalSettings = JsonUtility.FromJson<TGlobalSettings>(streamReader.ReadToEnd());
            }
        }
    }


    public class modern_mod<TSaveSettings> : modern_mod where TSaveSettings : IModSettings, new()
    {
        private TSaveSettings modSettings;

        public TSaveSettings settings
        {
            get
            {
                TSaveSettings set = modSettings;
                if ((object) set != null)
                    return set;
                return modSettings = Activator.CreateInstance<TSaveSettings>();
            }
            set { modSettings = value; }
        }

        private void loadSettings(SaveGameData data)
        {
            string name = GetType().Name;
            Log("Loading Mod Settings from Save.");
            if (data?.modData == null || !data.modData.ContainsKey(name))
                return;
            settings = Activator.CreateInstance<TSaveSettings>();
            settings.SetSettings(data.modData[name]);
            ISerializationCallbackReceiver set;
            if ((set = (object) settings as ISerializationCallbackReceiver) == null)
                return;
            set.OnAfterDeserialize();
        }

        private void saveSettings(SaveGameData data)
        {
            string name = GetType().Name;
            Log("Adding Settings to Save file");
            if (data.modData == null)
                data.modData = new ModSettingsDictionary();
            if (data.modData.ContainsKey(name))
                data.modData[name] = (IModSettings) settings;
            else
                data.modData.Add(name, (IModSettings) settings);
        }


        protected modern_mod()
        {
            Log("Instantiating Mod");
            ModHooks.Instance.BeforeSavegameSaveHook += saveSettings;
            ModHooks.Instance.AfterSavegameLoadHook += loadSettings;
        }
    }
}
    
