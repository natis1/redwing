using System;
using System.Collections;
using ModCommon;
using UnityEngine;

namespace redwing
{
    internal class redwing_fireball_spawner_behavior : MonoBehaviour
    {
        public readonly float lifespan = 3.0f;

        public void Start()
        {
            StartCoroutine(despawn());
        }

        private IEnumerator despawn()
        {
            yield return new WaitForSeconds(lifespan);
            Destroy(fbSpawn);
        }

        public GameObject fbSpawn;
    }

    internal class redwing_fireball_behavior : MonoBehaviour
    {
        public readonly float lifespan = 2.25f;
        
        public float xVelocity;
        public float yVelocity;
        public float rotationalVelocity;

        public Transform selfTranform;
        public Vector3 selfPosition;

        public const float G_FORCE = -1.5f;
        public const float TERMINAL_VELOCITY_Y = 15f;
        public const float CARTOON_FLOAT_TIME = 0.3f;
        private const float MAGMA_FRAMERATE = 15f;

        private const int FIREBALL_WIDTH = 150;
        private const int FIREBALL_HEIGHT = 150;

        private int fireballDmg;

        public float maxHeight = 8f;
        public float maxySpeed = 30f;

        public bool realisticPhysics;
        public bool doPhysics;
        public bool despawnBall;
        public Texture2D[] fireballMagmas;
        public Texture2D[] fireballMagmaFireballs;

        public int fireballMagmaFireballWidth;
        public int fireballMagmaFireballHeight;

        private GameObject[] ballObjs = new GameObject[4];

        public BoxCollider2D hitboxForPivot;
        
        private bool botLeftCollide, botRightCollide, topLeftCollide, topRightCollide;
        private bool isDoingHitboxStuff;
        private bool stopAppear;

        public static AudioClip fireballSizzle;
        public static AudioClip fireballImpact;
        public static int fbDamageBase;
        public static int fbDamageScale;
        public static int fbmDamageBase;
        public static int fbmDamageScale;

        public static int fireballMana;
        
        public AudioSource cachedAudioPlayer;

        public void Start()
        {
            StartCoroutine(despawn());

            StartCoroutine(realisticPhysics ? semiRealPhysics() : cartoonPhysics());

            StartCoroutine(ballAppear());
            
            fireballDmg = fbDamageBase + fbDamageScale * PlayerData.instance.GetInt("nailSmithUpgrades");
        }

        

        private IEnumerator ballAppear()
        {
            Vector3 ballSize = this.transform.localScale;
            float currentTime = 0f;
            while (currentTime < 0.2f && !stopAppear)
            {
                float alpha = currentTime * (float) (1.0/0.2);
                currentTime += Time.deltaTime;
                
                this.transform.localScale = ballSize * alpha;
                
                Color fireballSpriteColor = fireballSprite.color;
                fireballSpriteColor.a = alpha;
                fireballSprite.color = fireballSpriteColor;
                
                yield return null;
            }

            if (stopAppear) yield break;
            Color spriteColor = fireballSprite.color;
            spriteColor.a = 1.0f;
            fireballSprite.color = spriteColor;
            this.transform.localScale = ballSize;
        }
        
        private IEnumerator semiRealPhysics()
        {
            while (!doPhysics)
            {
                yield return null;
            }
            
            while (doPhysics)
            {
                float timePassed = Time.deltaTime;
                
                float actualYForce = (2 * G_FORCE) + (8 * G_FORCE) *
                                     ((TERMINAL_VELOCITY_Y + yVelocity) / TERMINAL_VELOCITY_Y);
                yVelocity += actualYForce * timePassed;
                
                selfPosition.x += xVelocity * timePassed;
                selfPosition.y += yVelocity * timePassed;
                
                selfTranform.position = selfPosition;
                fireball.transform.Rotate(0f, 0f, rotationalVelocity);
                
                yield return null;
            }
            
            
        }

        private IEnumerator cartoonPhysics()
        {
            while (!doPhysics)
            {
                yield return null;
            }

            bool gFall = false;
            float currentTime = 0f;
            float currentHeight = 0f;
            
            while (doPhysics && currentTime < lifespan)
            {
                float timePassed = Time.deltaTime;
                
                //float actualYForce = ((G_FORCE) * (TERMINAL_VELOCITY_Y + yVelocity) / TERMINAL_VELOCITY_Y);
                if (gFall)
                {
                    float actualYForce = ((G_FORCE) * (TERMINAL_VELOCITY_Y + yVelocity) / TERMINAL_VELOCITY_Y);
                    yVelocity += actualYForce;
                }
                else
                {
                    yVelocity = (float) ((maxySpeed) * (1.0 - currentHeight / (1.2 * maxHeight)));
                }


                currentTime += timePassed;

                if (yVelocity > 0f && currentHeight >= maxHeight)
                {
                    yVelocity = 0f;
                    float cartoonStartTime = currentTime;
                    gFall = true;
                    while (currentTime - cartoonStartTime < CARTOON_FLOAT_TIME)
                    {
                        timePassed = Time.deltaTime;
                        selfPosition.x += xVelocity * timePassed;
                        selfTranform.position = selfPosition;
                        fireball.transform.Rotate(0f, 0f, rotationalVelocity);
                        currentTime += timePassed;
                        yield return null;
                    }
                }
                
                
                selfPosition.x += xVelocity * timePassed;
                selfPosition.y += yVelocity * timePassed;
                currentHeight += yVelocity * timePassed;
                
                selfTranform.position = selfPosition;
                fireball.transform.Rotate(0f, 0f, rotationalVelocity);
                
                yield return null;
            }
        }

