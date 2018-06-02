using Modding;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ModCommon;
using HutongGames.PlayMaker;
using System.Collections;
using System.Reflection;

namespace redwing
{
    class RedwingFlameGen : MonoBehaviour
    {
        Texture2D[] fireBalls = new Texture2D[10];
        Texture2D[] fireTrails = new Texture2D[10];
        Texture2D[] firePillars = new Texture2D[10];
        Texture2D[] fireSpikes = new Texture2D[16];

        AudioSource soundPlayer;
        AudioClip[] soundFXClip = new AudioClip[3];
        private readonly int TESTING_CLIP = 2;
        int boopTimer;


        //float[][] soundEffects = new float[4][];

        // bullshit number picked because all the cool kids are doing it
        private readonly int AUDIO_SAMPLE_HZ = 44100;


        long[] pink_Rows = new long[30];
        long pink_RunningSum;   /* Used to optimize summing of generators. */
        float pink_Scalar;       /* Used to scale within range of -1.0 to +1.0 */
        int pink_IndexMask = (1 << 30) - 1;
        int pink_Index;
        //private readonly 



        public GameObject plane;
        public GameObject canvas;
        System.Random rng;

        

        private HeroController voidKnight;
        private GameObject sharpShadow;
        private PlayMakerFSM sharpShadowFSM;


        private readonly int FBTEXTURE_WIDTH = 800;
        private readonly int FBTEXTURE_HEIGHT = 800;

        private readonly int FTTEXTURE_WIDTH = 600;
        private readonly int FTTEXTURE_HEIGHT = 300;

        private readonly int FPTEXTURE_WIDTH = 500;
        private readonly int FPTEXTURE_HEIGHT = 1080;

        private readonly int FSTEXTURE_WIDTH = 40;
        private readonly int FSTEXTURE_HEIGHT = 500;

        public readonly double OPACITY_MASK = 1.0;

        public int sceneTimer;
        public int currentImg;

        private readonly double FB_COOLDOWN = 3.0f;
        private double fbTime = 0.0;

        // pick a factor of 360
        private readonly int INTERPOLATE_DEGREES = 18;

        // pick a factor of FTTEXTURE_WIDTH
        private readonly int FT_INTERPOLATE_PIXELS = 60;

        // pick a factor of FPTEXTURE_HEIGHT
        private readonly int FP_INTERPOLATE_PIXELS = 40;

        // pick a factor of FSTEXTURE_HEIGHT
        private readonly int FS_INTERPOLATE_PIXELS = 50;

        //public readonly Color[] flameIntensityCurve = { Color.red, new Color(1f, 0.3f, 0f), Color.yellow, Color.white };

        // What are the fire colors anyway?
        public readonly Color[] flameIntensityCurve = { Color.red, Color.yellow, Color.white, Color.white };

        // At what point do you switch from color X to color Y.
        public readonly double[] flameIntensityThresholds = { 0.4, 0.7, 2.5, 2.6 };


        public void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= Reset;
            ModHooks.Instance.DashPressedHook -= checkFireBalls;
            ModHooks.Instance.OnGetEventSenderHook -= FireSoul;
        }

        public void Start()
        {
            rng = new System.Random();
            Log("ATTEMPTING TO BUILD IMAGE");
            GenerateFlameTextures();
            Log("BUILD IMAGE SUCCESS ATTEMPTING TO SAVE");
            GenerateSoundEffects();

            //Log("Music made! attempting to play");
            //ModHooks.Instance.SlashHitHook += BoopOnHit;
            
            GameManager.instance.StartCoroutine(GetHeroFSMs());

            //ModHooks.Instance.DashPressedHook += checkFireBalls;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += Reset;
            //ModHooks.Instance.OnGetEventSenderHook += FireSoul;
        }

        private GameObject FireSoul(GameObject go, Fsm fsm)
        {
            Log("event sent. game object is: " + go.name + " and the fsm is: " + fsm.Name);
            if (go == sharpShadow)
            {
                PlayMakerFSM hm = FSMUtility.LocateFSM(fsm.GameObject, "health_manager") ?? FSMUtility.LocateFSM(fsm.GameObject, "health_manager_enemy");
                if (!Equals(hm, null))
                {
                    Log("You hit an enemy with sharp shadow. Wow. Awesome...");
                }
            }
            return go;
        }



