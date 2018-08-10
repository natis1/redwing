using UnityEngine;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;

namespace redwing
{
    internal class greymoth : MonoBehaviour
    {

        /*
         * Things to track:
         * 
         * Just dash upgrades I guess. Maybe did kill NKG but if we're expecting the player to want to be really OP
         * then they will also have grimmchild upgrades and that would make killing this one silly boss do too
         * much. Although blackmoth does check this so maybe it's worth considering.
         * 
         * 
         * */

        //private readonly float dashCooldownTime = HeroController.instance.DASH_COOLDOWN_CH;
        private static float dashCooldown = 0f;
        private float dashInvulTimer = 0f;
        private int antiTurboLeft = 0;
        private const int FUCK_TURBO_FRAMES = 1;
        public Vector2 DashDirection;
        public PlayMakerFSM sharpShadowFSM;
        public GameObject sharpShadow;
        
        // These bools track player upgrades
        public bool dashRegular;
        public bool shadowDash;

        public bool hasSharpShadowCached;

        private bool secondWind;

        private bool completedCoroutines;
        
        public void OnDestroy()
        {
            ModHooks.Instance.DashPressedHook -= dashTapped;
            ModHooks.Instance.DashVectorHook -= doDashDirection;
        }

        public void Start()
        {
            completedCoroutines = false;
            
            try
            {
                StartCoroutine(configureHero());
                ModHooks.Instance.DashPressedHook += dashTapped;
                ModHooks.Instance.DashVectorHook += doDashDirection;
                
                privateFields = new Dictionary<string, FieldInfo>();
                privateMethods = new Dictionary<string, MethodInfo>();
            }
            catch (Exception e)
            {
                log("Error setting up hooks. Error: " + e);
            }

            log("GreymothMod added to interpret dashes.");
        }

        private Vector2 doDashDirection(Vector2 orig)
        {
            Vector2 ret;
            float num = getDashLength();
            if (DashDirection.y <= 0.02 && DashDirection.y >= -0.02)
            {
                ret = num * DashDirection;
                
            } else if (DashDirection.x <= 0.02 && DashDirection.x >= -0.02)
            {
                ret = num * DashDirection;
            }
            else
            {
                ret = (num / Mathf.Sqrt(2)) * DashDirection;
            }
            
            return ret;
        }

        private float getDashLength()
        {
            return hasSharpShadowCached ?
                HeroController.instance.DASH_SPEED_SHARP : HeroController.instance.DASH_SPEED;
        }

        private void Update()
        {
            if (!completedCoroutines)
                return;
            
            if (dashCooldown > 0)
            {
                dashCooldown -= Time.deltaTime;
            }
            if (dashInvulTimer > 0)
            {
                dashInvulTimer -= Time.deltaTime;
            }

            try
            {
                if (dashInvulTimer > 0)
                {
                    HeroController.instance.cState.invulnerable = true;
                    antiTurboLeft = FUCK_TURBO_FRAMES + (int) (Time.fixedDeltaTime / Time.deltaTime);
                }
                else
                {
                    if (antiTurboLeft > 0)
                    {
                        antiTurboLeft--;
                        HeroController.instance.cState.invulnerable = false;
                    }
                }
            }
            catch (NullReferenceException e)
            {
                log("Null Reference trying to set Hero cState. Probably because you're transitioning levels " + e);
            }

            if (!secondWind && HeroController.instance.cState.onGround) secondWind = true;

        }

        private IEnumerator configureHero()
        {
            while (HeroController.instance == null)
                yield return new WaitForEndOfFrame();

            if (sharpShadow != null && sharpShadow.CompareTag("Sharp Shadow")) yield break;
            
            
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || !go.CompareTag("Sharp Shadow")) continue;
                    
                sharpShadow = go;
                sharpShadowFSM = FSMUtility.LocateFSM(sharpShadow, "damages_enemy");
                log($@"Found Sharp Shadow: {sharpShadow} - {sharpShadowFSM}.");
            }

