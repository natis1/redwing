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
    class RedwingFlame : MonoBehaviour
    {
        Texture2D[] fireBalls = new Texture2D[10];
        Texture2D[] fireTrails = new Texture2D[10];
        Texture2D[] firePillars = new Texture2D[10];
        Texture2D[] fireSpikes = new Texture2D[16];
        public GameObject plane;
        public GameObject canvas;
        System.Random rng;

        

        private GameObject voidKnight;
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

            GameManager.instance.StartCoroutine(GetHeroFSMs());

            ModHooks.Instance.DashPressedHook += checkFireBalls;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += Reset;
            ModHooks.Instance.OnGetEventSenderHook -= FireSoul;
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

        private void checkFireBalls()
        {
            float cooldown = (float) HeroController.instance.GetType().GetField("dashCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(HeroController.instance);
            if (cooldown <= 0)
            {
                HeroActions direction = GameManager.instance.inputHandler.inputActions;
                if (direction.up.IsPressed && !direction.down.IsPressed && !direction.left.IsPressed && !direction.right.IsPressed)
                {
                    spawnFireballs();
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
            sceneTimer++;

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

        

        

        private void Reset(Scene arg0, LoadSceneMode arg1)
        {
            sceneTimer = 0;
            currentImg = 0;
            Log("PLACING BUILD IMAGE SOMEWHERE");
            PlaceTextureSomewhere();


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