        //for testing only

        IEnumerator VolumeFade(AudioSource _AudioSource, float waitTime)
        {
            float _StartTime = Time.time;
            waitTime = waitTime + _StartTime;
            while (_StartTime < waitTime)
            {
                _StartTime = Time.time;
                yield return null;
            }
            _AudioSource.Stop();

        }



        void BoopOnHit(Collider2D otherCollider, GameObject gameObject)
        {
            if (voidKnight == null || voidKnight != HeroController.instance)
                return;


            if (boopTimer <= 0)
            {
                voidKnight.GetComponent<AudioSource>().volume = 1.0f;
                voidKnight.GetComponent<AudioSource>().PlayOneShot(soundFXClip[TESTING_CLIP]);
                StartCoroutine(VolumeFade(voidKnight.GetComponent<AudioSource>(), 1.1f));
                boopTimer = 10;
                Log("playing boop! " + gameObject.name);
            }
        }

        private void GenerateSoundEffects()
        {
            soundFXClip[0] = AudioClip.Create("fireball1", (int) (AUDIO_SAMPLE_HZ * 1.0) + 30000, 1, AUDIO_SAMPLE_HZ, false);
            soundFXClip[0].SetData(generateFBSound((int)(AUDIO_SAMPLE_HZ * 1.0) + 30000), 0);

            soundFXClip[1] = AudioClip.Create("laser1", AUDIO_SAMPLE_HZ * 10, 1, AUDIO_SAMPLE_HZ, false);
            soundFXClip[1].SetData(generateLaserSound(AUDIO_SAMPLE_HZ * 10), 0);

            soundFXClip[2] = AudioClip.Create("shieldwavy", AUDIO_SAMPLE_HZ * 3, 1, AUDIO_SAMPLE_HZ, false);
            soundFXClip[2].SetData(generateShieldSound(AUDIO_SAMPLE_HZ * 3), 0);

        }

        private float[] generateShieldSound(int length)
        {
            float[] fx = new float[length];

            fx = generateNoiseAtHZ(0.06, 127.666, fx, 0, fx.Length);
            fx = generateNoiseAtHZ(0.01, 128, fx, 0, fx.Length);
            //fx = generateWhiteNoise(0.03, 0, fx.Length, fx);
            fx = normalizeVolume(fx);
            return fx;
        }

        private float[] generatePinkNoise(float volume, float[] fx)
        {
            pink_Index = 0;
            pink_Scalar = 1.0f / (31.0f * (1 << (24 - 1)));
            pink_RunningSum = 0;

            try
            {
                for (int i = 0; i < fx.Length; i++)
                {
                    fx[i] = generatePinkValue() * volume;
                }
            } catch (Exception e)
            {
                Log("Error building pink noise " + e);
            }


            return fx;
        }

        private float generatePinkValue()
        {
            float f;
            long sum;
            long newRandom;
            pink_Index = (pink_Index++) & pink_IndexMask;
            if (pink_Index != 0)
            {
                int numZeros = 0;
                int n = pink_Index;
                while ( (n & 1) == 0)
                {
                    n = n >> 1;
                    numZeros++;
                }

                pink_RunningSum -= pink_Rows[numZeros];
                newRandom = rng.Next(int.MinValue, int.MaxValue);
                newRandom = (newRandom << 32);
                newRandom = newRandom | (long)rng.Next(int.MinValue, int.MaxValue);
                pink_RunningSum += newRandom;
                pink_Rows[numZeros] = newRandom;

            }

            newRandom = rng.Next(int.MinValue, int.MaxValue);
            newRandom = (newRandom << 32);
            newRandom = newRandom | (long)rng.Next(int.MinValue, int.MaxValue);
            sum = pink_RunningSum + newRandom;

            f = pink_Scalar * sum;
            if (f > 1.0)
            {
                f = 1.0f;
            } else if (f < -1.0)
            {
                f = 1.0f;
            }
            return f;
        }

        private float[] generateFBSound(int length)
        {
            float[] fx = new float[length];
            return generateFBSound(fx);
        }

        private float[] generateFBSound (float[] fx)
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
                Log("Made fireball sound without error");

