using System;
using System.Collections.Generic;
using System.IO;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class BaseSkill:EventDispatcher
    {
        protected List<SkillLine> _timerLines;
        protected AssetResource _resource;
        protected string _uri;
        protected string _url;
        public string resourceRootDir;
        public int loadPriority = 0;
        public uint retryCount = 0;
      
        protected BaseObject _caster;
        protected List<BaseObject> _targetList;
        protected SkillExData _casterSkillExData;

        private SkillTimeLineVO _timeLineVO;

        protected int _runedTime = 0;
        protected bool _isClosed = false;
        public GameObject parent;

        protected float _timeScale = 1.0f;

        public float timeScale
        {
            get { return _timeScale; }
            set { _timeScale = value; }
        }

        public BaseSkill()
        {
            _timerLines=new List<SkillLine>();
            _targetList=new List<BaseObject>(5);
            resourceRootDir = PathDefine.commonPath;
        }

        public bool load(string value)
        {
            if (string.IsNullOrEmpty(_uri) == false)
            {
                stop(false);
            }

            this._uri = value;
            if (string.IsNullOrEmpty(_uri))
            {
                this.simpleDispatch(EventX.FAILED);
                return false;
            }
            this._url = getURL(_uri);
            if (_resource != null)
            {
                AssetsManager.bindEventHandle(_resource, resourceHandle, false);
                _resource.release();
            }
            _resource = AssetsManager.getResource(_url, LoaderXDataType.AMF);
            autoDefaultResource(_resource);

            _resource.retain();
            AssetsManager.bindEventHandle(_resource, resourceHandle);
            _resource.load(retryCount, false, loadPriority);
            return true;
        }

        protected virtual void autoDefaultResource(AssetResource resource)
        {
            if (ApplicationVersion.isDebug && Application.isMobilePlatform == false)
            {
                //首次强制远程下载
                if (resource.retainCount == 0)
                {
                    resource.isForceRemote = true;
                }
            }
        }

        public string getURI()
        {
            return _uri;
        }

        public virtual string getURL(string uri) { 
            return resourceRootDir+"skill/" +uri+".amf";
        }

        protected virtual void resourceHandle(EventX e)
        {
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, resourceHandle,false);
            if (e.type != EventX.COMPLETE)
            {
                this.simpleDispatch(EventX.FAILED);
                return;
            }

            SkillTimeLineVO skillTimeLineVo =resource.data as SkillTimeLineVO;
            if (skillTimeLineVo == null)
            {
                this.simpleDispatch(EventX.FAILED);
                return;
            }

            initialize(skillTimeLineVo);
        }

        public void play(SkillTimeLineVO skillTimeLineVo)
        {
            initialize(skillTimeLineVo);
        }
        public void playTo(SkillTimeLineVO skillTimeLineVo, int index)
        {
            initialize(skillTimeLineVo);
            TickManager.Remove(update);
            _runedTime = index*100;
            update(0);
        }

        public void createBy(BaseObject caster, List<BaseObject> targetList, SkillExData exData)
        {
            this._caster = caster;
            if (caster != null)
            {
                caster.addSkill(this);
            }

            this._targetList.Clear();
            if (targetList != null)
            {
                foreach (BaseObject item in targetList)
                {
                    this._targetList.Add(item);
                }
            }

            this._casterSkillExData = exData;
            _isClosed = false;
        }

        protected virtual void initialize(SkillTimeLineVO value)
        {
            this._timeLineVO = value;
            _runedTime =0;
            _timerLines.Clear();

            List<SkillLineVO> lines = _timeLineVO.lines;
            int len = lines.Count;
            SkillLineVO lineVo;
            for (int i = 0; i < len; i++)
            {
                lineVo = lines[i];
                if (lineVo.enabled)
                {
                    _timerLines.Add(new SkillLine(this, lineVo));
                }
            }

            if (_casterSkillExData == null)
            {
                _casterSkillExData=new SkillExData();
            }
            this.simpleDispatch(EventX.START);
            TickManager.Add(update);
        }


        public void setExData(string key, object value)
        {
            if (_casterSkillExData == null)
            {
                _casterSkillExData = new SkillExData();
            }
            _casterSkillExData.Add(key,value);
        }

        public object getExData(string key)
        {
            object o = null;
             _casterSkillExData.TryGetValue(key,out o);
            return o;
        }

        public SkillExData getExData()
        {
            return _casterSkillExData;
        }

        public BaseObject getCaster()
        {
            return _caster;
        }

        public List<BaseObject> getTargetList()
        {
            return _targetList;
        }

        private Action<BaseSkill,SkillLine, EffectCreateEvent, SkillEventState> outEffectObjectEventStateUpdate=null;
        public void setOutEffectObjectEventStateUpdate(Action<BaseSkill, SkillLine, EffectCreateEvent, SkillEventState> value)
        {
            outEffectObjectEventStateUpdate = value;
        }
        internal void effectObjectEventStateUpdate(SkillLine skillLine,EffectCreateEvent effectEvent, SkillEventState state)
        {
            if (outEffectObjectEventStateUpdate != null)
            {
                outEffectObjectEventStateUpdate(this, skillLine, effectEvent, state);
            }
        }

        private void update(float deltaTime)
        {
            bool beClosed = true;
            int len = _timerLines.Count;
            for (int i = 0; i < len; i++)
            {
                SkillLine skillLine = _timerLines[i];
                if (skillLine.isClosed == false)
                {
                    skillLine.update(_runedTime);
                    beClosed = beClosed && skillLine.isClosed;
                }
            }

            _isClosed = beClosed;
            _runedTime += (int)(deltaTime * 1000 * timeScale);

            if (_isClosed)
            {
               stop(true);
            }
        }

        public void stop(bool fireEvent=true)
        {
            _runedTime = 0;
            _isClosed = true;

            TickManager.Remove(update);

            int len = _timerLines.Count;
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    SkillLine skillLine = _timerLines[i];
                    skillLine.forceStop();
                }
                _timerLines.Clear();
            }

            if (_resource != null)
            {
                AssetsManager.bindEventHandle(_resource, resourceHandle, false);
                _resource.release();
                _resource = null;
            }

            if (fireEvent)
            {
                this.simpleDispatch(EventX.COMPLETE);
            }
        }


        private List<BaseObject> timeLineEffects;
        public List<BaseObject> getTimeLineEffects()
        {
            if (timeLineEffects == null)
            {
                timeLineEffects = new List<BaseObject>();
            }
            else
            {
                timeLineEffects.Clear();
            }

            foreach (SkillLine timerLine in _timerLines)
            {
                foreach (BaseObject item in timerLine.effectList)
                {
                    if (item != null)
                    {
                        timeLineEffects.Add(item);
                    }
                }
            }
            return timeLineEffects;
        }

        public BaseObject getTimeLineEffectByName(string name)
        {
            foreach (SkillLine timerLine in _timerLines)
            {
                foreach (BaseObject item in timerLine.effectList)
                {
                    if (item != null && item.name == name)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public override void Dispose()
        {
            if (_isClosed == false)
            {
                stop(true);
            }
            base.Dispose();
        }

        public bool IsClosed
        {
            get { return _isClosed; }
        }

        
    }
}