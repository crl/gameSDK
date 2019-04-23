using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using foundation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace gameSDK
{
    public class BaseSceneManager : FoundationBehaviour, ISceneManager
    {
        public float MAX_PROGRESS = 0.8f;
        protected Coroutine __loadSceneCoroutine;
        private QueueLoader queueLoader = new QueueLoader();

        /// <summary>
        /// 场景配置
        /// </summary>
        protected SceneCFG _sceneCFG;

        public bool isReady { get; private set; }

        protected AssetResource _resource;
        protected string _currentsSeneName;
        protected string _currentSceneResourceName;
        public string prefix = "map/";
        protected Camera _sceneCamera;
        protected AudioSource _sceneAudioSource;
        protected Scene _currentScene;
        protected AsyncOperation unloadSceneAsyncOperation;

        public Action<GameObject> checkSceneGameObject
        {
            get; set;
        }
        public Vector3 bornPosition
        {
            get; set;
        }
        public Vector3 bornRotation
        {
            get; set;
        }

        public string currentSceneName
        {
            get { return _currentsSeneName; }
        }

        public virtual float getNavHeight(Vector3 position)
        {
            return bornPosition.y;
        }

        public virtual bool getNearNavPosition(Vector3 position, out Vector3 near)
        {
            near = Vector3.zero;
            return false;
        }

        public virtual List<Vector3> getNavPathList(Vector3 from, Vector3 to, bool useWaypoint = false)
        {
            return null;
        }

        protected virtual void Start()
        {
        }

        public virtual void routerMapElement(MonoCFG elementCfg)
        {
            if (elementCfg is UnitCFG)
            {
                if ((elementCfg as UnitCFG).unitType == UnitType.Start)
                {
                    bornPosition = elementCfg.transform.position;
                    bornRotation = elementCfg.transform.eulerAngles;
                }
            }
        }

        public virtual void setCenterPosition(Vector3 v)
        {
        }

        public bool load(string sceneName)
        {
            if (_currentsSeneName == sceneName)
            {
                if (isReady)
                {
                    this.simpleDispatch(EventX.COMPLETE);
                }
                return true;
            }

            clear();

            _currentsSeneName = sceneName;
            if (string.IsNullOrEmpty(_currentsSeneName) == false)
            {
                this.simpleDispatch(EventX.START, _currentsSeneName);
                doResourceLoadScene(_currentsSeneName);
                return true;
            }
            return false;
        }

        public virtual void clear()
        {
            queueLoader.recycle();

            if (_resource != null)
            {
                _resource.removeEventListener(EventX.PROGRESS, progressHandle);
                AssetsManager.bindEventHandle(_resource, completeHandle, false);
                _resource.release();
            }

            if (__loadSceneCoroutine != null)
            {
                StopCoroutine(__loadSceneCoroutine);
                __loadSceneCoroutine = null;
            }

            if (string.IsNullOrEmpty(_currentSceneResourceName) == false)
            {
                onUnloadPreScene(_currentSceneResourceName);
                _currentSceneResourceName = null;
            }

            _sceneCFG = null;
            if (_sceneCamera != null)
            {
                _sceneCamera = null;
                BaseApp.SwitchMainCamera(_sceneCamera);
            }

            if (_sceneAudioSource != null)
            {
                _sceneAudioSource = null;
            }

            isReady = false;
            _currentsSeneName = null;

            this.simpleDispatch(EventX.CLEAR);
        }

        protected virtual void doResourceLoadScene(string sceneName)
        {
            string url = getURL(sceneName);
            _resource = AssetsManager.getResource(url, LoaderXDataType.PREFAB);
            _resource.isAutoMainAsset = false;
            _resource.retain();
            _resource.addEventListener(EventX.PROGRESS, progressHandle);
            AssetsManager.bindEventHandle(_resource, completeHandle);

            _resource.load(2, true,100);
        }

        protected virtual string getURL(string value)
        {
            string uri = prefix + value + PathDefine.U3D;
            return PathDefine.scenePath + uri;
        }

        private void completeHandle(EventX e)
        {
            AssetResource resource = (AssetResource) e.target;
            resource.removeEventListener(EventX.PROGRESS, progressHandle);
            AssetsManager.bindEventHandle(_resource, completeHandle, false);

            if (e.type != EventX.COMPLETE)
            {
                this.simpleDispatch(EventX.FAILED);
                return;
            }
            string[] list = resource.getAllScenePaths();
            if (list == null || list.Length == 0)
            {
                DebugX.LogError("sceneResourceList is empty");
                this.simpleDispatch(EventX.FAILED);
                return;
            }
            _currentSceneResourceName = Path.GetFileNameWithoutExtension(list[0]);
            if (string.IsNullOrEmpty(_currentSceneResourceName))
            {
                DebugX.LogError("sceneResourceName is empty");
                this.simpleDispatch(EventX.FAILED);
                return;
            }
            StartCoroutine(loadScene2(_currentsSeneName, _currentSceneResourceName));
        }

        protected List<MonoCFG> monoCFGList;
        protected IEnumerator loadScene2(string sceneName,string sceneResourceName)
        {
            if (unloadSceneAsyncOperation != null)
            {
                while (!unloadSceneAsyncOperation.isDone)
                {
                    yield return null;
                }
                unloadSceneAsyncOperation = null;
            }

            BaseApp.ClearMemory(true);
            yield return null;

            AsyncOperation async = SceneManager.LoadSceneAsync(sceneResourceName, LoadSceneMode.Additive);
            while (!async.isDone)
            {
                this.simpleDispatch(EventX.PROGRESS, MAX_PROGRESS + async.progress * (1.0f- MAX_PROGRESS));
                yield return null;
            }

            if (_currentsSeneName != sceneName)
            {
                onUnloadPreScene(sceneResourceName);
                yield break;
            }

            //DebugX.Log("loadScene2 b");
            _currentScene = SceneManager.GetSceneByName(_currentSceneResourceName);
            while (!SceneManager.SetActiveScene(_currentScene))
            {
                yield return null;
            }

            if (monoCFGList == null)
            {
                monoCFGList = new List<MonoCFG>();
            }
            else if(monoCFGList.Count>0)
            {
                monoCFGList.Clear();
            }
            ///做初始化前的清理操作
            preReady();

            //DebugX.Log("loadScene2 0");

            foreach (GameObject gameObject in _currentScene.GetRootGameObjects())
            {
                MonoCFG[] tempList = gameObject.GetComponentsInChildren<MonoCFG>();
                if (tempList.Length > 0)
                {
                    foreach (MonoCFG cfg in tempList)
                    {
                        ///优先把SceneCFG给初始化掉
                        if (cfg is SceneCFG)
                        {
                            bindSceneCFG((SceneCFG)cfg);
                        }
                        monoCFGList.Add(cfg);
                    }
                }

                if (Application.isEditor)
                {
                    RenderUtils.ShaderFind(gameObject);
                }

                if (gameObject.CompareTag("MainCamera") && gameObject.activeInHierarchy)
                {
                    Camera tempCamera = gameObject.GetComponent<Camera>();
                    if (tempCamera != null)
                    {
                        _sceneCamera = tempCamera;
                        _sceneAudioSource = gameObject.GetComponent<AudioSource>();
                        switchCamera(gameObject);
                    }
                    continue;
                }

                if (checkSceneGameObject != null)
                {
                    checkSceneGameObject(gameObject);
                }
            }
            if (Application.isEditor)
            {
                RenderUtils.RebindMaterial(RenderSettings.skybox);
            }

            float startTime = Time.realtimeSinceStartup;
            if (monoCFGList.Count > 0)
            {
                foreach (MonoCFG cfg in monoCFGList)
                {
                    this.routerMapElement(cfg);
                    ///限制一下每一个router时间;
                    if (Time.realtimeSinceStartup - startTime > 0.2f)
                    {
                        startTime = Time.realtimeSinceStartup;
                        yield return null;
                    }
                }
            }

            //DebugX.Log("loadScene2 b");
            loadScene3(sceneResourceName);
        }

        protected virtual void preReady()
        {
           
        }

        protected virtual void loadScene3(string sceneResourceName)
        {
            //DebugX.Log("loadScene3 "+ queueLoader.length);
            if (queueLoader.length > 0)
            {
                AssetsManager.bindEventHandle(queueLoader, queueCompleteHandle);
                queueLoader.addEventListener(EventX.PROGRESS, queueProgressHandle);
                queueLoader.start();
                return;
            }
            onReady();
        }

        protected virtual void queueProgressHandle(EventX e)
        {
            this.dispatchEvent(e);
        }

        protected virtual void queueCompleteHandle(EventX e)
        {
            AssetsManager.bindEventHandle(queueLoader, queueCompleteHandle);
            queueLoader.removeEventListener(EventX.PROGRESS, queueProgressHandle);
        
            onReady();
        }

        protected virtual void onReady()
        {
            isReady = true;
            this.simpleDispatch(EventX.COMPLETE);
        }


        protected virtual AssetResource addPreload(string url, LoaderXDataType type, Action<EventX> resultHandle = null)
        {
            ///
            return queueLoader.add(url, type, resultHandle);
        }

        protected virtual void bindSceneCFG(SceneCFG value)
        {
            _sceneCFG = value;
        }

        public SceneCFG sceneCFG
        {
            get { return _sceneCFG; }
        }

        public virtual Camera getSceneCamera()
        {
            return _sceneCamera;
        }

        public virtual AudioSource getSceneAudioSource()
        {
            return _sceneAudioSource;
        }

        public virtual Scene getCurrentActiveScene()
        {
            return _currentScene;
        }

        public virtual void switchCamera(GameObject gameObject)
        {
#if UNITY_EDITOR
            BaseApp.SwitchMainCamera(_sceneCamera);
#else
            gameObject.SetActive(false);
#endif
        }

        /// <summary>
        /// 用于已加载地图的数据同步
        /// </summary>
        public virtual void sceneReadySync()
        {
            foreach (MonoCFG cfg in monoCFGList)
            {
                this.routerMapElement(cfg);
            }
        }

        protected virtual void onUnloadPreScene(string preSceneResourceName)
        {
            if (string.IsNullOrEmpty(preSceneResourceName))
            {
                return;
            }
            Scene scene = SceneManager.GetSceneByName(preSceneResourceName);
            if (scene.IsValid())
            {
                unloadSceneAsyncOperation = SceneManager.UnloadSceneAsync(scene);
            }
            else
            {
                DebugX.LogWarning("onUnloadPreScene:" + preSceneResourceName + " error");
            }
        }

        protected virtual void progressHandle(EventX e)
        {
            this.simpleDispatch(e.type, MAX_PROGRESS * (float)e.data);
        }

        public bool addEventListener(string type, Action<EventX> listener, int priority = 0)
        {
            return gameObject.addEventListener(type, listener, priority);
        }

        public bool hasEventListener(string type)
        {
            return gameObject.hasEventListener(type);
        }

        public bool removeEventListener(string type, Action<EventX> listener)
        {
            return gameObject.removeEventListener(type, listener);
        }

        public bool dispatchEvent(EventX e)
        {
            return gameObject.dispatchEvent(e);
        }

        public bool simpleDispatch(string type, object data = null)
        {
            return gameObject.simpleDispatch(type, data);
        }
    }
}