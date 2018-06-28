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
        
        
        
        private readonly Texture2D[] firePillars = new Texture2D[10];
        private readonly Texture2D[] fireSpikes = new Texture2D[16];
        private readonly Texture2D[] fireShields = new Texture2D[20];

        private readonly Texture2D[] fireTrails = new Texture2D[4];
        
        
        private const int TESTING_CLIP = 2;

        private AudioClip fireballHitSoundFX;
        private AudioClip laserCastSoundFX;
        private AudioClip shieldWavySoundFX;
        private AudioClip fireTrailSoundFX;
        private AudioClip sizzleSoundFX;
        
        private AudioClip shieldRechargeSoundFX;
        
        // in the event I ever add a shield with multiple hit points both of these will be useful.
        // for now just shield discharge is used.
        private AudioClip shieldHitSoundFX;
        private AudioClip shieldDischargeSoundFX;
        

        // What could this be for?
        private AudioClip sirenSoundFx;
        
        
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

        public const int FTTEXTURE_WIDTH = 1800;
        public const int FTTEXTURE_HEIGHT = 150;
        
        // the caps are as tall as the regular fire trails.
        public const int FTCAPTEXTURE_WIDTH = 150;

        public const int FPTEXTURE_WIDTH = 150;
        public const int FPTEXTURE_HEIGHT = 1080;

        public const int LASERTEXTURE_WIDTH = 80;
        public const int LASERTEXTURE_HEIGHT = 250;
        
        public const int FBMBTEXTURE_WIDTH = 85;
        public const int FBMBTEXTURE_HEIGHT = 85;

        public const int FSHIELDTEXTURE_WIDTH = 250;
        public const int FSHIELDTEXTURE_HEIGHT = 320;

        private const double OPACITY_MASK = 1.0;

        public int sceneTimer;
        public int currentImg;

        
        

        // pick a factor of 360
        private const int INTERPOLATE_DEGREES = 18;

        // pick a factor of FTTEXTURE_WIDTH
        private const int FT_INTERPOLATE_PIXELS = 60;

        // pick a factor of FPTEXTURE_HEIGHT
        private const int FP_INTERPOLATE_PIXELS = 40;

        // pick a factor of LASERTEXTURE_HEIGHT
        private const int FS_INTERPOLATE_PIXELS = 50;

        private const int FB_MAGMA_INTERPOLATE_PIXELS = 15;
        
        // pick a factor of FTTEXTURE_HEIGHT
        private const int FT_CAP_INTERPOLATE_PIXELS = 15;

        
        
        //public readonly Color[] flameIntensityCurve = { Color.red, new Color(1f, 0.3f, 0f), Color.yellow, Color.white };

        // What are the fire colors anyway?
        private readonly Color[] flameIntensityCurve = { Color.red, Color.yellow, Color.white, Color.white };

        // At what point do you switch from color X to color Y.
        private readonly double[] flameIntensityThresholds = { 0.4, 0.7, 2.5, 2.6 };

        private double randomPointOnSin;


        

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
            redwing_game_objects.fireballMagmas = fireballMagmas;
            redwing_game_objects.fireballMagmaFireballs = fireballMagmaBalls;

            redwing_pillar_detect_behavior.pillarTextures = firePillars;

            redwing_hooks.flameShieldTextures = fireShields;
            redwing_hooks.fireTrailTextures = fireTrails;

            redwing_fireball_behavior.fireballImpact = fireballHitSoundFX;
            redwing_fireball_behavior.fireballSizzle = sizzleSoundFX;
            redwing_laser_spawner_behavior.laserFX = laserCastSoundFX;
            redwing_hooks.shieldSoundEffect = shieldWavySoundFX;
            redwing_hooks.fireTrailSoundEffect = fireTrailSoundFX;
            redwing_hooks.shieldChargeSoundEffect = shieldRechargeSoundFX;
            redwing_hooks.shieldDischargeSoundEffect = shieldDischargeSoundFX;
            //redwing_game_objects.soundFxClip = soundFxClip;

        }

        


        private void generateSoundEffects()
        {
            randomPointOnSin = rng.NextDouble() * 2.0 * Math.PI;
            
            fireballHitSoundFX = AudioClip.Create("fireball1", (int) (AUDIO_SAMPLE_HZ * 1.0) + 30000, 1, AUDIO_SAMPLE_HZ, false);
            fireballHitSoundFX.SetData(generateFbSound((int)(AUDIO_SAMPLE_HZ * 1.0) + 30000), 0);

            laserCastSoundFX = AudioClip.Create("laser1", AUDIO_SAMPLE_HZ * 1, 1, AUDIO_SAMPLE_HZ, false);
            laserCastSoundFX.SetData(generateLaserSound(AUDIO_SAMPLE_HZ * 1), 0);

            shieldWavySoundFX = AudioClip.Create("shieldwavy", AUDIO_SAMPLE_HZ * 3, 1, AUDIO_SAMPLE_HZ, false);
            shieldWavySoundFX.SetData(generateShieldSound(AUDIO_SAMPLE_HZ * 3), 0);
            
            fireTrailSoundFX = AudioClip.Create("firetrail", AUDIO_SAMPLE_HZ * 2, 1, AUDIO_SAMPLE_HZ, false);
            fireTrailSoundFX.SetData(generateFireTrailSound(AUDIO_SAMPLE_HZ * 2), 0);
            
            sizzleSoundFX = AudioClip.Create("sizzle", (int) (AUDIO_SAMPLE_HZ * 1.5), 1, AUDIO_SAMPLE_HZ, false);
            sizzleSoundFX.SetData(generateSizzleSound((int) (AUDIO_SAMPLE_HZ * 1.5)), 0);
            
            shieldRechargeSoundFX = AudioClip.Create("shieldRecharge", (int) (AUDIO_SAMPLE_HZ * 0.7), 1, AUDIO_SAMPLE_HZ, false);
            shieldRechargeSoundFX.SetData(generateShieldChargeSound((int) (AUDIO_SAMPLE_HZ * 0.7)), 0);
            
            shieldDischargeSoundFX = AudioClip.Create("shieldRecharge", (int) (AUDIO_SAMPLE_HZ * 0.4), 1, AUDIO_SAMPLE_HZ, false);
            shieldDischargeSoundFX.SetData(generateShieldDischargeSound((int) (AUDIO_SAMPLE_HZ * 0.4)), 0);
            
        }
        
        private float[] generateShieldDischargeSound(int length)
        {
            float[] fx = new float[length];
            fx = generateChirp(1.0, 600.0, 200.0, 0, fx.Length, fx);
            
            // Tired of shitty sounding sound effects? Layer some white noise over it and nobody will notice.
            fx = generateWhiteNoise(0.4, 0, fx.Length, fx);
            //fx = generateNoiseAtHz(1.0, 100.0, fx, fx.Length / 2, fx.Length);
            fx = fadeAudio(0.0, fx.Length / 2, fx.Length - 4000, fx);
            fx = normalizeVolume(fx);
            
            return fx;
        }

        private float[] generateShieldChargeSound(int length)
        {
            float[] fx = new float[length];
            fx = generateChirp(1.0, 200.0, 400.0, 0, fx.Length, fx);
            //fx = generateNoiseAtHz(1.0, 400.0, fx, fx.Length / 2, fx.Length);
            fx = fadeAudio(0.0, fx.Length / 2, fx.Length - 8000, fx);
            fx = normalizeVolume(fx);
            
            return fx;
        }

        private float[] generateSizzleSound(int length)
        {
            float[] fx = new float[length];
            
            fx = generateWhiteNoise(0.2, 0, fx.Length, fx);
            
            fx = speedUpEffect(1.5, fx);
            fx = fadeAudio(2.0, 0, AUDIO_SAMPLE_HZ / 4, fx);
            fx = fadeAudio(0.0, AUDIO_SAMPLE_HZ / 2, length - AUDIO_SAMPLE_HZ / 2, fx);
            
            return fx;
        }

        private float[] generateFireTrailSound(int length)
        {
            float[] fx = new float[length];

            fx = generateWhiteNoise(0.2, 0, fx.Length, fx);
            fx = slowDownEffect(0.6666, fx);
            
            fx = fadeAudio(2.0, 0, AUDIO_SAMPLE_HZ / 4, fx);
            fx = fadeAudio(0.0, AUDIO_SAMPLE_HZ / 2, length - AUDIO_SAMPLE_HZ, fx);
            
            return fx;
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
                fx = generateWhiteNoise(4.0, 0, fx.Length, fx);
                fx = fadeAudio(2.0, 0, fx.Length / 4, fx);
                fx = lowPassFilter(800, fx);
                fx = lowPassFilter(800, fx);
                fx = highPassFilter(600, fx);
                fx = highPassFilter(600, fx);

                fx = slowDownEffect(0.666, fx);
                
                //fx = generateWhiteNoise(0.1, 0, fx.Length, fx);
                
                fx = fadeAudio(0.0, fx.Length/4 , fx.Length, fx);
                
                fx = normalizeVolume(fx);
                log("Made laser sound without error.");

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


        // how does this work.
        private float[] lowPassFilter(double frequency, float[] fx)
        {
            filter_butterworth a =
                new filter_butterworth((float)frequency, AUDIO_SAMPLE_HZ, filter_butterworth.pass_type.Lowpass, 1.0f);
            for (int i = 0; i < fx.Length; i++)
            {
                a.update(fx[i]);
                fx[i] = a.value;
            }

            return fx;
        }
        
        // this is really stupid and can't possibly work x2
        private float[] highPassFilter(double frequency, float[] fx)
        {

            filter_butterworth a =
                new filter_butterworth((float)frequency, AUDIO_SAMPLE_HZ, filter_butterworth.pass_type.Highpass, 1.0f);
            for (int i = 0; i < fx.Length; i++)
            {
                a.update(fx[i]);
                fx[i] = a.value;
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


        private float[] generateChirp(double volume, double startHz, double endHz, int startTime, int endTime,
            float[] fx)
        {
            double modulateTime = (endTime - startTime) / ((double)AUDIO_SAMPLE_HZ * 0.5);
            
            // ReSharper disable once ConvertIfStatementToReturnStatement Yes it looks sexy but really it's too much
            // for a one liner.
            if (startHz <= endHz)
            {
                return generateSirenSound(volume, startHz, endHz, startTime, endTime, modulateTime, 0, fx);
            }
            else
            {
                return generateSirenSound(volume, endHz, startHz, startTime, endTime, modulateTime, 0.5, fx);
            }
        }

        private float[] generateSirenSound(double volume, double minHz, double maxHz, int startTime, int maxTime, double modulateTime,
            float[] fx)
        {
            return generateSirenSound(volume, minHz, maxHz, startTime, maxTime, modulateTime, rng.NextDouble(), fx);
        }

        private float[] generateSirenSound(double volume, double minHz, double maxHz, int startTime, int maxTime, double modulateTime,
            double startingPoint, float[] fx)
        {
            
            
            // Basically because the difference between 100 and 200 Hz should be
            // the same as the difference between 1,000 and 2,000 Hz. This
            // is because of how ears work, basically.
            double minHzForComparison = Math.Log(minHz, 2.0);
            double maxHzForComparison = Math.Log(maxHz, 2.0);
            double modulateInterval = (modulateTime * AUDIO_SAMPLE_HZ);

            

            log("min freq is " + minHz);
            log("max freq is " + maxHz);
            
            for (int i = startTime; i < maxTime; i++)
            {
                double frequencyPos;
                if ( ( ((i - startTime) + startingPoint * modulateInterval ) % modulateInterval) > (modulateInterval / 2))
                {
                    frequencyPos =
                        (1.0 - (((i - startTime) + startingPoint * modulateInterval) % (modulateInterval / 2)) /
                         (modulateInterval / 2));
                }
                else
                {
                    frequencyPos = (((i - startTime) + startingPoint * modulateInterval )  % (modulateInterval / 2)) / (modulateInterval / 2);
                }

                if (frequencyPos > 1.0 || frequencyPos < 0.0)
                {
                    log("ERROR: freq out of bounds. Freq is " + frequencyPos);
                    break;
                }
                
                double freqForComparison =
                    ((1.0f - frequencyPos) * minHzForComparison + frequencyPos * maxHzForComparison);
                double actualFreq = Math.Pow(2.0, freqForComparison);
                
                double sinePitchTime = (AUDIO_SAMPLE_HZ / actualFreq) / 2.0;
                
                //if (i % 500 == 0)
                //    log("sine pitch time is " + sinePitchTime + " because the freq is " + actualFreq);
                // Will this work? Who knows.
                fx[i] += (float)(volume * (Math.Sin( randomPointOnSin + ( ((double) i) / sinePitchTime) )));
            }

            //log("did generate sound");
            return fx;
        }
        

        private float[] slowDownEffect(double speed, float[] fx)
        {
            float[] slowedFx = new float[fx.Length];
            int positionRoundedDown = 0;
            
            for (int i = 0; i < fx.Length; i++)
            {
                double realPosition = i * speed;
                positionRoundedDown = (int)realPosition;
                double weighting = realPosition % 1.0;
                if (positionRoundedDown + 1 < fx.Length)
                {
                    slowedFx[i] = (float)(((1.0 - weighting) * fx[positionRoundedDown]) + (weighting * fx[positionRoundedDown + 1]));
                }
                else
                {
                    break;
                }
            }

            return slowedFx;
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
                randomPointOnSin = rng.NextDouble() * 2.0 * Math.PI;
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

            /* Noise is generated with a sine wave formula
             * I think this is the proper way to do it
             * but I'm not 100% sure. Change to a different kind
             * if sine doesn't work?
             */

            try
            {
                for (int i = startTime; i < endTime; i++)
                {
                    fx[i] += (float)(volume * (Math.Sin(((double) i + randomPointOnSin) / sinePitchTime)));
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
            
            Texture2D[] fireTrailMiddles = new Texture2D[4];
            Texture2D[] fireTrailCaps = new Texture2D[4];
            Texture2D[] reverseFireTrailCaps = new Texture2D[4];
            try
            {
                for (int i = 0; i < fireTrails.Length; i++)
                {
                    fireTrailMiddles[i] = generateFireTrail();
                    fireTrailMiddles[i].Apply();
                    
                    fireTrailCaps[i] = generateFireTrailCap();
                    fireTrailCaps[i].Apply();
                    
                    reverseFireTrailCaps[i] = generateReverseFireTrailCap();
                    reverseFireTrailCaps[i].Apply();
                    
                    fireTrails[i] = new Texture2D(
                        fireTrailMiddles[i].width  + fireTrailCaps[i].width  + reverseFireTrailCaps[i].width,
                        fireTrailMiddles[i].height);

                    for (int x = 0; x < reverseFireTrailCaps[i].width; x++)
                    {
                        for (int y = 0; y < reverseFireTrailCaps[i].height; y++)
                        {
                            fireTrails[i].SetPixel(x, y, reverseFireTrailCaps[i].GetPixel(x, y));
                        }
                    }

                    int offset = reverseFireTrailCaps[i].width;
                    
                    for (int x = 0; x < fireTrailMiddles[i].width; x++)
                    {
                        for (int y = 0; y < fireTrailMiddles[i].height; y++)
                        {
                            fireTrails[i].SetPixel(x + offset, y, fireTrailMiddles[i].GetPixel(x, y));
                        }
                    }

                    offset += fireTrailMiddles[i].width;
                    
                    for (int x = 0; x < fireTrailCaps[i].width; x++)
                    {
                        for (int y = 0; y < fireTrailCaps[i].height; y++)
                        {
                            fireTrails[i].SetPixel(x + offset, y, fireTrailCaps[i].GetPixel(x, y));
                        }
                    }
                    
                    
                    
                    fireTrails[i].Apply();
                }
            } catch (Exception e)
            {
                log("Unable to build fire trails. Error " + e);
            }
            
            log("Built all flame textures.");

            
        }

        private Texture2D generateReverseFireTrailCap()
        {
            Texture2D meme = generateFireTrailCap();
            Texture2D newMeme = new Texture2D(FTCAPTEXTURE_WIDTH, FTTEXTURE_HEIGHT);
            
            // Just reverse it manually because computers are fast and who cares.
            for (int x = 0; x < FTCAPTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FTTEXTURE_HEIGHT; y++)
                {
                    newMeme.SetPixel(x, y, meme.GetPixel(FTCAPTEXTURE_WIDTH - 1 - x, y));
                }
            }

            return newMeme;
        }

        private Texture2D generateFireTrailCap()
        {
            Texture2D ftc = new Texture2D(FTCAPTEXTURE_WIDTH, FTTEXTURE_HEIGHT);
            double[] horzIntensity150 = new double[FTTEXTURE_HEIGHT];
            double[] horzOpacity150 = new double[FTTEXTURE_HEIGHT];

            for (int i = 0; i < FTTEXTURE_HEIGHT; i++)
            {
                if (i % FT_CAP_INTERPOLATE_PIXELS != 0) continue;

                horzIntensity150[i] = rng.NextDouble();
                horzOpacity150[i] = rng.NextDouble();
                horzIntensity150[i] = 0.5;
                horzOpacity150[i] = 0.5;
                
                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map horzIntensity150 -> -2- -1
                // and horzOpacity150 -> -1.5 - -1

                horzIntensity150[i] = (horzIntensity150[i]) * 0.2;
                horzOpacity150[i] = (horzOpacity150[i] * 0.2) - 0.6;
            }
            
            // Interpolation
            for (int i = 0; i < FTTEXTURE_HEIGHT - FT_CAP_INTERPOLATE_PIXELS; i++)
            {
                if (i % FT_CAP_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FT_CAP_INTERPOLATE_PIXELS;
                double avgWeighting = (double) offset / (double) FT_CAP_INTERPOLATE_PIXELS;
                
                horzIntensity150[i] = horzIntensity150[i - offset + FT_CAP_INTERPOLATE_PIXELS] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[i - offset + FT_CAP_INTERPOLATE_PIXELS] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }
            
            // Interpolation phase pt 2 (for wrap around)
            for (int i = FTTEXTURE_HEIGHT - FT_CAP_INTERPOLATE_PIXELS; i < FTTEXTURE_HEIGHT; i++)
            {
                if (i % FT_CAP_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FT_CAP_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FT_CAP_INTERPOLATE_PIXELS;

                horzIntensity150[i] = horzIntensity150[0] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[0] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }
            
            
            // Actually set the colors
            for (int x = 0; x < FTCAPTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FTTEXTURE_HEIGHT; y++)
                {
                    double distance = Math.Sqrt( 1.5* (x * x) + 1.0 * ( (y - FTTEXTURE_HEIGHT/ 2) * (y - FTTEXTURE_HEIGHT/ 2)));
                    
                    ftc.SetPixel(x, y, getFireColor
                    (distance, horzIntensity150[y], horzOpacity150[y],
                        FTCAPTEXTURE_WIDTH / 2, FTCAPTEXTURE_WIDTH/2, 3.4, 9.0));
                }
            }
            
            return ftc;
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
                // so apply a transformation to map vertIntensity150 -> 0-0.2
                // and vertOpacity150 -> -1 - -0.7
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
                        (FBTEXTURE_WIDTH - (int) (FBTEXTURE_WIDTH * index * 0.0834)), 3.4, 9.0));
                }
            }


            return fbm;
        }

        private Texture2D generateFireSpike()
        {
            Texture2D fs = new Texture2D(LASERTEXTURE_WIDTH, LASERTEXTURE_HEIGHT);
            double[] horzIntensity20 = new double[LASERTEXTURE_HEIGHT];
            double[] horzOpacity20 = new double[LASERTEXTURE_HEIGHT];


            // RNG phase
            for (int i = 0; i < LASERTEXTURE_HEIGHT; i++)
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
            for (int i = 0; i < LASERTEXTURE_HEIGHT - FS_INTERPOLATE_PIXELS; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FS_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FS_INTERPOLATE_PIXELS;

                horzIntensity20[i] = horzIntensity20[i - offset + FS_INTERPOLATE_PIXELS] * avgWeighting + horzIntensity20[i - offset] * (1.0 - avgWeighting);
                horzOpacity20[i] = horzOpacity20[i - offset + FS_INTERPOLATE_PIXELS] * avgWeighting + horzOpacity20[i - offset] * (1.0 - avgWeighting);
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = LASERTEXTURE_HEIGHT - FS_INTERPOLATE_PIXELS; i < LASERTEXTURE_HEIGHT; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS == 0) continue;
                int offset = i % FS_INTERPOLATE_PIXELS;
                double avgWeighting = (double)offset / (double)FS_INTERPOLATE_PIXELS;

                horzIntensity20[i] = horzIntensity20[0] * avgWeighting + horzIntensity20[i - offset] * (1.0 - avgWeighting);
                horzOpacity20[i] = horzOpacity20[0] * avgWeighting + horzOpacity20[i - offset] * (1.0 - avgWeighting);
            }


            // Actually set the colors
            for (int x = 0; x < LASERTEXTURE_WIDTH; x++)
            {
                double realDistance = (LASERTEXTURE_WIDTH / 2.0) - 0.5 - x;
                
                for (int y = 0; y < LASERTEXTURE_HEIGHT; y++)
                {
                    if (y > 50)
                    {
                        fs.SetPixel(x, y, getFireColor
                        ((realDistance), horzIntensity20[y], horzOpacity20[y], LASERTEXTURE_WIDTH / 2,
                            LASERTEXTURE_WIDTH / 2.0, 3.4, 9.0));
                    }
                    else
                    {
                        fs.SetPixel(x, y, getFireColor
                        ((int)( ((double)y / 50.0) * (double)realDistance), horzIntensity20[y], horzOpacity20[y], LASERTEXTURE_WIDTH / 2,
                            LASERTEXTURE_WIDTH / 2.0, 3.4, 9.0));
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
                horzOpacity150[i] = horzOpacity150[i] * 0.2 - 0.2;

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
                        FPTEXTURE_WIDTH / 2, FPTEXTURE_WIDTH / 2.0, 3.4, 9.0));
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
                
                
                // makes the caps look alright.
                if (i == 0 || i == 0 + FT_INTERPOLATE_PIXELS || i == FTTEXTURE_WIDTH - FT_INTERPOLATE_PIXELS ||
                    i == FTTEXTURE_WIDTH - 2 * FT_INTERPOLATE_PIXELS)
                {
                    //log("Fixing cap because i is " + i);
                    verticalIntensity150[i] = 0.5;
                    verticalOpacity150[i] = 0.5;
                }

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
                        FTTEXTURE_HEIGHT / 2, FTTEXTURE_HEIGHT / 2.0, 3.4, 9.0));
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
                    int angel = (int) getNearestAngel(x, y, FBMBTEXTURE_WIDTH, FBMBTEXTURE_HEIGHT);
                    fb.SetPixel(x, y, getFireColor
                        (getDistance(x, y, FBMBTEXTURE_WIDTH, FBMBTEXTURE_HEIGHT), radialIntensity400[angel],
                        radialOpacity400[angel], FBMBTEXTURE_WIDTH/2, FBMBTEXTURE_HEIGHT/2.0, 3.4, 9.0));
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
                    int angel = (int) getNearestAngel(x, y, FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT);
                    fb.SetPixel(x, y, getFireColor
                        (getDistance(x, y, FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT), radialIntensity400[angel],
                        radialOpacity400[angel], FBTEXTURE_HEIGHT/2, FBTEXTURE_HEIGHT/2.0, 3.4, 9.0));
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
                    if (dist < (ringStartAt - 15.0))
                    {
                        fShield.SetPixel(x, y, Color.clear);
                    } else if (dist < ringStartAt)
                    {
                        int angel = (int) getNearestAngel(x, y, FSHIELDTEXTURE_WIDTH, FSHIELDTEXTURE_HEIGHT);
                        fShield.SetPixel(x, y, getFireColor
                        ( ringStartAt - dist, radialIntensityEdge[angel],
                            0.0, 15, 15.0, 3.4, 1.0));
                    }
                    else
                    {

                        int angel = (int) getNearestAngel(x, y, FSHIELDTEXTURE_WIDTH, FSHIELDTEXTURE_HEIGHT);
                        fShield.SetPixel(x, y, getFireColor
                        (dist - ringStartAt, radialIntensityEdge[angel],
                            radialOpacityEdge[angel], (int)ringSize, (int)ringSize, 3.4, 1.5));
                    }
                }
            }
            return fShield;
        }

        private Color getFireColor(double distance, double intensity400, double opacity400, int intensInterpolateDist,
            double opacInterpDist, double intensWighting, double opacSharpness)
        {
            double intensity = getRealIntensity(distance, intensity400, intensInterpolateDist, intensWighting);
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

        private static double getRealIntensity(double distance, double intensity400, int interpolateDistance,
            double intensityWeighting)
        {
            if (distance < 0.0)
            {
                distance = -distance;
            }
            double averageWeighting = distance / (double)interpolateDistance;
            double intenReal = intensity400 * averageWeighting + ((1.0 - averageWeighting) * intensityWeighting);
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


        public static double getNearestAngel(int x, int y, int width, int height)
        {
            double angel;

            int relX = x - (width / 2);
            int relY = y - (height / 2);

            if (relX == 0)
            {
                return relY >= 0 ? 90.0 : 270.0;
            } else
            {
                angel = ( Math.Atan2(relY, relX) * 180.0 / Math.PI);
            }
            while (angel < 0.0)
            {
                angel += 360.0;
            }
            angel %= 360.0;

            return angel;
        }

        private static void log([NotNull] string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            Modding.Logger.Log("[Redwing] " + str);
        }
    }


    public class filter_butterworth
    {
        /// <summary>
        /// rez amount, from sqrt(2) to ~ 0.1
        /// </summary>
        private readonly float resonance;

        private readonly float frequency;
        private readonly int sampleRate;
        private readonly pass_type passType;

        private readonly float c, a1, a2, a3, b1, b2;

        /// <summary>
        /// Array of input values, latest are in front
        /// </summary>
        private readonly float[] inputHistory = new float[2];

        /// <summary>
        /// Array of output values, latest are in front
        /// </summary>
        private readonly float[] outputHistory = new float[3];

        public filter_butterworth(float frequency, int sampleRate, pass_type passType, float resonance)
        {
            this.resonance = resonance;
            this.frequency = frequency;
            this.sampleRate = sampleRate;
            this.passType = passType;

            switch (passType)
            {
                case pass_type.Lowpass:
                    c = 1.0f / (float) Math.Tan(Math.PI * frequency / sampleRate);
                    a1 = 1.0f / (1.0f + resonance * c + c * c);
                    a2 = 2f * a1;
                    a3 = a1;
                    b1 = 2.0f * (1.0f - c * c) * a1;
                    b2 = (1.0f - resonance * c + c * c) * a1;
                    break;
                case pass_type.Highpass:
                    c = (float) Math.Tan(Math.PI * frequency / sampleRate);
                    a1 = 1.0f / (1.0f + resonance * c + c * c);
                    a2 = -2f * a1;
                    a3 = a1;
                    b1 = 2.0f * (c * c - 1.0f) * a1;
                    b2 = (1.0f - resonance * c + c * c) * a1;
                    break;
            }
        }

        public enum pass_type
        {
            Highpass,
            Lowpass,
        }

        public void update(float newInput)
        {
            float newOutput = a1 * newInput + a2 * this.inputHistory[0] + a3 * this.inputHistory[1] -
                              b1 * this.outputHistory[0] - b2 * this.outputHistory[1];

            this.inputHistory[1] = this.inputHistory[0];
            this.inputHistory[0] = newInput;

            this.outputHistory[2] = this.outputHistory[1];
            this.outputHistory[1] = this.outputHistory[0];
            this.outputHistory[0] = newOutput;
        }

        public float value
        {
            get { return this.outputHistory[0]; }
        }
    }
}
