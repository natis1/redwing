using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Modding;
using RandomizerMod.Extensions;
using UnityEngine;
using static HeroController;
using Bounds = UnityEngine.Bounds;

namespace redwing
{
    public class redwing_hooks : MonoBehaviour
    {
        private void OnDestroy()
        {
            ModHooks.Instance.TakeDamageHook -= flameShieldAndLaser;
            ModHooks.Instance.DashVectorHook -= fireballsAndTrail;
            voidKnightSpellControl = null;
            
            if (flameShieldObj != null)
                Destroy(flameShieldObj);
        }
        
        public void Start()
        {
            StartCoroutine(getHeroFSMs());
            redwingSpawner = new redwing_game_objects();
            
            // wow all 4 primary effects in two neat little functions.
            ModHooks.Instance.TakeDamageHook += flameShieldAndLaser;
            ModHooks.Instance.DashVectorHook += fireballsAndTrail;
        }

        private Vector2 fireballsAndTrail(Vector2 change)
        {
            
            HeroActions direction = GameManager.instance.inputHandler.inputActions;
            if (direction.up.IsPressed && !direction.down.IsPressed)
            {
                if (fbTime <= 0.0)
                {
                    spawnFireballs();
                }
            }
            
            // ReSharper disable once InvertIf This looks really dumb
            if (ftTime <= 0.0)
            {
                currentTrailSprite = redwing_flame_gen.rng.Next(0, fireTrailTextures.Length - 1);
                netTrailDistance = 0;
                ftTime = HeroController.instance.SHADOW_DASH_COOLDOWN;
                spawnFireTrail(change);
            }
            
            return change;
        }
        


        private IEnumerator createFlameShield()
        {
            while (voidKnight == null)
            {
                yield return null;
            }

            
            
            flameShieldObj = new GameObject("redwingFlameShield", typeof(SpriteRenderer));
            currentFlameShieldTexture = 0;
            flameShieldSprite = flameShieldObj.GetComponent<SpriteRenderer>();
            flameShieldObj.transform.parent = voidKnight.transform;
            flameShieldObj.transform.localPosition = Vector3.zero;
            flameShieldObj.transform.localPosition = new Vector3(0f, -0.4f);
            flameShieldSprite.color = Color.white;
            flameShieldSprite.sprite = Sprite.Create(flameShieldTextures[currentFlameShieldTexture],
                new Rect(0, 0, redwing_flame_gen.FSHIELDTEXTURE_WIDTH, redwing_flame_gen.FSHIELDTEXTURE_HEIGHT),
                new Vector2(0.5f, 0.5f));
        }

        private int flameShieldAndLaser(ref int hazardtype, int damage)
        {
            if (hazardtype != (int) GlobalEnums.HazardType.SPIKES ||
                HeroController.instance.cState.invulnerable) return damage;
            
            if (invulnTime > 0.0)
            {
                return 0;
            }
            
            if (fsCharge <= 0.0 && damage > 0)
            {
                log("Shielding one damage");
                fsCharge = FS_RECHARGE;
                invulnTime = IFRAMES;
                damage--;
            }
            
            
            // ReSharper disable once InvertIf because patterns man
            if (laserTime <= 0.0)
            {
                log("Doing laser attack");
                laserTime = LASER_COOLDOWN;
                invulnTime = IFRAMES;
                justDidLaserAttack = true;
                redwingSpawner.addLasers();
                StartCoroutine(freezeKnight(0.3f));
                StartCoroutine(firinMaLaser());
            }

            return damage;
        }

        private redwing_game_objects redwingSpawner;
        private GameObject voidKnight;
        private GameObject sharpShadow;
        private PlayMakerFSM sharpShadowFsm;

        public static Texture2D[] flameShieldTextures;
        public static Texture2D[] fireTrailTextures;
        private GameObject flameShieldObj;
        private SpriteRenderer flameShieldSprite;
        private int currentFlameShieldTexture;

        private GameObject flamePillarDetect;
        private PlayMakerFSM voidKnightSpellControl;

