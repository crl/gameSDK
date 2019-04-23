using System;
using foundation;

namespace gameSDK
{
    [Serializable]
    public abstract class SkillEvent:ISkillEvent
    {
        [NonSerialized]
        protected BaseSkill baseSkill;
        [NonSerialized]
        protected SkillLine line;
        [NonSerialized]
        protected SkillPoint point;

        protected bool _enabled = true;
        public bool enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public virtual bool hasInterpolation
        {
            get { return false; }
        }

        public virtual void init(BaseSkill baseSkill, SkillLine line, SkillPoint point)
        {
            this.baseSkill = baseSkill;
            this.line = line;
            this.point = point;
        }

        /// <summary>
        /// 第一次启动
        /// </summary>
        public virtual void firstStart()
        {
        }

        /// <summary>
        /// 进入
        /// </summary>
        public virtual void enter()
        {
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="avg"></param>
        public virtual void update(float avg=0.0f)
        {
        }
        /// <summary>
        /// 退出
        /// </summary>
        public virtual void exit()
        {
        }

        /// <summary>
        /// 强制退出
        /// </summary>
        /// <param name="percent"></param>
        public virtual void forceStop(float percent)
        {
            if (percent!=-1)
            {
                exit();
            }
        }
        public abstract ISkillEvent clone();
    }
}