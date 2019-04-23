using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using foundation.monoExt;
using gameSDK;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace foundation
{
    public class AbstractPanel : SkinBase, IPanel, IAsync
    {
        public static LoaderXDataType DEFAULT_LOADERX_DATA_TYPE = LoaderXDataType.NONE;
        /// <summary>
        /// 所有打开的panel列表
        /// </summary>
        private static ASDictionary<string, AbstractPanel> openPanel = new ASDictionary<string, AbstractPanel>();

        /// <summary>
        /// todo 事件侦听 当前打开的面板列表;
        /// </summary>
        private static ASDictionary<string, List<AbstractPanel>> eventOpenPanel = new ASDictionary<List<AbstractPanel>>();

        public IMediator __refMediator
        {
            get;
            set;
        }
        public static float BACKGROUND_ALPHA = 0.5f;
        /// <summary>
        /// 在编辑器内直接加载prefab
        /// </summary>
        public static bool InEditorUITest = false;
        public static string InEditorUIPrefix = "Assets/Prefabs/assetBundle/UI/";
        /// <summary>
        /// 
        /// </summary>
        private static Stack<GameObject> backgroundPoolList = new Stack<GameObject>();
        public static bool DEBUG_UI = false;
        protected Action<EventX> readyHandle;
        private AssetResource resource;
        private bool _isReadyShow = false;
        protected bool _isBackgroundClickHide = false;
        protected bool _isClickOutHide = false;
        protected LoaderXDataType loaderXDataType = LoaderXDataType.PREFAB;
        protected string _uri;
        protected bool _resizeable = false;
        protected bool _isReady = false;
        protected bool _isModel = false;
        protected bool _isForceModel = false;

        protected int _effectType = 0;
        protected RFTweenerTask toggleTweener;
        protected bool _isToOpenPanel = true;

        protected GameObject _background;
        protected RawImage _backgroundGraphic;
        protected LoadState state = LoadState.NONE;
        protected GameObject _parent;
        protected GameObject _backgroundParent;

        /// <summary>
        /// 是否显示Loading 
        /// </summary>
        protected bool _isShowLoading = true;

        protected string typeName;

        public AbstractPanel()
        {
            autoDefaultSize = false;
            SetActive(false);

            if (DEFAULT_LOADERX_DATA_TYPE != LoaderXDataType.NONE)
            {
                loaderXDataType = DEFAULT_LOADERX_DATA_TYPE;
            }

            this.typeName = this.GetType().Name;
        }

        protected virtual void load()
        {
            if (state == LoadState.LOADING)
            {
                return;
            }


            if (precheckEditorLoad())
            {
                return;
            }

            if (resource != null)
            {
                resource.release();
                AssetsManager.bindEventHandle(resource, completeHandle, false);
                resource.removeEventListener(EventX.PROGRESS, progressHandle);
            }

            state = LoadState.LOADING;
            string url = getURL(_uri);
            //Debug.Log("url:"+url);
            if (_isShowLoading)
            {
                toggleLoadingUI(true);
            }
            resource = AssetsManager.getResource(url, loaderXDataType);
            resource.isAutoMainAsset = true;
            resource.retain();
            autoDefaultResource(resource);
            AssetsManager.bindEventHandle(resource, completeHandle);
            resource.addEventListener(EventX.PROGRESS, progressHandle);
            resource.load(1, true);
        }

        protected virtual bool precheckEditorLoad()
        {
            if (InEditorUITest)
            {
#if UNITY_EDITOR
                GameObject skinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InEditorUIPrefix + _uri + ".prefab");
                if (skinPrefab != null)
                {
                    remoteSkinHandle(GameObject.Instantiate(skinPrefab));
                    return true;
                }
#endif
            }
            return false;
        }


        protected virtual void autoDefaultResource(AssetResource resource)
        {
        }

        public string getURI()
        {
            return _uri;
        }

        protected virtual string getURL(string uri)
        {
            if (loaderXDataType == LoaderXDataType.PREFAB)
            {
                return PathDefine.uiPath + "ui/" + uri + PathDefine.U3D;
            }
            else
            {
                return "Prefabs/" + uri;
            }
        }

        public Coroutine StartCoroutine(IEnumerator action)
        {
            return _skin.StartCoroutine(action);
        }

        protected virtual void toggleLoadingUI(bool show)
        {
        }

        protected virtual void progressHandle(EventX e)
        {
            this.dispatchEvent(e);
        }

        protected virtual void completeHandle(EventX e)
        {
            if (_isShowLoading)
            {
                toggleLoadingUI(false);
            }
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, completeHandle, false);
            resource.removeEventListener(EventX.PROGRESS, progressHandle);
            if (DEBUG_UI)
            {
                DebugX.Log("UI:" + this._uri + ":" + e.type);
            }

            if (e.type == EventX.FAILED)
            {
                state = LoadState.ERROR;
                DebugX.Log("UI加载失败:" + this._uri + ":" + e.type);
                return;
            }
            state = LoadState.COMPLETE;
            GameObject value = resource.getNewInstance() as GameObject;

            remoteSkinHandle(value);
        }

        protected virtual void remoteSkinHandle(GameObject skinValue)
        {
            skin = skinValue;
            _isReady = true;

            if (readyHandle != null)
            {
                readyHandle(new EventX(EventX.READY));
                readyHandle = null;
            }

            this.simpleDispatch(EventX.READY);

            if (_isReadyShow)
            {
                _isReadyShow = false;
                this.doShow();
            }
        }

        /// <summary>
        /// 当前面板所有相关的PanelDelegate引用
        /// </summary>
        private List<PanelDelegate> panelDelegates = new List<PanelDelegate>();
        public T addDelegate<T>(GameObject skin) where T : PanelDelegate, new()
        {
            T del = Facade.RouterCreateInstance<T>();
            del.onRegister();
            del.root = this;
            del.skin = skin;
            panelDelegates.Add(del);
            return del;
        }

        /// <summary>
        /// 取得所有的panelDelegate;
        /// </summary>
        /// <returns></returns>
        public List<PanelDelegate> getPanelDelegates()
        {
            return panelDelegates;
        }

        public bool isDispose
        {
            get; private set;
        }

        public override void Dispose()
        {
            isDispose = true;
            if (isShow)
            {
                hide();
            }
            if (toggleTweener != null)
            {
                toggleTweener.endTween();
                toggleTweener = null;
            }

            clearSkinToReset();
            base.Dispose();
        }

        protected virtual void clearSkinToReset()
        {
            state = LoadState.NONE;
            _isReady = false;

            if (resource != null)
            {
                resource.release();
                AssetsManager.bindEventHandle(resource, completeHandle, false);
                resource.removeEventListener(EventX.PROGRESS, progressHandle);
                resource = null;
            }

            if (_skin != null)
            {
                GameObject s = _skin;
                skin = null;
                GameObject.Destroy(s);
            }
        }
        public bool isShow
        {
            get { return __panelShowState; }
        }

        public bool isReady
        {
            get { return _isReady; }
        }

        public virtual void show(GameObject container = null, bool isModel = false)
        {
            if (isShow)
            {
                return;
            }

            __panelShowState = true;

            if (!checkCanShow())
            {
                return;
            }


            if (_isToOpenPanel)
            {
                openPanel.Add(typeName, this);
            }

            if (_isForceModel)
            {
                _isModel = true;
            }
            else
            {
                _isModel = isModel;
            }

            if (container != null)
            {
                setParent(container);
            }


            doShow();
        }

        public virtual void setParent(GameObject value)
        {
            _parent = value;
        }

        public override object data
        {
            get { return _data; }

            set
            {
                _data = value;
                if (isReady)
                {
                    doData();
                }
                else
                {
                    addReayHandle(reaydoData);
                }
            }
        }

        private void reaydoData(EventX e)
        {
            doData();
        }

        protected override void preAwaken()
        {
            base.preAwaken();
            CallLater.Add(awakeLaterDo, 0.2f);
        }

        protected override void preSleep()
        {
            CallLater.Remove(awakeLaterDo);
            base.preSleep();
        }
        protected virtual void awakeLaterDo()
        {
        }

        /// <summary>
        /// 护持关系处理
        /// </summary>
        private void checkMutualHandle()
        {
            List<string> mutualPanel = ViewRelation.GetMutualPanel(this.GetType().Name);
            for (int i = 0; i < mutualPanel.Count; i++)
            {
                if (openPanel.ContainsKey(mutualPanel[i]))
                {
                    AbstractPanel panel = openPanel[mutualPanel[i]];
                    if (panel == null || panel == this)
                        continue;
                    if (panel.isShow == true)
                    {
                        panel.hide();
                    }
                }
            }
        }

        protected virtual void doShow()
        {
            if (toggleTweener != null)
            {
                toggleTweener.endTween();
                toggleTweener = null;
            }

            SetActive(true);

            if (isReady == false)
            {
                _isReadyShow = true;
                load();
                return;
            }

            if (skin == null)
            {
                return;
            }

            if (_parent == null)
            {
                _parent = UILocater.UILayer;
            }

            _skin.transform.SetParent(_parent.transform, false);

            if (_isModel)
            {
                showBackground();
            }
            if (_resizeable || _isModel)
            {
                ResizeMananger.Add(this);
            }

            bringTop();

            doToggle(true);
            toggleTweener = effectTween(true);
            if (_isToOpenPanel)
            {
                checkMutualHandle();
            }

            if (_isClickOutHide == true)
            {
                InputManager.getInstance().addEventListener(MouseEventX.MOUSE_DOWN, mouseEventHandle);
            }
            this.simpleDispatch(PanelEvent.SHOW);
            if (toggleTweener == null)
            {
                this.simpleDispatch(PanelEvent.MOTION_SHOW_FINISHED);
            }

            Facade.SimpleDispatch(PanelEvent.SHOW, this);
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

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="isShow"></param>
        protected virtual RFTweenerTask effectTween(bool isShow)
        {
            toggleTweener = doToggleEffectTween(isShow);
            if (toggleTweener != null)
            {
                toggleTweener.userData = isShow;
                toggleTweener.onComplete = recycleToggleEffectTween;
            }

            return toggleTweener;
        }

        protected virtual RFTweenerTask doToggleEffectTween(bool isShow)
        {
            return null;
        }

        protected virtual void recycleToggleEffectTween(RFTweenerTask task)
        {
            toggleTweener = null;
            bool value = (bool)task.userData;
            if (!value)
            {
                effectTweenEndHideSkin();
            }
            else
            {
                this.simpleDispatch(PanelEvent.MOTION_SHOW_FINISHED);
            }
        }


        protected virtual void effectTweenEndHideSkin()
        {
            SetActive(false);
            this.simpleDispatch(PanelEvent.MOTION_HIDE_FINISHED);
        }

        protected override void onSkinDestroy()
        {
            if (toggleTweener != null)
            {
                toggleTweener.endTween();
                toggleTweener = null;
            }
            base.onSkinDestroy();
        }


        private RFTweenerTask backgroundTweener;

        protected virtual void showBackground()
        {
            if (backgroundTweener != null)
            {
                backgroundTweener.endTween();
                backgroundTweener = null;
            }

            if (_background == null)
            {
                _background = GetBackGroundFromPool(out _backgroundGraphic);
                _backgroundGraphic.texture = UIUtils.GetSharedColorTexture(new Color(0, 0, 0, BACKGROUND_ALPHA));
            }
            if (_backgroundGraphic == null)
            {
                _backgroundGraphic = _background.GetComponent<RawImage>();
                _backgroundGraphic.texture = UIUtils.GetSharedColorTexture(new Color(0, 0, 0, BACKGROUND_ALPHA));
            }

            if (_backgroundParent == null)
            {
                _backgroundParent = _skin;
            }
            _background.transform.SetParent(_backgroundParent.transform, false);
            _background.transform.SetAsFirstSibling();

            _background.SetActive(true);

            Color a = _backgroundGraphic.color;
            a.a = 0.0f;
            _backgroundGraphic.color = a;

            float toAlpha = 1.0f;
            backgroundTweener = TweenUIColor.PlayAlpha(_backgroundGraphic, 0.2f, toAlpha);
            backgroundTweener.userData = true;
            backgroundTweener.onComplete = backgroundTweenerCompleteHandle;
            if (_isBackgroundClickHide)
            {
                Get(_background).addEventListener(MouseEventX.CLICK, backgroundClickHandle);
            }
        }

        protected virtual void hideBackground()
        {
            if (backgroundTweener != null)
            {
                backgroundTweener.stop();
                backgroundTweener = null;
            }

            if (_background == null)
            {
                return;
            }

            float toAlpha = 0f;
            backgroundTweener = TweenUIColor.PlayAlpha(_backgroundGraphic, 0.2f, toAlpha);
            backgroundTweener.userData = false;
            backgroundTweener.onComplete = backgroundTweenerCompleteHandle;

            if (_isBackgroundClickHide)
            {
                Get(_background).removeEventListener(MouseEventX.CLICK, backgroundClickHandle);
            }
        }

        protected virtual void doToggle(bool isShow)
        {
            if (isShow == false)
            {
            }
        }

        private void backgroundTweenerCompleteHandle(RFTweenerTask o)
        {
            backgroundTweener = null;

            if ((bool)o.userData == false)
            {
                RecycleBackGroundToPool(_background);
                _background = null;
                _backgroundGraphic = null;
            }
        }

        /// <summary>
        /// 创建背景
        /// </summary>
        protected static GameObject GetBackGroundFromPool(out RawImage rawImage)
        {
            GameObject go = null;
            while (backgroundPoolList.Count > 0)
            {
                go = backgroundPoolList.Pop();
                if (go != null)
                {
                    break;
                }
            }

            if (go != null)
            {
                rawImage = go.GetComponent<RawImage>();
            }
            else
            {
                rawImage = UIUtils.CreateRawImage("Background", UILocater.FollowLayer);
                rawImage.raycastTarget = true;
                go = rawImage.gameObject;
                go.transform.SetAsFirstSibling();
                rawImage.texture = UIUtils.GetSharedColorTexture(new Color(0, 0, 0, BACKGROUND_ALPHA));
            }
            return go;
        }

        protected static bool RecycleBackGroundToPool(GameObject background)
        {
            if (backgroundPoolList.Count > 20)
            {
                GameObject.Destroy(background);
                return false;
            }

            background.transform.SetParent(BaseApp.PoolContainer.transform, false);
            background.SetActive(false);
            backgroundPoolList.Push(background);
            return true;
        }

        protected virtual void clickHide()
        {
            hide();
        }

        public virtual void hide(EventX e = null)
        {
            if (isShow == false)
            {
                return;
            }
            doHide();
        }

        protected virtual void doHide()
        {
            if (toggleTweener != null)
            {
                toggleTweener.endTween();
                toggleTweener = null;
            }

            __panelShowState = false;

            _isReadyShow = false;
            if (_isToOpenPanel)
            {
                openPanel.Remove(this.typeName);
            }
            if (_resizeable || _isModel)
            {
                ResizeMananger.Remove(this);
            }
            if (_background != null)
            {
                hideBackground();
            }
            doToggle(false);
            toggleTweener = effectTween(false);
            this.simpleDispatch(PanelEvent.HIDE);
            if (toggleTweener == null)
            {
                //todo 换成动画完成时设置false;
                effectTweenEndHideSkin();
            }
            if (_isClickOutHide == true)
            {
                InputManager.getInstance().removeEventListener(MouseEventX.MOUSE_DOWN, mouseEventHandle);
            }

            Facade.SimpleDispatch(PanelEvent.HIDE, this);
        }

        public virtual void bringTop()
        {
            _skin.transform.SetAsLastSibling();
        }
        public virtual ASList<string> getSceneState()
        {
            return null;
        }

        public virtual void changeState(int mapType, string panelState)
        {
            includeChangeHandler();
        }


        public virtual void onResize(float width, float height)
        {
            if (_isModel && _background != null)
            {
                doResize((int)width, (int)height);
            }
        }

        protected virtual void doResize(int width, int height)
        {
            UIUtils.SetSize(_background.gameObject, width, height);
        }
        protected virtual void backgroundClickHandle(EventX e)
        {
            this.hide(e);
        }
        public bool startSync()
        {
            if (_isReady == false)
            {
                load();
            }
            return true;
        }
        public bool addReayHandle(Action<EventX> handle)
        {
            if (_isReady)
            {
                handle(new EventX(EventX.READY));
                return true;
            }

            readyHandle += handle;
            return true;
        }
        public bool removeReayHandle(Action<EventX> handle)
        {
            if (_isReady)
            {
                return false;
            }

            readyHandle -= handle;
            return true;
        }

        public bool removeAllReayHandle()
        {
            if (_isReady)
            {
                return false;
            }
            readyHandle = null;
            return true;
        }

        protected bool __panelShowState = false;

        /// <summary>
        /// 包含在那些里面
        /// </summary>
        protected int[] mapTypeIncludes;

        protected bool mapTypeIncludeFlag;

        /// <summary>
        /// 除了那些都显示 
        /// </summary>
        /// <param name="args"></param>
        protected void includeOutMapType(params int[] args)
        {
            mapTypeIncludeFlag = false;
            mapTypeIncludes = args;
        }

        /// <summary>
        /// 只显示在固定的地方
        /// </summary>
        /// <param name="args"></param>
        protected void includeInMapType(params int[] args)
        {
            mapTypeIncludeFlag = true;
            mapTypeIncludes = args;
        }

        /// <summary>
        /// 包含在那些里面
        /// </summary>
        protected string[] includes;

        protected bool includeFlag;

        /// <summary>
        /// 除了那些都显示 
        /// </summary>
        /// <param name="args"></param>
        protected void includeOut(params string[] args)
        {
            includeFlag = false;
            includes = args;
        }

        /// <summary>
        /// 只显示在固定的地方
        /// </summary>
        /// <param name="args"></param>
        protected void includeIn(params string[] args)
        {
            includeFlag = true;
            includes = args;
        }

        protected void includeChangeHandler()
        {
            if (mapTypeIncludes == null && includes == null) return;

            //保留原始显示状态
            var lastShowState = __panelShowState;

            if (lastShowState && checkCanShow())
            {
                doShow();
            }
            else
            {
                hide();
            }

            __panelShowState = lastShowState;
        }

        protected bool checkCanShow()
        {
            return canShowInMap() && canShowInState();
        }

        protected bool canShowInMap()
        {
            bool result;
            if (mapTypeIncludes == null)
            {
                return true;
            }
            else
            {
                if (mapTypeIncludeFlag)
                {
                    result = Array.IndexOf(mapTypeIncludes, Facade.MapIntKey) != -1;
                }
                else
                {
                    result = Array.IndexOf(mapTypeIncludes, Facade.MapIntKey) == -1;
                }
            }
            return result;
        }

        protected bool canShowInState()
        {
            bool result;
            if (includes == null)
            {
                return true;
            }
            if (includeFlag)
            {
                result = Array.IndexOf(includes, Facade.CurrentViewState) != -1;
            }
            else
            {
                result = Array.IndexOf(includes, Facade.CurrentViewState) == -1;
            }
            return result;
        }

        /// <summary>
        /// 关闭所有模式窗口
        /// </summary>
        /// <param name="exclude">除了这个面板</param>
        public static void closeAllModalPanel(string exclude)
        {
            List<AbstractPanel> closeList = new List<AbstractPanel>();
            foreach (string panelName in openPanel)
            {
                if (!string.IsNullOrEmpty(exclude) && panelName == exclude)
                {
                    continue;
                }
                AbstractPanel p = openPanel[panelName];
                if (p._isModel && p.isShow)
                {
                    closeList.Add(p);
                }
            }

            foreach (AbstractPanel p in closeList)
            {
                p.hide();
            }
        }
        protected override void prebindComponents()
        {
            if (_parent == null)
            {
                _parent = UILocater.UILayer;
            }
            ///先加入到指定的容器内
            /// why?
            /// becuase 默认会进入到当前激活的场景,但切换场景时会销毁当激活前场景所有东西
            /// 如果这个时候，你的skin的active=false时,它身上的所有mono类都会不执行
            /// 这时你会发现永远没有回调事件来通知你,你的业务代码可能还傻傻的以为skin还在
            /// 所有的加载业务也会认为加载也已经完成,就会有一堆搞不清楚的问题
            /// so 以后遇到莫名其秒的调用，然后unity又报is destoryed,那就得看是否维护的代码是在deactive状态加入的
            if (_skin != null)
            {
                _skin.transform.SetParent(_parent.transform, false);
            }

            /*
            UIPrefabSlot[] prefabSlots = _skin.GetComponentsInChildren<UIPrefabSlot>(true);
            foreach (UIPrefabSlot slot in prefabSlots)
            {
                if (slot.enabled)
                {
                    slot.__create();
                }
            }*/

            base.prebindComponents();
        }
    }


}