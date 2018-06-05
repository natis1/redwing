using System;
using UnityEngine;

namespace redwing
{
    internal class RedwingFireballBehavior : MonoBehaviour
    {
        public void OnCollisionEnter(Collider2D hitbox)
        {
            bool flag = hitbox.IsTouchingLayers(11) || hitbox.IsTouchingLayers(8);
            if (flag)
            {
                UnityEngine.Object.Destroy(this.fireball);
            }
        }


        public GameObject fireball;
    }
}
