using UnityEngine;

namespace redwing
{
    public class false_light : MonoBehaviour
    {
        private void Start()
        {
            log("I don't know what I'm doing here but I did load successfully.");
        }
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}