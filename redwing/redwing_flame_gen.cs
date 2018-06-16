using Modding;
using UnityEngine;
using System;
using UnityEngine.UI;
using HutongGames.PlayMaker;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using static HeroController;
using Object = UnityEngine.Object;

namespace redwing
{
    internal class redwing_flame_gen : MonoBehaviour
    {
        private readonly Texture2D[] fireBalls = new Texture2D[10];
        private readonly Texture2D[] fireballMagmas = new Texture2D[12];
        private readonly Texture2D[] fireballMagmaBalls = new Texture2D[4];
        
        private readonly Texture2D[] fireTrails = new Texture2D[10];
        private readonly Texture2D[] firePillars = new Texture2D[10];
        private readonly Texture2D[] fireSpikes = new Texture2D[16];
        private readonly Texture2D[] fireShields = new Texture2D[20];
        
        
        private readonly AudioClip[] soundFxClip = new AudioClip[3];
        private const int TESTING_CLIP = 2;
        private int boopTimer;


        //float[][] soundEffects = new float[4][];

        // bullshit number picked because all the cool kids are doing it
        private const int AUDIO_SAMPLE_HZ = 44100;


        private readonly long[] pinkRows = new long[30];
        private long pinkRunningSum;   /* Used to optimize summing of generators. */
        private float pinkScalar;       /* Used to scale within range of -1.0 to +1.0 */
        private const int PINK_INDEX_MASK = (1 << 30) - 1;

        private int pinkIndex;
        //private readonly 



        public GameObject plane;
        public GameObject canvas;
        public static System.Random rng;

        

        
        


        public const int FBTEXTURE_WIDTH = 150;
        public const int FBTEXTURE_HEIGHT = 150;

        private const int FTTEXTURE_WIDTH = 600;
        private const int FTTEXTURE_HEIGHT = 300;

        private const int FPTEXTURE_WIDTH = 500;
        private const int FPTEXTURE_HEIGHT = 1080;

        private const int FSTEXTURE_WIDTH = 80;
        private const int FSTEXTURE_HEIGHT = 1500;
        
        public const int FBMBTEXTURE_WIDTH = 50;
        public const int FBMBTEXTURE_HEIGHT = 50;

        public const int FSHIELDTEXTURE_WIDTH = 250;
        public const int FSHIELDTEXTURE_HEIGHT = 300;

        private const double OPACITY_MASK = 1.0;

        public int sceneTimer;
        public int currentImg;

        
        

        // pick a factor of 360
        private const int INTERPOLATE_DEGREES = 18;

        // pick a factor of FTTEXTURE_WIDTH
        private const int FT_INTERPOLATE_PIXELS = 60;

        // pick a factor of FPTEXTURE_HEIGHT
        private const int FP_INTERPOLATE_PIXELS = 40;

        // pick a factor of FSTEXTURE_HEIGHT
        private const int FS_INTERPOLATE_PIXELS = 50;

        private const int FB_MAGMA_INTERPOLATE_PIXELS = 15;

        
        
        //public readonly Color[] flameIntensityCurve = { Color.red, new Color(1f, 0.3f, 0f), Color.yellow, Color.white };

        // What are the fire colors anyway?
        private readonly Color[] flameIntensityCurve = { Color.red, Color.yellow, Color.white, Color.white };

        // At what point do you switch from color X to color Y.
        private readonly double[] flameIntensityThresholds = { 0.4, 0.7, 2.5, 2.6 };


        

        public void Start()
        {
            rng = new System.Random();
            
            log("Building fireballs. please wait...");
            generateFlameTextures();

            // memes
            log("Building sound effects from my mixtape...");
            generateSoundEffects();

            redwing_game_objects.fireBalls = fireBalls;
            redwing_game_objects.fireLasers = fireSpikes;
            redwing_game_objects.firePillars = firePillars;
            redwing_game_objects.fireTrails = fireTrails;
            redwing_game_objects.fireballMagmas = fireballMagmas;
            redwing_game_objects.fireballMagmaFireballs = fireballMagmaBalls;

            redwing_hooks.flameShieldTextures = fireShields;

            redwing_game_objects.soundFxClip = soundFxClip;

        }

        



        //for testing only

        private static IEnumerator volumeFade(AudioSource audioSource, float waitTime)
        {
            float startTime = Time.time;
            waitTime = waitTime + startTime;
            while (startTime < waitTime)
            {
                startTime = Time.time;
                yield return null;
            }
            audioSource.Stop();

        }
        
        private void generateSoundEffects()
        {
            soundFxClip[0] = AudioClip.Create("fireball1", (int) (AUDIO_SAMPLE_HZ * 1.0) + 30000, 1, AUDIO_SAMPLE_HZ, false);
            soundFxClip[0].SetData(generateFbSound((int)(AUDIO_SAMPLE_HZ * 1.0) + 30000), 0);

            soundFxClip[1] = AudioClip.Create("laser1", AUDIO_SAMPLE_HZ * 10, 1, AUDIO_SAMPLE_HZ, false);
            soundFxClip[1].SetData(generateLaserSound(AUDIO_SAMPLE_HZ * 10), 0);

            soundFxClip[2] = AudioClip.Create("shieldwavy", AUDIO_SAMPLE_HZ * 3, 1, AUDIO_SAMPLE_HZ, false);
            soundFxClip[2].SetData(generateShieldSound(AUDIO_SAMPLE_HZ * 3), 0);

        }

        private float[] generateShieldSound(int length)
        {
            float[] fx = new float[length];

            fx = generateNoiseAtHz(0.06, 127.666, fx, 0, fx.Length);
            fx = generateNoiseAtHz(0.01, 128, fx, 0, fx.Length);
            //fx = generateWhiteNoise(0.03, 0, fx.Length, fx);
            fx = normalizeVolume(fx);
            return fx;
        }

