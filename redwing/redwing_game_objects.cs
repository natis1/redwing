using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;
using Object = UnityEngine.Object;


namespace redwing
{
    internal class redwing_game_objects
    {
        public static Texture2D[] fireBalls;
        public static Texture2D[] fireballMagmas;
        public static Texture2D[] fireballMagmaFireballs;
        public static Texture2D[] pillarTextures;
        
        public static Texture2D[] fireLasers;
        
        public static GameObject voidKnight;
        
        private const float ROTATION_AMOUNT = (float) (360.0 / 16.0);
        

        //private const float TRANSFORM_XOFFSET = 0.69f;

        private GameObject fireballSpawn;
        private GameObject laserSpawn;
        private readonly GameObject[] fireballsGo = new GameObject[7];
        
        // Seriously. Fuck you Unity. I literally just want spinning fireballs.
        private readonly GameObject[] fireballPivotGOs = new GameObject[7];
        
        
        private GameObject shield;
        public readonly GameObject[] lasers = new GameObject[16];

        public static void addSinglePillar(float lifespan)
        {
            GameObject firePillar = new GameObject("redwingFlamePillar", typeof(redwing_pillar_behavior),
                typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
            firePillar.transform.localScale = new Vector3(1f, 1f, 1f);
            
            firePillar.transform.parent = voidKnight.transform;
            firePillar.transform.localPosition = Vector3.zero;
            
            int randomTextureToUse = redwing_flame_gen.rng.Next(0, pillarTextures.Length);
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
            hitEnemies.offset = new Vector2(0, 0);

            firePillar.GetComponent<redwing_pillar_behavior>().lifespan = lifespan;
            
            firePillar.SetActive(true);
        }
        

        public void addLasers()
        {
            if (voidKnight == null)
            {
                return;
            }
            
            laserSpawn = new GameObject("redwingLaserSpawner", typeof(redwing_laser_spawner_behavior),
                typeof(AudioSource));
            Vector3 laserSpawnPos = voidKnight.GetComponent<BoxCollider2D>().bounds.center;
            laserSpawn.transform.position = laserSpawnPos;
            // Why? Because layers 20, 9, 26, and 31 are explicitly ignored by the game
            // and layers above 31 don't exist
            laserSpawn.layer = 31;
            
            float rotationMod = (float)(redwing_flame_gen.rng.NextDouble() * ROTATION_AMOUNT);
            
            for (int i = 0; i < 16; i++)
            {
                lasers[i] = new GameObject("redwingLaser" + i, typeof(SpriteRenderer), typeof(Rigidbody2D),
                    typeof(BoxCollider2D), typeof(redwing_laser_behavior));
                lasers[i].transform.parent = laserSpawn.transform;
                lasers[i].transform.localPosition = Vector3.zero;
                
                lasers[i].layer = 0;
                lasers[i].transform.localScale = new Vector3(1f, 8f, 1f);
                Rigidbody2D laserPhysics = lasers[i].GetComponent<Rigidbody2D>();
                laserPhysics.isKinematic = true;
                
                Rect r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
                SpriteRenderer s = lasers[i].GetComponent<SpriteRenderer>();
                s.sprite = Sprite.Create(fireLasers[i], r, new Vector2(0.5f, 0f), 50);
                s.enabled = true;
                s.color = new Color(1f, 1f, 1f, 0f);
                
                redwing_laser_behavior shootEm = lasers[i].GetComponent<redwing_laser_behavior>();
                shootEm.drawEm = s;
                shootEm.spriteUsed = fireLasers[i];
                shootEm.enteredColliders = new List<Collider2D>();
                
                BoxCollider2D laserPassthrough = lasers[i].GetComponent<BoxCollider2D>();
                laserPassthrough.isTrigger = true;
                laserPassthrough.size = s.size;
                laserPassthrough.offset = new Vector2(0, 0);
                
                
                lasers[i].transform.Rotate(0f, 0f, ROTATION_AMOUNT * i + rotationMod);
            }
        }

        public static void addSingleLaser(float angel)
        {
            GameObject laserSpawnObj = new GameObject("redwingLaserSpawner", typeof(redwing_laser_spawner_behavior),
                typeof(AudioSource));
            Vector3 laserSpawnPos = voidKnight.GetComponent<BoxCollider2D>().bounds.center;
            laserSpawnPos.y += 0.2f;
            laserSpawnObj.transform.position = laserSpawnPos;
            laserSpawnObj.layer = 31;
            
            GameObject laser = new GameObject("redwingLaserSolo", typeof(SpriteRenderer), typeof(Rigidbody2D),
                typeof(BoxCollider2D), typeof(redwing_laser_behavior));
            laser.transform.parent = laserSpawnObj.transform;
            laser.transform.localPosition = Vector3.zero;
            int randomTexture = redwing_flame_gen.rng.Next(0, fireLasers.Length);
            redwing_laser_behavior shootEm = laser.GetComponent<redwing_laser_behavior>();
            Rect r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
            SpriteRenderer s = laser.GetComponent<SpriteRenderer>();
            s.sprite = Sprite.Create(fireLasers[randomTexture], r, new Vector2(0.5f, 0.5f), 50);
            s.enabled = true;
            s.color = new Color(1f, 1f, 1f, 0f);

            shootEm.drawEm = s;
            shootEm.spriteUsed = fireLasers[randomTexture];
            shootEm.soloLaser = true;
            shootEm.enteredColliders = new List<Collider2D>();

                
            laser.layer = 0;
            laser.transform.localScale = new Vector3(1f, 8f, 1f);
            Rigidbody2D laserPhysics = laser.GetComponent<Rigidbody2D>();
            laserPhysics.isKinematic = true;
            
            
            BoxCollider2D laserPassthrough = laser.GetComponent<BoxCollider2D>();
            laserPassthrough.isTrigger = true;
            laserPassthrough.size = s.size;
            laserPassthrough.offset = new Vector2(0, 0);
                
            laser.transform.Rotate(0f, 0f, angel);
            
            laser.SetActive(true);
            
            //Modding.Logger.Log("[Redwing] temp made solo laser ");
        }
        
        public void addFireballs()
        {
            if (voidKnight == null)
            {
                return;
            }
            fireballSpawn = new GameObject("redwingFireballSpawner", typeof(redwing_fireball_spawner_behavior));
            Vector3 fbSpawnPos = voidKnight.GetComponent<BoxCollider2D>().bounds.center;
            fbSpawnPos.y += 0.4f;
            fireballSpawn.transform.position = fbSpawnPos;
            
            fireballSpawn.layer = 31;

            fireballSpawn.SetActive(true);
            for (int i = 0; i < 7; i++)
            {
                
                fireballPivotGOs[i] = new GameObject("redwingFBPivot" + i, typeof(BoxCollider2D),
                    typeof(Rigidbody2D), typeof(redwing_fireball_behavior));
                fireballPivotGOs[i].transform.parent = fireballSpawn.transform;
                fireballPivotGOs[i].transform.localPosition = Vector3.zero;
                
                
                fireballPivotGOs[i].layer = 0;
                
                fireballsGo[i] = new GameObject("redwingFB" + i, typeof(SpriteRenderer), typeof(AudioSource));
                fireballsGo[i].transform.parent = fireballPivotGOs[i].transform;
                fireballsGo[i].transform.localPosition = Vector3.zero;
                
                // Layers 20, 9, 26, and 31 are explicitly ignored by the game
                // and layers above 31 don't exist
                fireballsGo[i].layer = 31;
                
                setupFireballSprite(fireballsGo[i].GetComponent<SpriteRenderer>());
                setupFireballPhysics(fireballPivotGOs[i].GetComponent<Rigidbody2D>());
                setupCustomFireballObject(i);
                
                fireballPivotGOs[i].SetActive(true);
                fireballsGo[i].SetActive(true);
            }
            
        }

        public static void addSingleFireball(float xVelocity, float yVelocity, float xTransform, float yTransform, string nameExtend)
        {
            if (voidKnight == null)
            {
                return;
            }

            GameObject fireballSpawnPoint = new GameObject("redwingFireballSpawner" + nameExtend, typeof(redwing_fireball_spawner_behavior));
            Vector3 fbSpawnPos = voidKnight.GetComponent<BoxCollider2D>().bounds.center;
            fbSpawnPos.y += 0.4f;
            fireballSpawnPoint.transform.position = fbSpawnPos;

            fireballSpawnPoint.layer = 31;

            fireballSpawnPoint.SetActive(true);

            GameObject fireballPivot = new GameObject("redwingFireball" + nameExtend, typeof(BoxCollider2D),
                typeof(Rigidbody2D), typeof(redwing_fireball_behavior));
            fireballPivot.transform.parent = fireballSpawnPoint.transform;
            fireballPivot.transform.localPosition = Vector3.zero;

            fireballPivot.layer = 0;

            GameObject fireballSprite = new GameObject("redwingFireballSprite" + nameExtend, typeof(SpriteRenderer), typeof(AudioSource));
            fireballSprite.transform.parent = fireballPivot.transform;
            fireballSprite.transform.localPosition = Vector3.zero;

            // Layers 20, 9, 26, and 31 are explicitly ignored by the game
            // and layers above 31 don't exist
            // set to layer where it won't be treated like an enemy.
            fireballSprite.layer = 31;

            setupFireballSprite(fireballSprite.GetComponent<SpriteRenderer>());
            setupFireballPhysics(fireballPivot.GetComponent<Rigidbody2D>());
            
            
            setupCustomFireballObject(fireballPivot.GetComponent<redwing_fireball_behavior>(),
                fireballPivot.GetComponent<BoxCollider2D>(), fireballSprite, xVelocity, yVelocity);

            redwing_fireball_behavior b = fireballPivot.GetComponent<redwing_fireball_behavior>();
            
            Vector3 selfPos = b.selfTranform.position;
            selfPos.x += xTransform;
            selfPos.y += yTransform;
            b.selfTranform.position = selfPos;
            b.selfPosition = selfPos;

            // just some basic algebra/calc to get the expected height using Newton laws.
            //double maxXPoint = 3.0 / (yVelocity);
            //b.maxHeight = (float) ( (-1.5 * Math.Pow(maxXPoint, 2.0)) + yVelocity * maxXPoint);
            b.realisticPhysics = true;
            

            fireballPivot.SetActive(true);
            fireballSprite.SetActive(true);
            
        }

        private static void setupCustomFireballObject(redwing_fireball_behavior behavior, BoxCollider2D collide,
            GameObject fireballSprite, float xVelocity, float yVelocity)
        {
            behavior.fireball = fireballSprite;
            
            collide.isTrigger = true;
            
            behavior.selfTranform = behavior.gameObject.transform;
            

            behavior.fireballSprite = fireballSprite.GetComponent<SpriteRenderer>();
            behavior.fireballPhysics = behavior.gameObject.GetComponent<Rigidbody2D>();
            collide.size = behavior.fireballSprite.size;
            Vector2 spriteSize = behavior.fireballSprite.size;
            //collide.offset = new Vector2(spriteSize.x / 2, 0);
            collide.offset = new Vector2(0, 0);
            behavior.rotationalVelocity = (float) ((redwing_flame_gen.rng.NextDouble() - 0.5 ) * 10.0 * 180.0 / Math.PI);

            behavior.xVelocity = xVelocity;
            behavior.yVelocity = yVelocity;
                        
            behavior.hitboxForPivot = collide;
                
            behavior.fireballMagmas = fireballMagmas;
            behavior.doPhysics = true;
            behavior.fireballMagmaFireballs = fireballMagmaFireballs;
            behavior.fireballMagmaFireballHeight = redwing_flame_gen.FBMBTEXTURE_HEIGHT;
            behavior.fireballMagmaFireballWidth = redwing_flame_gen.FBMBTEXTURE_WIDTH;

            behavior.cachedAudioPlayer = fireballSprite.GetComponent<AudioSource>();
            

        }

        private void setupCustomFireballObject(int i)
        {
            redwing_fireball_behavior behavior = fireballPivotGOs[i].GetComponent<redwing_fireball_behavior>();
            BoxCollider2D collide = fireballPivotGOs[i].GetComponent<BoxCollider2D>();
            
            float xVelocity;
            float xTransform = 0f;
            switch (i)
            {
                case 0:
                    xVelocity = 0f;
                    break;
                case 1:
                    xVelocity = 1.666f;
                    xTransform += 1f;
                    break;
                case 2:
                    xVelocity = -1.666f;
                    xTransform -= 1f;
                    break;
                case 3:
                    xVelocity = 3.333f;
                    xTransform += 2f;
                    break;
                case 4:
                    xVelocity = -3.333f;
                    xTransform -= 2f;
                    break;
                case 5:
                    xVelocity = 5f;
                    xTransform += 3f;
                    break;
                case 6:
                    xVelocity = -5f;
                    xTransform -= 3f;
                    break;
                default:
                    xVelocity = 0f;
                    break;
            }
            const float yVelocity = 30f;
            
            setupCustomFireballObject(behavior, collide, fireballsGo[i], xVelocity, yVelocity);
            
            Vector3 selfPos = behavior.selfTranform.position;
            selfPos.x += xTransform;
            behavior.selfTranform.position = selfPos;
            behavior.selfPosition = selfPos;
            
        }

        private static void setupFireballSprite(SpriteRenderer sprite)
        {
            Rect r = new Rect(0, 0, redwing_flame_gen.FBTEXTURE_WIDTH, redwing_flame_gen.FBTEXTURE_HEIGHT);
            int randomFireball = redwing_flame_gen.rng.Next(0, fireBalls.Length);
            sprite.sprite = Sprite.Create(fireBalls[randomFireball], r, new Vector2(0.5f, 0.5f));
            sprite.enabled = true;
            sprite.color = Color.white;
            
        }

        private static void setupFireballPhysics(Rigidbody2D physics)
        {
            physics.isKinematic = true;
            physics.gravityScale = 0f;
        }
        
        
        public static void applyHitInstance(GameObject target, int expectedDamage, AttackTypes damageType,
            GameObject source)
        {
            
            int realDamage = expectedDamage;
            
            double multiplier = 1;
            if (PlayerData.instance.GetBool("equippedCharm_25"))
            {
                multiplier *= 1.5;
            }
            if (PlayerData.instance.GetBool("equippedCharm_6") && PlayerData.instance.GetInt("health") == 1)
            {
                multiplier *= 1.75f;
            }

            realDamage = (int) Math.Round(realDamage * multiplier);
            
            if (realDamage <= 0)
            {
                return;
            }

            HealthManager targetHP = getHealthManagerRecursive(target);

            if (targetHP == null) return;
            
            //Modding.Logger.Log("[Redwing] Doing " + realDamage + " damage with attack name " + source.name);

            targetHP.hp -= realDamage;

            if (targetHP.hp <= 0f)
            {
                targetHP.Die(0f, AttackTypes.Generic, true);
            }
            
            FSMUtility.SendEventToGameObject(targetHP.gameObject, "BLOCKED HIT", false);
            
            FSMUtility.SendEventToGameObject(source, "HIT LANDED", false);
            if ((Object) targetHP.gameObject.GetComponent<DontClinkGates>() != (Object) null) return;
            
            FSMUtility.SendEventToGameObject(targetHP.gameObject, "HIT", false);    
            GameManager.instance.FreezeMoment(1);
            GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

            napalm memes = targetHP.gameObject.GetOrAddComponent<napalm>();
            memes.addNapalm(12.0, Color.green);
            /*
            targetHP.Hit(new HitInstance
            {
                Source = source,
                AttackType = damageType,
                CircleDirection = false,
                DamageDealt = realDamage,
                Direction = 0f,
                IgnoreInvulnerable = true,
                MagnitudeMultiplier = 1f,
                MoveAngle = 0f,
                MoveDirection = false,
                Multiplier = 1f,
                SpecialType = SpecialTypes.None,
                IsExtraDamage = false
            });*/
        }

        public static void addNapalm(GameObject target, double fireToAdd, Color fireColor)
        {
            HealthManager hm = getHealthManagerRecursive(target);
            napalm n = hm.gameObject.GetOrAddComponent<napalm>();
            n.addNapalm(fireToAdd, fireColor);
        }

        private static HealthManager getHealthManagerRecursive(GameObject target)
        {
            HealthManager targetHP = target.GetComponent<HealthManager>();
            int i = 6;
            while (targetHP == null && i > 0)
            {
                targetHP = target.GetComponent<HealthManager>();
                if (target.transform.parent == null)
                {
                    return targetHP;
                }
                target = target.transform.parent.gameObject;
                i--;
            }
            return targetHP;
        }
        
    }
}
