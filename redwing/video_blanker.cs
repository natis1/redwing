using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace redwing
{
    public class video_blanker : MonoBehaviour
    {
        private VideoPlayer v;
        
        private void Start()
        {
            v = gameObject.GetComponent<VideoPlayer>();
            StartCoroutine(blankVideo());
        }

        private IEnumerator blankVideo()
        {
            if (!v.isPlaying)
            {
                while (!v.isPlaying)
                {
                    yield return null;
                }
            }
            
            while (v.isPlaying)
            {
                yield return null;
            }
            // Code to trigger credits goes here
            Modding.Logger.Log("Switching to end credits scene.");
            GameManager._instance.LoadScene("End_Credits");
            //Destroy(gameObject);
            
        }
    }
}