        private float[] generatePinkNoise(float volume, float[] fx)
        {
            pinkIndex = 0;
            pinkScalar = 1.0f / (31.0f * (1 << (24 - 1)));
            pinkRunningSum = 0;

            try
            {
                for (int i = 0; i < fx.Length; i++)
                {
                    fx[i] = generatePinkValue() * volume;
                }
            } catch (Exception e)
            {
                log("Error building pink noise " + e);
            }


            return fx;
        }

        private float generatePinkValue()
        {
            long newRandom;
            pinkIndex = (pinkIndex++) & PINK_INDEX_MASK;
            if (pinkIndex != 0)
            {
                int numZeros = 0;
                int n = pinkIndex;
                while ( (n & 1) == 0)
                {
                    n = n >> 1;
                    numZeros++;
                }

                pinkRunningSum -= pinkRows[numZeros];
                newRandom = rng.Next(int.MinValue, int.MaxValue);
                newRandom = (newRandom << 32);
                newRandom = newRandom | (long)rng.Next(int.MinValue, int.MaxValue);
                pinkRunningSum += newRandom;
                pinkRows[numZeros] = newRandom;

            }

            newRandom = rng.Next(int.MinValue, int.MaxValue);
            newRandom = (newRandom << 32);
            newRandom = newRandom | (long)rng.Next(int.MinValue, int.MaxValue);
            long sum = pinkRunningSum + newRandom;

            float f = pinkScalar * sum;
            if (f > 1.0)
            {
                f = 1.0f;
            } else if (f < -1.0)
            {
                f = 1.0f;
            }
            return f;
        }

        private float[] generateFbSound(int length)
        {
            float[] fx = new float[length];
            return generateFbSound(fx);
        }

        private float[] generateFbSound (float[] fx)
        {
            try
            {
                float[] fx2 = new float[fx.Length];
                fx2 = generateWhiteNoise(2.0, 0, fx2.Length, fx2);
                fx2 = lowPassFilter(170, fx2);
                
                fx = highPitchExplosionSound(1.0, fx);

                fx = normalizeVolume(fx);
                
                for (int i = 0; i < fx.Length; i++)
                {
                    fx[i] = fx[i] + fx2[i];
                }

                fx = fadeAudio(0.0, fx.Length - 34000, fx.Length - 30000, fx);

                fx = normalizeVolume(fx);
                log("Made fireball sound without error");

                return fx;
            } catch (Exception e)
            {
                log("Unable to make sound because " + e);
                return null;
            }
        }

        private float[] generateLaserSound(int length)
        {
            float[] fx = new float[length];
            return generateLaserSound(fx);
        }

