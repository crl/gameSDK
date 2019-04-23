using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 人物状态 
    /// </summary>
    [RequireComponent(typeof(BaseObject))]
    public class StateModel:FoundationBehaviour
    {
        protected static Dictionary<int, StateVO> relationDictionary=new Dictionary<int, StateVO>();
        protected static Dictionary<string,string> tipsDict = new Dictionary<string, string>();
        public static Action<string> tipAction;
        public static StateVO RegisterStateVO(int stateType, string name = "")
        {
            StateVO stateVO = null;
            if (relationDictionary.TryGetValue(stateType, out stateVO) == false)
            {
                stateVO = new StateVO(stateType);

                relationDictionary.Add(stateType, stateVO);
            }
            stateVO.name = name;
            return stateVO;
        }

        public static StateVO GetStateVO(int stateType)
        {
            StateVO stateVO = null;
            relationDictionary.TryGetValue(stateType, out stateVO);
            return stateVO;
        }

        public static void RegisterTip(int stateType, int limitStateType,string msg)
        {
            string key = stateType + "_" + limitStateType;
            tipsDict[key] = msg;
        }

        /// <summary>
        /// xx状态不可xx
        /// </summary>
        /// <param name="stateType"></param>
        /// <param name="limitStateType"></param>
        /// <returns></returns>
        public static void ShowTips(int stateType, int limitStateType)
        {
            string key = stateType + "_" + limitStateType;
            string msg;
            tipsDict.TryGetValue(key, out msg);

            if (string.IsNullOrEmpty(msg) == false && tipAction != null)
            {
                tipAction(msg);
            }
        }

        protected Dictionary<Type,int> typeStateMaps=new Dictionary<Type, int>();
        protected Dictionary<int, ActorAction> instanceActions=new Dictionary<int, ActorAction>();
        protected List<ActorAction> runingActions=new List<ActorAction>();

        public T registerAction<T>() where T : ActorAction, new()
        {
            Type type = typeof(T);
            int stateID=-1;
            if (typeStateMaps.TryGetValue(type, out stateID) == false)
            {
                T newAction = new T();
                stateID = newAction.stateID;
                newAction.initialize(this);
                typeStateMaps.Add(type, stateID);

                instanceActions[stateID]= newAction;
            }
            return (T)instanceActions[stateID];
        }

        public T getAction<T>() where T : ActorAction, new()
        {
            ActorAction action;
            Type type = typeof(T);
            int stateID = -1;
            if (typeStateMaps.TryGetValue(type, out stateID) == false)
            {
                foreach (KeyValuePair<int, ActorAction> keyValuePair in instanceActions)
                {
                    action = keyValuePair.Value;
                    if (action is T)
                    {
                        typeStateMaps.Add(type, stateID);
                        return (T) action;
                    }
                }
            }
            return (T) instanceActions[stateID];
        }

        public ActorAction getAction(int stateID)
        {
            ActorAction actorAction = null;
            instanceActions.TryGetValue(stateID, out actorAction);
            return actorAction;
        }

        public bool checkCanDo(int stateID, bool tip = true)
        {
            int len = runingActions.Count;
            for (int i = 0; i < len; i++)
            {
                if (runingActions[i].checkCanDo(stateID, tip) == false)
                {
                    return false;
                }
            }
            return true;
        }


        public void doStateAction(int newState, Action<DefActorAction> stopAction = null, params object[] args)
        {
            ActorAction action;
            DefActorAction defActorAction;
            if (instanceActions.TryGetValue(newState, out action) == false)
            {
                defActorAction = new DefActorAction(newState);
                action = defActorAction;
                instanceActions.Add(newState, defActorAction);
            }
            else
            {
                defActorAction=action as DefActorAction;
            }

            if (defActorAction != null)
            {
                defActorAction.stopAction = stopAction;
                defActorAction.args = args;
            }

            doAction(action);
        }

        /// <summary>
        /// 运行一个动作
        /// </summary>
        /// <param name="newState"></param>
        public void doAction(ActorAction newAction)
        {
            int newState = newAction.stateID;
            int runingStateType;
            bool isRunningState = false;
            int len = runingActions.Count;
            if (len > 0)
            {
                List<ActorAction> tempList = SimpleListPool<ActorAction>.Get();
                ActorAction item = null;
                for (int i = len - 1; i > -1; i--)
                {
                    item = runingActions[i];
                    runingStateType = item.stateID;
                    if (runingStateType == newState)
                    {
                        isRunningState = true;
                        continue;
                    }

                    if (newAction.checkTerminateAction(runingStateType) == false)
                    {
                        continue;
                    }

                    runingActions.RemoveAt(i);
                    tempList.Add(item);
                }

                len = tempList.Count;
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        item = tempList[i];
                        item.stop();
                    }
                }
                SimpleListPool<ActorAction>.Release(tempList);
            }

            if (isRunningState)
            {
                newAction.restart();
            }
            else
            {
                newAction.start();
                if (newAction.isFinished == false)
                {
                    runingActions.Add(newAction);
                }
            }
        }

        public bool stopState(params int[] states)
        {
            bool has = false;
            ActorAction action = null;

            List<ActorAction> tempList = SimpleListPool<ActorAction>.Get();
            foreach (int state in states)
            {
                for (int i = runingActions.Count - 1; i > -1; i--)
                {
                    action = runingActions[i];
                    if (action.stateID == state)
                    {
                        has = true;
                        runingActions.RemoveAt(i);
                        tempList.Add(action);
                        break;
                    }
                }
            }

            int len = tempList.Count;
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    action = tempList[i];
                    action.stop();
                }
            }
            SimpleListPool<ActorAction>.Release(tempList);
            return has;
        }

        public bool isRunningState(params int[] stateIDs)
        {
            foreach (int state in stateIDs)
            {
                for (int i = runingActions.Count - 1; i > -1; i--)
                {
                    if (runingActions[i].stateID == state)
                    {
                        return true;
                    }
                }
            }
            return false;
        }



        protected virtual void Update()
        {
            doUpdate(UpdateType.Update,Time.deltaTime);
        }

        /// <summary>
        /// 有需求的自行加入
        /// </summary>
        /// <param name="updateType"></param>
        /// <param name="deltaTime"></param>
        /*protected virtual void FixedUpdate()
        {
            doUpdate(UpdateType.FixedUpdate, Time.fixedDeltaTime);
        }*/

        private void doUpdate(UpdateType updateType,float deltaTime)
        {
            int len = runingActions.Count;
            if (len == 0)
            {
                return;
            }

            List<ActorAction> tempList = SimpleListPool<ActorAction>.Get();
            for (int i =0; i <len; i++)
            {
                ActorAction action = runingActions[i];
                if (action.updateType == updateType)
                {
                    tempList.Add(action);
                }
            }
            len = tempList.Count;
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    ActorAction action = tempList[i];
                    if (action.isFinished == false)
                    {
                        action.update(deltaTime);
                    }
                    if (action.isFinished)
                    {
                        stopState(action.stateID);
                    }
                }
            }
            SimpleListPool<ActorAction>.Release(tempList);
        }

   

        protected override void onDestroy()
        {
            int len = runingActions.Count;
            for (int i = 0; i < len; i++)
            {
               runingActions[i].__onDestroy();
            }
            runingActions.Clear();

            base.onDestroy();
        }
    }

}
