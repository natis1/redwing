using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redwing
{
    public class redwing_trail_behavior : MonoBehaviour
    {
        private const float LIFESPAN = 3f;

        // woah... that's strong. especially when combined with the blackmoth dash.
        private const int TRAIL_DAMAGE = 15;

        public SpriteRenderer drawEm;
        public Texture2D spriteUsed;
        public Vector2 dashVector;
        private bool stopAnimation;
        
        public List<Collider2D> enteredColliders;

        public void Start()
        {
            enteredColliders = new List<Collider2D>();
            StartCoroutine(playAnimation());
            StartCoroutine(dashAnimation());
        }

        private IEnumerator dashAnimation()
        {
            yield return null;
            int maxDashLength = spriteUsed.width;
            int currentDashLength = 0;
            float estimatedPixelsMoved = 0f;
            const int pixelsPerUnit = 75;
            BoxCollider2D cachedCollider = gameObject.GetComponent<BoxCollider2D>();
            while ( (HeroController.instance.cState.dashing || HeroController.instance.cState.shadowDashing ||
                   HeroController.instance.cState.backDashing) && currentDashLength < maxDashLength && !stopAnimation)
            {
                estimatedPixelsMoved += dashVector.magnitude * Time.deltaTime * pixelsPerUnit;
                
                currentDashLength = (int) estimatedPixelsMoved;
                Rect spriteRect = new Rect(0, 0, currentDashLength, spriteUsed.height);
                drawEm.sprite = Sprite.Create(spriteUsed, spriteRect, new Vector2(0, 0.5f), pixelsPerUnit);

                Vector2 cachedColliderSize = cachedCollider.size;
                cachedColliderSize.x = estimatedPixelsMoved / pixelsPerUnit;
                cachedColliderSize.y = drawEm.size.y * 0.7f;
                cachedCollider.size = cachedColliderSize;
                cachedCollider.offset = new Vector2(cachedColliderSize.x / 2, 0);
                
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
                cachedColor.a = (float) ((0.2f + LIFESPAN - currentTime) / LIFESPAN);
                drawEm.color = cachedColor;
                yield return null;
            }

            Destroy(this.gameObject);
        }
        
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            int layer = collision.gameObject.layer;
            if (layer == 8)
            {
                stopAnimation = true;
            }
            
            
            if (layer != 11) return;
            
            burnThatMotherTrucker(collision.gameObject);
        }

        private void burnThatMotherTrucker(GameObject enemy)
        {
            FSMUtility.SendEventToGameObject(enemy, "TAKE DAMAGE", false);
            HitTaker.Hit(enemy, new HitInstance
            {
                Source = base.gameObject,
                AttackType = AttackTypes.Generic,
                CircleDirection = false,
                DamageDealt = TRAIL_DAMAGE,
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
            Modding.Logger.Log("[Greymoth] " + str);
        }
    }
}