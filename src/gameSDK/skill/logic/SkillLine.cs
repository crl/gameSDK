using System;
using System.Collections.Generic;
using foundation;

namespace gameSDK
{
    public class SkillLine
    {
        private SkillLineVO vo;
        private BaseSkill baseSkill;
        private SkillExData exData;
        private List<SkillPoint> points=new List<SkillPoint>();
        private int totalPoints = 0;
        private int lastTime = 0;

        public List<BaseObject> effectList
        {
            private set;
            get;
        }
        private int nextIndex = 0;
        private int runingCount = 0;
        protected bool _isClosed = false;
        public string name
        {
            get;
            private set;
        }
        public EventTargetType targetType;
        public List<SkillPoint> Points
        {
            get { return points; }
        }
        public SkillLine(BaseSkill baseSkill,SkillLineVO vo)
        {
            this.vo = vo;
            this.name = vo.name;
            this.baseSkill = baseSkill;
            this.exData = baseSkill.getExData();

            effectList = new List<BaseObject>();

            foreach (SkillPointVO pointVo in vo.points)
            {
                SkillPoint sp = new SkillPoint(baseSkill,this,pointVo);
                points.Add(sp);
                totalPoints++;
            }
            targetType = vo.targetType;
            lastTime = points[totalPoints - 1].startTime;
        }

        private SkillPoint _lastPointer;
        private SkillPoint _currentPointer;
        private int _runedTime=-1;
        public void update(int sinceTime)
        {
            _runedTime = sinceTime - lastTime * runingCount;
            SkillPoint nextPointer = null;
            if (nextIndex < totalPoints)
            {
                nextPointer = points[nextIndex];
            }
            else
            {
                if (runingCount < vo.playCount || vo.playCount==-1)
                {
                    if (_currentPointer != null)
                    {
                        _currentPointer.finished(runingCount);
                        _lastPointer = _currentPointer;
                        _currentPointer = null;
                    }
                    runingCount++;
                    nextIndex = 0;
                }
                else
                {
                    completeHandle(false);
                }
                return;
            }

            bool hasNewStart = false;
            while (_runedTime >= nextPointer.startTime)
            {
                if (_currentPointer!=null)
                {
                    _currentPointer.exit(false);
                }
                _currentPointer = nextPointer;
                _currentPointer.start(runingCount);
                hasNewStart = true;

                if (++nextIndex == totalPoints)
                {
                    if (runingCount < vo.playCount || vo.playCount==-1)
                    {
                        _runedTime -= lastTime;
                        runingCount++;
                        nextIndex = 0;
                        if (_currentPointer != null)
                        {
                            _currentPointer.finished(runingCount);
                            _lastPointer = _currentPointer;
                            _currentPointer = null;
                        }
                    }
                    else
                    {
                        completeHandle(false);
                    }
                    return;
                }
                nextPointer = points[nextIndex];
            }

            if (hasNewStart == false && _currentPointer!=null)
            {
                float total = nextPointer.startTime - _currentPointer.startTime;
                float load = _runedTime - _currentPointer.startTime;
                _currentPointer.update(load / total);
            }
        }

        public bool isClosed
        {
            get { return _isClosed; }
        }

        /// <summary>
        /// 当前的都播放完成了
        /// </summary>
        protected void completeHandle(bool isForce)
        {
            _isClosed = true;

            _lastPointer = null;

            if (_currentPointer != null)
            {
                _currentPointer.exit(isForce);
                _currentPointer = null;
            }

            int len = effectList.Count;
            if (len > 0)
            {
                BaseObject effectObject;
                for (int i = 0; i < len; i++)
                {
                    effectObject = effectList[i];
                    if (effectObject != null)
                    {
                        disposeEffect(effectObject);
                    }
                }
                effectList.Clear();
            }
        }

        private void disposeEffect(BaseObject effect)
        {
            if (effect == null)
            {
                return;
            }
            if (exData.forceLayer != -1)
            {
                if (effect.skin)
                {
                    effect.skin.SetLayerRecursively(LayerX.GetDefaultLayer());
                }
            }
            effect.dispose();
        }

        public void forceStop()
        {
            if (_isClosed)
            {
                return;
            }
            _isClosed = true;

            if (_lastPointer!=null)
            {
                _lastPointer.exit(true);
                _lastPointer = null;
            }

            completeHandle(true);
        }

        public void addEffect(BaseObject effectObject)
        {
            if (effectList.Contains(effectObject) == false)
            {
                effectList.Add(effectObject);
            }
        }
    }
}