        private IEnumerator despawn()
        {
            despawnBall = true;
            
            
            yield return new WaitForSeconds(lifespan);

            if (!despawnBall) yield break;            
            Destroy(fireball);
            Destroy(this.gameObject);
            
        }

        public void OnTriggerEnter2D(Collider2D hitbox)
        {
            int targetLayer = hitbox.gameObject.layer;
                        
            // why? These are hardcoded layers in the Damages Enemy class so they must not be important.
            // Or rather they must be specifically avoided for some reason.
            if (targetLayer == 20 || targetLayer == 9 || targetLayer == 26 || targetLayer == 31)
            {
                return;
            }
            else if (targetLayer == 11 || hitbox.gameObject.IsGameEnemy())
            {
                redwing_game_objects.applyHitInstance(hitbox.gameObject,
                    fireballDmg, AttackTypes.Generic, this.gameObject);

                if (doPhysics)
                {
                    fireballDmg = 0;
                    doPhysics = false;
                    cachedAudioPlayer.clip = fireballImpact;
                    cachedAudioPlayer.volume = (GameManager.instance.gameSettings.masterVolume *
                                                GameManager.instance.gameSettings.soundVolume * 0.01f * 0.3f);
                    cachedAudioPlayer.loop = false;
                    cachedAudioPlayer.Play();
                    ballExplode();
                    HeroController.instance.AddMPChargeSpa(fireballMana);
                }

            }
            
            
            if (targetLayer != 8) return;
            
            
            cachedAudioPlayer.clip = fireballSizzle;
            cachedAudioPlayer.volume = (GameManager.instance.gameSettings.masterVolume *
                                        GameManager.instance.gameSettings.soundVolume * 0.01f * 0.12f);
            cachedAudioPlayer.loop = false;
            cachedAudioPlayer.Play();
                        
            doPhysics = false;
            
            //center of the object. if we above it and within the bounds we hit it from the top
            //Vector2 centerMeme = hitbox.bounds.center;


            const float epsilon = 0.4f;
            
            Vector2 ourTopRight = hitboxForPivot.bounds.max;
            Vector2 ourBottomLeft = hitboxForPivot.bounds.min;
            //ourTopRight = new Vector2(ourTopRight.x + epsilon, ourTopRight.y + epsilon);
            //ourBottomLeft = new Vector2(ourBottomLeft.x - epsilon, ourBottomLeft.y - epsilon);
            
            Vector2 ourTopLeft = new Vector2(ourBottomLeft.x, ourTopRight.y);
            Vector2 ourBottomRight = new Vector2(ourTopRight.x, ourBottomLeft.y);

            Vector2 otherTopRight = hitbox.bounds.max;
            Vector2 otherBotLeft = hitbox.bounds.min;
            

            float br = getDistanceBetweenVectors(ourBottomRight, hitbox.closestPoint(ourBottomRight));
            float tr = getDistanceBetweenVectors(ourTopRight, hitbox.closestPoint(ourTopRight));
            float tl = getDistanceBetweenVectors(ourTopLeft, hitbox.closestPoint(ourTopLeft));
            float bl = getDistanceBetweenVectors(ourBottomLeft, hitbox.closestPoint(ourBottomLeft));
                
            if (!botRightCollide)
            {
                if (br < epsilon)
                {
                    botRightCollide = true;
                }
            }

            if (!topRightCollide)
            {
                if (tr < epsilon)
                {
                    topRightCollide = true;
                }
            }

            if (!topLeftCollide)
            {
                if (tl < epsilon)
                {
                    topLeftCollide = true;
                }
            }

            if (!botLeftCollide)
            {
                if (bl < epsilon)
                {
                    botLeftCollide = true;
                }
            }
            
            
            if (!isDoingHitboxStuff)
                StartCoroutine(doHitboxStuff());

        }

        private float getDistanceBetweenVectors(Vector2 a, Vector2 b)
        {
            return (float) (Math.Sqrt( Math.Pow(((double)(a.x - b.x)), 2.0) + Math.Pow(((double)(a.y - b.y)), 2.0)));
        }

