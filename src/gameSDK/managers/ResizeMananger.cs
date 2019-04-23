using System;
using UnityEngine;
using System.Collections.Generic;
using gameSDK;
using UnityEngine.UI;

namespace foundation
{

    /// <summary>
    ///  场景变化管理类
    /// </summary>
    public class ResizeMananger:FoundationBehaviour
    {
        protected List<Action<float, float>> resizeList;
        protected float screenWidth = 1;
        protected float screenHeight = 1;
        protected float uiScale = 1.0f;

        protected float canvasWidth = 1;
        protected float canvasHeight = 1;

        protected float canvasViewWidth = 1;
        protected float canvasViewHeight = 1;

        protected CanvasScaler canvasScaler;
        protected Camera uiCamera;

        public static float uiDiySize = 640f;

        public static float GetCanvasWidth()
        {
            return instance.canvasWidth;
        }

        public static float GetCanvasHeight()
        {
            return instance.canvasHeight;
        }
        public static float GetCanvasViewWidth()
        {
            return instance.canvasViewWidth;
        }

        public static float GetCanvasViewHeight()
        {
            return instance.canvasViewHeight;
        }



        public static float GetSceneWidth()
        {
            return instance.screenWidth;
        }

        public static float GetSceneHeight()
        {
            return instance.screenHeight;
        }

        public static float getUIScale()
        {
            return instance.uiScale;
        }

        private static ResizeMananger instance;

        public ResizeMananger()
        {
            resizeList = new List<Action<float, float>>();

            instance = this;
        }

        public static ResizeMananger getInstance()
        {
            if (instance == null)
            {
                instance = BaseApp.resizeMananger;
            }

            return instance;
        }

        public static void Add(IResizeable item)
        {
            if (item == null)
            {
                return;
            }
            getInstance().add(item.onResize);
        }

        public static void Remove(IResizeable item)
        {
            if (item == null)
            {
                return;
            }
            getInstance().remove(item.onResize);
        }


        public static void Add(Action<float, float> item)
        {
            if (item == null)
            {
                return;
            }
            getInstance().add(item);
        }

        public static void Remove(Action<float, float> item)
        {
            if (item == null)
            {
                return;
            }
            getInstance().remove(item);
        }


        public static void diy(float w, float h)
        {
            getInstance().resize(w, h);
        }

        protected virtual void Update()
        {
            resize(Screen.width, Screen.height);
        }

        public void resize(float w, float h)
        {
            if (w == screenWidth && h == screenHeight)
            {
                return;
            }

            //float screenAspect = w/h;

            int len = resizeList.Count;
            screenWidth  = w;
            screenHeight = h;

            if (canvasScaler != null)
            {
                Vector2 v = this.canvasScaler.referenceResolution;
                //float canvasAspect = v.x / v.y;
                if (this.canvasScaler.matchWidthOrHeight == 0.0f)
                {
                    canvasViewWidth = v.x;
                    canvasViewHeight = v.x/screenWidth*screenHeight;
                    uiScale = canvasViewHeight/v.y;
                }
                else
                {
                    canvasViewHeight = v.y;
                    canvasViewWidth = v.y/screenHeight * screenWidth;
                    uiScale = canvasViewWidth/v.x;
                }
            }
            else
            {
                uiScale = 1.0f;
                canvasWidth = w;
                canvasHeight = h;
            }

            for (int i = 0; i < len; i++)
            {
                Action<float, float> item = resizeList[i];
                item(canvasViewWidth, canvasViewHeight);
            }
            //DispatchEvent(e);
        }

        public static float GetAspect()
        {
            return instance.screenWidth/instance.screenHeight;
        }

        /// <summary>
        /// 添加;
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool add(Action<float, float> item)
        {
            refreash(item);
            if (resizeList.IndexOf(item) != -1)
            {
                return false;
            }
            resizeList.Add(item);
            return true;
        }

        public void refreash(Action<float, float> value)
        {
            value(canvasViewWidth, canvasViewHeight);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int remove(Action<float, float> item)
        {
            int index = resizeList.IndexOf(item);
            if (index == -1) return -1;

            resizeList.RemoveAt(index);
            return index;
        }

        public static void Resize(int width, int height)
        {
            getInstance().resize(width, height);
        }

        public void initialize(CanvasScaler canvasScaler,Camera uiCamera)
        {
            this.canvasScaler = canvasScaler;
            this.uiCamera = uiCamera;
        }
    }

}