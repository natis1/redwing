using Modding;
using UnityEngine;
using UnityEngine.UI;

namespace redwing
{
    public class time_attack : MonoBehaviour
    {
        private float timeRemaining;
        private bool didDestroy;
        private Text textObj;
        private GameObject canvas;

        private void Start()
        {   
            if (canvas != null) return;
            
            CanvasUtil.CreateFonts();
            canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            GameObject go =
                CanvasUtil.CreateTextPanel(canvas, "", 27, TextAnchor.MiddleCenter,
                    new CanvasUtil.RectData(
                        new Vector2(0, 0),
                        new Vector2(0, 0),
                        new Vector2(0, 0),
                        new Vector2(1.9f, 1.9f),
                        new Vector2(0.5f, 0.5f)));
            
            
            textObj = go.GetComponent<Text>();
            textObj.color = Color.black;
            textObj.font = CanvasUtil.TrajanBold;
            textObj.text = getTimeInCleanFormat(timeRemaining);
            textObj.fontSize = 50;
            textObj.CrossFadeAlpha(1f, 0f, false);
        }

        private void Update()
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining > 0.0f)
            {
                textObj.text = getTimeInCleanFormat(timeRemaining);
            }
            else if (timeRemaining > 0.0f)
            {
                textObj.color = Color.red;
                textObj.text = "00:00";
            }
            else if (!didDestroy)
            {
                didDestroy = true;
                Destroy(textObj);
                Destroy(canvas);
            }
        }


        public static string getTimeInCleanFormat(float time)
        {
            string seconds = (((int) time) % 60).ToString();
            if (seconds.Length == 1)
            {
                seconds = "0" + seconds;
            }
            string minutes = (((int) time) / 60).ToString();
            return (minutes + ":" + seconds);
        }
    }

}