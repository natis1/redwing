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
                        if (res.EndsWith(i + ".png"))
                        {
                            loadShieldTexture(i);
                        }
                    }
                } else if (res.EndsWith("spark.png"))
                {
                    spark = loadImageFromAssembly(res);
                } else if (res.Contains("nuke.frame00"))
                {
                    for (int i = 0; i < 43; i++)
                    {
                        if (i <= 9)
                        {
                            if (res.EndsWith(nukePrefix + "0" + i + ".png"))
                                nukeAnimation[i] = loadImageFromAssembly(res);
                        }
                        else
                        {
                            if (res.EndsWith(nukePrefix + i + ".png"))
                                nukeAnimation[i] = loadImageFromAssembly(res);
                        }
                    }
                } else if (res.Contains("nukesound.wav"))
                {
                    loadNukeSound(res);
                }
                //log("Found resource with name " + res);
            }

        }


        private static void loadNukeSound(string soundName)
        {
            /*
            float[][] audioData = serializeAudio(getBytes(soundName));

            
            nukeSound = AudioClip.Create("redwingNukeBeep", audioData[1].Length, (int) audioData[0][1],
                (int) audioData[0][0], false);
            nukeSound.SetData(audioData[1], 0);
            
            */
        }

        private static float[][] serializeAudio(byte[] wav)
        {
            // Determine if mono or stereo
            int channelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels
 
            // Get the frequency
            int freqency = bytesToInt(wav,24);
             
            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16
             
            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while(!(wav[pos]==100 && wav[pos+1]==97 && wav[pos+2]==116 && wav[pos+3]==97)) {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;
             
            // Pos is now positioned to start of actual sound data.
            int sampleCount = (wav.Length - pos)/2;     // 2 bytes per sample (16 bit sound mono)
            if (channelCount == 2) sampleCount /= 2;        // 4 bytes per sample (16 bit stereo)
             
            // Allocate memory (right will be null if only mono sound)
            float[] leftChannel = new float[sampleCount];
            float[] rightChannel = channelCount == 2 ? new float[sampleCount] : null;
            
            // Write to double array/s:
            int i=0;
            while (pos < wav.Length) {
                leftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
                if (channelCount == 2) {
                    // ReSharper disable once PossibleNullReferenceException because rider's stupid and this
                    // could literally never be null.
                    rightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;
            }
            float[][] audioData = new float[channelCount + 1][];
            audioData[0] = new float[3];
            audioData[0][0] = freqency;
            audioData[0][1] = channelCount;
            audioData[0][2] = sampleCount;

            audioData[1] = leftChannel;
            if (channelCount == 2)
            {
                audioData[2] = rightChannel;
            }

            return audioData;
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

        private static Texture2D loadImageFromAssembly(string imageName)
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
        
        // convert two bytes to one float in the range -1 to 1
        private static float bytesToFloat(byte firstByte, byte secondByte) {
            // convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0F;
        }

        private static int bytesToInt(byte[] bytes,int offset=0){
            int value=0;
            for(int i=0;i<4;i++){
                value |= ((int)bytes[offset+i])<<(i*8);
            }
            return value;
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
        private const string nukePrefix = "nuke.frame00";

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
        
        public static readonly Texture2D[] nukeAnimation = new Texture2D[43];

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