        private IEnumerator doHitboxStuff()
        {
            isDoingHitboxStuff = true;
            
            yield return null;
            
            
            int direction = 0;
            int collides = 0;

            if (topLeftCollide)
                collides++;
            if (topRightCollide)
                collides++;
            if (botLeftCollide)
                collides++;
            if (botRightCollide)
                collides++;
            
            if (!topRightCollide && topLeftCollide)
            {
                direction = 3;
                // left and right collides should just disappear into balls because the animation looks stupid otherwise
                // Not much I can do about it.
                collides = 0;
            } 
            else if (topRightCollide && !topLeftCollide)
            {
                direction = 1;
                collides = 0;
            } else if (!botLeftCollide && !botRightCollide)
            {
                direction = 2;
            } else if (topLeftCollide && topRightCollide && botLeftCollide && botRightCollide)
            {
                direction = 0;
                collides = 0;
            }
            
            fireball.transform.rotation = Quaternion.identity;           
            fireball.transform.Rotate(new Vector3(0f, 0f, (float) (direction * 90.0)));
            int balls = redwing_flame_gen.rng.Next(0, 4);
            
            
            
            if (collides > 1)
            {
                StartCoroutine(magmaFadeAnimation(direction));
                fireballDmg = fbmDamageBase + fbmDamageScale * PlayerData.instance.GetInt("nailSmithUpgrades");
            }
            else
            {
                ballExplode();
                fireballDmg = 0;
            }

            generateFireballMagmaBalls(balls);
            
        }

        private void ballExplode()
        {
            Color fireballSpriteColor = fireballSprite.color;
            fireballSpriteColor.a = 0f;
            fireballSprite.color = fireballSpriteColor;
            generateFireballMagmaBalls(4);
            generateFireballMagmaBalls(4);
            stopAppear = true;

        }

        private void generateFireballMagmaBalls(int balls)
        {
            if (balls == 0) return;
            
            for (int i = 0; i < balls; i++)
            {
                ballObjs[i] = new GameObject("FireballMagmaFireball" + i, typeof(Rigidbody2D),
                    typeof(redwing_fireball_magma_fireball_behavior), typeof(SpriteRenderer));
                ballObjs[i].transform.localPosition = selfTranform.position;
                ballObjs[i].transform.parent = fireball.transform;
                ballObjs[i].layer = 31;
                
                redwing_fireball_magma_fireball_behavior a = ballObjs[i].
                    GetComponent<redwing_fireball_magma_fireball_behavior>();
                a.self = ballObjs[i];
                a.selfSprite = ballObjs[i].GetComponent<SpriteRenderer>();
                Rigidbody2D b = ballObjs[i].GetComponent<Rigidbody2D>();
                b.velocity = new Vector2( ( (float)(redwing_flame_gen.rng.NextDouble() - 0.5) * 5f),
                    (float)(redwing_flame_gen.rng.NextDouble() * 2.5 + 2f));
                b.mass = 0.005f;
                b.isKinematic = true;
                
                Rect r = new Rect(0, 0, fireballMagmaFireballWidth, fireballMagmaFireballHeight);
                SpriteRenderer d = ballObjs[i].GetComponent<SpriteRenderer>();
                d.sprite = Sprite.Create(fireballMagmaFireballs[i], r, new Vector2(0.5f, 0.5f));
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
                    fireballSprite.sprite = Sprite.Create(fireballMagmas[frame], r, new Vector2(0.5f, 0.5f));
                    oldFrame = frame;
                }

                frame++;
                yield return new WaitForSeconds((float)(1.0 / MAGMA_FRAMERATE));

            }

            Destroy(fireball);
            Destroy(this.gameObject);
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
            Destroy(this);
        }

    }
    
    
    public static class collider_two_dimensional_extension
    {
        /// <summary>
        /// Return the closest point on a Collider2D relative to point
        /// </summary>
        public static Vector2 closestPoint(this Collider2D col, Vector2 point)
        {
            GameObject go = new GameObject("tempCollider");
            go.transform.position = point;
            CircleCollider2D c = go.AddComponent<CircleCollider2D>();
            c.radius = 0.1f;
            ColliderDistance2D dist = col.Distance(c);
            UnityEngine.Object.Destroy(go);
            return dist.pointA;
        }

        public static bool didCollide(this Collider2D col, Vector2 point)
        {
            GameObject go = new GameObject("tempCollider");
            go.transform.position = point;
            CircleCollider2D c = go.AddComponent<CircleCollider2D>();
            c.radius = 0.1f;
            bool collideWithCol = c.IsTouching(col);
            UnityEngine.Object.Destroy(go);
            return collideWithCol;
        }
        
    }
    
}
