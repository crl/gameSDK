using gameSDK;
using System;
using System.Collections.Generic;

namespace foundation
{
    public class Facade : EventDispatcher,IFacade
    {
        protected static readonly string SINGLETON_MSG = "Facade Singleton already constructed!";

        protected ASDictionary<string, object> mvcInjectLock;

        protected ASDictionary<Type, ISocketDecoder> socketDecodeMaps=new ASDictionary<Type, ISocketDecoder>();
        protected IInject injecter;

        protected IView<IProxy> model;
        protected IView<IMediator> view;

        protected static IFacade ins;

        public static IFacade GetInstance()
        {
            if (ins == null)
            {
                ins = new Facade();
            }
            return ins;
        }

        public Facade()
        {
            if (ins != null)
            {
                throw new Exception(SINGLETON_MSG);
            }
            ins = this;

            injecter = new MVCInject(this);

            mvcInjectLock = new ASDictionary<string, object>();

            model = new View<IProxy>();
            view = new View<IMediator>();
        }

        public void registerProxy(IProxy proxy)
        {
            model.register(proxy);
            this.registerEventInterester(proxy, InjectEventType.Always, true);
        }

        public IProxy getProxy(string proxyName)
        {
            IProxy proxy = model.get(proxyName);
            if (proxy == null)
            {
                Type cls = Singleton.getClass(proxyName);
                if (cls != null)
                {
                    proxy = (IProxy) routerCreateInstance(cls);
                    __unSafeInjectMVCInstance(proxy, proxyName);
                    registerProxy(proxy);
                }
            }
            return proxy;
        }

