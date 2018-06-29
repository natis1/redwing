using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Modding;
using UnityEngine;
using Bounds = UnityEngine.Bounds;

namespace redwing
{
    public class redwing_hooks : MonoBehaviour
    {
        private void OnDestroy()
        {
            ModHooks.Instance.TakeDamageHook -= flameShieldAndLaser;
            ModHooks.Instance.DashVectorHook -= fireballsAndTrail;
            ModHooks.Instance.DashPressedHook -= setTrailCooldown;
            ModHooks.Instance.SlashHitHook -= reduceFSCooldown;
                        
            if (overrideBlackmothNailDmg)
            {
                ModHooks.Instance.HitInstanceHook -= overrideBlackmothDamage;
            } else if (balancedMode)
            {
                ModHooks.Instance.HitInstanceHook -= overrideAllNonFireDamage;
                ModHooks.Instance.GetPlayerBoolHook -= greatSlashFromStart;
            }
            
            ModHooks.Instance.BeforeSavegameSaveHook -= restoreCharmCost;
            ModHooks.Instance.SavegameSaveHook -= ruinCharmCost;
            
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
            ModHooks.Instance.DashPressedHook += setTrailCooldown;
            ModHooks.Instance.SlashHitHook += reduceFSCooldown;

            if (overrideBlackmothNailDmg)
            {
                ModHooks.Instance.HitInstanceHook += overrideBlackmothDamage;
            } else if (balancedMode)
            {
                ModHooks.Instance.HitInstanceHook += overrideAllNonFireDamage;
                ModHooks.Instance.GetPlayerBoolHook += greatSlashFromStart;
            }
            
            ModHooks.Instance.BeforeSavegameSaveHook += restoreCharmCost;
            ModHooks.Instance.SavegameSaveHook += ruinCharmCost;
            
            ruinCharmCost(0);
        }

        private bool greatSlashFromStart(string originalset)
        {
            return originalset == "hasDashSlash" || PlayerData.instance.GetBoolInternal(originalset);
        }

        private void ruinCharmCost(int id)
        {
            PlayerData.instance.charmCost_40 = nailmasterGloryNotchCost;
        }

        private void restoreCharmCost(SaveGameData data)
        {
            
            PlayerData.instance.charmCost_26 = 1;
            
        }


        private void reduceFSCooldown(Collider2D othercollider, GameObject gameobject)
        {
            if (fsCharge >= 0.0 && othercollider.gameObject.IsGameEnemy())
            {
                fsCharge -= fsReduceOnHit;
            }
        }

        private void setTrailCooldown()
        {
            if (ftTime <= 0.0)
            {
                useFT = true;
            }
            ftTime = HeroController.instance.SHADOW_DASH_COOLDOWN;
        }

        private Vector2 fireballsAndTrail(Vector2 change)
        {
            if (blackmothSymbolsExist)
            {
                // Why does this work but not just checking it directly?
                if (blackmothGrubberCheck())
                {
                    return change;
                }
            }
            
            HeroActions direction = GameManager.instance.inputHandler.inputActions;
            if (direction.up.IsPressed && !direction.down.IsPressed && change.y > 0.00001f)
            {
                if (fbTime <= 0.0)
                {
                    spawnFireballs();
                    
                    // Stop fireballs from spawning mid dash.
                } else if (fbTime <= 0.2)
                {
                    fbTime = 0.2;
                }
            }
            
            // ReSharper disable once InvertIf This looks really dumb
            if (useFT)
            {
                currentTrailSprite = redwing_flame_gen.rng.Next(0, fireTrailTextures.Length - 1);
                netTrailDistance = 0;
                spawnFireTrail(change);
                useFT = false;
            }
            
            return change;
        }

        private bool blackmothGrubberCheck()
        {
            return BlackmothMod.Blackmoth.Instance.grubberOn;
        }


