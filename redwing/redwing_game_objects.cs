using System;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;


namespace redwing
{
    internal class redwing_game_objects
    {
        public static Texture2D[] fireBalls;
        public static Texture2D[] fireballMagmas;
        public static Texture2D[] fireballMagmaFireballs;
        
        public static Texture2D[] fireTrails;
        public static Texture2D[] firePillars;
        public static Texture2D[] fireLasers;
        
        public GameObject voidKnight;
        
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static int fireballLength = 150;
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static int fbDamage = 25; 

        private const float rotationAmount = (float) (360.0 / 16.0);
        

        //private const float TRANSFORM_XOFFSET = 0.69f;

        private GameObject fireballSpawn;
        private GameObject laserSpawn;
        private readonly GameObject[] fireballsGo = new GameObject[7];
        
        // Seriously. Fuck you Unity. I literally just want spinning fireballs.
        private readonly GameObject[] fireballPivotGOs = new GameObject[7];
        
        
        private GameObject shield;
        public readonly GameObject[] lasers = new GameObject[16];
        

        public void addLasers()
        {
            if (voidKnight == null)
            {
                return;
            }
            
            laserSpawn = new GameObject("redwingFireballSpawner", typeof(redwing_laser_spawner_behavior),
                typeof(AudioSource));
            Vector3 laserSpawnPos = voidKnight.GetComponent<BoxCollider2D>().bounds.center;
            laserSpawn.transform.position = laserSpawnPos;
            // Why? Because layers 20, 9, 26, and 31 are explicitly ignored by the game
            // and layers above 31 don't exist
            laserSpawn.layer = 31;
            
            float rotationMod = (float)(redwing_flame_gen.rng.NextDouble() * rotationAmount);
            
            for (int i = 0; i < 16; i++)
            {
                lasers[i] = new GameObject("redwingLaser" + i, typeof(SpriteRenderer), typeof(Rigidbody2D),
                    typeof(BoxCollider2D), typeof(redwing_laser_behavior));
                lasers[i].transform.parent = laserSpawn.transform;
                lasers[i].transform.localPosition = Vector3.zero;
                
                lasers[i].layer = 0;
                lasers[i].transform.localScale = new Vector3(2f, 12f, 2f);
                Rigidbody2D laserPhysics = lasers[i].GetComponent<Rigidbody2D>();
                laserPhysics.isKinematic = true;
                
                Rect r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
                SpriteRenderer s = lasers[i].GetComponent<SpriteRenderer>();
                s.sprite = Sprite.Create(fireLasers[i], r, new Vector2(0.5f, 0f));
                s.enabled = true;
                s.color = new Color(1f, 1f, 1f, 0f);
                BoxCollider2D laserPassthrough = lasers[i].GetComponent<BoxCollider2D>();
                laserPassthrough.isTrigger = true;
                laserPassthrough.size = s.size;
                Vector2 spriteSize = s.size;
                laserPassthrough.offset = new Vector2(spriteSize.x / 2, 0);
                redwing_laser_behavior shootEm = lasers[i].GetComponent<redwing_laser_behavior>();
                shootEm.drawEm = s;
                shootEm.spriteUsed = fireLasers[i];
                
                lasers[i].transform.Rotate(0f, 0f, rotationAmount * i + rotationMod);
            }
            
            
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
                //fireballPivotGOs[i].transform.position = Vector3.zero;
                fireballPivotGOs[i].transform.localPosition = Vector3.zero;
                
                
                fireballPivotGOs[i].layer = 0;
                
                fireballsGo[i] = new GameObject("redwingFB" + i, typeof(SpriteRenderer), typeof(AudioSource));
                fireballsGo[i].transform.parent = fireballPivotGOs[i].transform;
                //fireballsGo[i].transform.position = Vector3.zero;
                fireballsGo[i].transform.localPosition = Vector3.zero;
                
                // Layers 20, 9, 26, and 31 are explicitly ignored by the game
                // and layers above 31 don't exist
                fireballsGo[i].layer = 31;
                
                setupFireballSprite(fireballsGo[i].GetComponent<SpriteRenderer>(), i);
                setupFireballPhysics(fireballPivotGOs[i].GetComponent<Rigidbody2D>());
                setupCustomFireballObject(fireballPivotGOs[i].GetComponent<redwing_fireball_behavior>(),
                    fireballPivotGOs[i].GetComponent<BoxCollider2D>(), i);
                
                fireballPivotGOs[i].SetActive(true);
                fireballsGo[i].SetActive(true);
            }
            
        }

        private void setupCustomFireballObject(redwing_fireball_behavior behavior, BoxCollider2D collide, int i)
        {
            behavior.fireball = fireballsGo[i];
            
            collide.isTrigger = true;
            
            behavior.selfTranform = fireballPivotGOs[i].transform;
            
            Vector3 selfPos = behavior.selfTranform.position;

            behavior.fireballSprite = fireballsGo[i].GetComponent<SpriteRenderer>();
            behavior.fireballPhysics = fireballPivotGOs[i].GetComponent<Rigidbody2D>();
            collide.size = behavior.fireballSprite.size;
            Vector2 spriteSize = behavior.fireballSprite.size;
            collide.offset = new Vector2(spriteSize.x / 2, 0);
            behavior.rotationalVelocity = (float) ((redwing_flame_gen.rng.NextDouble() - 0.5 ) * 10.0 * 180.0 / Math.PI);
            
            switch (i)
            {
                case 0:
                    behavior.xVelocity = 0f;
                    break;
                case 1:
                    behavior.xVelocity = 1.666f;
                    selfPos.x += 1f;
                    break;
                case 2:
                    behavior.xVelocity = -1.666f;
                    selfPos.x -= 1f;
                    break;
                case 3:
                    behavior.xVelocity = 3.333f;
                    selfPos.x += 2f;
                    break;
                case 4:
                    behavior.xVelocity = -3.333f;
                    selfPos.x -= 2f;
                    break;
                case 5:
                    behavior.xVelocity = 5f;
                    selfPos.x += 3f;
                    break;
                case 6:
                    behavior.xVelocity = -5f;
                    selfPos.x -= 3f;
                    break;
                default:
                    behavior.xVelocity = 0f;
                    break;
            }

            behavior.selfTranform.position = selfPos;
            
            behavior.selfPosition = selfPos;
            behavior.yVelocity = 30f;
            behavior.hitboxForPivot = collide;
                
            behavior.fireballMagmas = fireballMagmas;
            behavior.doPhysics = true;
            behavior.fireballDmg = fbDamage;
            behavior.fireballMagmaFireballs = fireballMagmaFireballs;
            behavior.fireballMagmaFireballHeight = redwing_flame_gen.FBMBTEXTURE_HEIGHT;
            behavior.fireballMagmaFireballWidth = redwing_flame_gen.FBMBTEXTURE_WIDTH;

            behavior.cachedAudioPlayer = fireballsGo[i].GetComponent<AudioSource>();

        }

        private static void setupFireballSprite(SpriteRenderer sprite, int i)
        {
            Rect r = new Rect(0, 0, fireballLength, fireballLength);
            sprite.sprite = Sprite.Create(fireBalls[i], r, new Vector2(0.5f, 0.5f));
            sprite.enabled = true;
            sprite.color = Color.white;
            
        }

        private static void setupFireballPhysics(Rigidbody2D physics)
        {
            physics.isKinematic = true;
            physics.gravityScale = 0f;
        }

        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }



    }
}
