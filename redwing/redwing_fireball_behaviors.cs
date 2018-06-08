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
        public float rotationalVelocity;

        public Transform selfTranform;
        public Transform realSelfTransform;
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

        public BoxCollider2D hitboxForPivot;


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
                float qpu = (float) (timePassed / 4.0);
                
                float actualYForce = ((G_FORCE) * (TERMINAL_VELOCITY_Y + yVelocity) / TERMINAL_VELOCITY_Y);
                currentTime += timePassed;
                
                log("real transform is pos " + realSelfTransform.position.x + ", " + realSelfTransform.position.y);
                log("real transform is localpos " + realSelfTransform.localPosition.x + ", " + realSelfTransform.localPosition.y);

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
                            qpu = (float) (timePassed / 4.0);
                            
                            for (int i = 0; i < 4; i++)
                            {
                                if (!doPhysics) continue;
                                selfPosition.x += xVelocity * qpu;
                                
                                selfTranform.position = selfPosition;
                                realSelfTransform.localPosition = Vector3.zero;
                            }

                            currentTime += timePassed;
                            yield return null;
                        }
                    }
                }
                else
                {
                    yVelocity += actualYForce;
                }

                for (int i = 0; i < 4; i++)
                {
                    if (!doPhysics) continue;
                    selfPosition.x += xVelocity * qpu;
                    selfPosition.y += yVelocity * qpu;
                    
                    selfTranform.position = selfPosition;
                    realSelfTransform.localPosition = Vector3.zero;
                }

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
            
            log("Hit a layer 11 object with on trigger enter");
            
            
            
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.otherCollider.IsTouchingLayers(11)) return;
            
            log("Wow oncollisionenter2d isn't a myth it actually runs");
            doPhysics = false;
            
            float RectWidth = this.GetComponent<Collider2D>().bounds.size.x;
            float RectHeight = this.GetComponent<Collider2D>().bounds.size.y;
            
            Vector2 contactPt = other.contacts[0].point;
            Vector2 center = other.collider.bounds.center;

            int direction = 0;
            if (contactPt.y > center.y && //checks that circle is on top of rectangle
                (contactPt.x < center.x + RectWidth / 2.0 && contactPt.x > center.x - RectWidth / 2.0))
            {
                moveFireballBy(0f, contactPt.y - center.y);
                direction = 0;
            }
            else if (contactPt.y < center.y &&
                     (contactPt.x < center.x + RectWidth / 2.0 && contactPt.x > center.x - RectWidth / 2.0))
            {
                moveFireballBy(0f, contactPt.y - center.y);
                direction = 2;
            }
            else if (contactPt.x > center.x &&
                     (contactPt.y < center.y + RectHeight / 2.0 && contactPt.y > center.y - RectHeight / 2.0))
            {
                moveFireballBy(contactPt.x - center.x, 0f);
                direction = 3;
            }
            else if (contactPt.x < center.x &&
                     (contactPt.y < center.y + RectHeight / 2.0 && contactPt.y > center.y - RectHeight / 2.0))
            {
                moveFireballBy(contactPt.x - center.x, 0f);
                direction = 1;
            }
            
            // 0 = bottom. 1 = left. 2 = top. 3 = right

            fireballPhysics.angularVelocity = 0f;
            fireballPhysics.rotation = (float) (direction * 90);
            
            int balls = redwing_flame_gen.rng.Next(0, 4);
            
            StartCoroutine(magmaFadeAnimation(direction));
            generateFireballMagmaBalls(balls);
        }

        private void moveFireballBy(float xDif, float yDif)
        {
            selfPosition.x += xDif;
            selfPosition.y += yDif;
            selfTranform.position = selfPosition;
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
        public Rigidbody2D fireballPhysics;
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
