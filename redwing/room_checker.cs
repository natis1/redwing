using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ModCommon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace redwing
{
    public class room_checker : MonoBehaviour
    {
        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += getCurrentScene;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= getCurrentScene;
        }

        private void getCurrentScene(Scene from, Scene to)
        {
            if (to.name == "Cinematic_Ending_A")
            {
                generateEndingClip(0);
            }
            else if (to.name == "Cinematic_Ending_B")
            {
                generateEndingClip(1);
            }
            else if (to.name == "Cinematic_Ending_C")
            {
                generateEndingClip(2);
            }
            else if (to.name == "Dream_Final_Boss")
            {
                log("Loading false light control... Best of luck little knight. You will need it...");
                GameObject meme = new GameObject("corruptedRadianceController", typeof(false_light));
            }
        }

        private void generateEndingClip(int ending)
        {
            if (ending == 0)
            {
                StartCoroutine(loadEndingDelay("ending1.webm"));
            }
            else if (ending == 1)
            {
                StartCoroutine(loadEndingDelay("ending2.webm"));
            }
            else if (ending == 2)
            {
                StartCoroutine(loadEndingDelay("ending3.webm"));
            }

            //Destroy(GameObject.Find("Cinematic Player"));
        }

        private static void loadEnding(string endingName)
        {
            string videoPath = Application.dataPath + "/Managed/Mods/redwing/" + endingName;
            log("Loading video from path " + videoPath);

            if (!File.Exists(videoPath))
            {
                log("Custom redwing cutscenes are NOT installed. This is not an error.");
                log("Falling back to vanilla cutscenes because why not?");
                GameObject o = new GameObject("redwingEnding");
                return;
            }

            GameObject oldRenderer = GameObject.Find("Cinematic Player");
            
            
            GameObject newVideoPlayer = new GameObject("redwingEnding", typeof(VideoPlayer), typeof(AudioSource),
                typeof(video_blanker));

            newVideoPlayer.transform.position = oldRenderer.transform.position;
            newVideoPlayer.transform.rotation = oldRenderer.transform.rotation;

            AudioSource a = newVideoPlayer.GetComponent<AudioSource>();
            a.volume = (GameManager.instance.gameSettings.masterVolume *
                        GameManager.instance.gameSettings.soundVolume * 0.01f);
            a.playOnAwake = false;
            a.Stop();
            a.bypassEffects = true;
            
            VideoPlayer v = newVideoPlayer.GetOrAddComponent<VideoPlayer>();
            
            v.audioOutputMode = VideoAudioOutputMode.AudioSource;
            v.SetTargetAudioSource(0, a);

            v.url = videoPath;
            v.renderMode = VideoRenderMode.CameraNearPlane;
            v.isLooping = false;
            v.playOnAwake = false;
            v.waitForFirstFrame = true;
            
            v.audioOutputMode = VideoAudioOutputMode.Direct;
            v.targetCamera = Camera.current;
            v.aspectRatio = VideoAspectRatio.FitInside;
            
            v.Prepare();
            DestroyImmediate(oldRenderer);
        }

        private static IEnumerator loadEndingDelay(string endingName)
        {
            //yield return new WaitForSeconds(0.5f);
            yield return null;
            loadEnding(endingName);
            
            VideoPlayer rwe = GameObject.Find("redwingEnding").GetComponent<VideoPlayer>();
            if (rwe == null)
            {
                yield break;
            }
            while (!rwe.isPrepared)
            {
                log("Preparing video");
                yield return new WaitForSeconds(0.5f);
            }
            rwe.Play();
            GameObject.Find("redwingEnding").GetComponent<AudioSource>().Play();
        }

        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}