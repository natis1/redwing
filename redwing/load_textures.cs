using System.IO;
using System.Reflection;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

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
                        if (res.EndsWith("Frame" + i + ".png"))
                        {
                            loadShieldTexture(i);
                        }
                    }
                } else if (res.EndsWith("spark.png"))
                {
                    spark = loadImageFromAssembly(res);
                } else if (res.Contains(soulPrefix))
                {
                    if (res.EndsWith("empty.png"))
                    {
                        SOUL_HOLDER[0] = loadImageFromAssembly(res);
                    } else if (res.EndsWith("filledone.png"))
                    {
                        SOUL_HOLDER[1] = loadImageFromAssembly(res);
                    } else if (res.EndsWith("filledtwo.png"))
                    {
                        SOUL_HOLDER[2] = loadImageFromAssembly(res);
                    }
                }
                log("Found resource with name " + res);
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
                    FLAME_SHIELD_LOST[2] = shieldTex;
                    return;
                case 2:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_LOST[1] = shieldTex;
                    return;
                case 3:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_LOST[0] = shieldTex;
                    return;
                case 4:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE1[0] = shieldTex;
                    FLAME_SHIELD_CHARGE1[4] = shieldTex;
                    FLAME_SHIELD_CHARGE2[1] = shieldTex;
                    return;
                case 5:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE1[1] = shieldTex;
                    return;
                case 6:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE1[2] = shieldTex;
                    return;
                case 7:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE1[3] = shieldTex;
                    FLAME_SHIELD_CHARGE2[0] = shieldTex;
                    return;
                case 8:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE2[2] = shieldTex;
                    return;
                case 9:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE2[3] = shieldTex;
                    return;
                case 10:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE2[4] = shieldTex;
                    FLAME_SHIELD_CHARGED[0] = shieldTex;
                    return;
                case 11:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGE2[5] = shieldTex;
                    FLAME_SHIELD_CHARGED[1] = shieldTex;
                    return;
                case 12:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGED[2] = shieldTex;
                    return;
                case 13:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGED[3] = shieldTex;
                    return;
                case 14:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_CHARGED[4] = shieldTex;
                    return;
                case 15:
                    shieldTex = loadImageFromAssembly(prepend + flameNum + append);
                    FLAME_SHIELD_LOST[3] = shieldTex;
                    return;
                default:
                    log("Unable to process shield texture of unknown number " + flameNum);
                    break;
            }
        }

        private static Texture2D loadImageFromAssembly(string imageName)
        {
            //Create texture from bytes
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(getBytes(imageName));
            return tex;
        }
        
        private static byte[] getBytes(string filename){
            Stream dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename);
            if (dataStream == null) return null;
            
            byte[] buffer = new byte[dataStream.Length];
            dataStream.Read(buffer, 0, buffer.Length);
            dataStream.Dispose();
            return buffer;
        }


        private const string shieldPrefix = "shield.Frame";
        private const string soulPrefix = "holder";


        public const int FLAME_SHIELD_CHARGE1_INTRO_FRAMES = 2;
        public const int FLAME_SHIELD_CHARGE2_INTRO_FRAMES = 3;
        public const int FLAME_SHIELD_CHARGED_INTRO_FRAMES = 2;
        public static readonly Texture2D[] FLAME_SHIELD_CHARGE1 = new Texture2D[5];
        public static readonly Texture2D[] FLAME_SHIELD_CHARGE2 = new Texture2D[6];
        public static readonly Texture2D[] FLAME_SHIELD_CHARGED = new Texture2D[5];
        public static readonly Texture2D[] FLAME_SHIELD_LOST = new Texture2D[4];
        public static readonly Texture2D[] SOUL_HOLDER = new Texture2D[3];
        

        public static Texture2D spark;

        public static AudioClip nukeSound;

        //public const int flameLeftX = 200;
        public const int flameYOffset = 200;
        
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}