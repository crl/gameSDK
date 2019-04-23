using foundation;
using UnityEngine;

namespace gameSDK
{
    public abstract class AbstractAIState : EventDispatcher, IAIState
    {
        protected string _type;
        protected GameObject agent;
        protected string _nextState;

        /// <summary>
        /// 是否已完成初始化;
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// 当前状态名称;
        /// </summary>
        /// <param name="type"></param>

        public AbstractAIState()
        {
        }

        public virtual void setAgent(GameObject agent)
        {
            this.agent = agent;
        }

        public AIStateMachine stateMachine
        {
            get; set; }

        /// <summary>
        /// 是否初始化完成 
        /// </summary>

        public bool initialized
        {
            get
            {
                return _initialized;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>

        public virtual void initialize()
        {
            _initialized = true;
        }

        public virtual void update()
        {

        }

        /// <summary>
        /// 当前状态名称; 
        /// </summary>
        public string type
        {
            get
            {
                return _type;
            }
        }

        public string nextState
        {
            get
            {
                return _nextState;
            }
            set { _nextState = value; }
        }

        public virtual void exit()
        {
            //DebugX.Log("sleep:" + type);
            this.simpleDispatch(EventX.EXIT);
        }

        /// <summary>
        /// 进入当前状态; 
        /// </summary>

        public virtual void enter()
        {
            //DebugX.Log("awaken:" + type);
        }

        private static bool locked = false;
        public virtual bool changeState(string type)
        {
            if (locked == false)
            {
                locked = true;
                stateMachine.changeState(type);
                locked = false;
            }
            return true;
        }

        /// <summary>
        /// 更新属性，这样有些动态赋值的属性就不需要在update方法里面重复获取
        /// </summary>
        public virtual void updateProperties()
        {
            
        }
    }
}
