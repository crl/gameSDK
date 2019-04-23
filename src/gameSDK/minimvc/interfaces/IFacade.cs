using System;

namespace foundation
{
    public interface IFacade : INotifier, IInject,IEventDispatcher
    {
        /// <summary>
        /// 注册数据代理;
        /// </summary>
        /// <param name="proxy"></param>
        void registerProxy(IProxy proxy);

        /// <summary>
        /// 取得相应的数据代理; 
        /// </summary>
        /// <param name="proxyName"></param>
        /// <returns></returns>
        IProxy getProxy(string proxyName);

        T getProxy<T>(string proxyName="") where T:IProxy;

        /// <summary>
        /// 删除数据代理; 
        /// </summary>
        /// <param name="proxyName"></param>
        /// <returns></returns>
        IProxy removeProxy(string proxyName);

        /// <summary>
        /// 是否含有相应的数据代理; 
        /// </summary>
        /// <param name="proxyName"></param>
        /// <returns></returns>
        bool hasProxy(string proxyName);

        bool hasProxy<T>(string proxyName="") where T : IProxy;

        /// <summary>
        /// 注册视图代理控制器;
        /// </summary>
        /// <param name="mediator"></param>
        void registerMediator(IMediator mediator);

        /// <summary>
        ///  取得相应视图代理控制器; 
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns></returns>
        IMediator getMediator(string mediatorName);

        T getMediator<T>(string mediatorName = "") where T : IMediator;

        /// <summary>
        /// 删除视图代理控制器;  
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns></returns>
        IMediator removeMediator(string mediatorName);

        /// <summary>
        ///  是否含有视图代理控制器;
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns></returns>
        bool hasMediator(string mediatorName);

        bool hasMediator<T>(string mediatorName="") where T : IMediator;

        /// <summary>
        /// 启动eventInterests监听
        /// </summary>
        /// <param name="eventInterester"></param>
        /// <param name="isBind"></param>
        void registerEventInterester(IEventInterester eventInterester,InjectEventType injectEventType, bool isBind=true);

        /// <summary>
        /// 显示隐藏Mediator的视图;1打开，0关闭，-1打开或关闭
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IMediator toggleMediator(string mediatorName, int type = -1);
        T toggleMediator<T>(int type = -1, string mediatorName="") where T:IMediator;

        /// <summary>
        /// 直接运行mediator的exexute方法; 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="mediatorName"></param>
        /// <returns></returns>
        T executeMediator<T,M>(Action<M> action, M data = default(M), bool isShowView = true, string mediatorName="") where T : IMediator;

        IMediator executeMediator(string mediatorName, string eventType, object data = null, bool isShowView = true);
        /// <summary>
        /// 直接运行proxy的exexute方法; 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <param name="proxyName"></param>
        /// <returns></returns>
        T executeProxy<T,M>(Action<M> action, M data = default(M) , string proxyName = "") where T : IProxy;

        /// <summary>
        /// 注册Socket的解析类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        T registerSocketDecode<T>() where T : ISocketDecoder, new();

        /// <summary>
        /// 自动初始化;
        /// </summary>
        /// <param name="key"></param>
        void autoInitialize(string key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewStateIntKey">地图类型</param>
        /// <param name="newState">面板状态</param>
        void pushViewState(int viewStateIntKey,string newState);

        /// <summary>
        /// 回滚状态
        /// </summary>
        void popViewState(string viewState);

        /// <summary>
        /// 重置状态
        /// </summary>
        void resetViewState();

        /// <summary>
        /// 当前被锁定注册的对像 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        object getInjectLock(string className);

        object routerCreateInstance(Type type);
        T routerCreateInstance<T>() where T : new();

        void clearCache();

        void clear();
        
    }
}
