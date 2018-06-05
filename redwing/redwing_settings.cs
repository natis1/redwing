using Modding;

namespace redwing
{
    public static class version_info
    {
        public const int SETTINGS_VER = 1;
    }

    public class redwing_global_settings : IModSettings
    {


        public void reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();


            settingsVersion = version_info.SETTINGS_VER;
        }
        public int settingsVersion { get => GetInt();
            private set => SetInt(value); }

    }


    public class redwing_settings : IModSettings
    {
        // none needed

    }



}