        private bool justDidLaserAttack;
        private double ftTime = 0;
        private double fbTime = 0;
        private double laserTime = 0;
        private double fsCharge = 0;
        private double invulnTime = 0;
        private const double FB_COOLDOWN = 3.0f;
        private const double LASER_COOLDOWN = 1.5f;
        private const double FS_RECHARGE = 10f;
        private const double IFRAMES = 2f;

        private int currentTrailSprite = 0;
        private int netTrailDistance = 0;

        private const float FP_RANGE = 15f;

        private const int laserDamage = 35;

        private IEnumerable<Collider2D> allLaserEnemies;

        private const float FS_UPDATE_TIME = 0.2f;
        private float fsLastUpdate = 0f;
        
        //private readonly Rect = new Rect(50, 50, 100, 100);
        
        
        
        
        
        public void Update()
        {
            attackCooldownUpdate();
            
            if (flameShieldSprite != null)
                flameShieldUpdate();
            
        }

        private void flameShieldUpdate()
        {
            fsLastUpdate += Time.deltaTime;
            Color c = flameShieldSprite.color;

            float alpha;
            if (fsCharge <= 0.0)
            {
                alpha = 1.0f;
            }
            else
            {
                alpha = (float)( 0.5 * (FS_RECHARGE - fsCharge) / FS_RECHARGE );
            }

            c.a = alpha;
            
            flameShieldSprite.color = c;
            
            if (!(fsLastUpdate > FS_UPDATE_TIME)) return;
            
            fsLastUpdate = 0f;
            currentFlameShieldTexture++;
            if (currentFlameShieldTexture >= flameShieldTextures.Length)
            {
                currentFlameShieldTexture = 0;
            }
                
            flameShieldSprite.sprite = Sprite.Create(flameShieldTextures[currentFlameShieldTexture],
                new Rect(0, 0, redwing_flame_gen.FSHIELDTEXTURE_WIDTH, redwing_flame_gen.FSHIELDTEXTURE_HEIGHT),
                new Vector2(0.5f, 0.5f));
            

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

            if (ftTime > 0.0)
            {
                ftTime -= Time.deltaTime;
            }
        }

        private void spawnFireTrail(Vector2 delta)
        {
            float angle = (float) redwing_flame_gen.getNearestAngel((int) (delta.x * 100), (int) (delta.y * 100), 0, 0);

            GameObject trail = new GameObject("redwingFireTrail", typeof(redwing_trail_behavior),
                typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(DebugColliders));

            GameObject trailMemeSpawner = new GameObject("redwingFireTrailSpawner");
            trailMemeSpawner.transform.localPosition = voidKnight.transform.position;

            trail.transform.parent = trailMemeSpawner.transform;
            trail.transform.localPosition = Vector3.zero;
            trail.transform.Rotate(0f, 0f, angle);
            Rigidbody2D physics = trail.GetComponent<Rigidbody2D>();
            physics.isKinematic = true;
            BoxCollider2D detect = trail.GetComponent<BoxCollider2D>();
            detect.isTrigger = true;

            redwing_trail_behavior meme = trail.GetComponent<redwing_trail_behavior>();

            meme.dashVector = delta;
            meme.drawEm = trail.GetComponent<SpriteRenderer>();
            meme.spriteUsed = fireTrailTextures[currentTrailSprite];
            
            trailMemeSpawner.SetActive(true);
            trail.SetActive(true);
        } 

        private void spawnFireballs()
        {
            redwingSpawner.addFireballs();
            fbTime = FB_COOLDOWN;
        }
        
        private IEnumerator freezeKnight(float freezeTime)
        {
            //float realTimescale = Time.timeScale;
            Vector3 heroPostion = voidKnight.transform.position;
            while (freezeTime > 0.0f)
            {
                freezeTime -= Time.unscaledDeltaTime;
                //Time.timeScale = 0.5f;
                HeroController.instance.current_velocity = Vector2.zero;
                voidKnight.transform.position = heroPostion;
                yield return null;
            }

            //Time.timeScale = realTimescale;

        }

