using gameSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace foundation
{
    public abstract class Proxy :EventDispatcher,IProxy, IAsync
    {
        protected IFacade facade = Facade.GetInstance();
        /// <summary>
        /// 是否显示Loading 
        /// </summary>
        public bool isShowLoading = true;

        protected bool _ready = false;
        private bool _loaded = false;
        private AssetResource resource;
        protected string uri;
        private string url;
        protected object _data;
        protected Action<EventX> readyHandle;

        public virtual string name { get; internal set; }
        public Proxy() : this("")
        {

        }
        public Proxy(string proxyName)
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                proxyName = this.GetType().Name;
            }
            name = proxyName;
        }

        protected virtual void toggleLoadingUI(bool show)
        {
        }

        protected virtual void progressHandle(EventX e)
        {
        }

        public virtual void execute<T>(Action<T> action, T data = default(T)) { 
            if (isReady == false)
            {
                addReayHandle(e=>execute<T>(action, data));
                startSync();
                return;
            }
            action(data);
        }

        public virtual void onRegister()
        {
            
        }

        protected virtual void onCache()
        {

        }

        public virtual void onClearCache()
        {
            onCache();
        }

        public virtual void onRemove()
        {
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

        public bool isReady
        {
            get
            {
                return _ready;
            }
        }

        public bool startSync()
        {
            if (isReady == false)
            {
                load();
            }
            return true;
        }

        public bool addReayHandle(Action<EventX> handle)
        {
            if (_ready)
            {
                handle(new EventX(EventX.READY));
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

        protected virtual void proxyReadyHandle()
        {
            //DebugX.Log("proxy:{0} ready!",this.name);

            _ready = true;
            if (readyHandle != null)
            {
                readyHandle(new EventX(EventX.READY));
                readyHandle = null;
            }
            this.simpleDispatch(EventX.READY);
            facade.simpleDispatch(EventX.PROXY_READY, name);
        }

        
        public void load()
		{
			if (_loaded)
			{
				return;
			}

            _loaded = true;

            if (resource!=null)
			{
                AssetsManager.bindEventHandle(resource, resourceHandler, false);
			    resource.removeEventListener(EventX.PROGRESS, progressHandle);
                resource.release();
			}

            if (string.IsNullOrEmpty(uri))
            {
                proxyReadyHandle();
                return;
            }
            toggleLoadingUI(true);
            url =getURL(uri);
            resource = AssetsManager.getResource(url,LoaderXDataType.AMF);
            resource.retain();
            autoDefaultResource(resource);
            AssetsManager.bindEventHandle(resource, resourceHandler, true);
            resource.addEventListener(EventX.PROGRESS, progressHandle);

            resource.load();
        }

        protected virtual void autoDefaultResource(AssetResource resource)
        {
            if (ApplicationVersion.isDebug && Application.isMobilePlatform == false)
            {
                resource.isForceRemote = true;
            }
        }

        protected virtual string getURL(string uri)
        {
            return PathDefine.configPath+ uri;
        }

        private void resourceHandler(EventX e)
        {
            toggleLoadingUI(false);
            AssetResource resource=e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, resourceHandler, false);
            resource.removeEventListener(EventX.PROGRESS, progressHandle);

            if (e.type != EventX.COMPLETE)
            {
                _loaded = false;
                return;
            }

            BaseApp.Instance.StartCoroutine(syncParserData(resource.data));
        }

        protected virtual IEnumerator syncParserData(object data)
        {
            onDataComplete(data);
            yield return new WaitForEndOfFrame();
            proxyReadyHandle();
        }

        protected virtual void onDataComplete(object data)
        {
           
        }

      
    }
}
