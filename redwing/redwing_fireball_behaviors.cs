using System.Collections;
using UnityEngine;

namespace redwing
{
    internal class redwing_fireball_spawner_behavior : MonoBehaviour
    {
        public readonly float lifespan = 1.75f;

        public void Start()
        {
            StartCoroutine(despawn());
        }

        private IEnumerator despawn()
        {
            float currentTime = 0f;
            while (currentTime < lifespan)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }
            Modding.Logger.Log("[REDWING] Despawning fb because time ran out I guess");
            Destroy(fbSpawn);
        }

        public GameObject fbSpawn;
    }

    internal class redwing_fireball_behavior : MonoBehaviour
    {
        public readonly float lifespan = 1.5f;

        public float xVelocity;
        public float yVelocity;

        public Transform selfTranform;
        public Vector3 selfPosition;

        public const float G_FORCE = -1.5f;
        public const float TERMINAL_VELOCITY_Y = 100f;
        public const int CARTOON_FLOAT_FRAMES = 8;

        public bool doPhysics;


        public void Start()
        {
            StartCoroutine(despawn());
            StartCoroutine(cartoonPhysics());
            StartCoroutine(ballAppear());
        }

        private IEnumerator ballAppear()
        {
            Vector2 ballSize = fireballSprite.size;
            float currentTime = 0f;
            while (currentTime < 0.2f)
            {
                float alpha = currentTime * 5f;
                currentTime += Time.deltaTime;
                fireballSprite.size = ballSize * alpha;
                
                Color fireballSpriteColor = fireballSprite.color;
                fireballSpriteColor.a = alpha;
                fireballSprite.color = fireballSpriteColor;
                
                yield return null;
            }

            Color spriteColor = fireballSprite.color;
            spriteColor.a = 1.0f;
            fireballSprite.color = spriteColor;
            fireballSprite.size = ballSize;
        }

        private IEnumerator cartoonPhysics()
        {
            log("starting physics");
            while (!doPhysics)
            {
                yield return null;
            }
            
            float currentTime = 0f;
            int cartoonFrames = CARTOON_FLOAT_FRAMES;
            while (doPhysics && currentTime < lifespan)
            {
                log("doing physics round. local x is " + selfTranform.localPosition.x + " and selfPosition.x is " + selfPosition.x);
                float timePassed = Time.deltaTime;
                float actualYForce = ((G_FORCE) * (TERMINAL_VELOCITY_Y + yVelocity) / TERMINAL_VELOCITY_Y);
                currentTime += Time.deltaTime;

                if (yVelocity > 0f)
                {
                    yVelocity += actualYForce;
                    if (yVelocity <= 0f)
                    {
                        yVelocity = 0f;
                    }
                }
                else
                {
                    if (cartoonFrames > 0)
                    {
                        cartoonFrames--;
                    }
                    else
                    {
                        yVelocity += actualYForce;
                    }
                }

                selfPosition.x += xVelocity * timePassed;
                selfPosition.y += yVelocity * timePassed;
                selfTranform.localPosition = selfPosition;
                
                yield return null;
            }
        }

        private IEnumerator despawn()
        {
            float currentTime = 0f;
            while (currentTime < lifespan)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }
            Modding.Logger.Log("[REDWING] Despawning fb because time ran out I guess");
            Destroy(fireball);
        }

        public void OnTriggerEnter2D(Collider2D hitbox)
        {
            if (hitbox.IsTouchingLayers(11) || hitbox.IsTouchingLayers(8))
            {
                
                Destroy(fireball);
            }
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }


        public GameObject fireball;
        public SpriteRenderer fireballSprite;
    }
}
