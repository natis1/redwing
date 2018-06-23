using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace redwing
{
    public class redwing_trail_behavior : MonoBehaviour
    {
        private const float LIFESPAN = 1.6f;
        private const float DAMAGEPERIOD = 0.3f;
        private const int SECONDARYDAMAGETIMES = 5;

        // woah... that's strong. especially when combined with the blackmoth dash.
        public static int damagePriBase;
        public static int damagePriNail;
        public static int damageSecBase;
        public static int damageSecNail;
        private int cachedPrimaryDmg;
        private int cachedSecondaryDmg;

        public SpriteRenderer drawEm;
        public Texture2D spriteUsed;
        private bool stopAnimation;
        public BoxCollider2D voidKnightCollider;
        public List<Collider2D> enteredColliders;

        public AudioSource cachedAudio;

        private Vector2 initPosition;

        public void Start()
        {
            enteredColliders = new List<Collider2D>();
            
            cachedPrimaryDmg = damagePriBase + damagePriNail * PlayerData.instance.GetInt("nailSmithUpgrades");
            cachedSecondaryDmg = damageSecBase + damageSecNail * PlayerData.instance.GetInt("nailSmithUpgrades");

            
            cachedAudio.loop = false;
            cachedAudio.volume = (GameManager.instance.gameSettings.masterVolume *
                                  GameManager.instance.gameSettings.soundVolume * 0.01f * 0.4f);
            cachedAudio.Play();
            StartCoroutine(playAnimation());
            StartCoroutine(dashAnimation());
            StartCoroutine(secondaryAttacks());
        }

        private IEnumerator dashAnimation()
        {
            initPosition = voidKnightCollider.gameObject.transform.localPosition;
            
            yield return null;
            int maxDashLength = spriteUsed.width;
            int currentDashLength = 0;
            float estimatedPixelsMoved = 0f;
            const int pixelsPerUnit = 75;
            BoxCollider2D cachedCollider = gameObject.GetComponent<BoxCollider2D>();
            while ( (HeroController.instance.cState.dashing || HeroController.instance.cState.shadowDashing ||
                   HeroController.instance.cState.backDashing) && currentDashLength < maxDashLength && !stopAnimation)
            {
                Vector2 currentPosition = voidKnightCollider.gameObject.transform.localPosition;
                Vector2 deltaPosition = currentPosition - initPosition;
                
                //log($@"currentPos is {currentPosition} and init position is {initPosition}");
                estimatedPixelsMoved += deltaPosition.magnitude * pixelsPerUnit;
                
                currentDashLength = (int) estimatedPixelsMoved;
                Rect spriteRect = new Rect(0, 0, currentDashLength, spriteUsed.height);
                drawEm.sprite = Sprite.Create(spriteUsed, spriteRect, new Vector2(0, 0.5f), pixelsPerUnit);

                Vector2 cachedColliderSize = cachedCollider.size;
                cachedColliderSize.x = estimatedPixelsMoved / pixelsPerUnit;
                cachedColliderSize.y = voidKnightCollider.size.y;
                
                cachedCollider.size = cachedColliderSize;
                cachedCollider.offset = new Vector2(cachedColliderSize.x / 2, 0);

                initPosition = currentPosition;
                yield return null;
            }
            if (stopAnimation)
            {
                log("stopped animation because stopanimation true");
            }

            if (currentDashLength >= maxDashLength)
            {
                log("Stopped animation because your sprite is too small. fix it");
            }
        }


        private IEnumerator playAnimation()
        {
            float currentTime = 0f;
            Color cachedColor = drawEm.color;
            
            while (currentTime < LIFESPAN)
            {
                currentTime += Time.unscaledDeltaTime;
                cachedColor.a = (float) ((LIFESPAN - currentTime) / LIFESPAN);
                drawEm.color = cachedColor;
                yield return null;
            }

            Destroy(this.gameObject);
        }
        
        private IEnumerator secondaryAttacks()
        {
            yield return null;
            int secDamages = 0;
            while (secDamages < SECONDARYDAMAGETIMES)
            {
                secondaryDamage();
                secDamages++;
                yield return new WaitForSeconds(DAMAGEPERIOD);
            }
        }

        private void secondaryDamage()
        {            
            for (int index = enteredColliders.Count - 1; index >= 0; index--)
            {
                Collider2D enteredCollider = enteredColliders[index];
                if ((UnityEngine.Object) enteredCollider == (UnityEngine.Object) null || !enteredCollider.isActiveAndEnabled)
                    enteredColliders.RemoveAt(index);
            }
            
            log("dealing secondary dmg to " + enteredColliders.Count + " enemies");
            
            
            
            foreach (Collider2D collider in enteredColliders.ToList())
            {

                GameObject target = collider.gameObject;
                log("Doing primary pillar damage to target with name " + target.name);

                FSMUtility.SendEventToGameObject(target, "TAKE DAMAGE", false);
                
                // first hit counts as a spell because we want it to stagger.
                HitTaker.Hit(target, new HitInstance
                {
                    Source = base.gameObject,
                    AttackType = AttackTypes.Generic,
                    CircleDirection = false,
                    DamageDealt = cachedSecondaryDmg,
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
        
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            int layer = collision.gameObject.layer;
            if (layer != 11) return;

            if (this.enteredColliders.Contains(collision)) return;
            
            enteredColliders.Add(collision);
            burnThatMotherTrucker(collision.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            int layer = other.gameObject.layer;
            if (layer != 11) return;

            try
            {
                enteredColliders.Remove(other);
            }
            catch (Exception e)
            {
                log("failed to remove collider. error " + e);
            }
            
        }

        private void burnThatMotherTrucker(GameObject enemy)
        {
            FSMUtility.SendEventToGameObject(enemy, "TAKE DAMAGE", false);
            HitTaker.Hit(enemy, new HitInstance
            {
                Source = base.gameObject,
                AttackType = AttackTypes.Generic,
                CircleDirection = false,
                DamageDealt = cachedPrimaryDmg,
                Direction = 0f,
                IgnoreInvulnerable = true,
                MagnitudeMultiplier = 1.0f,
                MoveAngle = 0f,
                MoveDirection = false,
                Multiplier = 1f,
                SpecialType = SpecialTypes.None,
                IsExtraDamage = false
            }, 3);
        }
        
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}