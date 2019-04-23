using System;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class RemotePrefab : FoundationBehaviour
    {
        public string uri;
        protected LoaderXDataType loaderXDataType;
        protected bool _ready = false;
        protected Action<EventX> readyHandle;
        private AssetResource resource;
        public string prefix;
        protected GameObject _skin;

        public void load(string url, LoaderXDataType loaderXDataType = LoaderXDataType.PREFAB)
        {
            this.uri = url;
            this.loaderXDataType = loaderXDataType;
            url = getURL(uri);

            if (resource != null)
            {
                AssetsManager.bindEventHandle(resource, completeHandle,false);
                resource.release();
            }
            resource = AssetsManager.getResource(url, this.loaderXDataType);
            AssetsManager.bindEventHandle(resource, completeHandle);

            resource.retain();
            resource.load();
        }

        protected virtual string getURL(string uri)
        {
            if (loaderXDataType == LoaderXDataType.PREFAB)
            {
                return prefix + uri + PathDefine.U3D;
            }
            else
            {
                return "Prefabs/" + uri;
            }
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


        public bool isReady
        {
            get { return _ready; }
        }

        protected void completeHandle(EventX e)
        {
            _ready = true;
            AssetsManager.bindEventHandle(resource, completeHandle, false);

            if (_skin != null)
            {
                unbindComponents();
                GameObject.Destroy(_skin);
                _skin = null;
            }

            if (e.type == EventX.COMPLETE)
            {
                _skin = resource.getNewInstance() as GameObject;
                if (_skin!=null)
                {
                    bindComponents();
                }
            }

            if (readyHandle != null)
            {
                readyHandle(new EventX(EventX.READY));
                readyHandle = null;
            }

            this.simpleDispatch(EventX.READY, _skin);
        }

        protected virtual void unbindComponents()
        {

        }
        protected virtual void bindComponents()
        {
            _skin.transform.SetParent(this.transform, false);
            _skin.transform.localPosition = Vector3.zero;
            _skin.transform.localRotation = Quaternion.identity;
            _skin.transform.localScale = Vector3.one;
        }


        protected virtual void OnDestroy()
        {
            if (resource != null)
            {
                AssetsManager.bindEventHandle(resource, completeHandle, false);
                resource.release();
                resource = null;
            }

            if (readyHandle != null)
            {
                readyHandle = null;
            }
            _ready = false;

        }
    }
}