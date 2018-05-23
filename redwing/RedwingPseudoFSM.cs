using UnityEngine;
using Modding;
using System;

namespace redwing
{
    class RedwingPseudoFSM : MonoBehaviour
    {

        /*
         * Declassified things to track:
         * 
         * TODO: Determine names of these things as tracked by game.
         * 
         * Charms: Mark of pride, longnail, quickslash, dashmaster, sprintmaster (eww)
         * Heavy blow, Grubberfly elegy, Steady body (eww), Soul eater, catcher, Grimmchild
         * 
         * Abilities: mothwing, shadow dash, in sharp shadow
         * 
         * Other: Killed NKG? IG damage (optional). Got void heart?
         * 
         * 
         * */

        public static int firepower;

        // These bools track player upgrades
        public bool classified1;
        public bool classified2;

        public void OnDestroy()
        {
            ModHooks.Instance.DashPressedHook -= classifiedFunction1;
            ModHooks.Instance.DashVectorHook -= classifiedFunction2;
        }

        public void Start()
        {

            ModHooks.Instance.DashPressedHook += classifiedFunction1;
            ModHooks.Instance.DashVectorHook += classifiedFunction2;
        }

        private float classifiedFunction3()
        {
            float classifiedFloat1;

            // important charms:
            //
            // dashmaster, sprintmaster

            // longnail, mark of pride

            if (HeroController.instance.cState.shadowDashing)
            {
                classifiedFloat1 = HeroController.instance.DASH_SPEED_SHARP;
            } else
            {
                classifiedFloat1 = HeroController.instance.DASH_SPEED;
            }


            return classifiedFloat1;
        }

        private Vector2 classifiedFunction2()
        {
            //ModHooks.Instance.DashPressedHook += classifiedFunction1;

            Vector2 classifiedVector1 = new Vector2();
            classifiedVector1.x = 1f;
            classifiedVector1.y = 1f;

            return classifiedVector1;
        }

        private void classifiedFunction1()
        {
            throw new NotImplementedException();
        }


        private void classifiedAnimeFlight()
        {

        }

        


    }
}
