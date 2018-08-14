using System.Collections;
using ModCommon;
using Modding;
using UnityEngine;
using UnityEngine.UI;

namespace redwing
{
    public class false_light : MonoBehaviour
    {
        private const float timeToAutonuke = 29.8f;
        private bool runningRadiance;
        private const float nukeAnimFrameTime = 0.2f;
        private GameObject voidKnight;
        private time_attack cachedTimer;
        
        private GameObject corruptedLight;
        private HealthManager lightHM;
        private CustomEnemySpeed lightCustom;
        private bool startedNuke;
        
        private int currentDmgDone;
        private int actualDmgDone;

        private static readonly CustomEnemySpeed.WaitData[] WAITS = new[]
        {
            new CustomEnemySpeed.WaitData(2f, "Control", "TL"),
            new CustomEnemySpeed.WaitData(2f, "Control", "TR"),
            new CustomEnemySpeed.WaitData(2f, "Control", "A1 Cast End"),
            new CustomEnemySpeed.WaitData(2f, "Control", "Rage1 Start"),
            new CustomEnemySpeed.WaitData(2f, "Control", "Rage Comb"),
            new CustomEnemySpeed.WaitData(2f, "Control", "A2 Cast End")
        };

        private static readonly CustomEnemySpeed.WaitRandomData[] WAIT_RANDOMS = new[]
        {
            new CustomEnemySpeed.WaitRandomData(2f, "Control", "Arena 1 Idle")
        };

        private static readonly CustomEnemySpeed.AnimationData[] ANIMATIONS = new[]
        {
            new CustomEnemySpeed.AnimationData(2f, "Antic"),
            new CustomEnemySpeed.AnimationData(2f, "Cast"),
            new CustomEnemySpeed.AnimationData(2f, "Recover")
        };
        
        private void Start()
        {
            log("Starting false light load. This should still be a very easy boss if you are playing with blackmoth.");

            startedNuke = false;
            corruptedLight = GameObject.Find("Boss Control").FindGameObjectInChildren("Radiance");
            lightHM = corruptedLight.GetEnemyHealthManager();
            lightCustom = corruptedLight.GetOrAddComponent<CustomEnemySpeed>();

            foreach (CustomEnemySpeed.WaitRandomData w in WAIT_RANDOMS)
            {
                lightCustom.AddWaitRandomData(w);
            }
            
            foreach (CustomEnemySpeed.WaitData w in WAITS)
            {
                lightCustom.AddWaitData(w);
            }

            foreach (CustomEnemySpeed.AnimationData a in ANIMATIONS)
            {
                lightCustom.AddAnimationData(a);
            }
            
            lightCustom.StartSpeedMod();
            voidKnight = HeroController.instance.spellControl.gameObject;
            
            log("I don't know what I'm doing here but I did load successfully.");
            runningRadiance = true;
        }

        
        
        private void Update()
        {
            if (currentDmgDone == lightCustom.damageDone) return;
            
            actualDmgDone += lightCustom.damageDone - currentDmgDone;
            currentDmgDone = (int) (actualDmgDone * 0.4);
            if (currentDmgDone > 1500)
            {
                lightCustom.OverrideDamageDone(1001);
                currentDmgDone = lightCustom.damageDone;
                if (!startedNuke)
                {
                    StartCoroutine(goNuclear());
                    startedNuke = true;
                }
            }
            lightCustom.OverrideDamageDone(currentDmgDone);
        }

        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }

        private IEnumerator goNuclear()
        {
            cachedTimer = gameObject.GetOrAddComponent<time_attack>();
            cachedTimer.timeRemaining = timeToAutonuke;
            
            for (float time = timeToAutonuke; time > 0.0f; time -= Time.deltaTime)
            {
                if (!runningRadiance)
                {
                    yield break;
                } else if (Input.GetKeyDown(KeyCode.N))
                {
                    cachedTimer.timeRemaining = 0f;
                    break;
                }
                else
                {
                    yield return null;
                }
            }

            gameObject.transform.position = voidKnight.transform.position;
            voidKnight.GetComponent<AudioSource>().PlayOneShot(load_textures.nukeSound,
                (GameManager.instance.gameSettings.masterVolume *
                 GameManager.instance.gameSettings.soundVolume * 0.01f));
            yield return new WaitForSeconds(5f);
            if (!runningRadiance) yield break;
            
            GameManager.instance.LoadScene("Cinematic_Ending_C");
            //StartCoroutine(launchNuke());
            
        }

        private IEnumerator launchNuke()
        {
            yield return new WaitForSeconds(0.2f);
            Vector3 currentKnightPosition = voidKnight.transform.position;
            currentKnightPosition.y += 8f;
            StartCoroutine(freezeKnight(currentKnightPosition));
            
            GameObject plane = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            plane.name = "RedwingDreamWMD";
            plane.SetActive(true);
            Sprite renderSprite = Sprite.Create(load_textures.nukeAnimation[0], new Rect(0, 0,
                load_textures.nukeAnimation[0].width, load_textures.nukeAnimation[0].height), new Vector2(0.5f, 0.5f));
            GameObject nukeRenderer = CanvasUtil.CreateImagePanel(plane, renderSprite, new CanvasUtil.RectData(new Vector2(1920, 1080), new Vector2(0f, 0f)));
            nukeRenderer.name = "RedwingDreamWMDSprite";
            
            yield return new WaitForSeconds(nukeAnimFrameTime);
            for (int i = 1; i < load_textures.nukeAnimation.Length; i++)
            {
                nukeRenderer.GetComponent<Image>().sprite = Sprite.Create(load_textures.nukeAnimation[i], new Rect(0, 0,
                        load_textures.nukeAnimation[i].width, load_textures.nukeAnimation[i].height),
                    new Vector2(0.5f, 0.5f));
                yield return new WaitForSeconds(nukeAnimFrameTime);
            }
            
            runningRadiance = false;
            DestroyImmediate(nukeRenderer);
            DestroyImmediate(plane);
            GameManager.instance.LoadScene("Cinematic_Ending_C");
            Destroy(gameObject);
        }

        private IEnumerator freezeKnight(Vector3 locationToFreeze)
        {
            while (runningRadiance)
            {
                voidKnight.transform.position = locationToFreeze;
                yield return null;
            }
        }
    }
}