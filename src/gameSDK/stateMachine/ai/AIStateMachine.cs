using System;
using System.Collections.Generic;
using gameSDK;
using UnityEngine;

namespace foundation
{
    /// <summary>
    /// ai使用分层状态机(状态退出 回上一个状态，或者使用初始状态)
    /// </summary>
    public class AIStateMachine:MonoBehaviour
    {
        /// <summary>
        /// ai开关，默认是开着的
        /// </summary>
        public static bool aiSwitch = true;

        protected ASDictionary blackboard=new ASDictionary();
        protected Dictionary<string, IAIState> _mapStates;
        protected Stack<IAIState> _historyStates;
        protected List<IGoal> _goalList; 
        protected IAIState _initState;
        public float updateLimit = 0.1f;
        private float preTime=0;
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool pause = false;
        public AIStateMachine()
        {
            _historyStates =new Stack<IAIState>();
            _goalList=new List<IGoal>();
            _mapStates=new Dictionary<string, IAIState>();
        }

        protected virtual void Awake()
        {
        }

        public void addProperty(string key,object o)
        {
            blackboard.Add(key,o);
        }

        public T getProperty<T>(string key) where T : class
        {
            return blackboard.getValue<T>(key);
        }

        public void removeProperty(string key)
        {
            blackboard.Remove(key);
        }

        public void updateProperties(params Type[] list)
        {
            if (list.Length > 0)
            {

            }
            else
            {
                foreach (IAIState state in _mapStates.Values)
                {
                    state.updateProperties();
                }
            }
        }

        protected virtual void Update()
        {
            if (!aiSwitch || pause)
            {
                return;
            }

            if (Time.time - preTime < updateLimit)
            {
                return;
            }
            preTime = Time.time;

            int len = _goalList.Count;
            if (len > 0)
            {
                int maxPriority = 0;
                IGoal maxGoal = null;
                for (int i = 0; i < len; i++)
                {
                    IGoal goal = _goalList[i];

                    int priority = goal.getPriority();
                    if (priority > maxPriority)
                    {
                        maxPriority = priority;
                        maxGoal = goal;
                    }
                }

                if (maxGoal != null)
                {
                    maxGoal.execute();
                }
            }
            IAIState _currentState = getCurrentState();
            if (_currentState != null)
            {
                _currentState.update();
            }
        }

        public IAIState getCurrentState()
        {
            if (_historyStates.Count == 0)
            {
                if (_initState == null)
                {
                    return null;
                }
                _historyStates.Push(_initState);
                if (_initState.initialized == false)
                {
                    _initState.initialize();
                }
                _initState.addEventListener(EventX.EXIT, exitHandle);
                _initState.enter();
            }
            return _historyStates.Peek();
        }

        /// <summary>
        /// 添加不同的状态; 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool addState(IAIState value,bool isInit=false)
        {
            if (_mapStates.ContainsKey(value.type))
            {
                return false;
            }

            value.stateMachine = this;
            value.setAgent(gameObject);
            
            _mapStates.Add(value.type, value);

            if (isInit)
            {
                _initState = value;
            }
            return true;
        }

        public virtual bool addState<T>(bool isInit = false) where T: IAIState
        {
            IAIState state = Activator.CreateInstance<T>();
            return addState(state, isInit);
        }


        /// <summary>
        /// 添加目标;
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool addGoal(IGoal value)
        {
            if (_goalList.IndexOf(value) == -1)
            {
                _goalList.Add(value);
                return true;
            }
            return false;
        }

        public void changeState(string state)
        {
            IAIState newState = getState(state);
            if (newState != null)
            {
                _historyStates.Push(newState);
                newState.addEventListener(EventX.EXIT, exitHandle);

                if (newState.initialized == false)
                {
                    newState.initialize();
                }
                newState.enter();
            }else if (_initState != null)
            {
                _initState.changeState(state);
            }
        }

        /// <summary>
        /// 取得状态 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>		
        private IAIState getState(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            IAIState state;
            if (_mapStates.TryGetValue(type, out state))
            {
                return state;
            }
            return null;
        }

        private void exitHandle(EventX e)
        {
            IAIState state=(IAIState)e.target;;
            state.removeEventListener(EventX.EXIT, exitHandle);

            string newStateType = state.nextState;
            if (string.IsNullOrEmpty(newStateType) == false)
            {
                changeState(newStateType);
            }
            else if(_historyStates.Count>0)
            {
                _historyStates.Pop();
                if (_historyStates.Count > 0)
                {
                    state = _historyStates.Peek();
                    state.addEventListener(EventX.EXIT, exitHandle);
                    state.enter();
                }
            }
        }

        public void sleep()
        {
            IAIState state = _initState;
            if (_historyStates.Count > 0)
            {
                state = _historyStates.Peek();
            }
            if (state != null)
            {
                state.removeEventListener(EventX.EXIT, exitHandle);
                state.exit();
            }

            blackboard.Clear();

            pause = true;
        }

        public void awken()
        {
            IAIState state = _initState;
            if (_historyStates.Count > 0)
            {
                state = _historyStates.Peek();
            }
            if (state != null)
            {
                state.addEventListener(EventX.EXIT, exitHandle);
                state.enter();
            }

            pause = false;
        }
    }
}