        private IEnumerator firinMaLaser()
        {
            allLaserEnemies = new List<Collider2D>();
            const float waitTime = 0.2f;
            yield return new WaitForSecondsRealtime(waitTime);

            for (int i = 0; i < 16; i++)
            {
                allLaserEnemies = allLaserEnemies.Union(redwingSpawner.lasers[i].GetComponent<redwing_laser_behavior>()
                    .enteredColliders);
            }
            
            foreach (Collider2D collider in allLaserEnemies)
            {
                
                GameObject target = collider.gameObject;
                log("Doing laser damage to target with name " + target.name);
                
                FSMUtility.SendEventToGameObject(target, "TAKE DAMAGE", false);
                HitTaker.Hit(target, new HitInstance
                {
                    Source = base.gameObject,
                    AttackType = AttackTypes.Generic,
                    CircleDirection = false,
                    DamageDealt = laserDamage,
                    Direction = 0f,
                    IgnoreInvulnerable = true,
                    MagnitudeMultiplier = 1f,
                    MoveAngle = 0f,
                    MoveDirection = false,
                    Multiplier = 1f,
                    SpecialType = SpecialTypes.None,
                    IsExtraDamage = false
                }, 3);
            }

        }
        
        private IEnumerator getHeroFSMs()
        {
            while (GameManager.instance == null || HeroController.instance == null)
                yield return null;
            
            voidKnight = GameObject.Find("Knight");
            redwingSpawner.voidKnight = voidKnight;
            
            
            voidKnightSpellControl = FSMUtility.LocateFSM(voidKnight, "ProxyFSM");
            
            // ReSharper disable once InvertIf because idk it also looks dumb here.
            if (sharpShadow == null || !sharpShadow.CompareTag("Sharp Shadow"))
                foreach (GameObject ssGameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                {                    
                    if (ssGameObject == null || !ssGameObject.CompareTag("Sharp Shadow")) continue;
                    sharpShadow = ssGameObject;
                    sharpShadowFsm = FSMUtility.LocateFSM(sharpShadow, "damages_enemy");

                    log("Found sharpshadow");
                }
            
            StartCoroutine(setupFlamePillar());
            StartCoroutine(createFlameShield());
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

        private IEnumerator setupFlamePillar()
        {
            log("start flame pillar setup");
            while (voidKnightSpellControl == null)
            {
                yield return null;
            }
            
            // lul... grimm enemy range.
            flamePillarDetect = new GameObject("redwingFlamePillarDetect",
                typeof(redwing_pillar_detect_behavior), typeof(Rigidbody2D), typeof(CircleCollider2D));
            flamePillarDetect.transform.parent = voidKnight.transform;
            flamePillarDetect.transform.localPosition = Vector3.zero;
            
            
            CircleCollider2D fpRangeCollide = flamePillarDetect.GetComponent<CircleCollider2D>();
            Bounds bounds = fpRangeCollide.bounds;
            bounds.center = flamePillarDetect.transform.position;
            fpRangeCollide.isTrigger = true;
            fpRangeCollide.radius = FP_RANGE;

            log("detect enemy hitbox centered around " + bounds.center.x + ", " + bounds.center.y + " with radius: " +
                fpRangeCollide.radius);
            

            Rigidbody2D fpFakePhysics = flamePillarDetect.GetComponent<Rigidbody2D>();
            fpFakePhysics.isKinematic = true;
            
            
            
            
            
            //GrimmEnemyRange fpGrimmRange = flamePillarDetect.GetComponent<GrimmEnemyRange>();
            //fpGrimmRange
            
            CallMethod firePillarOnRecover = new CallMethod();
            try
            {
                firePillarOnRecover.behaviour = flamePillarDetect.GetComponent<redwing_pillar_detect_behavior>();
                firePillarOnRecover.methodName = "spawnFirePillar";
                firePillarOnRecover.parameters = new FsmVar[0];
                firePillarOnRecover.everyFrame = false;
                
                voidKnightSpellControl.getState("Focus Completed").addAction(firePillarOnRecover);
            } catch (Exception e)
            {
                log("Unable to add method: error " + e);
            }
            log("got to end of FP method");
        }
        
        
        
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
        
    }
}