using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 小雷达
    /// </summary>
    public class BaseRaderManager : FoundationBehaviour, IRaderManager
    {
        public static int MAX = 20;
        private Stack<BaseRaderItem> _pool = new Stack<BaseRaderItem>();
        protected GameObject _container;
        private Dictionary<BaseObject, BaseRaderItem> _raderMap = new Dictionary<BaseObject, BaseRaderItem>();
        private HashSet<BaseObject> _readyDoUnits=new HashSet<BaseObject>();
        private AssetResource _textureResource;
        protected Texture _texture;
        protected Rect _rect;
        protected bool _isReady = false;
        protected BaseObject _centerObject;
        public virtual void initContainer(GameObject value)
        {
            this._container = value;
            this._isReady = true;

            if (this._texture != null)
            {
                bindComponents();
            }

            foreach (BaseObject baseObject in _readyDoUnits)
            {
                createUnit(baseObject);
            }
            _readyDoUnits.Clear();
        }


        public Dictionary<BaseObject, BaseRaderItem> RaderMap
        {
            get { return _raderMap; }
        }

        /// <summary>
        /// 初始化场景
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="rect"></param>
        public virtual void changeScene(string uri, Rect rect)
        {
            this._rect = rect;
            string url = getURL(uri);

            if (_textureResource != null)
            {
                _textureResource.release();
                AssetsManager.bindEventHandle(_textureResource, onTextureHandle, false);
            }

            _textureResource = AssetsManager.getResource(url, LoaderXDataType.TEXTURE);
            _textureResource.retain();
            AssetsManager.bindEventHandle(_textureResource, onTextureHandle);
            _textureResource.load();
        }

        protected virtual void onTextureHandle(EventX e)
        {
            AssetsManager.bindEventHandle(_textureResource, onTextureHandle,false);
            if (e.type != EventX.COMPLETE)
            {
                return;
            }
            this._texture = _textureResource.getTexture();
            if (_isReady)
            {
                bindComponents();
            }
        }

        protected virtual string getURL(string uri)
        {
            return uri;
        }

        public virtual void changeScene(Texture texture, Rect rect)
        {
            this._texture = texture;
            this._rect = rect;

            if (_isReady)
            {
                bindComponents();
            }
        }

        protected virtual void bindComponents()
        {
            
        }

        private BaseRaderItem getFromPool()
        {
            BaseRaderItem item = null;
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
                item.SetActive(true);
            }
            else
            {
                GameObject go = UIUtils.CreateEmpty("rader", _container);
                item=newBaseRaderItem(go);
            }
            return item;
        }

        protected virtual BaseRaderItem newBaseRaderItem(GameObject go)
        {
            BaseRaderItem baseRaderItem = go.AddComponent<BaseRaderItem>();
            return baseRaderItem;
        }

        public virtual BaseRaderItem createUnit(BaseObject baseObject, bool isCenter = false)
        {
            if (baseObject == null)
            {
                return null;
            }
            if (isCenter)
            {
                _centerObject = baseObject;
            }

            if (_isReady == false)
            {
                _readyDoUnits.Add(baseObject);
                return null;
            }

            BaseRaderItem baseRaderItem = null;
            if (_raderMap.TryGetValue(baseObject, out baseRaderItem) == false)
            {
                baseRaderItem = getFromPool();
            }
            if (baseRaderItem != null && baseRaderItem.data == null)
            {
                baseRaderItem.data = baseObject;
                bindBaseObjectEvent(baseObject, true);
                _raderMap[baseObject]= baseRaderItem ;
            }

            translator3DTo2D(baseRaderItem, baseObject.position);

            this.simpleDispatch(EventX.CHANGE);
            return baseRaderItem;
        }


        protected virtual void translator3DTo2D(BaseRaderItem image, Vector3 position)
        {
        }

        public void disposeUnit(BaseObject baseObject)
        {
            if (baseObject == null)
            {
                return;
            }
            if (_isReady == false)
            {
                _readyDoUnits.Remove(baseObject);
                return;
            }

            BaseRaderItem baseRaderItem = null;
            if (_raderMap.TryGetValue(baseObject, out baseRaderItem) == false)
            {
                return;
            }
            _raderMap.Remove(baseObject);
            bindBaseObjectEvent(baseObject, false);
            if (baseRaderItem == null)
            {
                return;
            }
            if (_pool.Count < MAX)
            {
                baseRaderItem.SetActive(false);
                _pool.Push(baseRaderItem);
            }
            else
            {
                baseRaderItem.dispose(0);
            }

            this.simpleDispatch(EventX.CHANGE);
        }

        protected virtual void bindBaseObjectEvent(BaseObject baseObject, bool v)
        {
            if (v)
            {
                baseObject.addEventListener(ActorMoveEventX.NEXT_STEP, stepHandle);
                baseObject.addEventListener(ActorMoveEventX.REACHED, stepHandle);
            }
            else
            {
                baseObject.removeEventListener(ActorMoveEventX.NEXT_STEP, stepHandle);
                baseObject.removeEventListener(ActorMoveEventX.REACHED, stepHandle);
            }
        }

        protected virtual void stepHandle(EventX e)
        {
            MonoEventDispatcher dispatcher = e.target as MonoEventDispatcher;
            updateUnit(dispatcher.GetComponent<BaseObject>());
        }

        public virtual void clear()
        {
            List<BaseObject> list=new List<BaseObject>();
            foreach (BaseObject baseObject in _raderMap.Keys)
            {
                list.Add(baseObject);
            }

            foreach (BaseObject baseObject in list)
            {
                disposeUnit(baseObject);
            }
        }
        public virtual BaseRaderItem updateUnit(BaseObject baseObject)
        {
            if (baseObject == null)
            {
                return null;
            }
            BaseRaderItem baseRaderItem = null;
            if (_raderMap.TryGetValue(baseObject, out baseRaderItem) == false)
            {
                return null;
            }
            if (baseRaderItem == null)
            {
                return null;
            }
            translator3DTo2D(baseRaderItem, baseObject.position);
            return baseRaderItem;
        }
    }
}