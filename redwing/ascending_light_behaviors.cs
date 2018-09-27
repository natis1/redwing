using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redwing
{
    public class ascending_light : MonoBehaviour
    {
        public bool ghostBalls;
        public redwing_pillar_detect_behavior flamePillarDetect;
        public SpriteRenderer ballSprite;
        public float timeBeforeDeath = 2f;
        
        private void Start()
        {
            StartCoroutine(despawnBallOverTime());
        }

        private IEnumerator despawnBallOverTime()
        {
            Color c = ballSprite.color;
            
            for (float time = 0f; time < timeBeforeDeath; time += Time.deltaTime)
            {
                c.a = ( (timeBeforeDeath * 2f) - time) / timeBeforeDeath;
                ballSprite.color = c;
                yield return null;
            }
        }


        // stolen from: Token: 0x0600006E RID: 110 RVA: 0x00005168 File Offset: 0x00003368
        public GameObject getTarget()
        {
            List<GameObject> enemyList = flamePillarDetect.enemyList;
            GameObject result = null;
            float num = 99999f;
            if (enemyList.Count > 0)
            {
                for (int i = enemyList.Count - 1; i > -1; i--)
                {
                    if (enemyList[i] == null || !enemyList[i].activeSelf)
                    {
                        enemyList.RemoveAt(i);
                    }
                }
                foreach (GameObject gameObject in enemyList)
                {
                    // just pick enemy in range
                    if (ghostBalls && gameObject != null)
                    {
                        float sqrMagnitude = (base.transform.position - gameObject.transform.position).sqrMagnitude;
                        if (sqrMagnitude < num)
                        {
                            result = gameObject;
                            num = sqrMagnitude;
                        }

                        // Otherwise also check if you can raycast to them.
                    } else if (!Physics2D.Linecast(base.transform.position, gameObject.transform.position, 256))
                    {
                        float sqrMagnitude = (base.transform.position - gameObject.transform.position).sqrMagnitude;
                        if (sqrMagnitude < num)
                        {
                            result = gameObject;
                            num = sqrMagnitude;
                        }
                    }
                }
            }
            return result;
        }

    }
}