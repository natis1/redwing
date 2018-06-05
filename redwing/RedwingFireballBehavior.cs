using System;
using System.Collections;
using UnityEngine;

namespace redwing
{
    internal class RedwingFireballSpawnerBehavior : MonoBehaviour
    {
        public readonly float LIFESPAN = 1.75f;

        public void Start()
        {
            StartCoroutine(Despawn());
        }

        IEnumerator Despawn()
        {
            float currentTime = 0f;
            while (currentTime < LIFESPAN)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }
            Modding.Logger.Log("[REDWING] Despawning fb because time ran out I guess");
            UnityEngine.Object.Destroy(fbSpawn);
        }

        public GameObject fbSpawn;
    }

    internal class RedwingFireballBehavior : MonoBehaviour
    {
        public readonly float LIFESPAN = 1.5f;

        public void Start()
        {
            StartCoroutine(Despawn());
        }

        IEnumerator Despawn()
        {
            float currentTime = 0f;
            while (currentTime < LIFESPAN)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }
            Modding.Logger.Log("[REDWING] Despawning fb because time ran out I guess");
            UnityEngine.Object.Destroy(this.fireball);
        }

        public void OnTriggerEnter2D(Collider2D hitbox)
        {
            if (hitbox.IsTouchingLayers(11) || hitbox.IsTouchingLayers(8))
            {
                
                UnityEngine.Object.Destroy(this.fireball);
            }
        }


        public GameObject fireball;
    }
}
