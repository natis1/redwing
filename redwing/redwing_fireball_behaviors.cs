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
        private const float MAGMA_FRAMERATE = 5f;

        private const int FIREBALL_WIDTH = 150;
        private const int FIREBALL_HEIGHT = 150;
        
        public bool doPhysics;
        public bool despawnBall;
        public Texture2D[] fireballMagmas;


        public void Start()
        {
            StartCoroutine(despawn());
            StartCoroutine(cartoonPhysics());
            StartCoroutine(ballAppear());
        }

        private IEnumerator ballAppear()
        {
            Vector3 ballSize = fireball.transform.localScale;
            float currentTime = 0f;
            while (currentTime < 0.2f)
            {
                float alpha = currentTime * 5f;
                currentTime += Time.deltaTime;
                
                fireball.transform.localScale = ballSize * alpha;
                
                Color fireballSpriteColor = fireballSprite.color;
                fireballSpriteColor.a = alpha;
                fireballSprite.color = fireballSpriteColor;
                
                yield return null;
            }

            Color spriteColor = fireballSprite.color;
            spriteColor.a = 1.0f;
            fireballSprite.color = spriteColor;
            fireball.transform.localScale = ballSize;
        }

        private IEnumerator cartoonPhysics()
        {
            while (!doPhysics)
            {
                yield return null;
            }
            
            float currentTime = 0f;
            int cartoonFrames = CARTOON_FLOAT_FRAMES;
            while (doPhysics && currentTime < lifespan)
            {
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
            despawnBall = true;
            float currentTime = 0f;
            while (currentTime < lifespan)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }

            if (!despawnBall) yield break;
            Modding.Logger.Log("[REDWING] Despawning fb because time ran out I guess");
            Destroy(fireball);
        }

        public void OnTriggerEnter2D(Collider2D hitbox)
        {
            if (!hitbox.IsTouchingLayers(11) && !hitbox.IsTouchingLayers(8)) return;
            
            log("Hit a layer 11 or layer 8 object");
            doPhysics = false;
            StartCoroutine(magmaFadeAnimation(0));
        }

        private IEnumerator magmaFadeAnimation(int directionToFade)
        {
            despawnBall = false;
            float animTime = 0f;
            int frame = 0;
            int oldFrame = -1;
            
            while (frame < fireballMagmas.Length)
            {
                if (frame > oldFrame)
                {
                    Rect r = new Rect(0, 0, FIREBALL_WIDTH, FIREBALL_HEIGHT);
                    fireballSprite.sprite = Sprite.Create(fireballMagmas[frame], r, Vector2.zero);
                    oldFrame = frame;
                }
                
                
                const float timePerFrame = 1 / MAGMA_FRAMERATE;
                animTime += Time.deltaTime;
                frame = (int) (animTime / timePerFrame);

                yield return null;

            }
            log("Despawned because animation is done. Current frame is " + frame);
            Destroy(fireball);
            
            yield return null;
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }


        public GameObject fireball;
        public SpriteRenderer fireballSprite;
    }
}