        private float[] generateLaserSound(float[] fx)
        {
            try
            {
                //fx = generateNoise(0.9, 100.0, 10000, 15.0, fx);
                //fx = generatePinkNoise(0.7f, fx);
                //fx = generateNoiseAtHZ(1.2, 160, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(1.0, 100, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(1.5, 50, fx, 0, fx.Length);
                fx = generateWhiteNoise(6.0, 0, fx.Length, fx);

                fx = generateSawtooth(3.0, 4000, fx, 0, fx.Length);
                fx = generateSawtooth(2.0, 2000, fx, 0, fx.Length);
                fx = generateSawtooth(1.0, 1000, fx, 0, fx.Length);
                fx = generateSawtooth(4.0, 250, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(2.0, 4000, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(1.0, 2000, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(2.0, 1000, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(1.0, 500, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(2.0, 250, fx, 0, fx.Length);

                fx = normalizeVolume(fx);
                log("Made fireball sound without error");

                return fx;
            }
            catch (Exception e)
            {
                log("Unable to make sound because " + e);
                return null;
            }
        }

        // this is stupid and can't possibly work.
        private static float[] stupidLowPassFilter(float[] fx)
        {
            float f2 = 0;
            for (int i = 0; i < fx.Length; i++)
            {
                float f = fx[i];
                fx[i] = f2 + fx[i];
                f2 = f;
            }

            return fx;
        }


        // code mostly stolen from here: https://www.codeproject.com/tips/1092012/a-butterworth-filter-in-csharp
        // which is using a very permissive license that I can modify.
        // how the fuck does this work.
        private float[] lowPassFilter(double frequency, float[] fx)
        {

            //double Samplingrate = 1 / (double)AUDIO_SAMPLE_HZ;
            long dF2 = fx.Length - 1;        // The data range is set with dF2
            float[] dat2 = new float[dF2 + 4]; // Array with 3 extra points front and back
            float[] data = fx; // Ptr., changes passed data

            // Copy indata to Dat2
            for (long r = 0; r < dF2; r++)
            {
                dat2[2 + r] = fx[r];
            }
            dat2[1] = dat2[0] = fx[0];
            dat2[dF2 + 3] = dat2[dF2 + 2] = fx[dF2];
            log("Allocation complete without error");

            double wc = Math.Tan(frequency * Math.PI / ((double)AUDIO_SAMPLE_HZ) );
            double k1 = 1.414213562 * wc; // Sqrt(2) * wc
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;

            // RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
            float[] datYt = new float[dF2 + 4];
            datYt[1] = datYt[0] = fx[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                datYt[s] = (float)(a * dat2[s] + b * dat2[s - 1] + c * dat2[s - 2]
                           + d * datYt[s - 1] + e * datYt[s - 2]);
            }
            datYt[dF2 + 3] = datYt[dF2 + 2] = datYt[dF2 + 1];

            log("Recursive triggers complete");

            // FORWARD filter
            float[] datZt = new float[dF2 + 2];
            datZt[dF2] = datYt[dF2 + 2];
            datZt[dF2 + 1] = datYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                datZt[-t] = (float)(a * datYt[-t + 2] + b * datYt[-t + 3] + c * datYt[-t + 4]
                            + d * datZt[-t + 1] + e * datZt[-t + 2]);
            }
            log("Recursive triggers complete");

            // Calculated points copied for return
            for (long p = 0; p < dF2; p++)
            {
                data[p] = datZt[p];
            }

            return fx;
        }

        private float[] amplifyAudio(double volume, float[] fx)
        {
            for (int i = 0; i < fx.Length; i++)
            {
                fx[i] *= (float) volume;
            }
            return fx;
        }

        private float[] speedUpEffect(double speed, float[] fx)
        {
            float[] speededFx = new float[fx.Length];

            for (int i = 0; i < fx.Length; i++)
            {
                double realPosition = i * speed;
                int positionRoundedDown = (int)realPosition;
                double weighting = realPosition % 1.0;
                if (positionRoundedDown + 1 < fx.Length)
                {
                    speededFx[i] = (float)(((1.0 - weighting) * fx[positionRoundedDown]) + (weighting * fx[positionRoundedDown + 1]));

                }
                else
                {
                    break;
                }
            }

            return speededFx;
        }


        private float[] highPitchExplosionSound(double volume, float[] fx)
        {
            // we ideally make the original super loud to avoid having to do boosting.
            fx = generateWhiteNoise(volume, 0, fx.Length, fx);
            fx = fadeAudio(0.0, 0, fx.Length, fx);
            float[] pass1 = speedUpEffect(1.5, fx);
            pass1 = fadeAudio(0.0, 0, (int) (pass1.Length / 1.5), pass1);
            float[] pass2 = speedUpEffect(1.5, pass1);
            pass2 = fadeAudio(0.0, 0, (int)(pass2.Length / 2.25), pass2);
            float[] pass3 = speedUpEffect(1.5, pass2);

            //18dB
            fx = lowPassFilter(100.0, fx);
            fx = stupidLowPassFilter(fx);
            fx = amplifyAudio(4.8, fx);

            //fx = lowPassFilter(100.0, fx);
            //fx = amplifyAudio(1.5, fx);
            //pass1 = lowPassFilter(200.0, pass1);
            //pass1 = lowPassFilter(200.0, pass1);
            // 22dB
            pass1 = stupidLowPassFilter(pass1);
            pass1 = stupidLowPassFilter(pass1);
            pass1 = amplifyAudio(3.2, pass1);

            // 16dB
            pass2 = stupidLowPassFilter(pass2);
            //pass2 = lowPassFilter(400.0, pass2);
            pass2 = amplifyAudio(2.6, pass2);

            // 14dB
            pass3 = lowPassFilter(800.0, pass3);
            pass3 = amplifyAudio(3.4, pass3);

            // merge effects

            
            for (int i = 0; i < fx.Length; i++)
            {
                fx[i] = fx[i] + pass1[i] + pass2[i] + pass3[i];
            }
            fx = amplifyAudio(2.0, fx);


            fx = fadeAudio(0.0, (int)(fx.Length / 1.5), fx.Length, fx);

            return fx;
        }


        private float[] fadeAudio(double fadeVolume, int startTime, int endFade, float[] fx)
        {
            try
            {

                // first, smoothly fade the section needed
                for (int i = startTime; i < endFade; i++)
                {
                    double fadeAmount = (i - startTime) / ((double)(endFade - startTime));
                    // weighted avg
                    double weightedVol = ((1.0 - fadeAmount) + fadeAmount * fadeVolume);
                    float actualVolume = (float)(Math.Pow(weightedVol, 2.0));

                    fx[i] *= actualVolume;
                }

                // now fade everything else past the end.
                for (int i = endFade; i < fx.Length; i++)
                {
                    fx[i] *= (float)fadeVolume;
                }

            }
            catch (Exception e)
            {
                log("Unable to fade out audio. probably because out of bounds " + e);
            }

            return fx;
        }

        private float[] generateWhiteNoise(double volume, int startTime, int endTime, float[] fx)
        {
            try
            {
                for (int i = startTime; i < endTime; i++)
                {
                    double r = rng.NextDouble() * 2.0 - 1.0;
                    fx[i] += (float) (r * volume);
                }
            }
            catch (Exception e)
            {
                log("Unable to generate white noise from time " + startTime + " to " + endTime + " probably because out of bounds " + e);
            }

            return fx;
        }

        private float[] generateSawtooth(double volume, double frequency, float[] fx)
        {
            return generateSawtooth(volume, frequency, fx, 0, fx.Length);
        }

        private float[] generateSawtooth(double volume, double frequency, float[] fx, int startTime, int endTime)
        {
            // get number of samples that a pitch lasts
            double pitchTime = AUDIO_SAMPLE_HZ / frequency;

            // Don't generate every frequency at the same point
            // Gotta pick random ones.
            double randomOffset = rng.NextDouble() * pitchTime;

            bool inverseData = false;
            double doInverse = rng.NextDouble();
            if (doInverse >= 0.5)
            {
                inverseData = true;
            }

            

            try
            {
                for (int i = startTime; i < endTime; i++)
                {
                    double currentPos = (double)i + randomOffset;
                    double currentVal = 2 * (currentPos % pitchTime) / pitchTime;
                    if (inverseData)
                    {
                        currentVal *= -1;
                    }
                    fx[i] += (float) (currentVal * volume);

                }
            } catch (Exception e)
            {
                log("Unable to generate sawtooth from time " + startTime + " to " + endTime + " probably because out of bounds " + e);
            }

            return fx;
        }

        private float[] generateNoise(double volume, double freqMin, double freqMax,
            double freqInterval, float[] fx)
        {
            return generateNoise(volume, freqMin, freqMax, freqInterval, fx, 0, fx.Length);
        }

        private float[] generateNoise (double volume, double freqMin, double freqMax,
            double freqInterval, float[] fx, int startTime, int endTime)
        {
            double realVolume = volume * freqInterval / (freqMax - freqMin + freqInterval);
            for (double i = freqMin; i <= freqMax; i += freqInterval)
            {
                fx = generateNoiseAtHz(realVolume, i, fx, startTime, endTime);
            }

            return fx;
        }

        private float[] generateNoiseAtHz (double volume, double freq, float[] fx, int startTime, int endTime)
        {
            // get number of samples that a pitch lasts
            double pitchTime = AUDIO_SAMPLE_HZ / freq;

            // Then divide this number by 2 pi because sine formula
            // lasts that long
            double sinePitchTime = pitchTime / (2.0 * Math.PI);

            // Don't generate every frequency at the same point
            // Gotta pick random ones.
            double randomOffset = rng.NextDouble() * sinePitchTime;

            /* Noise is generated with a sine wave formula
             * I think this is the proper way to do it
             * but I'm not 100% sure. Change to a different kind
             * if sine doesn't work?
             */

            try
            {
                for (int i = startTime; i < endTime; i++)
                {
                    fx[i] += (float)(volume * (Math.Sin(((double) i + randomOffset) / sinePitchTime)));
                }
            }
            catch (Exception e)
            {
                log("Unable to generate noise from time " + startTime + " to " + endTime + " probably because out of bounds " + e);
            }
            return fx;
        }
        
        // Reduces all sounds to range from -1 to 1
        // without clipping.
        private float[] normalizeVolume (float[] fx)
        {
            float maxVolume = 0.0f;

            foreach (float t in fx)
            {
                if (t < 0.0f)
                {
                    if (-maxVolume > t)
                    {
                        maxVolume = -t;
                    }
                } else
                {
                    if (maxVolume < t)
                    {
                        maxVolume = t;
                    }
                }
            }
            if (maxVolume >= 1.0)
            {
                for (int i = 0; i < fx.Length; i++)
                {
                    fx[i] /= maxVolume;
                }
                log("Normalized volume by reducing it by a factor of " + maxVolume);
                log("This means <" + Math.Log10(maxVolume) / Math.Log10(2.0) + " bits of entropy out of 16 lost");
            } else
            {
                log("No volume normalization needed, max volume is only: " + maxVolume);
            }
            return fx;
        }

        // clips all sounds
        private float[] audioClipping (float maxVol, float[] fx)
        {
            if (maxVol > 1.0f)
            {
                maxVol = 1.0f;
            } else if (maxVol < 0.0f)
            {
                maxVol = 0.0f;
            }
            for (int i = 0; i < fx.Length; i++)
            {
                if (fx[i] > maxVol)
                {
                    fx[i] = maxVol;
                } else if (fx[i] < -maxVol)
                {
                    fx[i] = -maxVol;
                }
            }
            return fx;
        }

        

        public void placeTextureSomewhere()
        {

            plane = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));            
            plane.SetActive(true);
            Sprite renderSprite = Sprite.Create(fireBalls[currentImg], new Rect(0, 0, FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT), new Vector2(0, 0));
            canvas = CanvasUtil.CreateImagePanel(plane, renderSprite, new CanvasUtil.RectData(new Vector2(1920, 1080), new Vector2(0f, 0f)));
            
        }

        public Mesh createMesh(float width, float height)
        {
            Mesh m = new Mesh
            {
                name = "ScriptedMesh",
                vertices = new Vector3[]
                {
                    new Vector3(-width, -height, 0.01f),
                    new Vector3(width, -height, 0.01f),
                    new Vector3(width, height, 0.01f),
                    new Vector3(-width, height, 0.01f)
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)
                },
                triangles = new int[] {0, 1, 2, 0, 2, 3}
            };
            m.RecalculateNormals();
            return m;
        }

        private void generateFlameTextures()
        {

            try
            {
                for (int i = 0; i < fireBalls.Length; i++)
                {
                    fireBalls[i] = generateFireball();
                    fireBalls[i].Apply();
                }
            } catch (Exception e)
            {
                log("Unable to build fireballs. Error " + e);
            }

            try
            {
                for (int i = 0; i < fireTrails.Length; i++)
                {
                    fireTrails[i] = generateFireTrail();
                    fireTrails[i].Apply();
                }
            } catch (Exception e)
            {
                log("Unable to build fire trails. Error " + e);
            }

            try
            {
                for (int i = 0; i < firePillars.Length; i++)
                {
                    firePillars[i] = generateFirePillar();
                    firePillars[i].Apply();
                }
            } catch (Exception e)
            {
                log("Unable to build fire pillars. Error " + e);
            }

            try
            {
                for (int i = 0; i < fireSpikes.Length; i++)
                {
                    fireSpikes[i] = generateFireSpike();
                    fireSpikes[i].Apply();
                }
            }
            catch (Exception e)
            {
                log("Unable to build fire spikes. Error " + e);
            }
            
            try
            {
                for (int i = 0; i < fireballMagmas.Length; i++)
                {
                    fireballMagmas[i] = generateFireballMagma(i);
                    fireballMagmas[i].Apply();
                }
            }
            catch (Exception e)
            {
                log("Unable to build fireball magmas. Error " + e);
            }
            
            try
            {
                for (int i = 0; i < fireballMagmaBalls.Length; i++)
                {
                    fireballMagmaBalls[i] = generateFireballMagmaFireball();
                    fireballMagmaBalls[i].Apply();
                }
            }
            catch (Exception e)
            {
                log("Unable to build fireball magmas. Error " + e);
            }
            
            try
            {
                for (int i = 0; i < fireShields.Length; i++)
                {
                    fireShields[i] = generateFireShield();
                    fireShields[i].Apply();
                }
            }
            catch (Exception e)
            {
                log("Unable to build fire shields. Error " + e);
            }
            

            log("Built all flame textures.");

            
        }
        
        
        private Texture2D generateFireballMagma(int index)
        {
            Texture2D fbm = new Texture2D(FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT);
            double[] vertIntensity150 = new double[FBTEXTURE_WIDTH];
            double[] vertOpacity150 = new double[FBTEXTURE_WIDTH];
            
            // RNG phase
            for (int i = 0; i < FBTEXTURE_WIDTH; i++)
            {
                if (i % FB_MAGMA_INTERPOLATE_PIXELS != 0) continue;

                vertIntensity150[i] = rng.NextDouble();
                vertOpacity150[i] = rng.NextDouble();
                
                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map verticalIntensity150 -> 0-0.2
                // and verticalOpacity150 -> -1 - -0.7
                vertOpacity150[i] = (vertOpacity150[i] * 0.3) - 1.0f;
                vertIntensity150[i] = (vertIntensity150[i] * 0.2) - 1.5f * index;
            }
            
            // Interpolation phase
            
            for (int i = 0; i < FBTEXTURE_WIDTH - FB_MAGMA_INTERPOLATE_PIXELS; i++)
            {
                if (i % FB_MAGMA_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FB_MAGMA_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FB_MAGMA_INTERPOLATE_PIXELS;

                vertIntensity150[i] = vertIntensity150[i - offset + FB_MAGMA_INTERPOLATE_PIXELS] * avgWeighting + vertIntensity150[i - offset] * (1.0 - avgWeighting);
                vertOpacity150[i] = vertOpacity150[i - offset + FB_MAGMA_INTERPOLATE_PIXELS] * avgWeighting + vertOpacity150[i - offset] * (1.0 - avgWeighting);
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = FBTEXTURE_WIDTH - FB_MAGMA_INTERPOLATE_PIXELS; i < FBTEXTURE_WIDTH; i++)
            {
                if (i % FB_MAGMA_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FB_MAGMA_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FB_MAGMA_INTERPOLATE_PIXELS;

                vertIntensity150[i] = vertIntensity150[0] * avgWeighting + vertIntensity150[i - offset] * (1.0 - avgWeighting);
                vertOpacity150[i] = vertOpacity150[0] * avgWeighting + vertOpacity150[i - offset] * (1.0 - avgWeighting);
            }
            
            // Actually set the colors
            for (int x = 0; x < FBTEXTURE_WIDTH; x++)
            {
                int xDistance = x - FBTEXTURE_WIDTH / 2;
                if (xDistance < 0)
                {
                    xDistance = -xDistance;
                }

                xDistance = (int)(xDistance * (2.5 - (( 2.0 * index) / 12.0)));
                
                for (int y = 0; y < FBTEXTURE_HEIGHT; y++)
                {
                    int netDistance = (int) Math.Sqrt((xDistance * xDistance) + (y * y));
                    
                    fbm.SetPixel(x, y, getFireColor
                        ((netDistance), vertIntensity150[x], vertOpacity150[x], FBTEXTURE_WIDTH,
                        (FBTEXTURE_WIDTH - (int) (FBTEXTURE_WIDTH * index * 0.0834)), 9.0));
                }
            }


            return fbm;
        }

        private Texture2D generateFireSpike()
        {
            Texture2D fs = new Texture2D(FSTEXTURE_WIDTH, FSTEXTURE_HEIGHT);
            double[] horzIntensity20 = new double[FSTEXTURE_HEIGHT];
            double[] horzOpacity20 = new double[FSTEXTURE_HEIGHT];


            // RNG phase
            for (int i = 0; i < FSTEXTURE_HEIGHT; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS != 0) continue;
                horzIntensity20[i] = rng.NextDouble();

                horzOpacity20[i] = 0.5f;
                //horzOpacity20[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map verticalIntensity150 -> 0-0.2
                // and verticalOpacity150 -> 0.5
                //horzOpacity20[i] = horzOpacity20[i] * 0.2 - 0.6;

                horzIntensity20[i] = (horzIntensity20[i] * 0.2);
            }

            // Interpolation phase
            for (int i = 0; i < FSTEXTURE_HEIGHT - FS_INTERPOLATE_PIXELS; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FS_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FS_INTERPOLATE_PIXELS;

                horzIntensity20[i] = horzIntensity20[i - offset + FS_INTERPOLATE_PIXELS] * avgWeighting + horzIntensity20[i - offset] * (1.0 - avgWeighting);
                horzOpacity20[i] = horzOpacity20[i - offset + FS_INTERPOLATE_PIXELS] * avgWeighting + horzOpacity20[i - offset] * (1.0 - avgWeighting);
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = FSTEXTURE_HEIGHT - FS_INTERPOLATE_PIXELS; i < FSTEXTURE_HEIGHT; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FS_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FS_INTERPOLATE_PIXELS;

                horzIntensity20[i] = horzIntensity20[0] * avgWeighting + horzIntensity20[i - offset] * (1.0 - avgWeighting);
                horzOpacity20[i] = horzOpacity20[0] * avgWeighting + horzOpacity20[i - offset] * (1.0 - avgWeighting);
            }


            // Actually set the colors
            for (int x = 0; x < FSTEXTURE_WIDTH; x++)
            {
                double realDistance = (FSTEXTURE_WIDTH / 2.0) - 0.5 - x;
                
                for (int y = 0; y < FSTEXTURE_HEIGHT; y++)
                {
                    if (y > 150)
                    {
                        fs.SetPixel(x, y, getFireColor
                        ((realDistance), horzIntensity20[y], horzOpacity20[y], FSTEXTURE_WIDTH / 2,
                            FSTEXTURE_WIDTH / 2.0, 9.0));
                    }
                    else
                    {
                        fs.SetPixel(x, y, getFireColor
                        ((int)( ((double)y / 150.0) * (double)realDistance), horzIntensity20[y], horzOpacity20[y], FSTEXTURE_WIDTH / 2,
                            FSTEXTURE_WIDTH / 2.0, 9.0));
                    }
                    
                }
            }

            return fs;
        }

        private Texture2D generateFirePillar()
        {
            Texture2D fp = new Texture2D(FPTEXTURE_WIDTH, FPTEXTURE_HEIGHT);
            double[] horzIntensity150 = new double[FPTEXTURE_HEIGHT];
            double[] horzOpacity150 = new double[FPTEXTURE_HEIGHT];


            // RNG phase
            for (int i = 0; i < FPTEXTURE_HEIGHT; i++)
            {
                if (i % FP_INTERPOLATE_PIXELS != 0) continue;
                horzIntensity150[i] = rng.NextDouble();
                horzOpacity150[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map verticalIntensity150 -> 0-0.2
                // and verticalOpacity150 -> -1 - 0
                horzOpacity150[i] = horzOpacity150[i] * 0.2 - 0.6;

                horzIntensity150[i] = (horzIntensity150[i] * 0.2);
            }

            // Interpolation phase
            for (int i = 0; i < FPTEXTURE_HEIGHT - FP_INTERPOLATE_PIXELS; i++)
            {
                if (i % FP_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FP_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FP_INTERPOLATE_PIXELS;

                horzIntensity150[i] = horzIntensity150[i - offset + FP_INTERPOLATE_PIXELS] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[i - offset + FP_INTERPOLATE_PIXELS] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = FPTEXTURE_HEIGHT - FP_INTERPOLATE_PIXELS; i < FPTEXTURE_HEIGHT; i++)
            {
                if (i % FP_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FP_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FP_INTERPOLATE_PIXELS;

                horzIntensity150[i] = horzIntensity150[0] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[0] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }


            // Actually set the colors
            for (int x = 0; x < FPTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FPTEXTURE_HEIGHT; y++)
                {
                    fp.SetPixel(x, y, getFireColor
                        ((x - FPTEXTURE_WIDTH / 2), horzIntensity150[y], horzOpacity150[y],
                        FPTEXTURE_WIDTH / 2, FPTEXTURE_WIDTH / 2.0, 9.0));
                }
            }

            return fp;
        }

        private Texture2D generateFireTrail()
        {
            Texture2D ft = new Texture2D(FTTEXTURE_WIDTH, FTTEXTURE_HEIGHT);
            double[] verticalIntensity150 = new double[FTTEXTURE_WIDTH];
            double[] verticalOpacity150 = new double[FTTEXTURE_WIDTH];

            // RNG phase
            for (int i = 0; i < FTTEXTURE_WIDTH; i++)
            {
                if (i % FT_INTERPOLATE_PIXELS != 0) continue;
                
                verticalIntensity150[i] = rng.NextDouble();
                verticalOpacity150[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map verticalIntensity150 -> 0 - 0.2
                // and verticalOpacity150 -> -1 - 0
                verticalOpacity150[i] = verticalOpacity150[i] * 0.2 - 0.6;
                verticalIntensity150[i] = (verticalIntensity150[i] * 0.2);
            }

            // Interpolation phase
            for (int i = 0; i < FTTEXTURE_WIDTH - FT_INTERPOLATE_PIXELS; i++)
            {
                if (i % FT_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FT_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FT_INTERPOLATE_PIXELS;

                verticalIntensity150[i] = verticalIntensity150[i - offset + FT_INTERPOLATE_PIXELS] * avgWeighting + verticalIntensity150[i - offset] * (1.0 - avgWeighting);
                verticalOpacity150[i] = verticalOpacity150[i - offset + FT_INTERPOLATE_PIXELS] * avgWeighting + verticalOpacity150[i - offset] * (1.0 - avgWeighting);
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = FTTEXTURE_WIDTH - FT_INTERPOLATE_PIXELS; i < FTTEXTURE_WIDTH; i++)
            {
                if (i % FT_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FT_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FT_INTERPOLATE_PIXELS;

                verticalIntensity150[i] = verticalIntensity150[0] * avgWeighting + verticalIntensity150[i - offset] * (1.0 - avgWeighting);
                verticalOpacity150[i] = verticalOpacity150[0] * avgWeighting + verticalOpacity150[i - offset] * (1.0 - avgWeighting);
            }


            // Actually set the colors
            for (int x = 0; x < FTTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FTTEXTURE_HEIGHT; y++)
                {
                    ft.SetPixel(x, y, getFireColor
                        ( (y - FTTEXTURE_HEIGHT / 2), verticalIntensity150[x], verticalOpacity150[x],
                        FTTEXTURE_HEIGHT / 2, FTTEXTURE_HEIGHT / 2.0, 9.0));
                }
            }
            return ft;
        }
        
        private Texture2D generateFireballMagmaFireball()
        {
            Texture2D fb = new Texture2D(FBMBTEXTURE_WIDTH, FBMBTEXTURE_HEIGHT);
            double[] radialIntensity400 = new double[360];
            double[] radialOpacity400 = new double[360];

            // Generation
            for (int i = 0; i < 360 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES != 0) continue;
                radialIntensity400[i] = rng.NextDouble();
                radialOpacity400[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map radialIntensity400 -> 0 - 0.2
                // and radialOpacity400 -> -0.25 - -0.1
                radialIntensity400[i] = (radialIntensity400[i] * 0.2);
                radialOpacity400[i] = (radialOpacity400[i] * 1.15) - 0.25;
            }
            // Interpolation (to avoid really crazy looking balls)
            for (int i = 0; i < 360 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES == 0) continue;
                int offset = i % INTERPOLATE_DEGREES;
                double avgWeighting = (double)offset / (double)INTERPOLATE_DEGREES;

                radialIntensity400[i] = radialIntensity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialIntensity400[i - offset] * (1.0 - avgWeighting);
                radialOpacity400[i] = radialOpacity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialOpacity400[i - offset] * (1.0 - avgWeighting);
            }
            // Interpolation (but it wraps around)
            for (int i = 360 - INTERPOLATE_DEGREES; i < 360; i++)
            {
                int offset = i % INTERPOLATE_DEGREES;
                double avgWeighting = (double)offset / (double)INTERPOLATE_DEGREES;

                radialIntensity400[i] = radialIntensity400[0] * avgWeighting + radialIntensity400[i - offset] * (1.0 - avgWeighting);
                radialOpacity400[i] = radialOpacity400[0] * avgWeighting + radialOpacity400[i - offset] * (1.0 - avgWeighting);

            }
            // Actually set the colors
            for (int x = 0; x < FBMBTEXTURE_HEIGHT; x++)
            {
                for (int y = 0; y < FBMBTEXTURE_WIDTH; y++)
                {
                    int angel = getNearestAngel(x, y, FBMBTEXTURE_WIDTH, FBMBTEXTURE_HEIGHT);
                    fb.SetPixel(x, y, getFireColor
                        (getDistance(x, y, FBMBTEXTURE_WIDTH, FBMBTEXTURE_HEIGHT), radialIntensity400[angel],
                        radialOpacity400[angel], FBMBTEXTURE_WIDTH/2, FBMBTEXTURE_HEIGHT/2.0, 9.0));
                }
            }
            return fb;
        }

        private Texture2D generateFireball()
        {
            Texture2D fb = new Texture2D(FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT);
            double[] radialIntensity400 = new double[360];
            double[] radialOpacity400 = new double[360];

            // Generation
            for (int i = 0; i < 361 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES != 0) continue;
                radialIntensity400[i] = rng.NextDouble();
                radialOpacity400[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map radialIntensity400 -> 0 - 0.2
                // and radialOpacity400 -> -0.25 - -0.1
                radialIntensity400[i] = (radialIntensity400[i] * 0.2);
                radialOpacity400[i] = (radialOpacity400[i] * 0.15) - 0.25;
            }
            // Interpolation (to avoid really crazy looking balls)
            for (int i = 0; i < 360 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES == 0) continue;
                int offset = i % INTERPOLATE_DEGREES;
                double avgWeighting = (double)offset / (double)INTERPOLATE_DEGREES;

                radialIntensity400[i] = radialIntensity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialIntensity400[i - offset] * (1.0 - avgWeighting);
                radialOpacity400[i] = radialOpacity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialOpacity400[i - offset] * (1.0 - avgWeighting);
            }
            // Interpolation (but it wraps around)
            for (int i = 360 - INTERPOLATE_DEGREES; i < 360; i++)
            {
                int offset = i % INTERPOLATE_DEGREES;
                double avgWeighting = (double)offset / (double)INTERPOLATE_DEGREES;

                radialIntensity400[i] = radialIntensity400[0] * avgWeighting + radialIntensity400[i - offset] * (1.0 - avgWeighting);
                radialOpacity400[i] = radialOpacity400[0] * avgWeighting + radialOpacity400[i - offset] * (1.0 - avgWeighting);

            }
            // Actually set the colors
            for (int x = 0; x < FBTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FBTEXTURE_HEIGHT; y++)
                {
                    int angel = getNearestAngel(x, y, FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT);
                    fb.SetPixel(x, y, getFireColor
                        (getDistance(x, y, FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT), radialIntensity400[angel],
                        radialOpacity400[angel], FBTEXTURE_HEIGHT/2, FBTEXTURE_HEIGHT/2.0, 9.0));
                }
            }
            return fb;
        }

        private Texture2D generateFireShield()
        {
            Texture2D fShield = new Texture2D(FSHIELDTEXTURE_WIDTH, FSHIELDTEXTURE_HEIGHT);
            double[] radialIntensityEdge = new double[360];
            double[] radialOpacityEdge = new double[360];
            
            const double ringSize = 50.0;
            const double ringStartAt = ( (FSHIELDTEXTURE_WIDTH +FSHIELDTEXTURE_HEIGHT) / 4.0) - ringSize;
            
            
            // Generation
            for (int i = 0; i < 361 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES != 0) continue;
                radialIntensityEdge[i] = rng.NextDouble();
                radialOpacityEdge[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map radialIntensity400 -> 0 - 0.2
                // and radialOpacity400 -> -0.25 - -0.1
                radialIntensityEdge[i] = (radialIntensityEdge[i] * 0.3) - 4.5;
                radialOpacityEdge[i] = (radialOpacityEdge[i] * 0.25) - 0.75;
            }
            // Interpolation (to avoid really crazy looking shields)
            for (int i = 0; i < 360 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES == 0) continue;
                int offset = i % INTERPOLATE_DEGREES;
                double avgWeighting = (double)offset / (double)INTERPOLATE_DEGREES;

                radialIntensityEdge[i] = radialIntensityEdge[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialIntensityEdge[i - offset] * (1.0 - avgWeighting);
                radialOpacityEdge[i] = radialOpacityEdge[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialOpacityEdge[i - offset] * (1.0 - avgWeighting);
            }
            // Interpolation (but it wraps around)
            for (int i = 360 - INTERPOLATE_DEGREES; i < 360; i++)
            {
                int offset = i % INTERPOLATE_DEGREES;
                double avgWeighting = (double)offset / (double)INTERPOLATE_DEGREES;

                radialIntensityEdge[i] = radialIntensityEdge[0] * avgWeighting + radialIntensityEdge[i - offset] * (1.0 - avgWeighting);
                radialOpacityEdge[i] = radialOpacityEdge[0] * avgWeighting + radialOpacityEdge[i - offset] * (1.0 - avgWeighting);

            }
            // Actually set the colors
            for (int x = 0; x < FSHIELDTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FSHIELDTEXTURE_HEIGHT; y++)
                {
                    double dist = ovalDistance(x, y, FSHIELDTEXTURE_WIDTH, FSHIELDTEXTURE_HEIGHT);
                    if (dist < ringStartAt)
                    {
                        fShield.SetPixel(x, y, Color.clear);
                    }
                    else
                    {

                        int angel = getNearestAngel(x, y, FSHIELDTEXTURE_WIDTH, FSHIELDTEXTURE_HEIGHT);
                        fShield.SetPixel(x, y, getFireColor
                        (dist - ringStartAt, radialIntensityEdge[angel],
                            radialOpacityEdge[angel], (int)ringSize, (int)ringSize, 1.5));
                    }
                }
            }
            return fShield;
        }

        private Color getFireColor(double distance, double intensity400, double opacity400, int intensInterpolateDist, double opacInterpDist, double opacSharpness)
        {
            double intensity = getRealIntensity(distance, intensity400, intensInterpolateDist);
            // ReSharper disable once UseObjectOrCollectionInitializer because it looks dumb
            Color c = new Color();

            c.a = (float)getRealOpacity(distance, opacity400, opacInterpDist, opacSharpness);

            if (intensity < flameIntensityThresholds[1])
            {
                double intensitySeparation = flameIntensityThresholds[1] - flameIntensityThresholds[0];
                double intens1 = (intensity - flameIntensityThresholds[0]) / intensitySeparation;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[0].r * (float)intens2) + (flameIntensityCurve[1].r * (float)intens1);
                c.g = (flameIntensityCurve[0].g * (float)intens2) + (flameIntensityCurve[1].g * (float)intens1);
                c.b = (flameIntensityCurve[0].b * (float)intens2) + (flameIntensityCurve[1].b * (float)intens1);
            } else if (intensity < flameIntensityThresholds[2])
            {
                double intensitySeparation = flameIntensityThresholds[2] - flameIntensityThresholds[1];
                double intens1 = (intensity - flameIntensityThresholds[1]) / intensitySeparation;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[1].r * (float)intens2) + (flameIntensityCurve[2].r * (float)intens1);
                c.g = (flameIntensityCurve[1].g * (float)intens2) + (flameIntensityCurve[2].g * (float)intens1);
                c.b = (flameIntensityCurve[1].b * (float)intens2) + (flameIntensityCurve[2].b * (float)intens1);
            } else
            {
                double intensitySeparation = flameIntensityThresholds[3] - flameIntensityThresholds[2];
                double intens1 = (intensity - flameIntensityThresholds[2]) / intensitySeparation;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[2].r * (float)intens2) + (flameIntensityCurve[3].r * (float)intens1);
                c.g = (flameIntensityCurve[2].g * (float)intens2) + (flameIntensityCurve[3].g * (float)intens1);
                c.b = (flameIntensityCurve[2].b * (float)intens2) + (flameIntensityCurve[3].b * (float)intens1);
            }
            return c;
        }

