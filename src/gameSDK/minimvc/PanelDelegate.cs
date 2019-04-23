using gameSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace foundation
{
    public class PanelDelegate : SkinBase, IEventInterester,IResizeable
    {
        protected static IFacade facade;
        protected IProxy _model;
        protected bool backgroundClickHide = false;
        protected bool _isClickOutHide = false;
        private bool _isModel = false;
        protected bool _ready = false;

        protected float backGroundAlpha = 1;

        public PanelDelegate()
        {
            if (facade == null)
            {
                facade = Facade.GetInstance();
            }
        }
        public AbstractPanel root
        {
            get;
            internal set;
        }

        /// <summary>
        /// 添加一个方便的方法而已
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skin"></param>
        /// <returns></returns>
        protected T addDelegate<T>(GameObject skin) where T : PanelDelegate,new()
        {
            if (root != null)
            {
                return root.addDelegate<T>(skin);
            }
            return null;
        }
        
        public virtual void onRegister()
        {
            facade.inject(this);
            facade.registerEventInterester(this, InjectEventType.Always, true);
        }

        public virtual void onRemove()
        {
            facade.registerEventInterester(this, InjectEventType.Always, false);
        }

        public bool isReady
        {
            get
            {
                return _ready;
            }
        }


        public virtual void show(bool isModel = false)
        {
            _isModel = isModel;

            SetActive(true);
            setBackgroundActive(isModel);

            if (_isModel)
            {
                ResizeMananger.Add(this);
            }

            if (_model is IAsync)
            {
                IAsync asyncModel = _model as IAsync;
                if (asyncModel.isReady == false)
                {
                    _model.addEventListener(EventX.READY, preModelReadyHandle);
                    asyncModel.startSync();
                    return;
                }
            }

           

            if (_ready == false)
            {
                _ready = true;
                onReadyHandle();
            }
        }

        public virtual void hide()
        {
            SetActive(false);

            

            this.simpleDispatch(PanelEvent.CLOSE);
        }

        public virtual void toggle()
        {
            if (_isActive)
            {
                hide();
            }
            else
            {
                show();
            }
        }


        protected override void preAwaken()
        {
            if (_isClickOutHide == true)
            {
                InputManager.getInstance().addEventListener(MouseEventX.MOUSE_DOWN, mouseEventHandle);
            }
            base.preAwaken();
            CallLater.Add(awakeLaterDo, 0.2f);
        }

        protected override void preSleep()
        {
            if (_isClickOutHide == true)
            {
                InputManager.getInstance().removeEventListener(MouseEventX.MOUSE_DOWN, mouseEventHandle);
            }
            CallLater.Remove(awakeLaterDo);
            base.preSleep();
        }
        protected virtual void awakeLaterDo()
        {
            
        }

        protected Image background;
        protected virtual void setBackgroundActive(bool isModel)
        {
            if (isModel == false && background == null)
            {
                return;
            }

            if (background == null)
            {
                background = getImage("black", _skin);
                if (background == null)
                {
                    background = UIUtils.CreateImage("black", _skin);
                }
                background.transform.SetAsFirstSibling();
                background.sprite = UIUtils.GetSharedCDSprite();
                background.color = new Color(1,1,1,backGroundAlpha);
                background.raycastTarget = true;
                if (backgroundClickHide)
                {
                    Get(background).addEventListener(MouseEventX.CLICK, backgroundClickHandle);
                }
            }

            background.enabled = isModel;
        }
        protected virtual void backgroundClickHandle(EventX e)
        {
            this.hide();
        }
        protected void preModelReadyHandle(EventX e)
        {
            _ready = true;
            IProxy proxy = e.target as IProxy;
            proxy.removeEventListener(EventX.READY, preModelReadyHandle);
            onReadyHandle();
        }

        /// <summary>
        /// 鼠标事件
        /// </summary>
        /// <param name="obj"></param>
        private void mouseEventHandle(EventX e)
        {
            if (skin.activeInHierarchy == false) return;
            Vector3 mousePosition = (Vector3)e.data;
            if (e.type == MouseEventX.MOUSE_DOWN)
            {
                if (_isClickOutHide == true && !UITools.IsClickSkin(mousePosition, skin))
                {
                    hide();
                }
            }
        }

        public void setModel(IProxy value)
        {
            if (_model == value)
            {
                return;
            }
            if (_model != null)
            {
                registerModelEvent(_model, false);
            }
            _model = value;
        }

        protected override void stageHandle(EventX e)
        {
            if (e.type == EventX.ADDED_TO_STAGE)
            {
                facade.registerEventInterester(this,InjectEventType.Show,true);
                registerModelEvent(_model, true);
            }
            else if (e.type == EventX.REMOVED_FROM_STAGE)
            {
                facade.registerEventInterester(this, InjectEventType.Show, false);
                registerModelEvent(_model, false);
            }
            base.stageHandle(e);
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
                _model.addEventListener(EventX.CHANGE, modelEventHandle);
                foreach (string key in dic.Keys)
                {
                    _model.addEventListener(key, dic[key]);
                }
            }
            else
            {
                _model.removeEventListener(EventX.CHANGE, modelEventHandle);
                foreach (string key in dic.Keys)
                {
                    _model.removeEventListener(key, dic[key]);
                }
            }
        }


        protected virtual void onReadyHandle()
        {
        }

        [MVCEvent(InjectEventType.Model, EventX.CHANGE)]
        protected virtual void modelEventHandle(EventX e)
        {
            if (isReady)
            {
                updateView();
            }
        }

        protected virtual void hide(EventX e)
        {
            hide();
        }

        public virtual void onResize(float width, float height)
        {
            if (_isModel && background != null)
            {
                doResize((int)width, (int)height);
            }
        }

        protected virtual void doResize(int canvasPixelWidth, int canvasPixelHeight)
        {
            UIUtils.SetSize(background.gameObject, canvasPixelWidth, canvasPixelHeight);
        }
    }
}