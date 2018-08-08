using System.IO;
using System.Reflection;
using UnityEngine;

namespace redwing
{
    public static class load_textures
    {

        public static void loadAllTextures()
        {
            foreach (string res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (res.Contains(shieldPrefix))
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (res.Contains(i + ".png"))
                        {
                            loadShieldTexture(i);
                        }
                    }
                }
                //log("Found resource with name " + res);
            }

        }


        private static void loadShieldTexture(int flameNum)
        {
            const string prepend = "redwing.assets.shield.Frame";
            const string append = ".png";

            Texture2D shieldTex;
            switch (flameNum)
            {
                case 0:
                    log("Not sure how to handle 0. Flame shields are 1 indexed.");
                    return;
                case 1:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldLost[2] = shieldTex;
                    return;
                case 2:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldLost[1] = shieldTex;
                    return;
                case 3:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldLost[0] = shieldTex;
                    return;
                case 4:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge1[0] = shieldTex;
                    flameShieldCharge1[4] = shieldTex;
                    flameShieldCharge2[1] = shieldTex;
                    return;
                case 5:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge1[1] = shieldTex;
                    return;
                case 6:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge1[2] = shieldTex;
                    return;
                case 7:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge1[3] = shieldTex;
                    flameShieldCharge2[0] = shieldTex;
                    return;
                case 8:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge2[2] = shieldTex;
                    return;
                case 9:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge2[3] = shieldTex;
                    return;
                case 10:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge2[4] = shieldTex;
                    flameShieldCharged[0] = shieldTex;
                    return;
                case 11:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharge2[5] = shieldTex;
                    flameShieldCharged[1] = shieldTex;
                    return;
                case 12:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharged[2] = shieldTex;
                    return;
                case 13:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharged[3] = shieldTex;
                    return;
                case 14:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldCharged[4] = shieldTex;
                    return;
                case 15:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    flameShieldLost[3] = shieldTex;
                    return;
                default:
                    log("Unable to process shield texture of unknown number " + flameNum);
                    break;
            }
        }

        public static Texture2D loadImageFromAssembly(string imageName)
        {
            Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName);
            if (imageStream != null)
            {
                byte[] buffer = new byte[imageStream.Length];
                imageStream.Read(buffer, 0, buffer.Length);
                imageStream.Dispose();

                //Create texture from bytes
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(buffer);
                return tex;
            }
            
            log("Unable to load image for some reason because imageStream was null.");
            return new Texture2D(1, 1);
            
        }

        private const string shieldPrefix = "shield.Frame";

        //public const int flameShieldCharge1RepeatFrames = 3;
        //public const int flameShieldCharge2RepeatFrames = 4;
        //public const int flameShieldChargedRepeatFrames = 3;

        public const int flameShieldCharge1IntroFrames = 2;
        public const int flameShieldCharge2IntroFrames = 3;
        public const int flameShieldChargedIntroFrames = 2;
        public static readonly Texture2D[] flameShieldCharge1 = new Texture2D[5];
        public static readonly Texture2D[] flameShieldCharge2 = new Texture2D[6];
        public static readonly Texture2D[] flameShieldCharged = new Texture2D[5];
        public static readonly Texture2D[] flameShieldLost = new Texture2D[4];

        //public const int flameLeftX = 200;
        public const int flameYOffset = 200;
        
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}