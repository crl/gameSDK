using foundation;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class IconSlot : EventDispatcher,IDataRenderer
    {

        protected Image _image;

        public Image image {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                
                if (_image != null && _image.sprite == null)
                {
                    //初始化如果是白底,先隐藏
                    _image.enabled = false;
                }
            }
        }

        protected RawImage _rawImage;
        public RawImage rawImage {
            get
            {
                return _rawImage;
            }
            set
            {
                _rawImage = value;
                if (_rawImage != null && _rawImage.texture == null)
                {
                    //初始化如果是白底,先隐藏
                    _rawImage.enabled = false;
                }
            }
        }

        protected bool _enabled;

        public bool enabled
        {
            set
            {
                _enabled = value;
                if (image != null)
                {
                    if (_enabled)
                    {
                        image.material = null;
                        image.color = Color.white;
                    }
                    else
                    {
                        image.material = UIUtils.CreatShareGrayMaterial();
                        image.color = new Color(0, 1, 1, 1);
                    }
                }
                else if(rawImage != null)
                {
                    if (_enabled)
                    {
                        rawImage.material = null;
                        rawImage.color = Color.white;
                    }
                    else
                    {
                        rawImage.material = UIUtils.CreatShareGrayMaterial();
                        rawImage.color = new Color(0, 1, 1, 1);
                    }
                }
            }
            get { return _enabled; }
        }


        public bool showError=true;
        protected AssetResource _resource;
        /// <summary>
        /// 是否设置最佳尺寸
        /// </summary>
        public bool isSetNativeSize = false;
        /// <summary>
        /// 加载完成是否把enable设置为true
        /// </summary>
        public bool isSetEnabledTrue = true;

        public string prefix;

        public bool isForceRemote = false;

        public IconSlot()
        {
            this.prefix = PathDefine.commonPath;
        }

        protected string uri;

        protected object _data;
        public object data
        {
            get { return _data; }

            set
            {
                _data = value;
                doData();
            }
        }

        protected virtual void doData()
        {
        }

        public void load(string uri)
        {
            if (this.uri == uri)
            {
                return;
            }
            //这个为什么要设为空,如果有问题,请出写具体问题
            //setTexture(null);
            if (_resource != null)
            {
                _resource.release();
                AssetsManager.bindEventHandle(_resource, completeHandle, false);
            }
            this.uri = uri;
            string url = prefix + uri;

            _resource = AssetsManager.getResource(url, LoaderXDataType.TEXTURE);
            _resource.isForceRemote = isForceRemote;
            _resource.retain();
            AssetsManager.bindEventHandle(_resource, completeHandle);
            _resource.load();
        }

        public virtual void loadFace(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                load("face/" + name + ".png");
            }
            else if (showError)
            {
                load("icon/error.png");
            }
            else
            {
                //DebugX.LogWarning("加载的文件名为空");
                clear();
            }
        }

        public void clickTween()
        {
        }

        public virtual void loadIcon(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                load("icon/" + name + ".png");
            }
            else if (showError)
            {
                load("icon/error.png");
            }
            else { 
                //DebugX.LogWarning("加载的文件名为空");
                clear();
            }
        }

        public virtual void clear()
        {
            uri = null;
            setTexture(null);
        }

        private void completeHandle(EventX e)
        {
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, completeHandle, false);
            if (e.type != EventX.COMPLETE)
            {
                clear();
                return;
            }
            setTexture(resource);
            this.simpleDispatch(EventX.COMPLETE);
        }

        protected virtual void setTexture(AssetResource resource)
        {
            if (image == null && rawImage == null)
            {
                DebugX.LogWarning("IconSlot的RawImage或Image没赋值:"+uri);
                return;
            }

            if (image != null)
            {
                if (resource != null)
                {
                    image.sprite = resource.getSprite();
                    if (isSetEnabledTrue)
                    {
                        image.enabled = true;
                    }
                }
                else
                {
                    image.sprite = null;
                    image.enabled = false;
                }
                if (isSetNativeSize)
                {
                    image.SetNativeSize();
                }
            }

            if (rawImage != null)
            {
                if (resource != null)
                {
                    rawImage.texture = resource.getTexture();
                    if (isSetEnabledTrue)
                    {
                        rawImage.enabled = true;
                    }
                }
                else
                {
                    rawImage.texture = null;
                    rawImage.enabled = false;
                }
                if (isSetNativeSize)
                {
                    rawImage.SetNativeSize();
                }
            }
        }

        public void blink()
        {
            if (image != null)
            {
                image.CrossFadeAlpha(0.5f, 0.2f, true);
                CallLater.Add(() => image.CrossFadeAlpha(1.0f, 0.2f, true), 0.2f);
            }
            else if (rawImage != null)
            {
                rawImage.CrossFadeAlpha(0.5f, 0.2f, true);
                CallLater.Add(() => rawImage.CrossFadeAlpha(1.0f, 0.2f, true), 0.2f);
            }
        }

        
    }
}