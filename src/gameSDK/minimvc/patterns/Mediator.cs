using System;
using System.Collections.Generic;
using System.Reflection;

namespace foundation
{
    public abstract class Mediator :IMediator, IAsync
    {
        protected IFacade facade = Facade.GetInstance();
        private EventX ReadyEventX = new EventX(EventX.READY);
        private bool _isCached = false;
        protected bool _ready = false;
        protected bool _isAwake = false;
        protected IPanel _view;
        protected IProxy _model;

        protected bool _hasProgress = false;
        protected Action<EventX> readyHandle;

        /// <summary>
        /// 引用嵌套的Mediator;
        /// </summary>
        public IMediator __parent;
        /**
		 * 呼叫引用(谁触发了此mediator的打开); 
		 */
        public CallReferrer callReferrer { get; set; }
        public virtual string name { get; internal set; }
        public Mediator() : this("")
        {
            
        }
        public Mediator(string mediatorName)
        {
            if (string.IsNullOrEmpty(mediatorName))
            {
                mediatorName = this.GetType().Name;
            }
            name = mediatorName;
        }

        public bool addReayHandle(Action<EventX> handle)
        {
            if (_ready)
            {
                handle(ReadyEventX);
                return true;
            }

            readyHandle += handle;
            return true;
        }
        public bool removeReayHandle(Action<EventX> handle)
        {
            if (_ready)
            {
                return false;
            }

            readyHandle -= handle;
            return true;
        }

        public bool removeAllReayHandle()
        {
            if (_ready)
            {
                return false;
            }
            readyHandle = null;
            return true;
        }


        public bool isReady
        {
            get
            {
                return _ready;
            }
        }

        public bool startSync()
        {

            if (_view is IAsync)
            {
                IAsync asyncView = _view as IAsync;
                if (asyncView.isReady == false)
                {
                    asyncView.startSync();
                }
            }

            return true;
        }

        public void setView(IPanel value)
        {
            if (_view != null)
            {
                if (_hasProgress)
                {
                    _view.removeEventListener(EventX.PROGRESS, viewProgressHandle);
                }
                _view.__refMediator = null;
                _view.removeEventListener(EventX.READY, preViewReadyHandler);
                bindSetViewEvent(_view, false);
            }

            _view = value;

            if (_view != null)
            {
                _view.__refMediator = this;
                if (_view is IAsync)
                {
                    IAsync asyncView = _view as IAsync;
                    if (asyncView.isReady == false)
                    {
                        if (_hasProgress)
                        {
                            _view.addEventListener(EventX.PROGRESS, viewProgressHandle);
                        }
                        _view.addEventListener(EventX.READY, preViewReadyHandler);
                        return;
                    }
                }
                preViewReadyHandler(null);            
            }
        }

        public IPanel getView()
        {
            return _view;
        }
        public T getView<T>() where T:class,IPanel 
        {
            return _view as T;
        }

        protected virtual void viewProgressHandle(EventX e)
        {
        }
        protected virtual void modelProgressHandle(EventX e)
        {
        }

        protected void preViewReadyHandler(EventX e)
        {
            if (e != null)
            {
                IPanel panel = e.target as IPanel;
                panel.removeEventListener(EventX.READY, preViewReadyHandler);
                if (_hasProgress)
                {
                    panel.removeEventListener(EventX.PROGRESS, viewProgressHandle);
                }
            }
            viewReadyHandle();

            if (_view.isShow)
            {
                stageHandle(new EventX(EventX.ADDED_TO_STAGE));
            }
            bindSetViewEvent(_view, true);
           
            if (_model == null)
            {
                preMediatorReadyHandle();
                return;
            }

            if (_model is IAsync)
            {
                IAsync asyncModel = _model as IAsync;
                if (asyncModel.isReady == false)
                {
                    if (_hasProgress)
                    {
                        _model.addEventListener(EventX.PROGRESS, modelProgressHandle);
                    }
                    _model.addEventListener(EventX.READY, preModelReadyHandle);
                    asyncModel.startSync();
                    return;
                }
            }
            preMediatorReadyHandle();
        }