        private void createFlameShield()
        {
            
            flameShieldObj = new GameObject("redwingFlameShield", typeof(SpriteRenderer), typeof(AudioSource));
            currentFlameShieldTexture = 0;
            flameShieldSprite = flameShieldObj.GetComponent<SpriteRenderer>();
            flameShieldObj.transform.parent = voidKnight.transform;
            flameShieldObj.transform.localPosition = Vector3.zero;
            flameShieldObj.transform.localPosition = new Vector3(0f, -0.4f);
            flameShieldSprite.color = Color.white;
            flameShieldSprite.sprite = Sprite.Create(flameShieldTextures[currentFlameShieldTexture],
                new Rect(0, 0, redwing_flame_gen.FSHIELDTEXTURE_WIDTH, redwing_flame_gen.FSHIELDTEXTURE_HEIGHT),
                new Vector2(0.5f, 0.5f));

            flameShieldAudio = flameShieldObj.GetComponent<AudioSource>();
            //flameShieldAudio.clip = shieldSoundEffect;
            flameShieldAudio.loop = false;
        }

        private int flameShieldAndLaser(ref int hazardtype, int damage)
        {
            
            
            if (hazardtype != (int) GlobalEnums.HazardType.SPIKES ||
                HeroController.instance.cState.invulnerable) return damage;
            
            if (invulnTime > 0.0)
            {
                return 0;
            }
            
            // if you have grubberfly set damage to 0 always
            if (blackmothSymbolsExist)
            {
                if (blackmothGrubberCheck())
                {
                    return 0;
                }
            }
            
            if (fsCharge <= 0.0 && damage > 0)
            {
                log("Shielding one damage");
                fsCharge = fsRecharge;
                invulnTime = IFRAMES;
                flameShieldAudio.Stop();
                flameShieldAudio.volume = (GameManager.instance.gameSettings.masterVolume *
                                           GameManager.instance.gameSettings.soundVolume * 0.01f);
                flameShieldAudio.clip = shieldDischargeSoundEffect;
                flameShieldAudio.Play();
                playFSSound = true;
                damage--;
            }
            
            
            // ReSharper disable once InvertIf because patterns man
            if (laserTime <= 0.0 && (zeroDmgLaser || damage > 0))
            {
                log("Doing laser attack");
                laserTime = laserCooldown;
                invulnTime = IFRAMES;
                justDidLaserAttack = true;
                redwingSpawner.addLasers();
                StartCoroutine(freezeKnight(0.3f));
                StartCoroutine(firinMaLaser());
            }
            
            invulnTime = IFRAMES;

            return damage;
        }

        private redwing_game_objects redwingSpawner;
        private GameObject voidKnight;
        private GameObject sharpShadow;
        private PlayMakerFSM sharpShadowFsm;

        public static Texture2D[] flameShieldTextures;
        public static Texture2D[] fireTrailTextures;
        public static Texture2D[] fireTrailCaps;
        public static Texture2D[] reverseFireTrailCaps;
        public static AudioClip shieldSoundEffect;
        public static AudioClip fireTrailSoundEffect;
        public static AudioClip shieldChargeSoundEffect;
        public static AudioClip shieldDischargeSoundEffect;

        public static bool overrideBlackmothNailDmg;
        public static bool balancedMode;
        
        // True if the Blackmoth DLL is there and is of a high enough version to work with Redwing
        // If true you can directly use Blackmoth instead of through reflection which is a lot faster.
        public static bool blackmothSymbolsExist;
            
        private GameObject flameShieldObj;
        private SpriteRenderer flameShieldSprite;
        private AudioSource flameShieldAudio;
        private int currentFlameShieldTexture;

        private GameObject flamePillarDetect;
        private PlayMakerFSM voidKnightSpellControl;
        private PlayMakerFSM voidKnightNailArts;

