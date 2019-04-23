using System;
using foundation;
using UnityEngine;
using UnityEngine.UI;
namespace clayui
{
    public class BaseProgressBar : SkinBase
    {
        public Image bg;
        public Image bar;
        public Text label;
        public Image tweenBar;
        public float tweenTime = 0.5f;
        protected float _progress = 1.0f;
        protected float _minProgress = 0.0f;

        protected RFTweenerTask _tweener;

        protected Vector2 _defaultBarSize;
        protected Vector2 _defaultTweenBarSize;

        private bool _isInitialized = false;

        public void initialize()
        {
            _isInitialized = true;

            if (tweenBar != null)
            {
                if (tweenBar.type != Image.Type.Filled)
                {
                    _defaultTweenBarSize = tweenBar.GetComponent<RectTransform>().sizeDelta;
                }
            }
            if (bar != null)
            {
                if (bar.type != Image.Type.Filled)
                {
                    _defaultBarSize = bar.GetComponent<RectTransform>().sizeDelta;
                }
            }

            progress = 0;
        }

        public float progress
        {
            get { return _progress; }

            set { doProgress(value); }
        }

        public float minProgress
        {
            set { _minProgress = value; }
        }

        public void doProgress(float value,bool motion=true)
        {
            if (_isInitialized == false)
            {
                initialize();
            }
            _progress = Mathf.Max(value,_minProgress);
            if (_progress > 1.0f)
            {
                _progress = 1.0f;
            }
            if (bar != null)
            {
                if (bar.type == Image.Type.Filled)
                {
                    bar.fillAmount = _progress;
                }
                else
                {
                    RectTransform rect = bar.GetComponent<RectTransform>();
                    Vector2 v = new Vector2(_defaultBarSize.x*_progress, _defaultBarSize.y);
                    rect.sizeDelta = v;
                }
            }
            if (tweenBar != null)
            {
                if (_tweener != null)
                {
                    _tweener.stop();
                    _tweener = null;
                }
                if (tweenBar.type == Image.Type.Filled)
                {
                    if (motion)
                    {
                        _tweener = TweenGeneral.Play(tweenBar.gameObject, tweenBar.fillAmount,_progress,updateTweenBar, tweenTime);
                    }
                    else
                    {
                        tweenBar.fillAmount = _progress;
                    }
                }
                else
                {
                    Vector2 v = new Vector2(_defaultTweenBarSize.x * _progress, _defaultTweenBarSize.y);
                    if (motion)
                    {
                        _tweener = TweenImageSize.Play(tweenBar, tweenTime, v);
                    }
                    else
                    {
                        tweenBar.GetComponent<RectTransform>().sizeDelta=v;
                    }
                }

                if (_tweener != null)
                {
                    _tweener.onComplete = onTweenComplete;
                }
            }
        }

        private void updateTweenBar(float v)
        {
            tweenBar.fillAmount = v;
        }

        private void onTweenComplete(RFTweenerTask task)
        {
            _tweener = null;
        }

        public string text
        {
            set
            {
                if (label != null)
                {
                    label.text = value;
                }
            }
        }

        public virtual void reset()
        {
            if (bar != null)
            {
                bar.fillAmount = 1;
            }
            if (tweenBar != null)
            {
                tweenBar.fillAmount = 1;
            }
        }


        public virtual void setLoadedTotal(float loaded, float total)
        {
            progress = loaded/total;
        }
    }
}