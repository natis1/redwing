using System;
using System.Collections;
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

        public float maxHeight = 8f;
        public float maxySpeed = 30f;
        
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

        public void Start()
        {
            StartCoroutine(despawn());
            StartCoroutine(cartoonPhysics());
            StartCoroutine(ballAppear());
        }

        private IEnumerator ballAppear()
        {
            Vector3 ballSize = this.transform.localScale;
            float currentTime = 0f;
            while (currentTime < 0.2f && !stopAppear)
            {
                float alpha = currentTime * 5f;
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
            float currentTime = 0f;
            while (currentTime < lifespan)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }

            if (!despawnBall) yield break;
            Modding.Logger.Log("[REDWING] Despawning fb because time ran out I guess");
            
            Destroy(fireball);
            Destroy(this.gameObject);
            
        }

        
        /*
        private void OnCollisionEnter2D(Collision2D other)
        {
            
            if (other.collider.gameObject.layer != 8) return;
            
            log("Hit a layer 8 object with on trigger enter. obj name is " + other.collider.name);
            
            doPhysics = false;
            hitboxForPivot.isTrigger = true;
            
            
            float RectWidth = hitboxForPivot.bounds.size.x;
            float RectHeight = hitboxForPivot.bounds.size.y;
            
            //other.otherCollider.bounds.max
            
            
            //other.collider.OverlapPoint(Vector2.down);
            
            Vector2 contactPoint = other.contacts[0].point;
            Vector2 bottomLeft = other.collider.bounds.min;
            Vector2 topRight = other.collider.bounds.max;
            
            float pushUpDistance = topRight.y - contactPoint.y;
            float pushDownDistance = contactPoint.y - bottomLeft.y;

            float pushRightDistance = topRight.x - contactPoint.x;
            float pushLeftDistance = contactPoint.x - bottomLeft.x;
            int direction;
            
            if (pushUpDistance <= pushDownDistance && pushUpDistance <= pushRightDistance &&
                pushUpDistance <= pushLeftDistance)
            {
                direction = 0;
            } else if (pushDownDistance <= pushRightDistance && pushDownDistance <= pushLeftDistance)
            {
                direction = 2;
            } else if (pushRightDistance <= pushLeftDistance)
            {
                direction = 1;
            }
            else
            {
                direction = 3;
            }
            
            /*
            int direction = 0;
            if (pushUpDistance <= pushDownDistance && pushUpDistance <= pushRightDistance &&
                pushUpDistance <= pushLeftDistance)
            {
                direction = 0;
                moveFireballBy(0f, pushUpDistance);
            } else if (pushDownDistance <= pushRightDistance && pushDownDistance <= pushLeftDistance)
            {
                direction = 2;
                moveFireballBy(0f, -pushDownDistance);
            } else if (pushRightDistance <= pushLeftDistance)
            {
                direction = 1;
                moveFireballBy(pushRightDistance, 0f);
            }
            else
            {
                direction = 3;
                moveFireballBy(-pushLeftDistance, 0f);
            }
            
            fireball.transform.rotation = Quaternion.identity;
            fireball.transform.Rotate(new Vector3(0f, 0f, (float) (direction * 90.0)));
            int balls = redwing_flame_gen.rng.Next(0, 4);
            
            StartCoroutine(magmaFadeAnimation(direction));
            generateFireballMagmaBalls(balls);

            
        }
        
        */

        public void OnTriggerEnter2D(Collider2D hitbox)
        {
            
            if (hitbox.gameObject.layer != 8) return;
            
            log("Hit a layer 8 object with on trigger enter. obj name is " + hitbox.name);
            
            doPhysics = false;
            
            //center of the object. if we above it and within the bounds we hit it from the top
            //Vector2 centerMeme = hitbox.bounds.center;


            const float epsilon = 0.2f;
            
            Vector2 ourTopRight = hitboxForPivot.bounds.max;
            Vector2 ourBottomLeft = hitboxForPivot.bounds.min;
            //ourTopRight = new Vector2(ourTopRight.x + epsilon, ourTopRight.y + epsilon);
            //ourBottomLeft = new Vector2(ourBottomLeft.x - epsilon, ourBottomLeft.y - epsilon);
            
            Vector2 ourTopLeft = new Vector2(ourBottomLeft.x, ourTopRight.y);
            Vector2 ourBottomRight = new Vector2(ourTopRight.x, ourBottomLeft.y);

            Vector2 otherTopRight = hitbox.bounds.max;
            Vector2 otherBotLeft = hitbox.bounds.min;
            
            /*
            Bounds topLeftFakeCollider = new Bounds
            {
                center = ourTopLeft,
                size = new Vector3(epsilon, epsilon, 1000f)
            };
            Bounds botLeftFakeCollider = new Bounds
            {
                center = ourBottomLeft,
                size = new Vector3(epsilon, epsilon, 1000f)
            };
            Bounds topRightFakeCollider = new Bounds
            {
                center = ourTopRight,
                size = new Vector3(epsilon, epsilon, 1000f)
            };
            Bounds botRightFakeCollider = new Bounds
            {
                center = ourBottomRight,
                size = new Vector3(epsilon, epsilon, 1000f)
            };*/

            /*
            if (!botLeftCollide)
            {
                //hitboxForPivot.
                //botLeftCollide = hitbox.IsTouching();

            }*/

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
            
            /*
            if (!topRightCollide && topLeftCollide)
            {
                selfPosition.x += tl;
            } 
            else if (topRightCollide && !topLeftCollide)
            {
                selfPosition.x -= tr;
            } else if (!botLeftCollide && !botRightCollide)
            {
                selfPosition.y -= tr;
            }
            else if (botRightCollide)
            {
                selfPosition.y += br;
            } else if (botLeftCollide)
            {
                selfPosition.y += bl;
            }*/
            
            //if (!botLeftCollide)
            //    botLeftCollide = hitbox.OverlapPoint()
            /*
            if (ourBottomLeft.y >= otherBotLeft.y && ourBottomLeft.y <= otherTopRight.y &&
                ourBottomLeft.x >= otherBotLeft.x && ourBottomLeft.x <= otherTopRight.x)
                botLeftCollide = true;
           

            if (ourTopRight.y >= otherBotLeft.y && ourTopRight.y <= otherTopRight.y &&
                ourTopRight.x >= otherBotLeft.x && ourTopRight.x <= otherTopRight.x)
                topRightCollide = true;


            if (ourTopLeft.y >= otherBotLeft.y && ourTopLeft.y <= otherTopRight.y &&
                ourTopLeft.x >= otherBotLeft.x && ourTopLeft.x <= otherTopRight.x)
                topLeftCollide = true;

            if (ourBottomRight.y >= otherBotLeft.y && ourBottomRight.y <= otherTopRight.y &&
                ourBottomRight.x >= otherBotLeft.x && ourBottomRight.x <= otherTopRight.x)
                botRightCollide = true;
            */
            
            
            log("bot left is " + ourBottomLeft.x + ", " + ourBottomLeft.y +
                " and top right is " + ourTopRight.x + ", " + ourTopRight.y);

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
            
            log("The following are being collided with \ntopleft: " + topLeftCollide + " topright: " +topRightCollide +
                " botleft: " + botLeftCollide + " botright: " + botRightCollide);
            
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
            } 
            else if (topRightCollide && !topLeftCollide)
            {
                direction = 1;
            } else if (!botLeftCollide && !botRightCollide)
            {
                direction = 2;
            } else if (topLeftCollide && topRightCollide && botLeftCollide && botRightCollide)
            {
                direction = 2;
            }
            
            fireball.transform.rotation = Quaternion.identity;           
            fireball.transform.Rotate(new Vector3(0f, 0f, (float) (direction * 90.0)));
            int balls = redwing_flame_gen.rng.Next(0, 4);
            
            
            
            if (collides > 1)
            {
                StartCoroutine(magmaFadeAnimation(direction));
                this.GetComponent<DamageEnemies>().damageDealt = this.GetComponent<DamageEnemies>().damageDealt / 2;
            }
            else
            {
                ballExplode();
                this.GetComponent<DamageEnemies>().damageDealt = 0;
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
                
                
                const float timePerFrame = 1 / MAGMA_FRAMERATE;
                animTime += Time.deltaTime;
                frame = (int) (animTime / timePerFrame);

                yield return null;

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
            Modding.Logger.Log("[circlecolliderthingy] c " + c.bounds.center.x + ", " + c.bounds.center.y +
                               ", " + c.bounds.center.z);
            bool collideWithCol = c.IsTouching(col);
            UnityEngine.Object.Destroy(go);
            return collideWithCol;
        }
        
    }
    
}
