using System;
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
        public const float CARTOON_FLOAT_TIME = 0.3f;
        private const float MAGMA_FRAMERATE = 15f;

        private const int FIREBALL_WIDTH = 150;
        private const int FIREBALL_HEIGHT = 150;
        
        public bool doPhysics;
        public bool despawnBall;
        public Texture2D[] fireballMagmas;
        public Texture2D[] fireballMagmaFireballs;

        public int fireballMagmaFireballWidth;
        public int fireballMagmaFireballHeight;

        private GameObject[] ballObjs = new GameObject[4];


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
            while (doPhysics && currentTime < lifespan)
            {
                float timePassed = Time.deltaTime;
                float actualYForce = ((G_FORCE) * (TERMINAL_VELOCITY_Y + yVelocity) / TERMINAL_VELOCITY_Y);
                currentTime += timePassed;

                if (yVelocity > 0f)
                {
                    yVelocity += actualYForce;
                    if (yVelocity <= 0f)
                    {
                        yVelocity = 0f;
                        float cartoonStartTime = currentTime;
                        while (currentTime - cartoonStartTime < CARTOON_FLOAT_TIME)
                        {
                            timePassed = Time.deltaTime;
                            selfPosition.x += xVelocity * timePassed;
                            selfTranform.localPosition = selfPosition;
                            currentTime += timePassed;
                            yield return null;
                        }
                    }
                }
                else
                {
                    yVelocity += actualYForce;
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
            if (!hitbox.IsTouchingLayers(11)) return;
            
            log("Hit a layer 11 object");
            doPhysics = false;
            int balls = redwing_flame_gen.rng.Next(0, 4);
            generateFireballMagmaBalls(balls);
            StartCoroutine(magmaFadeAnimation(0));
        }

        private void generateFireballMagmaBalls(int balls)
        {
            if (balls == 0) return;
            
            log("building " + balls + " fireballs");
            for (int i = 0; i < balls; i++)
            {
                ballObjs[i] = new GameObject("FireballMagmaFireball" + i, typeof(Rigidbody2D), typeof(BoxCollider2D),
                    typeof(redwing_fireball_magma_fireball_behavior), typeof(SpriteRenderer),
                    typeof(IgnoreHeroCollision));
                ballObjs[i].transform.localPosition = selfTranform.position;
                ballObjs[i].transform.parent = fireball.transform;
                log("Ball position x is " + ballObjs[i].transform.position.x + " and localposition x " +
                                   "is " + ballObjs[i].transform.localPosition.x);
                
                redwing_fireball_magma_fireball_behavior a = ballObjs[i].
                    GetComponent<redwing_fireball_magma_fireball_behavior>();
                a.self = ballObjs[i];
                a.selfSprite = ballObjs[i].GetComponent<SpriteRenderer>();
                Rigidbody2D b = ballObjs[i].GetComponent<Rigidbody2D>();
                b.velocity = new Vector2( ( (float)(redwing_flame_gen.rng.NextDouble() - 0.5) * 5f),
                    (float)(redwing_flame_gen.rng.NextDouble() * 2.5 + 2f));
                b.mass = 0.005f;
                b.isKinematic = true;
                
                
                BoxCollider2D c = ballObjs[i].GetComponent<BoxCollider2D>();
                c.density = 0.005f;
                c.size = Vector2.zero;
                
                Rect r = new Rect(0, 0, fireballMagmaFireballWidth, fireballMagmaFireballHeight);
                SpriteRenderer d = ballObjs[i].GetComponent<SpriteRenderer>();
                d.sprite = Sprite.Create(fireballMagmaFireballs[i], r, Vector2.zero);
                d.enabled = true;
                d.color = Color.white;
                
                ballObjs[i].SetActive(true);

            }
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

    internal class redwing_fireball_magma_fireball_behavior : MonoBehaviour
    {
        public GameObject self;
        public SpriteRenderer selfSprite;
        private const float LIFETIME = 0.5f;
        
        public void Start()
        {
            StartCoroutine(fade());
        }

        private IEnumerator fade()
        {
            float life = 0f;
            while (life < LIFETIME)
            {
                life += Time.deltaTime;
                Color selfSpriteColor = selfSprite.color;
                selfSpriteColor.a = 1.0f - (float) (life / LIFETIME);
                selfSprite.color = selfSpriteColor;
                yield return null;
            }
            Modding.Logger.Log("Destroying ball because life ran out. life is " + life);
            Destroy(self);
        }

    }
}
