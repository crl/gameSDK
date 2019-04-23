using foundation;
using gameSDK;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class SkinBaseItemRender : SkinBase, IListItemRender
    {
        protected bool _isSelected = false;
        protected int _index;
        protected bool _clickEnable;

        public bool isSelected
        {
            get { return _isSelected; }

            set
            {
                _isSelected = value;
                doSelected(value);
            }
        }

        protected virtual void doSelected(bool value)
        {

        }

        public int index
        {
            get { return _index; }

            set { _index = value; }
        }

        public Action<string, IListItemRender,object> itemEventHandle { get; set; }

        public bool clickEnable
        {
            get { return _clickEnable; }
            set
            {
                _clickEnable = value;
                if (_skin != null)
                { 
                    if (_clickEnable)
                    {
                        Button btn = _skin.GetComponent<Button>();
                        if (btn == null)
                        {
                            btn = _skin.AddComponent<Button>();
                        }
                        btn.onClick.AddListener(clickHandler);
                    }
                    else
                    {
                        Button btn = _skin.GetComponent<Button>();
                        if (btn != null)
                        {
                            btn.onClick.RemoveListener(clickHandler);
                        }
                    }
                }
            }
        }

        protected void clickHandler()
        {
            this.simpleDispatch(EventX.CLICK);
        }

        public virtual void bringTop()
        {
            if (_skin != null)
            {
                _skin.transform.SetAsLastSibling();
            }
        }

        public T addDelegate<T>(GameObject skin) where T : PanelDelegate,new()
        {
            T del = Facade.RouterCreateInstance<T>();
            del.onRegister();
            del.skin = skin;
            return del;
        }


        public override void Dispose()
        {
            GameObject oldSkin = _skin;
            if (oldSkin!=null)
            {
                skin = null;
                GameObject.Destroy(oldSkin);
            }
            base.Dispose();
        }
    }
}