        private double getRealOpacity(double distance, double opacity400, double interpolateDistance, double opacSharpness)
        {
            if (distance < 0.0)
            {
                distance = -distance;
            }
            double averageWeighting = distance / interpolateDistance;
            double opactReal = opacity400 * averageWeighting + ((1.0 - averageWeighting) * opacSharpness);
            if (opactReal < 0.0)
            {
                opactReal = 0.0;
            } else if (opactReal >= 1.0)
            {
                opactReal = 1.0;
            }

            opactReal *= OPACITY_MASK;

            return opactReal;
        }

        private static double getRealIntensity(double distance, double intensity400, int interpolateDistance)
        {
            if (distance < 0.0)
            {
                distance = -distance;
            }
            double averageWeighting = distance / (double)interpolateDistance;
            double intenReal = intensity400 * averageWeighting + ((1.0 - averageWeighting) * 3.4);
            if (intenReal < 0.0)
            {
                intenReal = 0.0;
            } else if (intenReal >= 3.0)
            {
                intenReal = 3.0;
            }
            return intenReal;
        }

        private double ovalDistance(int x, int y, int width, int height)
        {
            int relX = x - (width / 2 );
            int relY = y - (height / 2);
            double widthToHeightRatio = (double)width / (double)height;
            
            

            return Math.Sqrt( ((double) (relX * relX) / widthToHeightRatio) +
                              ((double) (relY * relY) * widthToHeightRatio) );
        }

        private double getDistance(int x, int y, int width, int height)
        {
            int relX = x - (width / 2 );
            int relY = y - (height / 2);

            return Math.Sqrt( (double) (relX * relX) + (double) (relY * relY) );
        }


        private int getNearestAngel(int x, int y, int width, int height)
        {
            int angel;

            int relX = x - (width / 2);
            int relY = y - (height / 2);

            if (relX == 0)
            {
                return relY >= 0 ? 90 : 270;
            } else
            {
                angel = (int) ( Math.Atan2(relY, relX) * 180.0 / Math.PI);
            }
            while (angel < 0)
            {
                angel += 360;
            }
            angel %= 360;

            return angel;
        }

        private static void log([NotNull] string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}
