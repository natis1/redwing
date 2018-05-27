using Modding;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace redwing
{
    class RedwingFlame : MonoBehaviour
    {
        Texture2D tex;
        public GameObject plane;
        public GameObject canvas;


        private readonly int TEXTURE_WIDTH = 400;
        private readonly int TEXTURE_HEIGHT = 400;
        private readonly int TEXTURE_WMIDPOINT = 200;
        private readonly int TEXTURE_HMIDPOINT = 200;
        private readonly int INTERPOLATE_DEGREES = 8;

        public readonly Color[] flameIntensityCurve = { Color.red, new Color(1f, 0.3f, 0f), Color.yellow, Color.white };

        public void Start()
        {
            Log("ATTEMPTING TO BUILD IMAGE");
            GenerateFlameTextures();
            Log("BUILD IMAGE SUCCESS ATTEMPTING TO SAVE");

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += Reset;
        }

        

        public void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= Reset;
        }

        private void Reset(Scene arg0, LoadSceneMode arg1)
        {
            Log("PLACING BUILD IMAGE SOMEWHERE");
            PlaceTextureSomewhere();
        }


        public void PlaceTextureSomewhere()
        {

            plane = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

            
            //plane = new GameObject("Plane");

            //SpriteRenderer rend = plane.AddComponent<SpriteRenderer>();
            
            Sprite renderSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(300, 300));
            //rend.sprite = renderSprite;

            canvas = CanvasUtil.CreateImagePanel(plane, renderSprite, new CanvasUtil.RectData(new Vector2(400f, 400f), new Vector2(300f, 300f) ));

            // make it able to be walked on 
            //plane.tag = "HeroWalkable";
            //plane.layer = 6;

            // Dimensions 
            /*
            MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = CreateMesh(5f, 5f);
            MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Particles/Additive");
            
            // Renderer
            renderer.material.mainTexture = tex;
            renderer.material.color = Color.white;

            // Collider
            //BoxCollider2D a = plane.AddComponent<BoxCollider2D>();
            //a.isTrigger = false;

            plane.transform.SetPosition3D(100, 17, -0.05f);
            */

            //canvas.AddComponent<GameObject>();


            plane.SetActive(true);
            canvas.SetActive(true);
            

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
            System.Random rng = new System.Random();
            if (tex == null)
            {
                tex = new Texture2D(400, 400);
            }
            double[] radialIntensity400 = new double[360];
            double[] radialOpacity400 = new double[360];

            // Generation
            for (int i = 0; i < 355; i++)
            {
                if (i % INTERPOLATE_DEGREES == 0)
                {
                    radialIntensity400[i] = rng.NextDouble();
                    radialOpacity400[i] = rng.NextDouble();

                    // because c# sucks NextDouble can't return arbitrary numbers
                    // so apply a transformation to map radialIntensity400 -> 1-1.5
                    // and radialOpacity400 -> -0.4 - 0.4
                    radialIntensity400[i] = (radialIntensity400[i] * 0.5) + 1.0;
                    radialOpacity400[i] = (radialOpacity400[i] * 0.8) - 0.4;
                }
            }

            // Interpolation (to avoid really crazy looking balls)
            for (int i = 0; i < 355; i++)
            {
                if (i % INTERPOLATE_DEGREES != 0)
                {
                    int offset = i % INTERPOLATE_DEGREES;
                    double avgWeighting = (double)offset / INTERPOLATE_DEGREES;

                    radialIntensity400[i] = radialIntensity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialIntensity400[i - offset] * (1.0 - avgWeighting);
                    radialOpacity400[i] = radialOpacity400[i - offset + INTERPOLATE_DEGREES] * avgWeighting + radialOpacity400[i - offset] * (1.0 - avgWeighting);
                }
            }


            for (int x = 0; x < 400; x++)
            {
                for (int y = 0; y < 400; y++)
                {
                    int angel = getNearestAngel(x, y);
                    tex.SetPixel(x, y, getFireColor(getDistance(x, y), radialIntensity400[angel], radialOpacity400[angel]));
                }
            }

            tex.Apply();

        }

        public Color getFireColor(double distance, double intensity400, double opacity400)
        {
            double intensity = getRealIntensity(distance, intensity400);
            Color c = new Color();

            c.a = (float)getRealOpacity(distance, opacity400);

            if (intensity < 1.0)
            {
                double intens1 = intensity;
                double intens2 = 1.0 - intensity;
                c.r = (flameIntensityCurve[0].r * (float)intens2) + (flameIntensityCurve[1].r * (float)intens1);
                c.g = (flameIntensityCurve[0].g * (float)intens2) + (flameIntensityCurve[1].g * (float)intens1);
                c.b = (flameIntensityCurve[0].b * (float)intens2) + (flameIntensityCurve[1].b * (float)intens1);
            } else if (intensity < 2.0)
            {
                double intens1 = intensity - 1.0;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[1].r * (float)intens2) + (flameIntensityCurve[2].r * (float)intens1);
                c.g = (flameIntensityCurve[1].g * (float)intens2) + (flameIntensityCurve[2].g * (float)intens1);
                c.b = (flameIntensityCurve[1].b * (float)intens2) + (flameIntensityCurve[2].b * (float)intens1);
            } else
            {
                double intens1 = intensity - 2.0;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[2].r * (float)intens2) + (flameIntensityCurve[3].r * (float)intens1);
                c.g = (flameIntensityCurve[2].g * (float)intens2) + (flameIntensityCurve[3].g * (float)intens1);
                c.b = (flameIntensityCurve[2].b * (float)intens2) + (flameIntensityCurve[3].b * (float)intens1);
            }
            return c;
        }

        public double getRealOpacity(double distance, double opacity400)
        {
            double averageWeighting = distance / 200.0;
            double opactReal = opacity400 * averageWeighting + ((1.0 - averageWeighting) * 1.6);
            if (opactReal < 0.0)
            {
                opactReal = 0.0;
            } else if (opactReal >= 1.0)
            {
                opactReal = 1.0;
            }
            return opactReal;
        }

        public double getRealIntensity(double distance, double intensity400)
        {
            double averageWeighting = distance / 200.0;
            double intenReal = intensity400 * averageWeighting + ((1.0 - averageWeighting) * 4.0);
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
            int relX = x - TEXTURE_WMIDPOINT;
            int relY = y - TEXTURE_HMIDPOINT;

            return Math.Sqrt( (double) (relX * relX) + (double) (relY * relY) );
        }

        

        public int getNearestAngel(int x, int y)
        {
            int angel = 0;

            int relX = x - TEXTURE_WMIDPOINT;
            int relY = y - TEXTURE_HMIDPOINT;

            if (relX == 0)
            {
                if (relY >= 0)
                {
                    return 90;
                } else
                {
                    return 270;
                }
            } else if (relX > 0)
            {
                angel = (int) ( Math.Atan2(relY, relX) * 180.0 / Math.PI);
            } else
            {
                angel = (int) ( Math.Atan2(relY, relX) * 180.0 / Math.PI)  + 180;
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

        new public void Log(String str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}