        private bool justDidLaserAttack;
        private double ftTime = 0;
        private double fbTime = 0;
        private double laserTime = 0;
        private double fsCharge = 0;
        private double invulnTime = 0;
        
        
        public static double fbCooldown;
        public static double laserCooldown;
        public static double fsRecharge;
        public static double fsReduceOnHit;
        public static bool zeroDmgLaser;
        
        
        private const double IFRAMES = 0.8f;

        private int currentTrailSprite = 0;
        private int netTrailDistance = 0;

        private const float FP_RANGE = 15f;

        public static int laserDamageBase;
        public static int laserDamagePerNail;
        public static int nailmasterGloryNotchCost;

        private IEnumerable<Collider2D> allLaserEnemies;

        private const float FS_UPDATE_TIME = 0.2f;
        private float fsLastUpdate = 0f;
        private bool useFT = false;
        private bool playFSSound;
        
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
                if (playFSSound && UIManager.instance.uiState == UIState.PLAYING)
                {                    
                    flameShieldAudio.Stop();
                    flameShieldAudio.volume = (GameManager.instance.gameSettings.masterVolume *
                                               GameManager.instance.gameSettings.soundVolume * 0.01f);
                    // make sound less annoying before you play it or maybe make it not loop
                    flameShieldAudio.clip = shieldChargeSoundEffect;
                    flameShieldAudio.Play();
                    playFSSound = false;
                } else if (playFSSound == false && UIManager.instance.uiState != UIState.PLAYING)
                {
                    //flameShieldAudio.Stop();
                    //playFSSound = true;
                }
            }
            else
            {
                alpha = (float)( 0.5 * (fsRecharge - fsCharge) / fsRecharge );
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
            float angle = (float) redwing_flame_gen.getNearestAngel((int) (delta.x * 10000), (int) (delta.y * 10000), 0, 0);

            GameObject trail = new GameObject("redwingFireTrail", typeof(redwing_trail_behavior),
                typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(MeshFilter), typeof(MeshRenderer),
                typeof(AudioSource));

            GameObject trailMemeSpawner = new GameObject("redwingFireTrailSpawner");
            Vector3 voidKnightPos = voidKnight.transform.position;
            voidKnightPos.y -= 0.4f;
            trailMemeSpawner.transform.localPosition = voidKnightPos;

            trail.transform.parent = trailMemeSpawner.transform;
            trail.transform.localPosition = Vector3.zero;
            trail.transform.Rotate(0f, 0f, angle);
            Rigidbody2D physics = trail.GetComponent<Rigidbody2D>();
            physics.isKinematic = true;
            BoxCollider2D detect = trail.GetComponent<BoxCollider2D>();
            detect.isTrigger = true;

            redwing_trail_behavior meme = trail.GetComponent<redwing_trail_behavior>();

            
            
            meme.memeFilter = trail.GetComponent<MeshFilter>();
            meme.memeRenderer = trail.GetComponent<MeshRenderer>();
            meme.memeTextureUsed = fireTrailTextures[currentTrailSprite];
            meme.voidKnightCollider = voidKnight.GetComponent<BoxCollider2D>();
            meme.cachedAudio = trail.GetComponent<AudioSource>();
            meme.cachedAudio.clip = fireTrailSoundEffect;
            
            
            trailMemeSpawner.SetActive(true);
            trail.SetActive(true);
        } 

