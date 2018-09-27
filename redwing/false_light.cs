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
            float vol = GameManager.instance.gameSettings.masterVolume *
                        GameManager.instance.gameSettings.soundVolume * 0.01f;
            if (GameManager.instance.gameSettings.masterVolume <= 5)
            {
                vol /= 2f;
            }

            if (GameManager.instance.gameSettings.soundVolume <= 5)
            {
                vol /= 2f;
            }
            
            voidKnight.GetComponent<AudioSource>().PlayOneShot(load_textures.nukeSound, vol);
            yield return new WaitForSeconds(5f);
            if (!runningRadiance) yield break;
            
            GameManager.instance.LoadScene("Cinematic_Ending_C");
            //StartCoroutine(launchNuke());
            
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