                return fx;
            } catch (Exception e)
            {
                Log("Unable to make sound because " + e);
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
                ///fx = generateNoiseAtHZ(2.0, 1000, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(1.0, 500, fx, 0, fx.Length);
                //fx = generateNoiseAtHZ(2.0, 250, fx, 0, fx.Length);

                fx = normalizeVolume(fx);
                Log("Made fireball sound without error");

                return fx;
            }
            catch (Exception e)
            {
                Log("Unable to make sound because " + e);
                return null;
            }
        }

        // this is stupid and can't possibly work.
        private float[] stupidLowPassFilter(float[] fx)
        {
            float f = 0;
            float f2 = 0;
            for (int i = 0; i < fx.Length; i++)
            {
                f = fx[i];
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
            float[] Dat2 = new float[dF2 + 4]; // Array with 3 extra points front and back
            float[] data = fx; // Ptr., changes passed data

            // Copy indata to Dat2
            for (long r = 0; r < dF2; r++)
            {
                Dat2[2 + r] = fx[r];
            }
            Dat2[1] = Dat2[0] = fx[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = fx[dF2];
            Log("Allocation complete without error");

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
            float[] DatYt = new float[dF2 + 4];
            DatYt[1] = DatYt[0] = fx[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                DatYt[s] = (float)(a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2]
                           + d * DatYt[s - 1] + e * DatYt[s - 2]);
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

            Log("Recursive triggers complete");

            // FORWARD filter
            float[] DatZt = new float[dF2 + 2];
            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                DatZt[-t] = (float)(a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4]
                            + d * DatZt[-t + 1] + e * DatZt[-t + 2]);
            }
            Log("Recursive triggers complete");

            // Calculated points copied for return
            for (long p = 0; p < dF2; p++)
            {
                data[p] = DatZt[p];
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
            float[] speededFX = new float[fx.Length];

            for (int i = 0; i < fx.Length; i++)
            {
                double realPosition = i * speed;
                int positionRoundedDown = (int)realPosition;
                double weighting = realPosition % 1.0;
                if (positionRoundedDown + 1 < fx.Length)
                {
                    speededFX[i] = (float)(((1.0 - weighting) * fx[positionRoundedDown]) + (weighting * fx[positionRoundedDown + 1]));

                }
                else
                {
                    break;
                }
            }

            return speededFX;
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
                Log("Unable to fade out audio. probably because out of bounds " + e);
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
                Log("Unable to generate white noise from time " + startTime + " to " + endTime + " probably because out of bounds " + e);
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
                Log("Unable to generate sawtooth from time " + startTime + " to " + endTime + " probably because out of bounds " + e);
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
                fx = generateNoiseAtHZ(realVolume, i, fx, startTime, endTime);
            }

            return fx;
        }

        private float[] generateNoiseAtHZ (double volume, double freq, float[] fx, int startTime, int endTime)
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
                Log("Unable to generate noise from time " + startTime + " to " + endTime + " probably because out of bounds " + e);
            }
            return fx;
        }
        
        // Reduces all sounds to range from -1 to 1
        // without clipping.
        private float[] normalizeVolume (float[] fx)
        {
            float maxVolume = 0.0f;

            for (int i = 0; i < fx.Length; i++)
            {
                if (fx[i] < 0.0f)
                {
                    if (-maxVolume > fx[i])
                    {
                        maxVolume = -fx[i];
                    }
                } else
                {
                    if (maxVolume < fx[i])
                    {
                        maxVolume = fx[i];
                    }
                }
            }
            if (maxVolume >= 1.0)
            {
                for (int i = 0; i < fx.Length; i++)
                {
                    fx[i] /= maxVolume;
                }
                Log("Normalized volume by reducing it by a factor of " + maxVolume);
                Log("This means <" + Math.Log10(maxVolume) / Math.Log10(2.0) + " bits of entropy out of 16 lost");
            } else
            {
                Log("No volume normalization needed, max volume is only: " + maxVolume);
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

        private void checkFireBalls()
        {
            float cooldown = (float) HeroController.instance.GetType().GetField("dashCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(HeroController.instance);
            if (cooldown <= 0)
            {
                HeroActions direction = GameManager.instance.inputHandler.inputActions;
                if (direction.up.IsPressed && !direction.down.IsPressed && !direction.left.IsPressed && !direction.right.IsPressed)
                {
                    if (fbTime <= 0.0)
                    {
                        spawnFireballs();
                        fbTime = FB_COOLDOWN;
                    }
                } else
                {
                    spawnFireTrail();
                }


            }


        }

        private void spawnFireTrail()
        {
            Log("I should be spawning a fire trail right now");
        }


        private void spawnFireballs()
        {
            Log("I should be spawning fireballs right now");
        }


        IEnumerator GetHeroFSMs()
        {
            while (HeroController.instance == null)
                yield return new WaitForEndOfFrame();

            if (sharpShadow == null || sharpShadow.tag != "Sharp Shadow")
                foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (gameObject != null && gameObject.tag == "Sharp Shadow")
                    {
                        sharpShadow = gameObject;
                        sharpShadowFSM = FSMUtility.LocateFSM(sharpShadow, "damages_enemy");

                        Log("Found sharpshadow");
                    }
                }
        }

        public void Update()
        {
            //sceneTimer++;
            if (voidKnight == null)
            {
                voidKnight = HeroController.instance;
            }

            if (boopTimer > 0)
            {
                boopTimer--;
            }

            attackCooldownUpdate();

            if (sceneTimer % 30 == 0)
            {
                
                if (currentImg < 10)
                {
                    canvas.GetComponent<Image>().sprite = Sprite.Create(fireBalls[currentImg], new Rect(0, 0, FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT), new Vector2(0, 0));
                } else if (currentImg < 20)
                {
                    canvas.GetComponent<Image>().sprite = Sprite.Create(firePillars[currentImg - 10], new Rect(0, 0, FPTEXTURE_WIDTH, FPTEXTURE_HEIGHT), new Vector2(0, 0));
                } else if (currentImg < 30)
                {
                    canvas.GetComponent<Image>().sprite = Sprite.Create(fireTrails[currentImg - 20], new Rect(0, 0, FTTEXTURE_WIDTH, FTTEXTURE_HEIGHT), new Vector2(0, 0));
                } else
                {
                    canvas.GetComponent<Image>().sprite = Sprite.Create(fireSpikes[currentImg - 30], new Rect(0, 0, FSTEXTURE_WIDTH, FSTEXTURE_HEIGHT), new Vector2(0, 0));
                }
                currentImg++;

                if (currentImg >= 46)
                {
                    currentImg = 0;
                }
                canvas.SetActive(true);
            }

            
        }

        private void attackCooldownUpdate()
        {
            if (fbTime > 0.0)
            {
                fbTime -= Time.deltaTime;
            } else
            {
                fbTime = 0.0;
            }


        }

        private void Reset(Scene arg0, LoadSceneMode arg1)
        {
            sceneTimer = 1;
            currentImg = 0;
            Log("PLACING BUILD IMAGE SOMEWHERE");

            
            //PlaceTextureSomewhere();


        }


        public void PlaceTextureSomewhere()
        {

            plane = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));            
            plane.SetActive(true);
            Sprite renderSprite = Sprite.Create(fireBalls[currentImg], new Rect(0, 0, FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT), new Vector2(0, 0));
            canvas = CanvasUtil.CreateImagePanel(plane, renderSprite, new CanvasUtil.RectData(new Vector2(1920, 1080), new Vector2(0f, 0f)));



        }

        public Mesh CreateMesh(float width, float height)
        {
            Mesh m = new Mesh();
            m.name = "ScriptedMesh";
            m.vertices = new Vector3[]
            {
                new Vector3(-width, -height, 0.01f),
                new Vector3(width, -height, 0.01f),
                new Vector3(width, height, 0.01f),
                new Vector3(-width, height, 0.01f)
            };
            m.uv = new Vector2[]
            {
                new Vector2 (0, 0),
                new Vector2 (0, 1),
                new Vector2(1, 1),
                new Vector2 (1, 0)
            };
            m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();
            return m;
        }

        public void GenerateFlameTextures()
        {

            try
            {
                for (int i = 0; i < fireBalls.Length; i++)
                {
                    fireBalls[i] = GenerateFireball();
                    fireBalls[i].Apply();
                }
            } catch (Exception e)
            {
                Log("Unable to build fireballs. Error " + e);
            }

            try
            {
                for (int i = 0; i < fireTrails.Length; i++)
                {
                    fireTrails[i] = GenerateFireTrail();
                    fireTrails[i].Apply();
                }
            } catch (Exception e)
            {
                Log("Unable to build fire trails. Error " + e);
            }

            try
            {
                for (int i = 0; i < firePillars.Length; i++)
                {
                    firePillars[i] = GenerateFirePillar();
                    firePillars[i].Apply();
                }
            } catch (Exception e)
            {
                Log("Unable to build fire pillars. Error " + e);
            }

            try
            {
                for (int i = 0; i < fireSpikes.Length; i++)
                {
                    fireSpikes[i] = GenerateFireSpike();
                    fireSpikes[i].Apply();
                }
            }
            catch (Exception e)
            {
                Log("Unable to build fire spikes. Error " + e);
            }

            Log("Built all flame textures.");

            
        }

        public Texture2D GenerateFireSpike()
        {
            Texture2D fs = new Texture2D(FSTEXTURE_WIDTH, FSTEXTURE_HEIGHT);
            double[] horzIntensity20 = new double[FSTEXTURE_HEIGHT];
            double[] horzOpacity20 = new double[FSTEXTURE_HEIGHT];


            // RNG phase
            for (int i = 0; i < FSTEXTURE_HEIGHT; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS == 0)
                {
                    horzIntensity20[i] = rng.NextDouble();

                    horzOpacity20[i] = 0.5f;
                    //horzOpacity20[i] = rng.NextDouble();

                    // because c# sucks NextDouble can't return arbitrary numbers
                    // so apply a transformation to map verticalIntensity150 -> 0-0.2
                    // and verticalOpacity150 -> -1 - 0
                    //horzOpacity20[i] = horzOpacity20[i] * 0.2 - 0.6;

                    horzIntensity20[i] = (horzIntensity20[i] * 0.2);
                }
            }

            // Interpolation phase
            for (int i = 0; i < FSTEXTURE_HEIGHT - FS_INTERPOLATE_PIXELS; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS != 0)
                {
                    int offset = i % FS_INTERPOLATE_PIXELS;
                    double avgWeighting = (double)offset / (double)FS_INTERPOLATE_PIXELS;

                    horzIntensity20[i] = horzIntensity20[i - offset + FS_INTERPOLATE_PIXELS] * avgWeighting + horzIntensity20[i - offset] * (1.0 - avgWeighting);
                    horzOpacity20[i] = horzOpacity20[i - offset + FS_INTERPOLATE_PIXELS] * avgWeighting + horzOpacity20[i - offset] * (1.0 - avgWeighting);
                }
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = FSTEXTURE_HEIGHT - FS_INTERPOLATE_PIXELS; i < FSTEXTURE_HEIGHT; i++)
            {
                if (i % FS_INTERPOLATE_PIXELS != 0)
                {
                    int offset = i % FS_INTERPOLATE_PIXELS;
                    double avgWeighting = (double)offset / (double)FS_INTERPOLATE_PIXELS;

                    horzIntensity20[i] = horzIntensity20[0] * avgWeighting + horzIntensity20[i - offset] * (1.0 - avgWeighting);
                    horzOpacity20[i] = horzOpacity20[0] * avgWeighting + horzOpacity20[i - offset] * (1.0 - avgWeighting);
                }
            }


            // Actually set the colors
            for (int x = 0; x < FSTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FSTEXTURE_HEIGHT; y++)
                {
                    fs.SetPixel(x, y, getFireColor
                        ((x - FSTEXTURE_WIDTH / 2), horzIntensity20[y], horzOpacity20[y], FSTEXTURE_WIDTH / 2, FSTEXTURE_WIDTH / 2));
                }
            }

            return fs;
        }

        public Texture2D GenerateFirePillar()
        {
            Texture2D fp = new Texture2D(FPTEXTURE_WIDTH, FPTEXTURE_HEIGHT);
            double[] horzIntensity150 = new double[FPTEXTURE_HEIGHT];
            double[] horzOpacity150 = new double[FPTEXTURE_HEIGHT];


            // RNG phase
            for (int i = 0; i < FPTEXTURE_HEIGHT; i++)
            {
                if (i % FP_INTERPOLATE_PIXELS == 0)
                {
                    horzIntensity150[i] = rng.NextDouble();
                    horzOpacity150[i] = rng.NextDouble();

                    // because c# sucks NextDouble can't return arbitrary numbers
                    // so apply a transformation to map verticalIntensity150 -> 0-0.2
                    // and verticalOpacity150 -> -1 - 0
                    horzOpacity150[i] = horzOpacity150[i] * 0.2 - 0.6;

                    horzIntensity150[i] = (horzIntensity150[i] * 0.2);
                }
            }

            // Interpolation phase
            for (int i = 0; i < FPTEXTURE_HEIGHT - FP_INTERPOLATE_PIXELS; i++)
            {
                if (i % FP_INTERPOLATE_PIXELS != 0)
                {
                    int offset = i % FP_INTERPOLATE_PIXELS;
                    double avgWeighting = (double)offset / (double)FP_INTERPOLATE_PIXELS;

                    horzIntensity150[i] = horzIntensity150[i - offset + FP_INTERPOLATE_PIXELS] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                    horzOpacity150[i] = horzOpacity150[i - offset + FP_INTERPOLATE_PIXELS] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
                }
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = FPTEXTURE_HEIGHT - FP_INTERPOLATE_PIXELS; i < FPTEXTURE_HEIGHT; i++)
            {
                if (i % FP_INTERPOLATE_PIXELS != 0)
                {
                    int offset = i % FP_INTERPOLATE_PIXELS;
                    double avgWeighting = (double)offset / (double)FP_INTERPOLATE_PIXELS;

                    horzIntensity150[i] = horzIntensity150[0] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                    horzOpacity150[i] = horzOpacity150[0] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
                }
            }


            // Actually set the colors
            for (int x = 0; x < FPTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FPTEXTURE_HEIGHT; y++)
                {
                    fp.SetPixel(x, y, getFireColor
                        ((x - FPTEXTURE_WIDTH / 2), horzIntensity150[y], horzOpacity150[y], FPTEXTURE_WIDTH / 2, FPTEXTURE_WIDTH / 2));
                }
            }

            return fp;
        }

        public Texture2D GenerateFireTrail()
        {
            Texture2D ft = new Texture2D(FTTEXTURE_WIDTH, FTTEXTURE_HEIGHT);
            double[] verticalIntensity150 = new double[FTTEXTURE_WIDTH];
            double[] verticalOpacity150 = new double[FTTEXTURE_WIDTH];

            // RNG phase
            for (int i = 0; i < FTTEXTURE_WIDTH; i++)
            {
                if (i % FT_INTERPOLATE_PIXELS == 0)
                {
                    verticalIntensity150[i] = rng.NextDouble();
                    verticalOpacity150[i] = rng.NextDouble();

                    // because c# sucks NextDouble can't return arbitrary numbers
                    // so apply a transformation to map verticalIntensity150 -> 0 - 0.2
                    // and verticalOpacity150 -> -1 - 0
                    verticalOpacity150[i] = verticalOpacity150[i] * 0.2 - 0.6;
                    verticalIntensity150[i] = (verticalIntensity150[i] * 0.2);
                }
            }

            // Interpolation phase
            for (int i = 0; i < FTTEXTURE_WIDTH - FT_INTERPOLATE_PIXELS; i++)
            {
                if (i % FT_INTERPOLATE_PIXELS != 0)
                {
                    int offset = i % FT_INTERPOLATE_PIXELS;
                    double avgWeighting = (double)offset / (double)FT_INTERPOLATE_PIXELS;

                    verticalIntensity150[i] = verticalIntensity150[i - offset + FT_INTERPOLATE_PIXELS] * avgWeighting + verticalIntensity150[i - offset] * (1.0 - avgWeighting);
                    verticalOpacity150[i] = verticalOpacity150[i - offset + FT_INTERPOLATE_PIXELS] * avgWeighting + verticalOpacity150[i - offset] * (1.0 - avgWeighting);
                }
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = FTTEXTURE_WIDTH - FT_INTERPOLATE_PIXELS; i < FTTEXTURE_WIDTH; i++)
            {
                if (i % FT_INTERPOLATE_PIXELS != 0)
                {
                    int offset = i % FT_INTERPOLATE_PIXELS;
                    double avgWeighting = (double)offset / (double)FT_INTERPOLATE_PIXELS;

                    verticalIntensity150[i] = verticalIntensity150[0] * avgWeighting + verticalIntensity150[i - offset] * (1.0 - avgWeighting);
                    verticalOpacity150[i] = verticalOpacity150[0] * avgWeighting + verticalOpacity150[i - offset] * (1.0 - avgWeighting);
                }
            }


            // Actually set the colors
            for (int x = 0; x < FTTEXTURE_WIDTH; x++)
            {
                for (int y = 0; y < FTTEXTURE_HEIGHT; y++)
                {
                    ft.SetPixel(x, y, getFireColor
                        ( (y - FTTEXTURE_HEIGHT / 2), verticalIntensity150[x], verticalOpacity150[x], FTTEXTURE_HEIGHT / 2, FTTEXTURE_HEIGHT / 2));
                }
            }
            return ft;
        }

        public Texture2D GenerateFireball()
        {
            Texture2D fb = new Texture2D(FBTEXTURE_WIDTH, FBTEXTURE_HEIGHT);
            double[] radialIntensity400 = new double[360];
            double[] radialOpacity400 = new double[360];

            // Generation
            for (int i = 0; i < 360 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES == 0)
                {
                    radialIntensity400[i] = rng.NextDouble();
                    radialOpacity400[i] = rng.NextDouble();

                    // because c# sucks NextDouble can't return arbitrary numbers
                    // so apply a transformation to map radialIntensity400 -> 0 - 0.2
                    // and radialOpacity400 -> -0.25 - -0.1
                    radialIntensity400[i] = (radialIntensity400[i] * 0.2);
                    radialOpacity400[i] = (radialOpacity400[i] * 0.15) - 0.25;
                }
            }
            // Interpolation (to avoid really crazy looking balls)
            for (int i = 0; i < 360 - INTERPOLATE_DEGREES; i++)
            {
                if (i % INTERPOLATE_DEGREES != 0)
                {
                    int offset = i % INTERPOLATE_DEGREES;
                    double avgWeighting = (double)offset / (double)INTERPOLATE_DEGREES;

                    radialIntensity400[i] = radialIntensity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialIntensity400[i - offset] * (1.0 - avgWeighting);
                    radialOpacity400[i] = radialOpacity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialOpacity400[i - offset] * (1.0 - avgWeighting);
                }
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
                    int angel = getNearestAngel(x, y);
                    fb.SetPixel(x, y, getFireColor
                        (getDistance(x, y), radialIntensity400[angel], radialOpacity400[angel], FBTEXTURE_HEIGHT/2, FBTEXTURE_HEIGHT/2));
                }
            }
            return fb;
        }

        public Color getFireColor(double distance, double intensity400, double opacity400, int intensInterpolateDist, int opacInterpDist)
        {
            double intensity = getRealIntensity(distance, intensity400, intensInterpolateDist);
            Color c = new Color();

            c.a = (float)getRealOpacity(distance, opacity400, opacInterpDist);

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

        public double getRealOpacity(double distance, double opacity400, int interpolateDistance)
        {
            if (distance < 0.0)
            {
                distance = -distance;
            }
            double averageWeighting = distance / (double)interpolateDistance;
            double opactReal = opacity400 * averageWeighting + ((1.0 - averageWeighting) * 14);
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

        public double getRealIntensity(double distance, double intensity400, int interpolateDistance)
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

        public double getDistance(int x, int y)
        {
            int relX = x - (FBTEXTURE_WIDTH / 2 );
            int relY = y - (FBTEXTURE_HEIGHT / 2);

            return Math.Sqrt( (double) (relX * relX) + (double) (relY * relY) );
        }

        

        public int getNearestAngel(int x, int y)
        {
            int angel = 0;

            int relX = x - (FBTEXTURE_WIDTH / 2);
            int relY = y - (FBTEXTURE_HEIGHT / 2);

            if (relX == 0)
            {
                if (relY >= 0)
                {
                    return 90;
                } else
                {
                    return 270;
                }
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

        public void BuildDashFireFSM()
        {

        }

        public void Log(String str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}
