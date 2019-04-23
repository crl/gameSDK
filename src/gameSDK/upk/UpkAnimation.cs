using foundation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace foundation
{
    public class UpkAnimation : EventDispatcher
    {
        private List<SpriteInfoVO> mSprites;
        private bool mLoop;
        private bool isReady = false;
        private bool mPlaying;
        private List<float> mDurations;
        private List<float> mStartTimes;
        private float mDefaultFrameDuration;
        private float mCurrentTime;
        private int mCurrentFrame=-1;
        public Image image;
        public bool autoNativeSize;
        private bool isInnerCreate = false;
        public string prefix = PathDefine.uiPath;
        public UpkAnimation(Image image=null)
        {
            if (image == null)
            {
                isInnerCreate = true;
                image = UIUtils.CreateImage("image", UILocater.FollowLayer);
                UIUtils.SetSize(image.gameObject,1,1);
                image.raycastTarget = false;
            }
            this.image = image;
            SetActive(false);
        }



        public void setParent(GameObject layerGameObject)
        {
            if (image != null)
            {
                image.transform.SetParent(layerGameObject.transform, false);
            }
        }

        public override void Dispose()
        {
            if (isInnerCreate)
            {
                if (image!=null)
                {
                    GameObject.Destroy(image);
                    image = null;
                }
            }
            base.Dispose();
        }

        private void SetActive(bool v)
        {
            if (image != null)
            {
                image.gameObject.SetActive(v);
            }
        }

        public Vector3 position
        {
            get { return image.transform.localPosition; }
            set { image.transform.localPosition = value; }
        }

        public Vector3 scale
        {
            get { return image.transform.localScale; }
            set { image.transform.localScale = value; }
        }


        private AssetResource resource;
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

            mSprites = o.keys;

            this.mLoop = o.loop;
            init(o);

            if (mPlaying)
            {
                doPlay();
            }
        }

        private void doPlay()
        {
            tick(0);
            SetActive(true);
            TickManager.Add(tick);
        }

        public void init(UpkAniVO vo)
        {
            float fps = vo.fps;
            if (fps <= 0) throw new ArgumentException("Invalid fps: " + fps);
            List<SpriteInfoVO> sprites = vo.keys;
            int numFrames = sprites.Count;

            mDefaultFrameDuration = 1.0f/fps;

            mPlaying = true;
            mCurrentTime = 0.0f;
            mCurrentFrame = 0;
            mSprites = sprites;
            //mSounds = new List<AudioSource>(numFrames);

            mDurations = new List<float>(numFrames);
            mStartTimes = new List<float>(numFrames);


            float startPosition = 0;
            float duraction = 0;
            for (int i = 0; i < numFrames; ++i)
            {
                mStartTimes.Add(startPosition);
                duraction = mDefaultFrameDuration + sprites[0].delay;
                mDurations.Add(duraction);
                startPosition += duraction;
                //mSounds.Add(null);
            }

        }

        private string aniName;
        public void play(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            mPlaying = true;
            if (this.aniName == name)
            {
                if (isReady)
                {
                    doPlay();
                }
                return;
            }

            isReady = false;

            this.aniName = name;
            string uri = getURL(aniName);
            if (resource != null)
            {
                resource.release();
                AssetsManager.bindEventHandle(resource, completeHandle, false);
            }
           
            resource = AssetsManager.getResource(uri, LoaderXDataType.ASSETBUNDLE);
            resource.retain();
            AssetsManager.bindEventHandle(resource, completeHandle);
            resource.load();
        }

        protected virtual string getURL(string uri)
        {
            return PathDefine.uiPath + "ui/" + uri + PathDefine.U3D;
        }

        public void gotoAndStop(int frame)
        {
            if (image != null)
            {
                int numFrames=0;

                if (mSprites != null)
                {
                    numFrames=mSprites.Count;
                }
                int index = Math.Max(0, Mathf.Min(numFrames, frame));

                currentFrame = index;
            }

            SetActive(true);
        }

        public void pause()
        {
            mPlaying = false;
        }

        public void stop()
        {
            mPlaying = false;
            currentFrame = 0;

            SetActive(false);
            TickManager.Remove(tick);
        }

        public int numFrames
        {
            get { return mSprites.Count; }
        }

        public int currentFrame
        {
            get { return mCurrentFrame; }

            set
            {
                mCurrentFrame = value;
                mCurrentTime = 0.0f;

                for (int i = 0; i < value; ++i)
                {
                    mCurrentTime += getFrameDuration(i);
                }
                if (mSprites != null)
                {
                    image.sprite = mSprites[mCurrentFrame].sprite;
                    if (autoNativeSize)
                    {
                        image.SetNativeSize();
                    }
                }

            }
        }

        public float getFrameDuration(int frameID)
        {
            if (frameID < 0 || frameID >= numFrames) throw new ArgumentException("Invalid frame id");
            return mDurations[frameID];
        }

        public float totalTime
        {
            get
            {
                int numFrames = mSprites.Count;
                return mStartTimes[(numFrames - 1)] + mDurations[(numFrames - 1)];
            }
        }


        public void tick(float deltaTime)
        {
            if (!mPlaying) return;

            int finalFrame;
            int previousFrame = mCurrentFrame;
            float restTime = 0.0f;
            bool breakAfterFrame = false;
            bool dispatchCompleteEvent = false;
            float totalTime = this.totalTime;

            if (mLoop && mCurrentTime >= totalTime)
            {
                mCurrentTime = 0.0f;
                mCurrentFrame = 0;
            }

            if (mCurrentTime < totalTime)
            {
                mCurrentTime += deltaTime;
                finalFrame = mSprites.Count - 1;

                while (mCurrentTime > mStartTimes[mCurrentFrame] + mDurations[mCurrentFrame])
                {
                    if (mCurrentFrame == finalFrame)
                    {
                        if (mLoop && !hasEventListener(EventX.COMPLETE))
                        {
                            mCurrentTime -= totalTime;
                            mCurrentFrame = 0;
                        }
                        else
                        {
                            breakAfterFrame = true;
                            restTime = mCurrentTime - totalTime;
                            dispatchCompleteEvent = true;
                            mCurrentFrame = finalFrame;
                            mCurrentTime = totalTime;
                        }
                    }
                    else
                    {
                        mCurrentFrame++;
                    }

                    if (breakAfterFrame)
                    {
                        break;
                    }
                }

                // special case when we reach *exactly* the total time.
                if (mCurrentFrame == finalFrame && mCurrentTime == totalTime)
                {
                    dispatchCompleteEvent = true;
                }
            }

            if (mCurrentFrame != previousFrame)
            {
                image.sprite = mSprites[mCurrentFrame].sprite;
                if (autoNativeSize)
                {
                    image.SetNativeSize();
                }
            }
            if (dispatchCompleteEvent)
            {
                stop();
                simpleDispatch(EventX.COMPLETE);
            }
            if (mLoop && restTime > 0.0)
            {
                tick(deltaTime);
            }
        }
    }
}