            completedCoroutines = true;
            log("Completed needed coroutines. Your game is loaded.");
            //HeroController.instance.gameObject.AddComponent<SuperDashHandler>();
        }

        
        
        private bool dashTapped()
        {
            if (antiTurboLeft != 0)
                return true;

            hasSharpShadowCached = PlayerData.instance.GetBool("equippedCharm_16");
            
            getPrivateField("dashQueueSteps").SetValue(HeroController.instance, 0);
            getPrivateField("dashQueuing").SetValue(HeroController.instance, false);
            HeroActions direction = GameManager.instance.inputHandler.inputActions;
            
            if (direction.up.IsPressed)
            {
                DashDirection.y = 1f;
            }
            else if (direction.down.IsPressed && !HeroController.instance.cState.onGround)
            {
                DashDirection.y = -1f;
            }
            else
            {
                DashDirection.y = 0;
            }

            if (direction.right.IsPressed)
            {
                DashDirection.x = 1f;
            }
            else if (direction.left.IsPressed)
            {
                DashDirection.x = -1f;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (DashDirection.y == 0)
            {
                DashDirection.x = HeroController.instance.cState.facingRight ? 1 : -1;
            }
            else
            {
                DashDirection.x = 0;
            }
            //log($@"Dash direction: {DashDirection}");
            if (!PlayerData.instance.GetBool("hasDash"))
            {
                DashDirection.y = 0;
                DashDirection.x = HeroController.instance.cState.facingRight ? 1 : -1;
            }

            if (!PlayerData.instance.GetBool("hasShadowDash"))
            {
                if (Math.Abs(DashDirection.y) > 0.001f && Math.Abs(DashDirection.x) > 0.001f)
                {
                    DashDirection.x = 0f;
                }
            }
            
            doDash();
            
            // Fixes TC problem where after tink sharp shadow is broken
            sharpShadowFSM.SetState("Idle");
            return true;
        }

        private void doDash()
        {
            if ((HeroController.instance.hero_state == ActorStates.no_input ||
                 HeroController.instance.hero_state == ActorStates.hard_landing ||
                 HeroController.instance.hero_state == ActorStates.dash_landing || !(dashCooldown <= 0f) ||
                 HeroController.instance.cState.dashing || HeroController.instance.cState.backDashing ||
                 (HeroController.instance.cState.attacking &&
                  !((float) getPrivateField("attack_time").GetValue(HeroController.instance) >=
                    HeroController.instance.ATTACK_RECOVERY_TIME)) || HeroController.instance.cState.preventDash ||
                 (!HeroController.instance.cState.onGround && airDashed() &&
                  !HeroController.instance.cState.wallSliding))) return;
            
            
            if ((!HeroController.instance.cState.onGround || Math.Abs(DashDirection.y) > 0.001f) && !HeroController.instance.inAcid)
            {
                getPrivateField("airDashed").SetValue(HeroController.instance, true);
            }

            invokePrivateMethod("ResetAttacksDash");
            invokePrivateMethod("CancelBounce");
            ((HeroAudioController)getPrivateField("audioCtrl").GetValue(HeroController.instance)).StopSound(HeroSounds.FOOTSTEPS_RUN);
            ((HeroAudioController)getPrivateField("audioCtrl").GetValue(HeroController.instance)).StopSound(HeroSounds.FOOTSTEPS_WALK);
            ((HeroAudioController)getPrivateField("audioCtrl").GetValue(HeroController.instance)).StopSound(HeroSounds.DASH);
            invokePrivateMethod("ResetLook");

            HeroController.instance.cState.recoiling = false;
                if (HeroController.instance.cState.wallSliding)
            {
                HeroController.instance.FlipSprite();
                if (HeroController.instance.cState.facingRight && Math.Abs(DashDirection.y) < 0.001f )
                {
                    DashDirection.x = 1f;
                } else if (Math.Abs(DashDirection.y) < 0.001f)
                {
                    DashDirection.x = -1f;
                }
            }
            else if (GameManager.instance.inputHandler.inputActions.right.IsPressed)
            {
                HeroController.instance.FaceRight();
            }
            else if (GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                HeroController.instance.FaceLeft();
            }
            HeroController.instance.cState.dashing = true;
            getPrivateField("dashQueueSteps").SetValue(HeroController.instance, 0);
            HeroController.instance.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
            HeroController.instance.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            HeroController.instance.dashingDown = false;
            
            dashCooldown = HeroController.instance.DASH_COOLDOWN_CH;

            getPrivateField("shadowDashTimer").SetValue(HeroController.instance, getPrivateField("dashCooldownTimer").GetValue(HeroController.instance));
            HeroController.instance.proxyFSM.SendEvent("HeroCtrl-ShadowDash");
            HeroController.instance.cState.shadowDashing = true;
            ((AudioSource)getPrivateField("audioSource").GetValue(HeroController.instance)).PlayOneShot(HeroController.instance.sharpShadowClip, 1f);
            HeroController.instance.sharpShadowPrefab.SetActive(true);

            if (HeroController.instance.cState.shadowDashing)
            {
                if (HeroController.instance.transform.localScale.x > 0f)
                {
                    getPrivateField("dashEffect").SetValue(HeroController.instance, HeroController.instance.shadowdashBurstPrefab.Spawn(new Vector3(HeroController.instance.transform.position.x + 5.21f, HeroController.instance.transform.position.y - 0.58f, HeroController.instance.transform.position.z + 0.00101f)));
                    ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale = new Vector3(1.919591f, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.y, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.z);
                }
                else
                {
                    getPrivateField("dashEffect").SetValue(HeroController.instance, HeroController.instance.shadowdashBurstPrefab.Spawn(new Vector3(HeroController.instance.transform.position.x - 5.21f, HeroController.instance.transform.position.y - 0.58f, HeroController.instance.transform.position.z + 0.00101f)));
                    ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale = new Vector3(-1.919591f, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.y, ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale.z);
                }
                HeroController.instance.shadowRechargePrefab.SetActive(true);
                FSMUtility.LocateFSM(HeroController.instance.shadowRechargePrefab, "Recharge Effect").SendEvent("RESET");
                ParticleSystem ps = HeroController.instance.shadowdashParticlesPrefab.GetComponent<ParticleSystem>();
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = true;
                HeroController.instance.shadowRingPrefab.Spawn(HeroController.instance.transform.position);
            }
            else
            {
                HeroController.instance.dashBurst.SendEvent("PLAY");
                ParticleSystem ps = HeroController.instance.dashParticlesPrefab.GetComponent<ParticleSystem>();
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = true;
            }
            if (HeroController.instance.cState.onGround && !HeroController.instance.cState.shadowDashing)
            {
                getPrivateField("dashEffect").SetValue(HeroController.instance, HeroController.instance.backDashPrefab.Spawn(HeroController.instance.transform.position));
                ((GameObject)getPrivateField("dashEffect").GetValue(HeroController.instance)).transform.localScale = new Vector3(HeroController.instance.transform.localScale.x * -1f, HeroController.instance.transform.localScale.y, HeroController.instance.transform.localScale.z);
            }
            dashInvulTimer = 0.3f;
        }

        private bool airDashed()
        {
            bool didAirDash = (bool) getPrivateField("airDashed").GetValue(HeroController.instance);

            // Weird but basically if you have already airdashed but you have dashmaster,
            // then you can dash exactly once again.
            if (!didAirDash || !PlayerData.instance.GetBool("equippedCharm_31") || !secondWind) return didAirDash;
            secondWind = false;
            return false;
        }


        private static void log(string str)
        {
            Modding.Logger.Log("[Greymoth] " + str);
        }
        
        
        private FieldInfo getPrivateField(string fieldName)
        {
            if (!privateFields.ContainsKey(fieldName))
            {
                privateFields.Add(fieldName,
                    HeroController.instance.GetType()
                        .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            return privateFields[fieldName];
        }

        private void invokePrivateMethod(string methodName)
        {
            if (!privateMethods.ContainsKey(methodName))
            {
                privateMethods.Add(methodName,
                    HeroController.instance.GetType()
                        .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance));
            }
            privateMethods[methodName]?.Invoke(HeroController.instance, new object[] { });
        }
        
        private Dictionary<string, FieldInfo> privateFields;
        private Dictionary<string, MethodInfo> privateMethods;
    }
}
