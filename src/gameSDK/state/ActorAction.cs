using foundation;
using UnityEngine;

namespace gameSDK
{
    public class ActorAction
    {
        public UpdateType updateType = UpdateType.Update;

        protected bool _isFinished=false;
        protected BaseObject _baseObject;
        protected GameObject _owner;
        protected StateModel _stateModel;
        protected float _updateLimitTime = 0;

        private int _stateID = -1;
        public int stateID { get { return _stateID; }
            private set { _stateID = value; } 
        }
        public ActorAction(int stateID)
        {
            this.stateID = stateID;
        }

        public virtual void initialize(StateModel stateModel)
        {
            this._stateModel = stateModel;
            this._baseObject = stateModel.GetComponent<BaseObject>();
            this._owner = _baseObject.gameObject;
            this._baseObject.addReayHandle(onReadyInitialize);
        }

        protected virtual void onReadyInitialize(EventX e)
        {
        }

        public bool isFinished
        {
            get { return _isFinished; }
            internal set { _isFinished = value; }
        }

        internal virtual void restart()
        {
            doStart();
        }
        /// <summary>
        /// 开始操作
        /// </summary>
        /// <returns>是否完成</returns>
        internal virtual void start()
        {
            doStart();
        }

        protected virtual void doStart()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>是否完成</returns>
        public virtual void update(float deltaTime)
        {
            if (_updateLimitTime > 0)
            {
                _updateLimitTime -= deltaTime;
                return;
            }
            doUpdate();
        }

        protected virtual void doUpdate()
        {
        }


        /// <summary>
        /// 结束操作
        /// </summary>
        /// <param name="state">被什么状态结束的</param>
        internal void stop()
        {
            doStop();
            _isFinished = true;
        }

        protected virtual void doStop()
        {
        }

        /// <summary>
        ///  是否允许新状态的加入
        /// </summary>
        /// <param name="newState"></param>
        /// <returns></returns>
        public virtual bool checkCanDo(int newState,bool tip=true)
        {
            StateVO stateVo = StateModel.GetStateVO(this.stateID);
            if (stateVo!=null && stateVo.limits.IndexOf(newState) != -1)
            {
                if (tip)
                {
                    StateModel.ShowTips(this.stateID, newState);
                }
                
                return false;
            }
            return true;
        }

        /// <summary>
        ///  是否可终止正在运行的状态
        /// </summary>
        /// <param name="runingState"></param>
        /// <returns></returns>
        public bool checkTerminateAction(int runingState)
        {
            StateVO stateVo = StateModel.GetStateVO(this.stateID);
            if (stateVo!=null && stateVo.terminates.IndexOf(runingState) != -1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 执行
        /// </summary>
        public virtual void execute()
        {
            if (_stateModel.checkCanDo(stateID) && _baseObject.isReady)
            {
                _stateModel.doAction(this);
            }
        }


        public virtual void __onDestroy()
        {
            stop();
        }
    }
}