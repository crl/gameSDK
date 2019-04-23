using System;

namespace gameSDK
{
    public class CoreState
    {
        public static StateModel stateModel;
        public static bool CheckCanDo(int state, bool tip = true)
        {
            if (stateModel == null)
            {
                return true;
            }
            return stateModel.checkCanDo(state, tip);
        }

        public static bool IsRunningState(int state)
        {
            if (stateModel == null)
            {
                return false;
            }
            return stateModel.isRunningState(state);
        }

        public static bool StopState(int state)
        {
            if (stateModel == null)
            {
                return false;
            }
            return stateModel.stopState(state);
        }

        public static void DoState(int stateID, Action<DefActorAction> action = null, params object[] args)
        {
            if (stateModel == null)
            {
                return;
            }
            stateModel.doStateAction(stateID, action, args);
        }

        public static void DoAction(ActorAction action)
        {
            if (stateModel == null)
            {
                return;
            }
            stateModel.doAction(action);
        }
    }
}
