using foundation;
using foundation.story;
using System;
using System.Collections;
using UnityEngine;

namespace gameSDK
{
    public interface ITalkUI
    {
       // void StartTalk(Cutscene playableDirector, OverlayText talkerPlayable);
       // void ClearTalk(Cutscene playableDirector, OverlayText talkerPlayable);
    }

    public interface IStoryUI : ITalkUI, IPanel
    {
    }
    public class BaseStoryManager: FoundationBehaviour, ITalkUI
    {
        private bool _isEndState = true;
        protected string _currentStroryName;
        protected AssetResource _resource;
        protected Coroutine _coroutine;
        protected IStoryUI _uilayer;
        public string prefix = "map/";
        public float storyFadeTime = 0.2f;
        public float screenFadeTime = 1.0f;

        protected Action<string> _currentCallBack;
//        protected Cutscene _currentCutScene;
//        public void StartTalk(Cutscene playableDirector, OverlayText talkerPlayable)
//        {
//            getStoryUI().StartTalk(playableDirector, talkerPlayable);
//        }

        virtual protected IStoryUI getStoryUI()
        {
            if (_uilayer == null)
            {
                _uilayer = new BaseStoryUIImplement();
            }
            return _uilayer;
        }

//        public void ClearTalk(Cutscene playableDirector, OverlayText talkerPlayable)
//        {
//            getStoryUI().ClearTalk(playableDirector, talkerPlayable);
//        }

        public virtual void playStory(string name, Action<string> callback = null)
        {
            stopAll();

            _currentStroryName = name;
            _currentCallBack = callback;
            string url = getURL(name);
            _resource = AssetsManager.getResource(url, LoaderXDataType.PREFAB);
            _resource.retain();
            AssetsManager.bindEventHandle(_resource, resourceHandle);
            _resource.load();
        }

        protected string currentStroryName
        {
            get { return _currentStroryName; }
        }

        public bool isCurrent(string name)
        {
            return _currentStroryName == name;
        }

        public virtual void stopAll()
        {
            if (_resource != null)
            {
                AssetsManager.bindEventHandle(_resource, resourceHandle, false);
                _resource.release();
                _resource = null;
            }
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                FadeGUI.Enabled = false;
                _coroutine = null;
            }

//            if (_currentCutScene != null)
//            {
//                CutsceneInject.PreStop(_currentCutScene);
//                _currentCutScene.Stop();
//
//                CutsceneInject.DeInit(_currentCutScene);
//                DestroyImmediate(_currentCutScene.gameObject);
//                _currentCutScene = null;
//                _isEndState = true;
//
//                this.preStartGameLogic();
//                this.startGameLogic();
//                this.postStartGameLogic();
//            }
        }

        protected virtual string getURL(string value)
        {
            string uri = prefix + value + PathDefine.U3D;
            return PathDefine.storyPath + uri;
        }

        private void resourceHandle(EventX e)
        {
//            if (e.type == EventX.COMPLETE)
//            {
//                GameObject newCutScene = _resource.getNewInstance() as GameObject;
//                if (newCutScene != null)
//                {
//                    _currentCutScene = newCutScene.GetComponent<Cutscene>();
//                    if (_currentCutScene != null && _currentCutScene.isActive)
//                    {
//                        _currentCutScene.Stop();
//                    }
//                }
//            }
//            _isEndState = false;
//            if (_currentCutScene != null)
//            {
//                _coroutine=StartCoroutine(CorotineFadeGUI(true));
//            }
//            else
//            {
//                this.preStartGameLogic();
//                this.startGameLogic();
//                this.postStartGameLogic();
//            }
        }

        public bool hasFadeTransform = true;
        public bool hasFadeOutTransform = true;
        public bool hasFadeInTransform = true;

        private IEnumerator CorotineFadeGUI(bool isShow)
        {
            if (isShow)
            {
                preStopGameLogic();
            }
            else
            {
//                if (_currentCutScene != null)
//                {
//                    CutsceneInject.PreStop(_currentCutScene);
//                }

                preStartGameLogic();
            }
            FadeGUI.Enabled = true;
           

            bool isBreak = false;
            float t = 0;
            if (hasFadeTransform)
            {
                if (hasFadeOutTransform)
                {
                    ///从白到黑
                    while (t < storyFadeTime)
                    {
                        t += Time.deltaTime;
                        float v = (t / storyFadeTime);
                        FadeGUI.Alpha = Mathf.Sin(v * Mathf.PI/2f);
                        yield return null;
                    }
                }

                FadeGUI.Alpha = Mathf.Sin(Mathf.PI/2f);
            }

//            if (isShow)
//            {
//                if (_currentCutScene != null)
//                {
//                    stopGameLogic();
//                    CutsceneInject.Init(_currentCutScene);
//                    _currentCutScene.Play();
//                }
//                else
//                {
//                    isBreak = true;
//                    preStartGameLogic();
//                    startGameLogic();
//                    postStartGameLogic();
//                    ///清空当前
//                    _currentStroryName = null;
//                }
//            }
//            else
//            {
//                if (_currentCutScene != null)
//                {
//                    _currentCutScene.Stop();
//                    CutsceneInject.DeInit(_currentCutScene);
//                    DestroyImmediate(_currentCutScene.gameObject);
//
//                    _currentCutScene = null;
//                }

//                startGameLogic();
//                ///清空当前
//                _currentStroryName = null;
//            }

            if (hasFadeTransform && hasFadeInTransform)
            {
                t = 0;
                ///从黑到白
                while (t < screenFadeTime)
                {
                    t += Time.deltaTime;
                    float v = 1.0f + (t / screenFadeTime);
                    FadeGUI.Alpha = Mathf.Sin(v * Mathf.PI/2f);
                    yield return null;
                }
            }


            FadeGUI.Enabled = false;

            if (isBreak == false)
            {
                if (isShow)
                {
                    postStopGameLogic();
                }
                else
                {
                    postStartGameLogic();
                }
            }
        }

        protected virtual void Update()
        {
//            if (_currentCutScene != null)
//            {
//                if (_isEndState==false && (_currentCutScene.length-_currentCutScene.currentTime)<storyFadeTime)
//                {
//                    _isEndState = true;
//                    _coroutine= StartCoroutine(CorotineFadeGUI(false));
//                }
//            }
        }
        protected virtual void preStopGameLogic()
        {
        }
        protected virtual void stopGameLogic()
        {
        }
        protected virtual void postStopGameLogic()
        {
        }

        protected virtual void preStartGameLogic()
        {
         
        }

        protected virtual void startGameLogic()
        {

            if (_uilayer != null)
            {
                _uilayer.hide();
            }

            Action<string> oldAction = _currentCallBack;
            if (oldAction != null)
            {
                _currentCallBack = null;
                oldAction(_currentStroryName);
            }
            _currentStroryName = null;
        }

        protected virtual void postStartGameLogic()
        {

        }
    }
}