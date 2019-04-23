using foundation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace gameSDK
{
    public class BaseApp : AbstractApp
    {
        public static BaseApp Instance;
        new public static bool IsHasEditorCode
        {
            get;
            protected set;
        }
        public BaseApp()
        {
#if UNITY_EDITOR
            IsHasEditorCode = true;
#else
            IsHasEditorCode = false;
#endif
        }

        public static TwoDRender twoDRender { get; protected set; }
        public static BaseSkillManager skillManager { get; protected set; }
        public static BaseActorManager actorManager { get; protected set; }
        public static BaseSceneManager sceneManager { get; protected set; }
        public static BaseRaderManager raderManager { get; protected set; }
        public static BaseEffectManager effectManager { get; protected set; }
        public static BaseStoryManager storyManager { get; protected set; }
        public static BaseCameraController cameraController { get; protected set; }
        public static ResizeMananger resizeMananger { get; protected set; }
        public static StateMachine sceneMachine
        {
            get;
            protected set;
        }

        public HideFlags containerHideFlags = HideFlags.HideAndDontSave;
        [SerializeField]
        private bool isUseServer = false;
        protected bool isInitialized = false;


        public static bool IsUseServer
        {
            get { return Instance.isUseServer || useShell; }
        }

        /// <summary>
        /// 是否使用了壳程序
        /// </summary>
        public static bool IsUseShell
        {
            get { return useShell; }
        }

        private static bool useShell = false;
        public static void InjectByShell(Dictionary<string, string> dic)
        {
            useShell = true;
            ApplicationVersion.InjectByShell(dic);
        }
        public static Vector2 WorldToUIPoint(Vector3 worldVector3, RectTransform rectTransform = null)
        {
            Vector2 resultVector2 = Vector2.zero;
            if (MainCamera == null)
            {
                return resultVector2;
            }

            Vector3 screenVector3 = MainCamera.WorldToScreenPoint(worldVector3);

            if (MainCamera.targetTexture != null)
            {
                float with = MainCamera.targetTexture.width;
                float height = MainCamera.targetTexture.height;
                screenVector3.x = screenVector3.x / with * Screen.width;
                screenVector3.y = screenVector3.y / height * Screen.height;
            }
          
            if (rectTransform == null)
            {
                rectTransform = UICanvasTransform;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenVector3, UICamera, out resultVector2);
            return resultVector2;       
        }

        /// <summary>
        /// 是否焦点在文本控件上
        /// </summary>
        /// <returns></returns>
        public static bool IsFocusTextField()
        {
            GameObject go=EventSystem.current.currentSelectedGameObject;
            if (go != null)
            {
                return go.GetComponent<InputField>()!=null;
            }
            return false;
        }

        public static Vector2 UIWorldPositionToScreen(Vector3 worldVector3)
        {
            return UICamera.WorldToScreenPoint(worldVector3);
        }

        public static Vector2 ScreenToUIPoint(Vector2 screenVector3,RectTransform rectTransform=null)
        {
            Vector2 resultVector2 = Vector2.zero;
            if (rectTransform == null)
            {
                rectTransform = UICanvasTransform;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenVector3, UICamera, out resultVector2);
            return resultVector2;
        }
        protected virtual void Start()
        {
        }

        protected virtual void OnEnable()
        {
            if (isInitialized)
            {
                return;
            }

            cleanNotification();

            isInitialized = true;

            Instance = this;
            autoCreateChildren();
         
            BaseRigsterUtils.init();

            preInitialize();
            initialize();
            postInitialize();
        }


        protected virtual void OnDisable()
        {
        }

        protected virtual void OnApplicationQuit()
        {
        }

        protected virtual void OnApplicationPause(bool b)
        {
        }
        protected override void autoCreateChildren()
        {
            GameObject go= GameObject.Find("Main Camera");
            if (go == null)
            {
                go=GameObject.Find("Camera");
            }

            if (go != null)
            {
                AudioListener audioListener = go.GetComponent<AudioListener>();
                BaseApp.MainCamera = go.GetComponent<Camera>();
                SetDefaultCamera(BaseApp.MainCamera);
                SetDefaultAudioListener(audioListener);
            }
            else
            {
                BaseApp.MainCamera=Camera.current;
                if (BaseApp.MainCamera == null)
                {
                    BaseApp.MainCamera = Camera.main;
                }
            }

            go = GameObject.Find("Directional Light");
            if (go == null)
            {
                go = GameObject.Find("SunLight");
            }
            if (go != null)
            {
                BaseApp._DefaultSunLight = go.GetComponent<Light>();
            }

            base.autoCreateChildren();
        }


        public static void SwitchMainCamera(Camera value)
        {
            if (_MainCamera == value)
            {
                return;
            }

            if (_MainCamera != null)
            {
                _MainCamera.enabled=false;
            }
            _MainCamera = value;
            if (_MainCamera == null)
            {
                _MainCamera = _DefaultMainCamera;
            }

            if (_MainCamera != null)
            {
                if (_MainCamera.enabled==false)
                {
                    _MainCamera.enabled=true;
                }
                if (cameraController != null)
                {
                    cameraController.refreashCamera();
                }
            }
            return;
        }

        public static void SetDefaultAudioListener(AudioListener value)
        {
            _DefaultAudioListener = value;
            if (value)
            {
                SwitchAudioListener(value);
            }
        }
        public static void SetDefaultCamera(Camera value)
        {
            _DefaultMainCamera = value;
            if (value)
            {
                SwitchMainCamera(value);
            }
        }

        public static AudioListener CurrentAudioListener
        {
            get
            {
                if (_CurrentAudioListener == null)
                {
                    return _DefaultAudioListener;
                }
                return _CurrentAudioListener;
            }
        }

        /// <summary>
        /// 默认与当前的声音侦听器;
        /// </summary>
        private static AudioListener _DefaultAudioListener;
        private static AudioListener _CurrentAudioListener;
        public static void SwitchAudioListener(AudioListener audioListener)
        {
            if (_CurrentAudioListener == audioListener)
            {
                if (_CurrentAudioListener == null)
                {
                    ResetDefaultAudioListener();
                }
                return;
            }

            if (_CurrentAudioListener)
            {
                _CurrentAudioListener.enabled = false;
            }

            _CurrentAudioListener = audioListener;

            if (_CurrentAudioListener == null)
            {
                ResetDefaultAudioListener();
            }
            else
            {
                if (_CurrentAudioListener.enabled == false)
                {
                    _CurrentAudioListener.enabled = true;
                }
                if (_DefaultAudioListener == null)
                {
                    _DefaultAudioListener = _CurrentAudioListener;
                }
            }
        }

        private static void ResetDefaultAudioListener()
        {
            _CurrentAudioListener = _DefaultAudioListener;
            if (_CurrentAudioListener && _CurrentAudioListener.enabled == false)
            {
                _CurrentAudioListener.enabled = true;
            }
        }

        protected virtual void initializeUILayer(Canvas uiCanvas, Camera uiCamera)
        {
            CanvasScaler = uiCanvas.GetComponent<CanvasScaler>();
            GraphicRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();
            UILocater.initialize(uiCanvas, CanvasScaler);
            UICamera = uiCamera;
            UICanvas = uiCanvas;

            UICanvasTransform = UICanvas.GetComponent<RectTransform>();
            resizeMananger.initialize(CanvasScaler, UICamera);
        }

        protected virtual void preInitialize()
        {
            if (coreLoaderQueue == null)
            {
                coreLoaderQueue = GetComponent<CoreLoaderQueue>();
                if (coreLoaderQueue == null)
                {
                    coreLoaderQueue = gameObject.AddComponent<CoreLoaderQueue>();
                }
            }

            if (sceneMachine == null)
            {
                sceneMachine = GetComponent<StateMachine>();
                if (sceneMachine == null)
                {
                    sceneMachine = gameObject.AddComponent<StateMachine>();
                }
            }

            if (raderManager == null)
            {
                raderManager = ActorContainer.GetComponent<BaseRaderManager>();
                if (raderManager == null)
                {
                    raderManager = ActorContainer.AddComponent<BaseRaderManager>();
                }
            }
            if (resizeMananger == null)
            {
                resizeMananger = GetComponent<ResizeMananger>();
                if (resizeMananger == null)
                {
                    resizeMananger = gameObject.AddComponent<ResizeMananger>();
                }
            }

            if (inputManager == null)
            {
                inputManager = GetComponent<InputManager>();
                if (inputManager == null)
                {
                    inputManager = gameObject.AddComponent<InputManager>();
                }
            }

            if (twoDRender == null)
            {
                twoDRender = GetComponent<TwoDRender>();
                if (twoDRender == null)
                {
                    twoDRender = gameObject.AddComponent<TwoDRender>();
                }
            }
            if (skillManager == null)
            {
                skillManager = GetComponent<BaseSkillManager>();
                if (skillManager == null)
                {
                    skillManager = gameObject.AddComponent<BaseSkillManager>();
                }
            }

            if (storyManager == null)
            {
                storyManager = GetComponent<BaseStoryManager>();
                if (storyManager == null)
                {
                    storyManager = gameObject.AddComponent<BaseStoryManager>();
                }
            }

            if (rectTriggerManager == null)
            {
                rectTriggerManager = GetComponent<BaseRectTriggerManager>();
                if (rectTriggerManager == null)
                {
                    rectTriggerManager = gameObject.AddComponent<BaseRectTriggerManager>();
                }
            }
            if (actorManager == null)
            {
                actorManager = GetComponent<BaseActorManager>();
                if (actorManager == null)
                {
                    actorManager = gameObject.AddComponent<BaseActorManager>();
                }
            }
            if (effectManager == null)
            {
                effectManager = GetComponent<BaseEffectManager>();
                if (effectManager == null)
                {
                    effectManager = gameObject.AddComponent<BaseEffectManager>();
                }
            }

            if (soundsManager == null)
            {
                soundsManager = GetComponent<BaseSoundsManager>();
                if (soundsManager == null)
                {
                    soundsManager = gameObject.AddComponent<BaseSoundsManager>();
                }
            }

            if (sceneManager == null)
            {
                sceneManager = GetComponent<BaseSceneManager>();
                if (sceneManager == null)
                {
                    sceneManager = gameObject.AddComponent<BaseSceneManager>();
                }
            }
            if (cameraController == null)
            {
                cameraController = GetComponent<BaseCameraController>();
                if (cameraController == null)
                {
                    cameraController = gameObject.AddComponent<BaseCameraController>();
                }
            }
        }


        //本地推送
        public static void NotificationMessage(string message, int hour, bool isRepeatDay)
        {
            Instance.notificationMessage(message, hour, isRepeatDay);
        }
        public static void CleanNotification()
        {
            Instance.cleanNotification();
        }

        protected virtual void notificationMessage(string message, int hour, bool isRepeatDay)
        {
            int year = System.DateTime.Now.Year;
            int month = System.DateTime.Now.Month;
            int day = System.DateTime.Now.Day;
            System.DateTime newDate = new System.DateTime(year, month, day, hour, 0, 0);
            notificationMessage(message, newDate, isRepeatDay);
        }
        //本地推送 你可以传入一个固定的推送时间
        protected virtual void notificationMessage(string message, System.DateTime newDate, bool isRepeatDay)
        {
//            if (Application.platform == RuntimePlatform.IPhonePlayer)
//            {
//                //推送时间需要大于当前时间
//                if (newDate > System.DateTime.Now)
//                {
//                    UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
//                    localNotification.fireDate = newDate;
//                    localNotification.alertBody = message;
//                    localNotification.applicationIconBadgeNumber = 1;
//                    localNotification.hasAction = true;
//                    if (isRepeatDay)
//                    {
//                        //是否每天定期循环
//                        localNotification.repeatCalendar = UnityEngine.iOS.CalendarIdentifier.ChineseCalendar;
//                        localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
//                    }
//                    localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
//                    UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
//                }
//            }
        }

        protected virtual void cleanNotification()
        {
//            if (Application.platform == RuntimePlatform.IPhonePlayer)
//            {
//                UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification();
//                l.applicationIconBadgeNumber = -1;
//                UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow(l);
//                UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
//                UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
//            }
        }

        private int scaleWidth = 0;
        private int scaleHeight = 0;
        protected virtual void setResolutionByDesignScale(int designWidth, int designHeight)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (scaleWidth == 0 && scaleHeight == 0)
                {
                    int width = Screen.currentResolution.width;
                    int height = Screen.currentResolution.height;
                    float s1 = (float) designWidth / (float) designHeight;
                    float s2 = (float) width / (float) height;
                    if (s1 < s2)
                    {
                        designWidth = (int) Mathf.FloorToInt(designHeight * s2);
                    }
                    else if (s1 > s2)
                    {
                        designHeight = (int) Mathf.FloorToInt(designWidth / s2);
                    }
                    float contentScale = (float) designWidth / (float) width;
                    if (contentScale < 1.0f)
                    {
                        scaleWidth = designWidth;
                        scaleHeight = designHeight;
                    }
                }
                if (scaleWidth > 0 && scaleHeight > 0)
                {
                    if (scaleWidth % 2 == 0)
                    {
                        scaleWidth += 1;
                    }
                    else
                    {
                        scaleWidth -= 1;
                    }
                    doSetResolution(scaleWidth, scaleHeight);
                }
            }
        }

        /// <summary>
        /// 重新设置屏幕分辨率
        /// </summary>
        /// <param name="scaleWidth"></param>
        /// <param name="scaleHeight"></param>
        protected virtual void doSetResolution(int scaleWidth, int scaleHeight)
        {
            Screen.SetResolution(scaleWidth, scaleHeight, true);
        }

        protected virtual void postInitialize()
        {
           
        }

        protected virtual void initialize()
        {
            GameObject go = GameObject.Find("UICamera");
            if (go != null)
            {
                Camera uiCamera = go.GetComponent<Camera>();
                Transform tr = uiCamera.transform.Find("Canvas");
                if (tr != null)
                {
                    Canvas uiCanvas = tr.GetComponent<Canvas>();
                    uiCanvas.pixelPerfect = false;
                    initializeUILayer(uiCanvas, uiCamera);
                }
            }
        }


        public static string CurrentSceneState
        {
            get { return sceneMachine.currentState; }
            set { sceneMachine.currentState = value; }
        }

        public static bool IsCurrentSceneState(params string[] parms)
        {
            foreach (string s in parms)
            {
                if (s == sceneMachine.currentState)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stati">没好办法 填什么都行</param>
        /// <returns></returns>
        public static T GetSceneState<T>(string type) where T : IState
        {
            return (T)sceneMachine.getState(type);
        }

        public static IState GetSceneState(string type)
        {
            return sceneMachine.getState(type);
        }
       
        protected virtual void Update()
        {
            TickManager.Update(Time.deltaTime);
        }

        protected virtual void OnDrawGizmos()
        {
            TickManager.OnDrawGizmos(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            TickManager.FixedUpdate(Time.fixedDeltaTime);
        }

        protected virtual void LateUpdate()
        {
            TickManager.LateUpdate(Time.time);
        }

        /// <summary>
        /// SDK返回数据的接口
        /// </summary>
        /// <param name="value"></param>
        public void PlatformSDKReceive(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                DebugX.Log("PlatformSDKReceive:"+value);
            }

            string[] str = value.Split('~');
            if (str.Length > 0)
            {
                string key = str[0];
                string args = "";
                if (str.Length > 1)
                {
                    args = str[1];
                }
                PlatformSDK.Receive(key, args);
            }
        }


        public virtual void PauseApp(bool value=true)
        {
            if (IsGamePause == value)
            {
                return;
            }
            IsGamePause = value;
            if (IsGamePause)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = DEF_TIME_SCALE;
            }
        }


        /// <summary>
        /// SDK返回数据的接口
        /// </summary>
        /// <param name="value"></param>
        public virtual void Receive(string value)
        {
            PlatformSDKReceive(value);
        }

        public static void ClearPool()
        {
            Transform c = PoolContainer.transform;
            while (c.childCount > 0)
            {
                Transform t = c.GetChild(0);
                if (t != null)
                {
                    GameObject.DestroyImmediate(t.gameObject);
                }
            }
        }
        public static void ClearMemory(bool gc=false)
        {
            Resources.UnloadUnusedAssets();
            if (gc)
            {
                GC.Collect();
            }
        }
    }
}