        protected void preModelReadyHandle(EventX e)
        {
            IProxy proxy = e.target as IProxy;
            if (_hasProgress)
            {
                proxy.removeEventListener(EventX.PROGRESS, modelProgressHandle);
            }
            proxy.removeEventListener(EventX.READY, preModelReadyHandle);
            modelReadyHandle();
            if (proxy == _model)
            {
                preMediatorReadyHandle();
            }
        }

        protected virtual void viewReadyHandle()
        {
            
        }

        protected virtual void modelReadyHandle()
        {
            
        }

        protected virtual void preMediatorReadyHandle()
        {
            mediatorReadyHandle();
            //DebugX.Log("mediator:{0} ready!",this.name);
            _ready = true;
            if (_view.activeInHierarchy)
            {
                facade.registerEventInterester(this, InjectEventType.Show, true);
                if (_isAwake == false)
                {
                    _isAwake = true;
                    this.preAwaken();
                }
            }

            if (readyHandle != null)
            {
                readyHandle(ReadyEventX);
                readyHandle = null;
            }
            facade.simpleDispatch(EventX.MEDIATOR_READY, name);
        }

        protected virtual void mediatorReadyHandle()
        {
            
        }

        protected void bindSetViewEvent(IPanel view, bool isBind)
        {
            if (isBind)
            {
                view.addEventListener(EventX.ADDED_TO_STAGE, stageHandle);
                view.addEventListener(EventX.REMOVED_FROM_STAGE, stageHandle);

                view.addEventListener(PanelEvent.MOTION_SHOW_FINISHED, stageHandle);
                view.addEventListener(PanelEvent.MOTION_HIDE_FINISHED, stageHandle);
            }
            else
            {
                view.removeEventListener(EventX.ADDED_TO_STAGE, stageHandle);
                view.removeEventListener(EventX.REMOVED_FROM_STAGE, stageHandle);

                view.removeEventListener(PanelEvent.MOTION_SHOW_FINISHED, stageHandle);
                view.removeEventListener(PanelEvent.MOTION_HIDE_FINISHED, stageHandle);
            }
        }

        protected void stageHandle(EventX e)
        {
            switch (e.type)
            {
                case EventX.ADDED_TO_STAGE:
                    facade.registerEventInterester(this, InjectEventType.Show,true);
                    if (_model is IEventInterester)
                    {
                        facade.registerEventInterester(this._model,InjectEventType.Show, true);
                    }
                  
                    if (isCanAwaken() && isReady && _isAwake == false)
                    {
                        _isAwake = true;
                        if (_model != null)
                        {
                            registerModelEvent(_model, true);
                        }
                        preAwaken();
                    }
                    break;
                case EventX.REMOVED_FROM_STAGE:
                    facade.registerEventInterester(this,InjectEventType.Show, false);
                    if (_model is IEventInterester)
                    {
                        facade.registerEventInterester(this._model,InjectEventType.Show, false);
                    }
                 
                    if (isReady && _isAwake)
                    {
                        _isAwake = false;
                        if (_model != null)
                        {
                            registerModelEvent(_model, false);
                        }
                        preSleep();
                    }
                    break;


                case PanelEvent.MOTION_HIDE_FINISHED:
                    viewMotionFinishedHandle(false);
                    break;
                case PanelEvent.MOTION_SHOW_FINISHED:
                    viewMotionFinishedHandle(true);
                    break;
            }
        }

        private Dictionary<InjectEventType, Dictionary<string, Action<EventX>>> _eventInterests;
        public Dictionary<string, Action<EventX>> getEventInterests(InjectEventType type)
        {
            if (_eventInterests == null)
            {
                _eventInterests = new Dictionary<InjectEventType, Dictionary<string, Action<EventX>>>();
                MVCEventAttribute.CollectionEventInterests(this, _eventInterests);
            }
            Dictionary<string, Action<EventX>> e;
            if (_eventInterests.TryGetValue(type, out e) == false)
            {
                e = new Dictionary<string, Action<EventX>>();
                _eventInterests.Add(type, e);
            }
            return e;
        }


