using UnityEngine;


// Radiance help and forgive me if I ever need to use this class for anything.
namespace redwing
{
    public class redwing_hacky_workarounds : MonoBehaviour
    {
        private void OnDestroy()
        {
            
        }


        private void Start()
        {
            
        }
        
        
        private static void log(string str)
        {
            Modding.Logger.Log("[Redwing] " + str);
        }
    }
}