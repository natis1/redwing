using UnityEngine;
using Modding;
using System;

namespace redwing
{
    class Greymoth : MonoBehaviour
    {

        /*
         * Declassified things to track:
         * 
         * dash upgrades, did kill nkg.
         * 
         * 
         * */

        public static int firepower;

        // These bools track player upgrades
        public bool dashRegular;
        public bool shadowDash;

        public void OnDestroy()
        {
            ModHooks.Instance.DashPressedHook -= dashTapped;
            ModHooks.Instance.DashVectorHook -= buildDashVector;
        }

        public void Start()
        {

            ModHooks.Instance.DashPressedHook += dashTapped;
            ModHooks.Instance.DashVectorHook += buildDashVector;
        }

        private float getDashLength()
        {
            float length;

            if (HeroController.instance.cState.shadowDashing)
            {
                length = HeroController.instance.DASH_SPEED_SHARP;
            } else
            {
                length = HeroController.instance.DASH_SPEED;
            }


            return length;
        }

        private Vector2 buildDashVector()
        {
            //ModHooks.Instance.DashPressedHook += classifiedFunction1;

            // is avenging I hope
            Vector2 dashAngle = new Vector2();
            dashAngle.x = 1f;
            dashAngle.y = 1f;

            return dashAngle;
        }

        private void dashTapped()
        {
            throw new NotImplementedException();
        }


        private void crystalDashAnime()
        {

        }

        


    }
}
