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
        public static int damagePriBase;
        public static int damagePriNail;
        public static int damageSecBase;
        public static int damageSecNail;
        public static int damageSecondaryTimes;


        private int cachedPrimaryDmg;
        private int cachedSecondaryDmg;
        
        
        private void Start()
        {
            enteredColliders = new List<Collider2D>();

            cachedPrimaryDmg = damagePriBase + damagePriNail * PlayerData.instance.GetInt("nailSmithUpgrades");
            cachedSecondaryDmg = damageSecBase + damageSecNail * PlayerData.instance.GetInt("nailSmithUpgrades");
            
            StartCoroutine(fadeOut());
            StartCoroutine(destroyPillar());
        }

        private IEnumerator fadeOut()
        {
            float life = 0f;
            SpriteRenderer cachedSprite = this.gameObject.GetComponent<SpriteRenderer>();
            Color cachedColor = cachedSprite.color;

            while (life < LIFESPAN)
            {
                life += Time.deltaTime;
                cachedColor.a = (0.1f + LIFESPAN - life) / LIFESPAN;
                cachedSprite.color = cachedColor;
                yield return null;
            }

            cachedColor.a = 0f;
            cachedSprite.color = cachedColor;
        }

        private IEnumerator destroyPillar()
        {
            float life = 0f;
            int secondaryAttacks = 0;
            yield return null;
            primaryDamage();
            
            while (secondaryAttacks < damageSecondaryTimes)
            {
                yield return new WaitForSeconds(LIFESPAN / damageSecondaryTimes);
                secondaryDamage();
                secondaryAttacks++;
            }
            Destroy(this.gameObject);
        }


        private void primaryDamage()
        {
            foreach (Collider2D collider in enteredColliders.ToList())
            {
                HealthManager targetHP = collider.gameObject.GetComponent<HealthManager>();
                if (targetHP != null)
                {
                    targetHP.Hit(new HitInstance
                    {
                        Source = base.gameObject,
                        AttackType = AttackTypes.Spell,
                        CircleDirection = false,
                        DamageDealt = cachedPrimaryDmg,
                        Direction = 0f,
                        IgnoreInvulnerable = true,
                        MagnitudeMultiplier = 1f,
                        MoveAngle = 0f,
                        MoveDirection = false,
                        Multiplier = 1f,
                        SpecialType = SpecialTypes.None,
                        IsExtraDamage = false
                    });
                }
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
            foreach (Collider2D collider in enteredColliders.ToList())
            {

                HealthManager targetHP = collider.gameObject.GetComponent<HealthManager>();
                if (targetHP != null)
                {
                    targetHP.Hit(new HitInstance
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
                    });
                }
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
            firePillar = new GameObject("redwingFlamePillar", typeof(redwing_pillar_behavior),
                typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
            firePillar.transform.localScale = new Vector3(1f, 1f, 1f);
            GameObject fireAtJerk = null;

            if (isEnemyInRange())
            {
                try
                {
                    fireAtJerk = firePillarTarget();
                }
                catch (Exception e)
                {
                    log("spawn fire pillar failed with error " + e);
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
            img.sprite = Sprite.Create(pillarTextures[randomTextureToUse], pillarSpriteRect,
                new Vector2(0.5f, 0.5f), 30f);
            img.color = Color.white;

            Rigidbody2D fakePhysics = firePillar.GetComponent<Rigidbody2D>();
            fakePhysics.isKinematic = true;
            BoxCollider2D hitEnemies = firePillar.GetComponent<BoxCollider2D>();
            hitEnemies.isTrigger = true;
            hitEnemies.size = img.size;
            hitEnemies.offset = new Vector2(img.size.x / 2, 0);
            
            firePillar.SetActive(true);
        }
        
        // From grimmchild upgrades and: Token: 0x0600006E RID: 110 RVA: 0x00005168 File Offset: 0x00003368
        private GameObject firePillarTarget()
        {
            GameObject result = null;
            float num = 99999f;
            if (enemyList.Count <= 0) return null;
            
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