       public T getProxy<T>(string proxyName = "") where T : IProxy
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                proxyName = typeof(T).Name;
            }
            return (T) getProxy(proxyName);
        }

        public IProxy removeProxy(string proxyName)
        {
            IProxy proxy = model.remove(proxyName);
            if (proxy != null)
            {
                this.registerEventInterester(proxy, InjectEventType.Always, false);
            }
            return proxy;
        }

        public bool hasProxy(string proxyName)
        {
            return model.has(proxyName);
        }

        public bool hasProxy<T>(string proxyName) where T : IProxy
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                proxyName = typeof(T).Name;
            }
            return model.has(proxyName);
        }

        public void registerMediator(IMediator mediator)
        {
            view.register(mediator);
            this.registerEventInterester(mediator, InjectEventType.Always, true);
        }

        public IMediator getMediator(string mediatorName)
        {
            //看此mediator是否已经存在
            IMediator mediator = view.get(mediatorName);
            if (mediator == null)
            {
                //获取这个class（所有Mediator都在FacadeX中注册过添加到Singleton的ClassMap中了）
                Type cls = Singleton.getClass(mediatorName);
                if (cls != null)
                {
                    //创建这个类的实例
                    mediator = (IMediator) routerCreateInstance(cls);
                    __unSafeInjectMVCInstance(mediator, mediatorName);
                    registerMediator(mediator);
                }
                else
                {
                    DebugX.Log(mediatorName + "未注册");
                }
            }
            return mediator;
        }
        protected void __unSafeInjectMVCInstance( IMVCHost host, string hostAliasName="")
        {
            if (string.IsNullOrEmpty(hostAliasName))
            {
                hostAliasName = host.name;
            }
            mvcInjectLock[hostAliasName] = host;
            //给类的成员变量赋值、注册这个类
            inject(host);
            mvcInjectLock.Remove(hostAliasName);
        }

        public T getMediator<T>(string mediatorName="") where T : IMediator
        {
            if (string.IsNullOrEmpty(mediatorName))
            {
                mediatorName = typeof(T).Name;
            }
            return (T) getMediator(mediatorName);
        }

        public IMediator removeMediator(string mediatorName)
        {
            IMediator mediator= view.remove(mediatorName);
            if (mediator!=null)
            {
                this.registerEventInterester(mediator, InjectEventType.Always, false);
            }

            return mediator;
        }

        public bool hasMediator(string mediatorName)
        {
            return view.has(mediatorName);
        }

        public bool hasMediator<T>(string mediatorName) where T : IMediator
        {
            if (string.IsNullOrEmpty(mediatorName))
            {
                mediatorName = typeof(T).Name;
            }
            return view.has(mediatorName);
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object routerCreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public virtual T routerCreateInstance<T>() where T : new()
        {
            return new T();
        }

        public T registerSocketDecode<T>() where T : ISocketDecoder, new()
        {
            Type type = typeof(T);
            ISocketDecoder socketDecoder;
            if (socketDecodeMaps.TryGetValue(type, out socketDecoder)==false)
            {
                socketDecoder = Singleton.getInstance<T>();
                socketDecodeMaps.Add(type, socketDecoder);
            }
            return (T)socketDecoder;
        }

        public void registerEventInterester(IEventInterester eventInterester, InjectEventType injectEventType,
            bool isBind = true)
        {
            if (eventInterester == null)
            {
                return;
            }

            Dictionary<string, Action<EventX>> eventInterests = eventInterester.getEventInterests(injectEventType);

            if (isBind)
            {
                foreach (string eventType in eventInterests.Keys)
                {
                    this.addEventListener(eventType, eventInterests[eventType]);
                }
            }
            else
            {
                foreach (string eventType in eventInterests.Keys)
                {
                    this.removeEventListener(eventType, eventInterests[eventType]);
                }
            }
        }



        public IMediator toggleMediator(string mediatorName, int type = -1)
        {
            if (string.IsNullOrEmpty(mediatorName))
            {
                return null;
            }

            if (type == 0)
            {
                if (hasMediator(mediatorName) == false)
                {
                    return null;
                }
            }

            IMediator mediator = getMediator(mediatorName);
            if (mediator == null)
            {
                return null;
            }
            if (mediator is IAsync)
            {
                //如果是异步的 并且isReady为false 就先执行startAsync方法，并把toggleMediator方法添加到readyHandle队列，返回mediator
                IAsync async = mediator as IAsync;
                if (async.isReady == false)
                {
                    async.addReayHandle(delegate(EventX msg)
                    {
                        toggleMediator(mediatorName, type);
                    });
                    async.startSync();
                    return mediator;
                }
            }
            //获取view，根据type决定调用show或hide
            IPanel view = mediator.getView();
            switch (type)
            {
                case 1:
                    if (view.isShow == false)
                    {
                        view.show();
                    }
                    else
                    {
                        view.bringTop();
                    }
                    break;
                case 0:
                    if (view.isShow)
                    {
                        view.hide();
                    }
                    break;
                case -1:
                    if (view.isShow)
                    {
                        view.hide();
                    }
                    else
                    {
                        view.show();
                    }
                    break;
            }
            return mediator;
        }

        public T toggleMediator<T>(int type = -1, string mediatorName = "") where T:IMediator
        {
            if (string.IsNullOrEmpty(mediatorName))
            {
                mediatorName = typeof(T).Name;
            }

            return (T) toggleMediator(mediatorName, type);
        }

        public IMediator executeMediator(string mediatorName, string eventType, object data = null, bool isShowView = true)
        {
            IMediator mediator = getMediator(mediatorName);
            if (mediator != null)
            {
                mediator.execute(eventType, data, isShowView);
            }
            return mediator;
        }
        public T executeMediator<T, M>(Action<M> action, M data = default(M), bool isShowView = true, string mediatorName = "")
            where T : IMediator
        {
            T mediator = getMediator<T>(mediatorName);
            mediator.execute<M>(action, data,isShowView);
            return mediator;
        }

        public T executeProxy<T,M>(Action<M> action, M data = default(M) , string proxyName = "") where T : IProxy
        {
            T proxy = getProxy<T>(proxyName);
            proxy.execute<M>(action, data);
            return proxy;
        }

        public virtual void autoInitialize(string currentState)
        {

        }

        private ASList<IMediator> temps = new ASList<IMediator>();
        protected List<string> stateViewStack=new List<string>();
        public static int MapIntKey { get; internal set; }
        public static string CurrentViewState { get; internal set; }

        public virtual void pushViewState(int mapIntKey, string newState)
        {
            doPushViewState(mapIntKey, newState);
        }

        protected virtual void doPushViewState(int mapIntKey, string newState,string lastState=null) {
            if (lastState == null)
            {
                int last = stateViewStack.Count - 1;
                if (last != -1)
                {
                    lastState = stateViewStack[last];
                }
                else
                {
                    lastState = "";
                }
            }
            int index = stateViewStack.IndexOf(newState);
            if (index != -1)
            {
                stateViewStack.RemoveAt(index);
            }
            stateViewStack.Add(newState);

            if (lastState == newState && MapIntKey == mapIntKey)
            {
                return;
            }
            MapIntKey = mapIntKey;
            CurrentViewState = newState;

            ASDictionary<string, IMediator> mediatorMap = getMeditorDic();

            temps.Clear();
            foreach (string name in mediatorMap)
            {
                temps.Add(mediatorMap[name]);
            }
            foreach (IMediator mediator in temps)
            {
                IPanel panel = mediator.getView();
                if (panel == null)
                {
                    continue;
                }
                panel.changeState(MapIntKey, newState);
            }

            simpleDispatch(EventX.STATE_CHANGE, MapIntKey);
        }

        public virtual void popViewState(string viewState)
        {
            ///最新时候的再做删除,因为插入时会有更新
            int index = stateViewStack.IndexOf(viewState);
            if (index != -1)
            {
                stateViewStack.RemoveAt(index);
            }

            if (CurrentViewState == viewState)
            {
                int last = stateViewStack.Count - 1;
                string lastState = "";
                if (last != -1)
                {
                    lastState = stateViewStack[last];
                }

                doPushViewState(MapIntKey, lastState, viewState);
            }
        }

        public virtual void resetViewState()
        {
            MapIntKey = -1;
            CurrentViewState = "";
        }

        public object inject(object target)
        {
            if (injecter != null)
            {
                return injecter.inject(target);
            }
            return target;
        }

        public object getInjectLock(string className)
        {
            object value;
            if (mvcInjectLock.TryGetValue(className, out value))
            {
                return value;
            }
            return null;
        }

        public void clear()
        {
            view.clear();
            model.clear();

            foreach (string key in model.all())
            {
                removeProxy(key);
            }
        }

        public void clearCache()
        {
            view.clearCache();
            model.clearCache();
            foreach (ISocketDecoder item in socketDecodeMaps.Values)
            {
                item.clearCache();
            }

            this.simpleDispatch(EventX.CLEAR_CACHE);
        }

        public ASDictionary<string, IMediator> getMeditorDic()
        {
            return view.all();
        }

        public static IMediator ToggleMediator(string mediatorName, int type = -1)
        {
            if (ins == null)
            {
                return null;
            }

            return ins.toggleMediator(mediatorName, type);
        }

        public static T ToggleMediator<T>(int type = -1, string mediatorName = "") where T:IMediator
        {
            if (string.IsNullOrEmpty(mediatorName))
            {
                mediatorName = typeof(T).Name;
            }

            return (T) ToggleMediator(mediatorName, type);
        }

        public static void ClearCache()
        {
            if (ins == null)
            {
                return;
            }

            ins.clearCache();
        }

        public static T GetProxy<T>(string name = "") where T : IProxy
        {
            if (ins == null)
            {
                return default(T);
            }

            return ins.getProxy<T>(name);
        }

        public static T GetMediator<T>(string name = "") where T : IMediator
        {
            if (ins == null)
            {
                return default(T);
            }
            return ins.getMediator<T>(name);
        }

        public static bool HasMediator<T>(string name = "") where T : IMediator
        {
            if (ins == null)
            {
                return false;
            }
            return ins.hasMediator<T>(name);
        }
        public static bool HasMediator(string name)
        {
            if (ins == null)
            {
                return false;
            }
            return ins.hasMediator(name);
        }

        public static bool AddEventListener(string type, Action<EventX> listener, int priority = 0)
        {
            if (ins == null)
            {
                return false;
            }
            return ins.addEventListener(type, listener, priority);
        }

        public static bool RemoveEventListener(string type, Action<EventX> listener)
        {
            if (ins == null)
            {
                return false;
            }
            return ins.removeEventListener(type, listener);
        }

        public static void RegisterEventInterester(IEventInterester eventInterester,InjectEventType injectEventType=InjectEventType.Show, bool isBind = true)
        {
            if (ins == null)
            {
                return;
            }
            ins.registerEventInterester(eventInterester,injectEventType, isBind);
        }

        public static bool SimpleDispatch(string type, object data = null)
        {
            if (ins == null)
            {
                return false;
            }
            return ins.simpleDispatch(type, data);
        }

        public static bool DispatchEvent(EventX e)
        {
            if (ins == null)
            {
                return false;
            }
            return ins.dispatchEvent(e);
        }

        public static IMediator ExecuteMediator(string mediatorName, string eventType,object data=null, bool isShowView = true)
        {
            return ins.executeMediator(mediatorName, eventType, data, isShowView);
        }
        public static T ExecuteMediator<T,M>(Action<M> action, M data = default(M), bool isShowView = true, string mediatorName = "")
            where T : IMediator
        {
            if (ins == null)
            {
                return default(T);
            }
            return ins.executeMediator<T, M>(action, data, isShowView, mediatorName);
        }

        public static T ExecuteProxy<T,M>(Action<M> action, M data = default(M), string proxyName = "") where T : IProxy
        {
            if (ins == null)
            {
                return default(T);
            }
            return ins.executeProxy<T,M>(action, data, proxyName);
        }

        public static T RouterCreateInstance<T>() where T : new()
        {
            if (ins == null)
            {
                return default(T);
            }
            return ins.routerCreateInstance<T>();
        }


        public static void ResetViewState()
        {
            if (ins == null)
            {
                return;
            }
            ins.resetViewState();
        }
        public static void PushViewState(int mapType, string panelState)
        {
            if (ins == null)
            {
                return;
            }
            ins.pushViewState(mapType, panelState);
        }

        public static void PopViewState(string panelState)
        {
            ins.popViewState(panelState);
        }


        public static object Inject(object target)
        {
            if (ins == null)
            {
                return null;
            }

            return ins.inject(target);
        }
    }
}
