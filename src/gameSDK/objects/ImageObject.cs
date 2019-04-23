using System;
using foundation;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 面板显示3D模型
    /// </summary>
    public class ImageObject : BaseObject
    {
        public bool receiveShadows = false;
       
        protected GameObject _station;
        protected AssetResource stationResource;
        protected string _stationURI;
        public string stationURI
        {
            get { return _stationURI; }
            set { _stationURI = value; }
        }

        protected override void Awake()
        {
            layer = LayerX.GetUI3DLayer();
            gameObject.layer = layer;
            base.Awake();
        }

        public override void load(string uri)
        {
            base.load(uri);

            if (string.IsNullOrEmpty(_stationURI)==false)
            {
                loadStation(_stationURI, true);
            }
        }

 
        /// <summary>
        /// 加载ui站台资源(人物站立的台子上)
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="forceReload"></param>
        protected void loadStation(string uri, bool forceReload = false)
        {
            if (_stationURI == uri && forceReload == false)
            {
                return;
            }
            _stationURI = uri;

            if (stationResource != null)
            {
                AssetsManager.bindEventHandle(stationResource, onStationHandle, false);
                stationResource.release();
                stationResource = null;
            }
            string url = getURL(_stationURI);

            if (AssetsManager.routerResourceDelegate != null)
            {
                stationResource = AssetsManager.routerResourceDelegate(_stationURI, _stationURI, prefix);
            }
            if (stationResource == null)
            {
                stationResource = AssetsManager.getResource(url, loaderXDataType);
            }
            stationResource.retain();
            AssetsManager.bindEventHandle(stationResource, onStationHandle);
            stationResource.load();
        }

        /// <summary>
        /// 台子加载完成;
        /// </summary>
        /// <param name="e"></param>
        protected void onStationHandle(EventX e)
        {
            if (gameObject == null)
            {
                DebugX.Log("onSkinHandle is destory");
                return;
            }
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, onStationHandle, false);
            if (e.type == EventX.FAILED)
            {
                return;
            }
            GameObject go = getStationByResource(resource);
            if (_station != null)
            {
                recycleStation();
            }
            _station = go;
            if (_station != null && _skin!=null)
            {
                bindStationComponents();
            }
        }

        public GameObject station
        {
            get { return _station; }
        }

        protected virtual GameObject getStationByResource(AssetResource resource)
        {
            return resource.getNewInstance() as GameObject;
        }

        protected virtual void bindStationComponents()
        {
            _station.transform.SetParent(this.skinParentTransform, false);
            setContentLayer(_station.transform, layer);
            _station.transform.localPosition = Vector3.zero;
            _station.transform.localRotation = Quaternion.identity;
        }

        protected override void bindComponents()
        {
            base.bindComponents();

            if (_skin != null)
            {
                setContentLayer(_skin.transform, layer);

                if (receiveShadows == false)
                {
                    SkinnedMeshRenderer[] renders = _skin.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer render in renders)
                    {
                        render.receiveShadows = false;
                    }
                }
            }

            if (_station != null)
            {
                bindStationComponents();
            }
        }

        protected virtual void recycleStation()
        {
            if (_station != null)
            {
                GameObject.Destroy(_station);
                _station = null;
            }
        }

        protected override void onDestroy()
        {
            base.onDestroy();

            if (stationResource != null)
            {
                AssetsManager.bindEventHandle(stationResource, onStationHandle, false);
                stationResource.release();
                stationResource = null;
            }
            if (_station != null)
            {
                recycleStation();
            }
        }
    }
}