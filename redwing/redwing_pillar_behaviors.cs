using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace redwing
{
    public class redwing_pillar_behavior : MonoBehaviour
    {
        private const float LIFESPAN = 1.0f;
        private const int damagePrimary = 30;
        private const int damageSecondary = 3;
        private const int damageSecondaryTimes = 6;
        
        
        private void Start()
        {
            log("started firepillar");
            enteredColliders = new List<Collider2D>();
            

            StartCoroutine(destroyPillar());
        }

        private IEnumerator destroyPillar()
        {
            float life = 0f;
            int secondaryAttacks = 0;
            SpriteRenderer cachedSprite = this.gameObject.GetComponent<SpriteRenderer>();
            Color cachedColor = cachedSprite.color;
            yield return null;
            primaryDamage();
            DamageEnemies test;
            
            while (secondaryAttacks < damageSecondaryTimes)
            {
                int damagesDone = (int)(damageSecondaryTimes * (life / LIFESPAN));
                if (damagesDone > secondaryAttacks)
                {
                    try
                    {
                        secondaryDamage();
                    }
                    catch (Exception e)
                    {
                        log("unable to do damage because " + e);
                    }

                    secondaryAttacks++;
                }

                cachedColor.a = (0.1f + LIFESPAN - life) / LIFESPAN;
                cachedSprite.color = cachedColor;
                life += Time.deltaTime;
                yield return null;
            }
            log("ending firepillar");
            Destroy(this.gameObject);
        }


        private void primaryDamage()
        {
            
            log("dealing primary dmg to " + enteredColliders.Count + " enemies");
            foreach (Collider2D collider in enteredColliders.ToList())
            {
                GameObject target = collider.gameObject;
                log("Doing primary pillar damage to target with name " + target.name);

                FSMUtility.SendEventToGameObject(target, "TAKE DAMAGE", false);
                
                // first hit counts as a spell because we want it to stagger.
                HitTaker.Hit(target, new HitInstance
                {
                    Source = base.gameObject,
                    AttackType = AttackTypes.Spell,
                    CircleDirection = false,
                    DamageDealt = damagePrimary,
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

        private void secondaryDamage()
        {
            log("removing dead enemies from colliders");
            
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
                    DamageDealt = damageSecondary,
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

            if (!this.enteredColliders.Contains(collision))
            {
                enteredColliders.Add(collision);
            }
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

        public List<Collider2D> enteredColliders;
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
    
    
    public class redwing_pillar_detect_behavior : MonoBehaviour
    {        
        public static Texture2D[] pillarTextures;
        public GameObject firePillar;
        
        public List<GameObject> enemyList;

        private void Start()
        {
            enemyList = new List<GameObject>();
        }


        // ReSharper disable once UnusedMember.Global - Used implicitly
        public void spawnFirePillar()
        {
            log("meme");
            
            firePillar = new GameObject("redwingFlamePillar", typeof(redwing_pillar_behavior),
                typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
            firePillar.transform.localScale = new Vector3(3f, 3f, 3f);
            GameObject fireAtJerk = null;

            if (isEnemyInRange())
            {
                try
                {
                    fireAtJerk = firePillarTarget();
                }
                catch (Exception e)
                {
                    log("meme failed with error " + e);
                }
            }


            if (fireAtJerk != null)
            {
                firePillar.transform.parent = null;
                firePillar.transform.localPosition = Vector3.zero;
                Vector3 pillarRelativePosition = new Vector3(
                    fireAtJerk.transform.position.x,
                    this.gameObject.transform.position.y,
                    fireAtJerk.transform.position.z);
                firePillar.transform.position = fireAtJerk.gameObject.transform.position;
            }
            else
            {
                firePillar.transform.parent = this.gameObject.transform;
                firePillar.transform.localPosition = Vector3.zero;
            }
            
            
            
            int randomTextureToUse = redwing_flame_gen.rng.Next(0, pillarTextures.Length - 1);
            SpriteRenderer img = firePillar.GetComponent<SpriteRenderer>();
            Rect pillarSpriteRect = new Rect(0, 0,
                redwing_flame_gen.FPTEXTURE_WIDTH, redwing_flame_gen.FPTEXTURE_HEIGHT);
            img.sprite = Sprite.Create(pillarTextures[randomTextureToUse], pillarSpriteRect, new Vector2(0.5f, 0.5f));
            img.color = Color.white;

            Rigidbody2D fakePhysics = firePillar.GetComponent<Rigidbody2D>();
            fakePhysics.isKinematic = true;
            BoxCollider2D hitEnemies = firePillar.GetComponent<BoxCollider2D>();
            hitEnemies.isTrigger = true;
            hitEnemies.size = img.size;
            hitEnemies.offset = new Vector2(img.size.x / 2, 0);
            
            firePillar.SetActive(true);
            log("meme done. Random texture number is " + randomTextureToUse + " and that sprite texture is " +
                pillarTextures[randomTextureToUse].width + ", " + pillarTextures[randomTextureToUse].height);
            
            
        }
        
        // From grimmchild upgrades and: Token: 0x0600006E RID: 110 RVA: 0x00005168 File Offset: 0x00003368
        private GameObject firePillarTarget()
        {
            log("found enemy list. there are " + enemyList.Count + " enemies");
            GameObject result = null;
            float num = 99999f;
            if (enemyList.Count <= 0) return result;
            
            for (int i = enemyList.Count - 1; i > -1; i--)
            {
                if (enemyList[i] == null || !enemyList[i].activeSelf)
                {
                    enemyList.RemoveAt(i);
                }
            }
            foreach (GameObject enemyGameObject in enemyList)
            {
                // just pick enemy in range
                if (enemyGameObject == null) continue;
                float sqrMagnitude = (this.gameObject.transform.position - enemyGameObject.transform.position).sqrMagnitude;
                if (!(sqrMagnitude < num)) continue;
                result = enemyGameObject;
                num = sqrMagnitude;
            }
            return result;
        }
        
        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (otherCollider.gameObject.layer != 11) return;
            log("found 'enemy' with name " + otherCollider.name);
            enemyList.Add(otherCollider.gameObject);
        }

        private void OnTriggerExit2D(Collider2D otherCollider)
        {
            if (otherCollider.gameObject.layer != 11) return;
            enemyList.Remove(otherCollider.gameObject);
        }

        private bool isEnemyInRange()
        {
            return enemyList.Count != 0;
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}