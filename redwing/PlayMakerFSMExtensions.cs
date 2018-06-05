using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace RandomizerMod.Extensions
{
    internal static class play_maker_fsm_extensions
    {
        private static readonly FieldInfo FSM_STRING_PARAMS = typeof(ActionData).GetField("fsmStringParams", BindingFlags.NonPublic | BindingFlags.Instance);

        public static List<FsmString> getStringParams(this ActionData self)
        {
            return (List<FsmString>)FSM_STRING_PARAMS.GetValue(self);
        }

        public static FsmState getState(this PlayMakerFSM self, string name)
        {
            foreach (FsmState state in self.FsmStates)
            {
                if (state.Name == name) return state;
            }

            return null;
        }

        public static void removeActionsOfType<T>(this FsmState self)
        {
            List<FsmStateAction> actions = new List<FsmStateAction>();

            foreach (FsmStateAction action in self.Actions)
            {
                if (!(action is T))
                {
                    actions.Add(action);
                }
            }

            self.Actions = actions.ToArray();
        }

        public static T[] getActionsOfType<T>(this FsmState self) where T : FsmStateAction
        {
            List<T> actions = new List<T>();

            foreach (FsmStateAction action in self.Actions)
            {
                if (action is T)
                {
                    actions.Add((T)action);
                }
            }

            return actions.ToArray();
        }

        public static void clearTransitions(this FsmState self)
        {
            self.Transitions = new FsmTransition[0];
        }

        public static void addTransition(this FsmState self, string eventName, string toState)
        {
            List<FsmTransition> transitions = self.Transitions.ToList();

            FsmTransition trans = new FsmTransition();
            trans.ToState = toState;

            if (FsmEvent.EventListContains(eventName))
            {
                trans.FsmEvent = FsmEvent.GetFsmEvent(eventName);
            }
            else
            {
                trans.FsmEvent = new FsmEvent(eventName);
            }

            transitions.Add(trans);

            self.Transitions = transitions.ToArray();
        }

        public static void addAction(this FsmState self, FsmStateAction action)
        {
            List<FsmStateAction> actions = self.Actions.ToList();
            actions.Add(action);
            self.Actions = actions.ToArray();
        }
    }
}
