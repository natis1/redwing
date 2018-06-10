using System.Collections;
using UnityEngine;

namespace redwing
{
    public class redwing_laser_behaviors : MonoBehaviour
    {
        private const float LIFESPAN = 0.5f;

        public void Start()
        {
            StartCoroutine(despawn());
        }

        private IEnumerator despawn()
        {
            float currentTime = 0f;
            while (currentTime < LIFESPAN)
            {
                yield return null;
                currentTime += Time.unscaledDeltaTime;
            }
            Modding.Logger.Log("[REDWING] Despawning laser");
            Destroy(this.gameObject);
        }

        private IEnumerator playAnimation()
        {
            
            yield return null;
            
        }
        
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}