        protected virtual void registerModelEvent(IProxy _model, bool v)
        {
            if (_model == null)
            {
                return;
            }
            Dictionary<string, Action<EventX>> dic = getEventInterests(InjectEventType.Model);
            if (v)
            {
                foreach (string key in dic.Keys)
                {
                    _model.addEventListener(key, dic[key]);
                }
            }
            else
            {
                foreach (string key in dic.Keys)
                {
                    _model.removeEventListener(key, dic[key]);
                }
            }
        }

        protected virtual void viewMotionFinishedHandle(bool isShow)
        {
            if (isShow == false)
            {
                CallReferrer oldCallReferrer = callReferrer;
                if (oldCallReferrer != null)
                {
                    callReferrer = null;
                    oldCallReferrer.execute();
                    CallReferrer.Recycle(oldCallReferrer);
                }
            }
        }

        /// <summary>
        /// 现在状态是否可让它唤醒 
        /// </summary>
        protected virtual bool isCanAwaken()
        {
            return true;
        }

        public IProxy getModel()
        {
            return _model;
        }


        public void setModel(IProxy value)
        {
            if (_model == value)
            {
                return;
            }
            if (_model != null)
            {
                if (_model is IAsync)
                {
                    if (_hasProgress)
                    {
                        _model.removeEventListener(EventX.PROGRESS, modelProgressHandle);
                    }
                    _model.removeEventListener(EventX.READY, preModelReadyHandle);
                }
                registerModelEvent(_model, false);
            }

            _model = value;
            if (_model != null && isReady && _isAwake)
            {
                registerModelEvent(_model, true);
            }
        }
        public virtual void execute(string eventType, object data = null,bool isShowView=true)
        {
            if (isReady == false)
            {
                addReayHandle(e=>execute(eventType, data, isShowView));
                startSync();
                return;
            }
            if (isShowView)
            {
                toggleSelf(1);
            }

            ///已完成的直接调用
            MethodInfo method = this.GetType().GetMethod(eventType);
            if (method != null)
            {
                method.Invoke(this, new object[] { data });
            }
        }
        public void execute<T>(Action<T> action, T data = default(T), bool isShowView = true)
        {
            if (isReady == false)
            {
                addReayHandle(e => execute<T>(action, data, isShowView));
                startSync();
                return;
            }

            if (isShowView)
            {
                toggleSelf(1);
            }
            ///已完成的直接调用
            action(data);
        }

        public void execute<T>(Action<T> action, T data = default(T))
        {
            this.execute(action, data, false);
        }


        [MVCEvent(InjectEventType.Model,EventX.CHANGE)]
        protected virtual void modelEventHandle(EventX e)
        {
            updateView(e);
        }

        protected virtual void updateView(EventX e = null)
        {

        }

        protected virtual void eventHandle(EventX e)
        {
            
        }

        public virtual void onRegister()
        {

        }

        protected virtual void onCache()
        {
        }

        public virtual void onClearCache()
        {
            _isCached = false;
            if (_view != null && _view.isShow)
            {
                _view.hide();
            }
        }

        public virtual void onRemove()
        {
            if (_model != null)
            {
                registerModelEvent(_model,false);
            }
        }

    

        /// <summary>
        /// 显示隐藏Mediator的视图;1打开，0关闭，-1打开或关闭
        /// </summary>
        /// <param name="type"></param>
        public virtual void toggleSelf(int type = -1)
        {
            facade.toggleMediator(this.name, type);
        }

        protected virtual void preAwaken()
        {
            if (_isCached==false)
            {
                _isCached = true;
                onCache();
            }
            awaken();
            updateView();
            CallLater.Add(awakeLaterDo, 0.2f);

            facade.simpleDispatch(EventX.MEDIATOR_SHOW, name);
        }

        protected virtual void awaken()
        {
        }

        protected virtual void awakeLaterDo()
        {
        }

        protected virtual void preSleep()
        {
            CallLater.Remove(awakeLaterDo);
            sleep();

            facade.simpleDispatch(EventX.MEDIATOR_HIDE, name);
        }

        protected virtual void sleep()
        {

        }

        public bool isShow
        {
            get
            {
                if (_view != null)
                {
                    return _view.isShow;
                }
                return false;
            }
        }

  
    }
}
