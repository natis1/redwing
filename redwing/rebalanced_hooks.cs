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
    public class rebalanced_hooks : MonoBehaviour
    {
        private GameObject voidKnight;
        private PlayMakerFSM voidKnightSpellControl;
        private PlayMakerFSM voidKnightNailArts;

        private bool hasGreatSlash;
        private bool hasNailArt;
        
        
        //private AudioSource flameShieldAudio;
        private GameObject flameShieldObj;
        private flame_shield_control flameShield;
        private flame_gauge flameGaugeControl;
        
        private GameObject flamePillarDetect;
        private const float IFRAMES = 2.5f;
        private bool invulnerable = false;
        private const float FP_RANGE = 15f;
        private const double CYCLONE_COOLDOWN = 0.22;
        private bool canFireballs = true;
        private bool canFireTrail = true;
        private int currentTrailSprite = 0;
        public float flamePowerGainRate = 0.09f;

        public float flamePower = 1.0f;
        private float flamePowerCap;
        private IEnumerable<Collider2D> allLaserEnemies;
        
        private readonly redwing_game_objects redwingSpawner = new redwing_game_objects();
        
        private void Start()
        {
            hasGreatSlash = PlayerData.instance.GetBool("hasDashSlash");
            hasNailArt = PlayerData.instance.GetBool("hasNailArt");
            flamePowerCap = 1.0f + 0.34f * (PlayerData.instance.vesselFragments / 3);
            if (flamePowerCap > 2.0f)
            {
                flamePowerCap = 2.0f;
            }
            
            StartCoroutine(getHeroFSMs());
            
            ModHooks.Instance.TakeDamageHook += flameShieldAndLaser;
            ModHooks.Instance.DashVectorHook += fireballsAndTrail;
            
            if (redwing_hooks.overrideBlackmothNailDmg)
            {
                ModHooks.Instance.HitInstanceHook += overrideBlackmothDamage;
            }
            ModHooks.Instance.BeforeSavegameSaveHook += restoreCharmCost;
            ModHooks.Instance.SavegameSaveHook += ruinCharmCost;
            ModHooks.Instance.FocusCostHook += focusDisabler;
            On.HealthManager.TakeDamage += addNapalm;
            ruinCharmCost(0);
        }

        private void OnDestroy()
        {
            ModHooks.Instance.TakeDamageHook -= flameShieldAndLaser;
            ModHooks.Instance.DashVectorHook -= fireballsAndTrail;
            
            ModHooks.Instance.BeforeSavegameSaveHook -= restoreCharmCost;
            ModHooks.Instance.SavegameSaveHook -= ruinCharmCost;

            ModHooks.Instance.FocusCostHook -= focusDisabler;
            try
            {
                On.HealthManager.TakeDamage -= addNapalm;
            }
            catch (Exception e)
            {
                log("meme " + e);
            }

        }
        
        private void addNapalm(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
        {
            if ((hitinstance.DamageDealt * hitinstance.Multiplier) >= (self.hp))
            {
                orig(self, hitinstance);
                return;
            }
            
            if (hitinstance.Source.name.Contains("Fireball2"))
            {
                redwing_game_objects.addNapalm(self.gameObject,
                    PlayerData.instance.GetBool("equippedCharm_19") ? 30.0 : 24.0, Color.red);
            } else if (hitinstance.Source.name.Contains("Fireball"))
            {
                redwing_game_objects.addNapalm(self.gameObject,
                    PlayerData.instance.GetBool("equippedCharm_19") ? 20.0 : 15.0, Color.white);
            } else if (hitinstance.Source.name.Contains("Q Fall Damage"))
            {
                redwing_game_objects.addNapalm(self.gameObject,
                    PlayerData.instance.GetBool("equippedCharm_19") ? 20.0 : 15.0, Color.white);
            }
            
            orig(self, hitinstance);
        }

        private float focusDisabler()
        {
            if (!(flamePower < 0.34f)) return 1f;
            if (!(flameShield.fsCharge <= 0.0)) return 1000f;
            reclaimFlamePower();
            return 1f;

        }

        private void ruinCharmCost(int id)
        {
            PlayerData.instance.charmCost_26 = redwing_hooks.nailmasterGloryNotchCost;
        }

        private void restoreCharmCost(SaveGameData data)
        {
            flamePowerCap = 1.0f + 0.34f * (PlayerData.instance.vesselFragments / 3);
            if (flamePowerCap > 2.0f)
            {
                flamePowerCap = 2.0f;
            }
            
            PlayerData.instance.charmCost_26 = 1;
        }

        private Vector2 fireballsAndTrail(Vector2 change)
        {
            if (redwing_hooks.blackmothSymbolsExist)
            {
                // Why does this work but not just checking it directly?
                if (redwing_hooks.blackmothGrubberCheck())
                {
                    return change;
                }
            }

            HeroActions direction = GameManager.instance.inputHandler.inputActions;
            if (direction.up.IsPressed && !direction.down.IsPressed && change.y > 0.00001f)
            {
                if (flamePower > 1.0f && canFireballs)
                {
                    flamePower -= 1.0f;
                    StartCoroutine(noMoreFireballs());
                    redwingSpawner.addFireballs();                    
                }
            }
            
            // ReSharper disable once InvertIf This looks really dumb
            if (canFireTrail)
            {
                currentTrailSprite = redwing_flame_gen.rng.Next(0, redwing_hooks.fireTrailTextures.Length);
                StartCoroutine(noMoreTrails());
            }


            return change;
        }

        private IEnumerator noMoreTrails()
        {
            canFireTrail = false;
            yield return new WaitForSeconds(1.5f);
            canFireTrail = true;
        }

        private IEnumerator noMoreFireballs()
        {
            canFireballs = false;
            yield return new WaitForSeconds(1.5f);
            canFireballs = true;
        }

        private int flameShieldAndLaser(ref int hazardType, int damage)
        {
            if (!canTakeDamage(hazardType)) return damage;
            if (invulnerable)
            {
                return 0;
            }
            // if you have grubberfly set damage to 0 always. Fixes blackmoth bug
            if (redwing_hooks.blackmothSymbolsExist)
            {
                if (redwing_hooks.blackmothGrubberCheck())
                {
                    return 0;
                }
            }
            
            if (flameShield.fsCharge <= 0.0)
            {
                log("Shielding one damage");
                flameShield.discharge();
                damage--;
            }

            if (damage > 0 && flamePower >= 0.5f)
            {
                flamePower -= 0.5f;
                redwingSpawner.addLasers();
                StartCoroutine(freezeKnight(0.3f));
                StartCoroutine(firinMaLaser());
            }
            else
            {
                flamePower += 0.3f;
                log("Sorry you were hit. Here's some fire and fury to inspire you to do better next time!");
            }
            

            StartCoroutine(invulnerableFrames());
            return damage;
        }


        private void Update()
        {
            HeroController.instance.SetMPCharge(99);
            flamePower += flamePowerGainRate * Time.deltaTime;
            
            if (flamePower > 0.5f && !flameShield.canCharge)
            {
                flameShield.enableCharge();
                flamePower -= 0.5f;
            }

            if (flamePower < 0)
                flamePower = 0;

            if (flamePower > flamePowerCap)
                flamePower = flamePowerCap;

            // drawflamepower function
        }

        private void reclaimFlamePower()
        {
            if (!flameShield.canCharge) return;
            flamePower += 0.5f;
            flameShield.disableCharge();
        }

        private IEnumerator invulnerableFrames()
        {
            invulnerable = true;
            yield return new WaitForSeconds(IFRAMES);
            invulnerable = false;
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
            setupFlameGauge();
        }

        private void setupFlameGauge()
        {
            
            GameObject flameGaugeDisplay = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            GameObject flameGauge = CanvasUtil.CreateImagePanel(flameGaugeDisplay,
                Sprite.Create(load_textures.SOUL_HOLDER[0],
                    new Rect(0, 0, load_textures.SOUL_HOLDER[0].width, load_textures.SOUL_HOLDER[0].height),
                    new Vector2(0.5f, 0.5f)), new CanvasUtil.RectData(
                    new Vector2(0, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 0),
                    new Vector2(0.05f, 0.05f)));
            flameGaugeControl = flameGauge.AddComponent<flame_gauge>();
            GameCameras.instance.hudCanvas.PrintSceneHierarchyTree("hud.txt");
        }

        private void setupNailArtFireballs()
        {
            try
            {
                addAction(voidKnightNailArts, "G Slash", new CallMethod
                {
                    behaviour = GameManager.instance.gameObject.GetComponent<rebalanced_hooks>(),
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
                        behaviour = GameManager.instance.gameObject.GetComponent<rebalanced_hooks>(),
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
                        behaviour = GameManager.instance.gameObject.GetComponent<rebalanced_hooks>(),
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
        
        private void createFlameShield()
        {
            flameShieldObj = new GameObject("redwingFlameShield", typeof(SpriteRenderer), typeof(AudioSource), typeof(flame_shield_control));
            flameShield = flameShieldObj.GetComponent<flame_shield_control>();
            flameShieldObj.transform.parent = voidKnight.transform;
            flameShieldObj.transform.localPosition = Vector3.zero;
            flameShieldObj.transform.localPosition = new Vector3(0f, -0.4f);
        }
        
        // ReSharper disable once UnusedMember.Global because used implicitly
        public void greatSlashFireballs()
        {
            if (flamePower < 0.3)
            {
                return;
            }

            flamePower -= 0.3f;
            float[] yVelo = {8f, 11f, 14f, 5f};
            float[] yTrans = {0.5f, 0.5f, 0.5f, 0.5f};
            float[] xVelo = {11f, 8f, 5f, 14f};
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

                if (!hasGreatSlash)
                {
                    return;
                }
            }

        }

        // ReSharper disable once UnusedMember.Global because used implicitly
        public void cycloneSlashFireballs()
        {

            if (flamePower < 0.3f)
            {
                return;
            }

            flamePower -= 0.3f;
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
            //redwing_game_objects.addSinglePillar(HeroController.instance.INVUL_TIME_CYCLONE);
        }

        // ReSharper disable once UnusedMember.Global because used implicitly
        public void dashSlashFireballs()
        {
            //log("did dash slash");
            if (flamePower < 0.3)
            {
                return;
            }

            flamePower -= 0.3f;
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
            meme.memeTextureUsed = redwing_hooks.fireTrailTextures[currentTrailSprite];
            meme.voidKnightCollider = voidKnight.GetComponent<BoxCollider2D>();
            meme.cachedAudio = trail.GetComponent<AudioSource>();
            meme.cachedAudio.clip = redwing_hooks.fireTrailSoundEffect;
            
            
            trailMemeSpawner.SetActive(true);
            trail.SetActive(true);
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
                    redwing_hooks.laserDamageBase +
                    redwing_hooks.laserDamagePerNail * PlayerData.instance.GetInt("nailSmithUpgrades"), voidKnight, 0.2f);
            }

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
            //log("game wants to do " + hit.DamageDealt + " dmg with multiplier " + hit.Multiplier);
            hit.DamageDealt = nailDamage;
            hit.Multiplier = fsmMultiplier;
            
            
            //log("damage dealt is  " + nailDamage + " dmg with multiplier " + fsmMultiplier);
                
            //log("running override for hitter of name " + hitter.GameObject.name);
            
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
        
        private static bool canTakeDamage(int hazardType)
        {
            return (HeroController.instance.damageMode != DamageMode.NO_DAMAGE &&
                    HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION &&
                    (!HeroController.instance.cState.invulnerable && !HeroController.instance.cState.recoiling) &&
                    (!HeroController.instance.playerData.isInvincible && !HeroController.instance.cState.dead &&
                     (!HeroController.instance.cState.hazardDeath && !BossSceneController.IsTransitioning)) &&
                    (HeroController.instance.damageMode != DamageMode.HAZARD_ONLY || hazardType != 1) &&
                    (!HeroController.instance.cState.shadowDashing || hazardType != 1) &&
                    ((double) HeroController.instance.parryInvulnTimer <= 0.0 || hazardType != 1));
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }


    }
}