using foundation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class UpkFontRender : SkinBase
    {
        private bool isReady = false;
        public bool centerIt = false;
        public string prefixValue="";
        public string subfixValue = "";
        public string searchValueKey="";

        private List<SpriteInfoVO> sprites;
        private AssetResource resource;
        private Dictionary<string,int> keys=new Dictionary<string, int>();

        private string _text;
        private List<Image> children = new List<Image>();
        private static Stack<Image> pools = new Stack<Image>();
        

        public UpkFontRender(GameObject container=null)
        {
            if (container == null)
            {
                container=UIUtils.CreateEmpty("fontRender", UILocater.FollowLayer);
            }
            skin = container;
        }

        public Vector3 position
        {
            get { return _skin.transform.localPosition; }
            set { _skin.transform.localPosition = value; }
        }

        public Vector3 scale
        {
            get { return _skin.transform.localScale; }
            set
            {
                value.z = 1;
                _skin.transform.localScale = value;
            }
        }

        public void load(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            this.name = name;
            string uri = "upk/" + name + ".upk";
            string url = PathDefine.commonPath + uri;

            if (resource != null)
            {
                resource.release();
                AssetsManager.bindEventHandle(resource, completeHandle, false);
            }
            isReady = false;
            resource = AssetsManager.getResource(url, LoaderXDataType.ASSETBUNDLE);
            resource.retain();
            AssetsManager.bindEventHandle(resource, completeHandle);
            resource.load();
        }

        private void completeHandle(EventX e)
        {
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, completeHandle, false);

            if (e.type != EventX.COMPLETE)
            {
                return;
            }
            UpkAniVO o = resource.getMainAsset() as UpkAniVO;
            if (null == o)
            {
                return;
            }

            isReady = true;
            sprites = o.keys;
          
            keys.Clear();

            int len = sprites.Count;
            for (int i = 0; i < len; i++)
            {
                keys.Add(sprites[i].name, i);
            }

            string _oldText = _text;
            _text = "";
            text = _oldText;
        }

        public string text
        {
            get { return _text; }
            set
            {
                if (this._text == value)
                {
                    return;
                }

                this._text = value;
                if (isReady == false)
                {
                    return;
                }

                recycle();

                if (string.IsNullOrEmpty(_text))
                {
                    return;
                }

                Image image;
                Sprite sprite;
                int pos = 0;
                if (string.IsNullOrEmpty(prefixValue)==false)
                {
                    image = getImage();
                    RectTransform rectTransform = image.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(pos, 0);


                    int index = (int)keys[prefixValue];

                    sprite = sprites[index].sprite;
                    Rect rect = sprite.rect;
                    rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                    image.sprite = sprite;
                    pos += (int)rect.width;
                    children.Add(image);
                }
                
                int len = _text.Length;
                for (int i = 0; i < len; i++)
                {
                    image = getImage();
                    RectTransform rectTransform=image.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(pos, 0);
                    char t = _text[i];
                    string key=getMapping(t);

                    int index = 0;
                    if (keys.TryGetValue(searchValueKey + key, out index) == false)
                    {
                        continue;
                    }
                    
                    sprite=sprites[index].sprite;
                    Rect rect = sprite.rect;
                    rectTransform.sizeDelta=new Vector2(rect.width,rect.height);
                    image.sprite = sprite;
                    pos += getMappingWidth(t,rect.width);
                    children.Add(image);
                }

                if (string.IsNullOrEmpty(subfixValue)==false)
                {
                    image = getImage();
                    RectTransform rectTransform = image.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(pos, 0);
                    int index = (int)keys[subfixValue];

                    sprite = sprites[index].sprite;
                    Rect rect = sprite.rect;
                    rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                    image.sprite = sprite;
                    pos += (int)rect.width;
                    children.Add(image);
                }

                if (centerIt)
                {
                    int delta=(int)(pos/2);
                    len = children.Count;
                    for (int i = 0; i < len; i++)
                    {
                        image = children[i];
                        RectTransform rectTransform = image.GetComponent<RectTransform>();
                        Vector3 positon = rectTransform.anchoredPosition;
                        positon.x -= delta;
                        rectTransform.anchoredPosition = positon;
                    }
                }
            }
        }

        public int diyW = -1;
        public int diyH = -1;
        private int getMappingWidth(char v, float width)
        {
            int result = 0;
            if (mappingSize.TryGetValue(v, out result))
            {
                return result;
            }

            if (diyW != -1)
            {
                return diyW;
            }

            return (int)width;
        }

        public void addMapping(char key, string value)
        {
            if (mapping.ContainsKey(key) == false)
            {
                mapping.Add(key, value);
            }
            else
            {
                mapping[key] = value;
            }
        }

        public void addMappingSize(char key, int value)
        {
            if (mappingSize.ContainsKey(key) == false)
            {
                mappingSize.Add(key, value);
            }
            else
            {
                mappingSize[key] = value;
            }
        }

        private Dictionary<char,string> mapping=new Dictionary<char, string>(); 
        private Dictionary<char,int> mappingSize=new Dictionary<char, int>(); 
        private string getMapping(char v)
        {
            string result = null;
            if (mapping.TryGetValue(v, out result))
            {
                return result;
            }
            return v.ToString();
        }

        private void recycle()
        {
            int len = children.Count;
            for (int i = 0; i < len; i++)
            {
                Image image = children[i];
                image.gameObject.SetActive(false);

                pools.Push(image);
            }
            children.Clear();
        }


        private Image getImage()
        {
            Image image;
            if (pools.Count>0)
            {
                image = pools.Pop();
                image.gameObject.SetActive(true);
                image.transform.SetParent(_skin.transform,false);
            }
            else
            {
                image= UIUtils.CreateImage("image", _skin);
                RectTransform rectTransform=image.GetComponent<RectTransform>();
                rectTransform.pivot=new Vector2(0,0.5f);
            }
            return image;
        }
    }

}