using System;
using System.Collections;
using System.Reflection;
using HutongGames.PlayMaker;
using Modding;
using UnityEngine;
using static HeroController;

namespace redwing
{
    public class redwing_hooks : MonoBehaviour
    {
        private void OnDestroy()
        {
            ModHooks.Instance.DashPressedHook -= checkFireBalls;
            ModHooks.Instance.TakeDamageHook -= laserAttack;
            ModHooks.Instance.TakeDamageHook -= flameShield;
        }
        
        public void Start()
        {
            StartCoroutine(getHeroFsMs());
            redwingSpawner = new redwing_game_objects();
            ModHooks.Instance.DashPressedHook += checkFireBalls;
            ModHooks.Instance.TakeDamageHook += laserAttack;
            ModHooks.Instance.TakeDamageHook += flameShield;
        }

        private int flameShield(ref int hazardtype, int damage)
        {
            if (hazardtype != (int) GlobalEnums.HazardType.SPIKES ||
                HeroController.instance.cState.invulnerable) return damage;
            
            if (invulnTime > 0.0)
            {
                return 0;
            }
            if (!(fsCharge <= 0.0) || damage <= 0) return damage;
            
            
            log("Shielding one damage");
            fsCharge = FS_RECHARGE;
            invulnTime = IFRAMES;
            damage--;

            return damage;
        }

        private int laserAttack(ref int hazardtype, int damageamount)
        {
            
            // Team cherry pls. Spikes = acid... thanks
            if (hazardtype != (int) GlobalEnums.HazardType.SPIKES || !(laserTime <= 0.0) ||
                HeroController.instance.cState.invulnerable) return damageamount;
            
            log("Doing laser attack");
            laserTime = LASER_COOLDOWN;
            redwingSpawner.addLasers();
            StartCoroutine(freezeKnight(1f));
            
            return damageamount;
        }

        private redwing_game_objects redwingSpawner;
        private GameObject voidKnight;
        private GameObject sharpShadow;
        private PlayMakerFSM sharpShadowFsm;
        private double fbTime;
        private double laserTime;
        private double fsCharge;
        private double invulnTime;
        private const double FB_COOLDOWN = 3.0f;
        private const double LASER_COOLDOWN = 1.5f;
        private const double FS_RECHARGE = 10f;
        private const double IFRAMES = 2f;
        

        private void checkFireBalls()
        {
            float cooldown;
            try
            {
                // ReSharper disable once PossibleNullReferenceException because in try catch thing.
                cooldown = (float) instance.GetType()
                    .GetField("dashCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(instance);

            }
            catch (Exception e)
            {
                cooldown = 0.5f;
                log("Failed to set cooldown by reflection because " + e);
            }

            if (!(cooldown <= 0)) return;

            HeroActions direction = GameManager.instance.inputHandler.inputActions;
            if (direction.up.IsPressed && !direction.down.IsPressed && !direction.left.IsPressed && !direction.right.IsPressed)
            {
                if (!(fbTime <= 0.0)) return;
                spawnFireballs();
                
            } else
            {
                spawnFireTrail();
            }
        }
        
        public void Update()
        {
            //sceneTimer++;
            if (voidKnight == null)
            {
                voidKnight = GameObject.Find("Knight");
                redwingSpawner.voidKnight = voidKnight;
            }
            attackCooldownUpdate();
        }
        
        private void attackCooldownUpdate()
        {
            if (fsCharge > 0.0)
            {
                fsCharge -= Time.deltaTime;
            }
            
            if (laserTime > 0.0)
            {
                laserTime -= Time.deltaTime;
            }
            
            if (fbTime > 0.0)
            {
                fbTime -= Time.deltaTime;
            }

            if (invulnTime > 0.0)
            {
                invulnTime -= Time.deltaTime;
            }
        }
        
        private void spawnFireTrail()
        {
            log("I should be spawning a fire trail right now");
        }


        private void spawnFireballs()
        {
            redwingSpawner.addFireballs();
            fbTime = FB_COOLDOWN;
        }
        
        private IEnumerator freezeKnight(float freezeTime)
        {
            float realTimescale = Time.timeScale;
            Vector3 heroPostion = voidKnight.transform.position;
            while (freezeTime > 0.0f)
            {
                freezeTime -= Time.unscaledDeltaTime;
                Time.timeScale = 0.5f;
                HeroController.instance.current_velocity = Vector2.zero;
                voidKnight.transform.position = heroPostion;
                
                
                yield return null;
            }

            Time.timeScale = realTimescale;

        }
        
        private IEnumerator getHeroFsMs()
        {
            while (GameManager.instance == null)
                yield return new WaitForEndOfFrame();

            // ReSharper disable once InvertIf because idk it also looks dumb here.
            if (sharpShadow == null || !sharpShadow.CompareTag("Sharp Shadow"))
                foreach (GameObject ssGameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                {                    
                    if (ssGameObject == null || !ssGameObject.CompareTag("Sharp Shadow")) continue;
                    sharpShadow = ssGameObject;
                    sharpShadowFsm = FSMUtility.LocateFSM(sharpShadow, "damages_enemy");

                    log("Found sharpshadow");
                }
        }
        
        private GameObject fireSoul(GameObject go, Fsm fsm)
        {
            log("event sent. game object is: " + go.name + " and the fsm is: " + fsm.Name);
            if (go == sharpShadow)
            {
                PlayMakerFSM hm = FSMUtility.LocateFSM(fsm.GameObject, "health_manager") ?? FSMUtility.LocateFSM(fsm.GameObject, "health_manager_enemy");
                if (!Equals(hm, null))
                {
                    log("You hit an enemy with sharp shadow. Wow. Awesome...");
                }
            }
            return go;
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
        
    }
}