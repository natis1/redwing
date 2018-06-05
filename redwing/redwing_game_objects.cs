using UnityEngine;


namespace redwing
{
    internal class redwing_game_objects
    {
        public static Texture2D[] fireBalls;
        public static Texture2D[] fireTrails;
        public static Texture2D[] firePillars;
        public static Texture2D[] fireLasers;
        public static AudioClip[] soundFxClip;

        public static GameObject voidKnight;
        
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static int fireballLength = 150;
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static int fbDamage = 10;

        private const float TRANSFORM_XOFFSET = 0.69f;

        private GameObject fireballSpawn;
        private readonly GameObject[] fireballsGo = new GameObject[7];

        private GameObject shield;
        private GameObject[] lasers = new GameObject[16];
        
        
        public void addFireballs()
        {
            if (voidKnight == null)
            {
                return;
            }
            fireballSpawn = new GameObject("redwingFireballSpawner", typeof(redwing_fireball_spawner_behavior));
            fireballSpawn.transform.position = voidKnight.GetComponent<BoxCollider2D>().bounds.center;
            Vector3 fbSpawnPos = fireballSpawn.transform.position;
            fbSpawnPos.x = fbSpawnPos.x - TRANSFORM_XOFFSET;
            fireballSpawn.transform.position = fbSpawnPos;


            fireballSpawn.SetActive(true);
            for (int i = 0; i < 7; i++)
            {
                fireballsGo[i] = new GameObject("redwingFB" + i, typeof(DamageEnemies), typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(redwing_fireball_behavior));
                fireballsGo[i].layer = 169;
                fireballsGo[i].transform.localPosition = fireballSpawn.transform.position;
                fireballsGo[i].transform.parent = fireballSpawn.transform;
                setupFireballDamage(fireballsGo[i].GetComponent<DamageEnemies>());
                setupFireballSprite(fireballsGo[i].GetComponent<SpriteRenderer>(), i);
                setupCustomObject(fireballsGo[i].GetComponent<redwing_fireball_behavior>(), fireballsGo[i].GetComponent<BoxCollider2D>(), i);
                
                fireballsGo[i].SetActive(true);
            }
            log("Spawned in fireballs");
            
        }

        private void setupCustomObject(redwing_fireball_behavior behavior, BoxCollider2D collide, int i)
        {
            behavior.fireball = fireballsGo[i];
            collide.size = new Vector2(1.0f, 1.0f);
            
            behavior.selfTranform = fireballsGo[i].transform;
            Vector3 selfPos = behavior.selfTranform.localPosition;
            
            
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

            behavior.selfTranform.localPosition = selfPos;
            behavior.selfPosition = selfPos;
            behavior.yVelocity = 50f;
            behavior.doPhysics = true;


        }

        private static void setupFireballSprite(SpriteRenderer sprite, int i)
        {
            Rect r = new Rect(0, 0, fireballLength, fireballLength);
            sprite.sprite = Sprite.Create(fireBalls[i], r, Vector2.zero);
            sprite.enabled = true;
            sprite.color = Color.white;
        }

        private static void setupFireballDamage(DamageEnemies hitbox)
        {
            hitbox.ignoreInvuln = false;
            hitbox.damageDealt = fbDamage;
            hitbox.attackType = AttackTypes.Spell;

        }

        private static void setupFireballPhysics(Rigidbody2D physics, BoxCollider2D collide, int i)
        {
            physics.mass = 1.0f;
            physics.drag = 0.0f;
            physics.angularVelocity = 0.05f;
            physics.isKinematic = true;

            //collide.size = new Vector2(1.0f, 1.0f);
            //collide.isActiveAndEnabled = false;

            
            
            
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
                default:
                    physics.velocity = new Vector2(0f, 0f);
                    
                    break;
            }
            
        }

        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }



    }
}