        private void spawnFireballs()
        {
            redwingSpawner.addFireballs();
            fbTime = fbCooldown;
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
                
                
                redwing_game_objects.applyHitInstance(collider.gameObject,
                    laserDamageBase + laserDamagePerNail * PlayerData.instance.GetInt("nailSmithUpgrades"),
                    AttackTypes.Generic, voidKnight);
            }

        }
        
        private IEnumerator getHeroFSMs()
        {
            while (GameManager.instance == null || HeroController.instance == null)
                yield return null;
            
            voidKnight = GameObject.Find("Knight");
            redwing_game_objects.voidKnight = voidKnight;
            
            
            voidKnightSpellControl = FSMUtility.LocateFSM(voidKnight, "ProxyFSM");
            voidKnightNailArts = FSMUtility.LocateFSM(voidKnight, "Nail Arts");
            
            setupFlamePillar();
            setupNailArtFireballs();
            createFlameShield();
        }

        private void setupNailArtFireballs()
        {
            try
            {
                addAction(voidKnightNailArts, "G Slash", new CallMethod
                {
                    behaviour = GameManager.instance.gameObject.GetComponent<redwing_hooks>(),
                    methodName = "greatSlashFireballs",
                    parameters = new FsmVar[0],
                    everyFrame = false
                }
                );
                log("Added fireballs to great slash.");
            }
            catch (Exception e)
            {
                log("Unable to add fireball method to greatslash " + e);
            }
            
            try
            {
                addAction(voidKnightNailArts, "Cyclone Spin", new CallMethod
                    {
                        behaviour = GameManager.instance.gameObject.GetComponent<redwing_hooks>(),
                        methodName = "cycloneSlashFireballs",
                        parameters = new FsmVar[0],
                        everyFrame = false
                    }
                );
                
                log("Added fireballs to cyclone slash.");
            }
            catch (Exception e)
            {
                log("Unable to add fireball method to cyclone " + e);
            }
            
            try
            {
                addAction(voidKnightNailArts, "Dash Slash", new CallMethod
                    {
                        behaviour = GameManager.instance.gameObject.GetComponent<redwing_hooks>(),
                        methodName = "dashSlashFireballs",
                        parameters = new FsmVar[0],
                        everyFrame = false
                    }
                );
                
                log("Added fireballs to dash slash.");
            }
            catch (Exception e)
            {
                log("Unable to add fireball method to the slash nobody uses " + e);
            }
            
            
        }

        private void setupFlamePillar()
        {            
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

            Rigidbody2D fpFakePhysics = flamePillarDetect.GetComponent<Rigidbody2D>();
            fpFakePhysics.isKinematic = true;
            
            try
            {
                CallMethod firePillarOnRecover = new CallMethod
                {
                    behaviour = flamePillarDetect.GetComponent<redwing_pillar_detect_behavior>(),
                    methodName = "spawnFirePillar",
                    parameters = new FsmVar[0],
                    everyFrame = false
                };
                
                addAction(voidKnightSpellControl, "Focus Completed", firePillarOnRecover);
            } catch (Exception e)
            {
                log("Unable to add method: error " + e);
            }
            log("got to end of FP method");
        }
        
        // ReSharper disable once UnusedMember.Global because used implicitly
        public void greatSlashFireballs()
        {

            float[] yVelo = {14f, 11f, 8f, 5f};
            float[] yTrans = {0.5f, 0.5f, 0.5f, 0.5f};
            float[] xVelo = {5f, 8f, 11f, 14f};
            float[] xTrans = {1f, 1f, 1f, 1f};

            bool right = HeroController.instance.cState.facingRight;
            bool fakeGreatslash = !PlayerData.instance.GetBoolInternal("hasDashSlash");
            
            for (int i = 0; i < 4; i++)
            {
                if (right)
                {
                    redwing_game_objects.addSingleFireball(xVelo[i], yVelo[i], xTrans[i], yTrans[i], "s" + i);
                }
                else
                {
                    redwing_game_objects.addSingleFireball(-xVelo[i], yVelo[i], -xTrans[i], yTrans[i], "s" + i);
                }

                if (fakeGreatslash)
                {
                    return;
                }
            }

        }

        // ReSharper disable once UnusedMember.Global because used implicitly
        public void cycloneSlashFireballs()
        {
            log("did cyclone slash");
            StartCoroutine(randomCycloneBalls());
            redwing_game_objects.addSinglePillar(HeroController.instance.INVUL_TIME_CYCLONE);
        }

        // ReSharper disable once UnusedMember.Global because used implicitly
        public void dashSlashFireballs()
        {
            log("did dash slash");
            
            float[] yVelo = {17f, 15f, 13f, 10f};
            float[] yTrans = {0.5f, 0.5f, 0.5f, 0.5f};
            float[] xVelo = {2f, 4f, 6f, 8f};
            float[] xTrans = {1f, 1f, 1f, 1f};

            bool right = HeroController.instance.cState.facingRight;
            for (int i = 0; i < 4; i++)
            {
                if (right)
                {
                    redwing_game_objects.addSingleFireball(xVelo[i], yVelo[i], xTrans[i], yTrans[i], "s" + i);
                }
                else
                {
                    redwing_game_objects.addSingleFireball(-xVelo[i], yVelo[i], -xTrans[i], yTrans[i], "s" + i);
                }
            }

            redwing_game_objects.addSingleLaser(right ? 270 : 90);
        }

        private IEnumerator randomCycloneBalls()
        {
            const float timeBetweenBalls = 0.08f;
            float timePassed = 0f;
            float maxTime = HeroController.instance.INVUL_TIME_CYCLONE;
            log("max time is " + maxTime);
            while (timePassed < maxTime)
            {
                float xVelo = (float) ((redwing_flame_gen.rng.NextDouble() - 0.5) * 12.0);
                float yVelo = (float) ((redwing_flame_gen.rng.NextDouble() + 0.5) * 11.0);
                float xTrans = 0f;
                float yTrans = 1.0f;

                if (xVelo < 2.0f && xVelo > 0f)
                {
                    xVelo += 1.5f;
                    xVelo *= 3f;
                } else if (xVelo <= 0f && xVelo > -2.0f)
                {
                    xVelo -= 1.5f;
                    xVelo *= 3f;
                }
                if (xVelo < 3.0f && xVelo > -3.0f)
                {
                    xVelo *= 1.5f;
                }
                
                redwing_game_objects.addSingleFireball(xVelo, yVelo, xTrans, yTrans, "s");
                
                yield return new WaitForSeconds(timeBetweenBalls);
                timePassed += timeBetweenBalls;
            }
            
        }
        
        private HitInstance overrideAllNonFireDamage(Fsm owner, HitInstance hit)
        {
            hit.DamageDealt = 1;
            return hit;
        }
        
        private HitInstance overrideBlackmothDamage(Fsm hitter, HitInstance hit)
        {
            
            if (!hitter.GameObject.name.Contains("Slash")) return hit;
            
            
            int nailDamage = 5 + PlayerData.instance.GetInt("nailSmithUpgrades") * 4;
            double multiplier = 1;
            float fsmMultiplier = 1;
            if (PlayerData.instance.GetBool("equippedCharm_25"))
            {
                multiplier *= 1.5;
            }
            if (PlayerData.instance.GetBool("equippedCharm_6") && PlayerData.instance.GetInt("health") == 1)
            {
                fsmMultiplier = 1.75f;
            }

            if (hitter.GameObject.name.Contains("Great"))
            {
                multiplier = 2.5 * fsmMultiplier;
                fsmMultiplier = 1;
            }

            if (hitter.GameObject.name.Contains("Dash"))
            {
                multiplier = 2.5;
                fsmMultiplier = 1;
            }
            nailDamage = (int) Math.Round( (double)nailDamage * multiplier);
            log("game wants to do " + hit.DamageDealt + " dmg with multiplier " + hit.Multiplier);
            hit.DamageDealt = nailDamage;
            hit.Multiplier = fsmMultiplier;
            
            
            log("damage dealt is  " + nailDamage + " dmg with multiplier " + fsmMultiplier);
                
            log("running override for hitter of name " + hitter.GameObject.name);
            
            return hit;
        }
        
        private static void addAction(PlayMakerFSM fsm, string stateName, FsmStateAction action)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                actions[actions.Length - 1] = action;

                t.Actions = actions;
            }
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
        
    }
}