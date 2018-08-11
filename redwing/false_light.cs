using System;
using System.Collections;
using Modding;
using UnityEngine;
using UnityEngine.UI;

namespace redwing
{
    public class false_light : MonoBehaviour
    {
        private const float timeToAutonuke = 29.8f;
        private bool runningRadiance;
        private const float nukeAnimFrameTime = 0.2f;
        private GameObject voidKnight;
        
        private void Start()
        {
            log("Starting false light load.");
            
            voidKnight = HeroController.instance.spellControl.gameObject;
            
            log("I don't know what I'm doing here but I did load successfully.");
            runningRadiance = true;
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }



        private IEnumerator goNuclear()
        {
            
            for (float time = timeToAutonuke; time > 0.0f; time -= Time.deltaTime)
            {
                if (!runningRadiance)
                {
                    yield break;
                } else if (Input.GetKeyDown(KeyCode.N))
                {
                    break;
                }
                else
                {
                    yield return null;
                }
            }
            if (!runningRadiance) yield break;
            
            PlayerData.instance.health = PlayerData.instance.maxHealth;
            StartCoroutine(HeroController.instance.HazardRespawn());
            StartCoroutine(launchNuke());
            
        }

        private IEnumerator launchNuke()
        {
            yield return new WaitForSeconds(0.2f);
            Vector3 currentKnightPosition = voidKnight.transform.position;
            currentKnightPosition.y += 8f;
            StartCoroutine(freezeKnight(currentKnightPosition));
            
            GameObject plane = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            plane.name = "RedwingDreamWMD";
            plane.SetActive(true);
            Sprite renderSprite = Sprite.Create(load_textures.nukeAnimation[0], new Rect(0, 0,
                load_textures.nukeAnimation[0].width, load_textures.nukeAnimation[0].height), new Vector2(0.5f, 0.5f));
            GameObject nukeRenderer = CanvasUtil.CreateImagePanel(plane, renderSprite, new CanvasUtil.RectData(new Vector2(1920, 1080), new Vector2(0f, 0f)));
            nukeRenderer.name = "RedwingDreamWMDSprite";
            
            yield return new WaitForSeconds(nukeAnimFrameTime);
            for (int i = 1; i < load_textures.nukeAnimation.Length; i++)
            {
                nukeRenderer.GetComponent<Image>().sprite = Sprite.Create(load_textures.nukeAnimation[i], new Rect(0, 0,
                        load_textures.nukeAnimation[i].width, load_textures.nukeAnimation[i].height),
                    new Vector2(0.5f, 0.5f));
                yield return new WaitForSeconds(nukeAnimFrameTime);
            }
            
            runningRadiance = false;
            DestroyImmediate(nukeRenderer);
            DestroyImmediate(plane);
            GameManager.instance.LoadScene("Cinematic_Ending_C");
            Destroy(gameObject);
        }

        private IEnumerator freezeKnight(Vector3 locationToFreeze)
        {
            while (runningRadiance)
            {
                voidKnight.transform.position = locationToFreeze;
                yield return null;
            }
        }
    }
}