using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            } else if (to.name == "Cinematic_Ending_B")
            {
                generateEndingClip(1);
            } else if (to.name == "Cinematic_Ending_C")
            {
                generateEndingClip(2);
            } else if (to.name == "Dream_Final_Boss")
            {
                log("Loading false light control... Best of luck little knight. You will need it...");
                GameObject meme = new GameObject("corruptedRadianceController", typeof(false_light));
            }
            
            
            
        }

        private void generateEndingClip(int ending)
        {
            VideoPlayer v = null;
            if (ending == 0)
            {
                v = loadEnding("ending1.webm");
            } else if (ending == 1)
            {
                v = loadEnding("ending2.webm");
            } else if (ending == 2)
            {
                v = loadEnding("ending3.webm");
            }

            if (v != null)
            {
                GameObject newCinematicPlayer = new GameObject("redwingEnding");
                newCinematicPlayer.AddComponent(v);
            }
        }

        private VideoPlayer loadEnding(string endingName)
        {

            foreach (string res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                log("Found resource with name " + res);
                CinematicPlayer meme;
                
                if (res.Contains(endingName))
                {
                    log("Resource matches " + endingName);
                    
                    /*
                    VideoClip meme = new VideoClip();
                    Stream videoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                    if (videoStream != null)
                    {
                        byte[] buffer = new byte[videoStream.Length];
                        videoStream.Read(buffer, 0, buffer.Length);
                        videoStream.Dispose();
                        
                    }
                    else
                    {
                        log("Unable to load ending because error reading file " + res);
                        return null;
                    }*/
                }
                
            }

            return null;
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}