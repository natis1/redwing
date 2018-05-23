using Modding;

namespace redwing
{
    public class VersionInfo
    {
        readonly public static int SettingsVer = 3;
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
