using Modding;

namespace redwing
{
    public class VersionInfo
    {
        public static readonly int SettingsVer = 1;
    }

    public class redwingGlobalSettings : IModSettings
    {


        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();


            SettingsVersion = VersionInfo.SettingsVer;
        }
        public int SettingsVersion { get => GetInt(); set => SetInt(value); }

    }


    public class redwingSettings : IModSettings
    {
        // none needed

    }



}
