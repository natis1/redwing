using Modding;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ModCommon;
using HutongGames.PlayMaker;
using System.Collections;
using System.Reflection;


namespace redwing
{
    class RedwingGOs : MonoBehaviour
    {
        public static Texture2D[] fireBalls;
        public static Texture2D[] fireTrails;
        public static Texture2D[] firePillars;
        public static Texture2D[] fireLasers;
        public static AudioClip[] soundFXClip;

        public static GameObject voidKnight;
        public static int fireballLength = 150;

        public static int FBDamage = 10;

        public readonly float TRANSFORM_XOFFSET = 2.77f;

        GameObject FireballSpawn;
        GameObject[] FireballsGO = new GameObject[7];

        GameObject shield;
        GameObject[] lasers = new GameObject[16];

        public void OnDestroy()
        {

        }

        public void Start()
        {

        }


        public void AddFireballs()
        {
            if (voidKnight == null)
            {
                return;
            }
            FireballSpawn = new GameObject("redwingFireballSpawner", typeof(RedwingFireballSpawnerBehavior));
            FireballSpawn.transform.position = RedwingGOs.voidKnight.GetComponent<BoxCollider2D>().bounds.center;
            Vector3 fbSpawnPos = FireballSpawn.transform.position;
            fbSpawnPos.x = fbSpawnPos.x - TRANSFORM_XOFFSET;


            FireballSpawn.SetActive(true);
            for (int i = 0; i < 7; i++)
            {
                FireballsGO[i] = new GameObject("redwingFB" + i, typeof(DamageEnemies), typeof(SpriteRenderer), typeof(IgnoreHeroCollision), typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(RedwingFireballBehavior));
                FireballsGO[i].transform.localPosition = FireballSpawn.transform.position;
                FireballsGO[i].transform.parent = FireballSpawn.transform;
                SetupFireballPhysics(FireballsGO[i].GetComponent<Rigidbody2D>(), FireballsGO[i].GetComponent<BoxCollider2D>(), i);
                SetupFireballDamage(FireballsGO[i].GetComponent<DamageEnemies>());
                SetupFireballSprite(FireballsGO[i].GetComponent<SpriteRenderer>(), i);
                SetupCustomObject(FireballsGO[i].GetComponent<RedwingFireballBehavior>(), i);
                
                FireballsGO[i].SetActive(true);
            }
            Log("Spawned in fireballs");
            
        }

        private void SetupCustomObject(RedwingFireballBehavior behavior, int i)
        {
            behavior.fireball = FireballsGO[i];
        }

        public void SetupFireballSprite(SpriteRenderer sprite, int i)
        {
            Rect r = new Rect(0, 0, fireballLength, fireballLength);
            sprite.sprite = Sprite.Create(fireBalls[i], r, Vector2.zero);
            sprite.enabled = true;
            sprite.color = Color.white;
        }

        public void SetupFireballDamage(DamageEnemies hitbox)
        {
            hitbox.ignoreInvuln = false;
            hitbox.damageDealt = FBDamage;
            hitbox.attackType = AttackTypes.Spell;

        }

        public void SetupFireballPhysics(Rigidbody2D physics, BoxCollider2D collide, int i)
        {
            physics.mass = 1.0f;
            physics.drag = 0.0f;
            physics.angularVelocity = 0.05f;
            physics.isKinematic = false;

            collide.size = new Vector2(1.0f, 1.0f);
            
            
            
            switch (i)
            {
                case 0:
                    physics.velocity = new Vector2(0, 6f);
                    break;
                case 1:
                    physics.velocity = new Vector2(2f, 6f);
                    break;
                case 2:
                    physics.velocity = new Vector2(-2f, 6f);
                    break;
                case 3:
                    physics.velocity = new Vector2(4f, 6f);
                    break;
                case 4:
                    physics.velocity = new Vector2(-4f, 6f);
                    break;
                case 5:
                    physics.velocity = new Vector2(6f, 6f);
                    break;
                case 6:
                    physics.velocity = new Vector2(-6f, 6f);
                    break;
            }
            
        }

        public void Log(String str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }



    }
}
